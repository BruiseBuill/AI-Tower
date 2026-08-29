using Game.Content;
using Game.Core;
using Game.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 战斗 HUD：能量条、倒计时、当前部署档位、暂停面板与结算面板。
    /// 所有数值来自 BattleFlow 的事件，HUD 不反向修改玩法状态（只转发玩家操作）。
    /// 部署档位由关卡定义中的士兵能量消耗决定，新增兵种无需改 UI 代码。
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        [Header("能量")]
        [SerializeField] Image energyFill;
        [SerializeField] Text energyLabel;

        [Header("时限")]
        [SerializeField] Text timerText;
        [SerializeField, Tooltip("存档中已有本关通关记录时显示")]
        Text clearedBadge;

        [Header("部署状态")]
        [SerializeField] Text deployStatusLabel;

        [Header("暂停")]
        [SerializeField] Button pauseButton;
        [SerializeField] GameObject pausePanel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button pauseRetryButton;

        [Header("结算")]
        [SerializeField] GameObject resultPanel;
        [SerializeField] Text resultTitle;
        [SerializeField] Text resultNote;
        [SerializeField] Button resultRetryButton;

        BattleFlow flow;

        /// <summary>由 BattleSetup 在战斗开始前调用。</summary>
        public void Bind(LevelDefinition level, BattleFlow battleFlow)
        {
            flow = battleFlow;

            flow.EnergyChanged += HandleEnergyChanged;
            flow.TimeChanged += HandleTimeChanged;
            flow.StateChanged += HandleStateChanged;

            pauseButton.onClick.AddListener(flow.TogglePause);
            resumeButton.onClick.AddListener(flow.TogglePause);
            pauseRetryButton.onClick.AddListener(flow.Retry);
            resultRetryButton.onClick.AddListener(flow.Retry);

            GameSaveData save = SaveService.Load();
            if (clearedBadge != null)
            {
                clearedBadge.gameObject.SetActive(save.IsLevelCompleted(level.ContentId));
            }

            HandleEnergyChanged(flow.Energy, flow.EnergyMax);
            HandleTimeChanged(flow.RemainingSeconds);
            HandleStateChanged(flow.State);
        }

        void OnDestroy()
        {
            if (flow == null)
            {
                return;
            }
            flow.EnergyChanged -= HandleEnergyChanged;
            flow.TimeChanged -= HandleTimeChanged;
            flow.StateChanged -= HandleStateChanged;
        }

        void HandleEnergyChanged(float current, float max)
        {
            if (energyFill != null && max > 0f)
            {
                energyFill.fillAmount = Mathf.Clamp01(current / max);
            }
            if (energyLabel != null)
            {
                energyLabel.text = $"能量 {Mathf.FloorToInt(current)}/{Mathf.FloorToInt(max)}";
            }
            RefreshDeploymentStatus();
        }

        void HandleTimeChanged(float remainingSeconds)
        {
            if (timerText == null)
            {
                return;
            }
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        void HandleStateChanged(BattleState state)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(state == BattleState.Paused);
            }
            if (resultPanel != null)
            {
                resultPanel.SetActive(state == BattleState.Won || state == BattleState.Lost);
                if (state == BattleState.Won)
                {
                    if (resultTitle != null) resultTitle.text = "胜利";
                    if (resultNote != null) resultNote.text = "敌方大本营已攻克，关卡进度已保存";
                }
                else if (state == BattleState.Lost)
                {
                    if (resultTitle != null) resultTitle.text = "失败";
                    if (resultNote != null) resultNote.text = "倒计时结束，未能攻克大本营";
                }
            }
            RefreshDeploymentStatus();
        }

        void RefreshDeploymentStatus()
        {
            if (flow == null)
            {
                return;
            }

            SoldierData selected = flow.CurrentDeployableSoldier;
            if (energyFill != null)
            {
                energyFill.color = selected != null
                    ? selected.TierColor
                    : new Color32(0x66, 0x66, 0x70, 0xFF);
            }
            if (deployStatusLabel == null)
            {
                return;
            }

            deployStatusLabel.text = selected == null
                ? "能量不足"
                : $"{selected.DisplayName}  消耗 {Mathf.RoundToInt(selected.EnergyCost)}";
        }

    }
}
