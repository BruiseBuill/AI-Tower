using BF;
using Game.Core;
using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// 战场输入适配器：复用 BF.InputManager 的鼠标/触屏点击语义，
    /// 将屏幕坐标转换为世界坐标后交给战斗总控。
    /// </summary>
    public sealed class BattleInputManager : InputManager
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }

        void OnEnable()
        {
            InputManager.onClick += HandleScreenClick;
        }

        void OnDisable()
        {
            InputManager.onClick -= HandleScreenClick;
        }

        void HandleScreenClick(Vector3 screenPosition)
        {
            if (!BattleFlow.IsActive || Camera.main == null)
            {
                return;
            }

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            BattleFlow.Current.TryDeployAt(worldPosition);
        }
    }
}
