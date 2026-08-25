using BF;
using Game.Combat;
using Game.Content;
using Game.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// 战斗状态。Ready = 装配完成未开始；Playing = 进行中（可部署、可战斗）。
    /// </summary>
    public enum BattleState
    {
        Ready,
        Playing,
        Paused,
        Won,
        Lost
    }

    /// <summary>
    /// 战斗总控：状态机、能量恢复、倒计时、部署入口与胜负结算。
    /// 场景内唯一实例，组件通过 BattleFlow.Current / BattleFlow.IsActive 访问。
    /// </summary>
    public class BattleFlow : MonoBehaviour
    {
        public static BattleFlow Current { get; private set; }

        /// <summary>战斗是否正在进行。所有战斗组件以此为总开关（暂停/结算后自动停止）。</summary>
        public static bool IsActive => Current != null && Current.state == BattleState.Playing;

        [SerializeField, Tooltip("只读状态展示，运行时由代码驱动")]
        BattleState state = BattleState.Ready;

        LevelDefinition level;
        float energy;
        float remainingSeconds;
        bool saveWritten;

        public BattleState State => state;
        public LevelDefinition Level => level;
        public float Energy => energy;
        public float EnergyMax => level != null ? level.EnergyMax : 0f;
        public float RemainingSeconds => remainingSeconds;

        /// <summary>能量变化（当前值, 上限）。</summary>
        public event System.Action<float, float> EnergyChanged;
        /// <summary>剩余秒数变化。</summary>
        public event System.Action<float> TimeChanged;
        /// <summary>状态切换。</summary>
        public event System.Action<BattleState> StateChanged;

        void Awake()
        {
            Current = this;
            Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        /// <summary>由 BattleSetup 在生成战场前调用，注入关卡配置。</summary>
        public void Init(LevelDefinition levelDefinition)
        {
            level = levelDefinition;
            energy = levelDefinition.EnergyStart;
            remainingSeconds = levelDefinition.TimeLimitSeconds;
        }

        /// <summary>战场装配完成后开始战斗。</summary>
        public void StartBattle()
        {
            if (state != BattleState.Ready)
            {
                return;
            }
            GameManager.Instance().IsGame = true;
            GameManager.Instance().IsPlaying = true;
            SetState(BattleState.Playing);
            EnergyChanged?.Invoke(energy, EnergyMax);
            TimeChanged?.Invoke(remainingSeconds);
        }

        void Update()
        {
            if (state != BattleState.Playing || level == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            float newEnergy = Mathf.Min(level.EnergyMax, energy + level.EnergyRegenPerSecond * deltaTime);
            if (!Mathf.Approximately(newEnergy, energy))
            {
                energy = newEnergy;
                EnergyChanged?.Invoke(energy, level.EnergyMax);
            }

            remainingSeconds -= deltaTime;
            TimeChanged?.Invoke(remainingSeconds);
            if (remainingSeconds <= 0f)
            {
                remainingSeconds = 0f;
                Lose();
            }
        }

        /// <summary>
        /// 玩家部署入口：能量足够则扣除能量并生成士兵。
        /// 返回是否部署成功（能量不足或战斗未进行时为 false）。
        /// </summary>
        public bool TryDeploy(SoldierDefinition definition)
        {
            if (state != BattleState.Playing || definition == null || definition.Prefab == null)
            {
                return false;
            }
            if (!PoolManager.Instance().IsContain(definition.Prefab.name))
            {
                Debug.LogError($"[Battle] 士兵预制体未注册到 PoolManager：{definition.Prefab.name}");
                return false;
            }
            if (energy < definition.EnergyCost)
            {
                return false;
            }

            energy -= definition.EnergyCost;
            EnergyChanged?.Invoke(energy, level.EnergyMax);
            SpawnSoldier(definition);
            return true;
        }

        void SpawnSoldier(SoldierDefinition definition)
        {
            GameObject instance = CombatPool.Spawn(definition.Prefab);
            SoldierControl control = instance.GetComponent<SoldierControl>();
            control.Initialize(new SoldierInit
            {
                Definition = definition,
                Path = level.SoldierPath as System.Collections.Generic.List<Vector3> ?? new System.Collections.Generic.List<Vector3>(level.SoldierPath),
                SpawnOffset = new Vector3(0f, Random.Range(-0.35f, 0.35f), 0f)
            });
            control.Open();
        }

        /// <summary>大本营被摧毁时由 BattleSetup 转发。</summary>
        public void NotifyBaseDestroyed()
        {
            if (state != BattleState.Playing)
            {
                return;
            }
            Win();
        }

        public void TogglePause()
        {
            if (state == BattleState.Playing)
            {
                SetState(BattleState.Paused);
                GameManager.Instance().IsPlaying = false;
            }
            else if (state == BattleState.Paused)
            {
                SetState(BattleState.Playing);
                GameManager.Instance().IsPlaying = true;
            }
        }

        /// <summary>重新加载当前场景（结算面板与暂停面板的重试入口）。</summary>
        public void Retry()
        {
            Time.timeScale = 1f;
            GameManager gameManager = GameManager.Instance();
            if (gameManager != null)
            {
                gameManager.IsPlaying = true;
            }
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
            {
                SceneManager.LoadScene(scene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(scene.path);
            }
        }

        void Win()
        {
            SetState(BattleState.Won);
            GameManager.Instance().IsPlaying = false;
            WriteSave();
        }

        void Lose()
        {
            SetState(BattleState.Lost);
            GameManager.Instance().IsPlaying = false;
        }

        void WriteSave()
        {
            if (saveWritten || level == null)
            {
                return;
            }
            saveWritten = true;
            SaveService.MarkLevelCompleted(level.ContentId, remainingSeconds);
        }

        void SetState(BattleState newState)
        {
            if (state == newState)
            {
                return;
            }
            state = newState;
            StateChanged?.Invoke(state);
        }
    }
}
