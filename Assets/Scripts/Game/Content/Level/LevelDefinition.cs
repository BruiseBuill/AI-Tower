using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 关卡定义：时限、能量规则、可部署士兵和敌方布阵。
    /// 这是关卡编辑器的持久化边界，也是运行时装配战场的唯一数据入口。
    /// 世界坐标以战斗相机位于原点、正交尺寸 5 为参考（16:9 下水平约 ±8.9）。
    /// 未来扩展（多路径、部署区、地形机制等）在此资产上新增字段，避免场景脚本写死数值。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Level Definition", fileName = "LevelDefinition")]
    public class LevelDefinition : ContentDefinition
    {
        [Serializable]
        public class TowerPlacement
        {
            public TowerData definition;
            public Vector3 position;
        }

        [Header("时限")]
        [SerializeField, Min(1f), Tooltip("倒计时秒数。归零前未摧毁大本营则失败")]
        float timeLimitSeconds = 99f;

        [Header("关卡入口")]
        [SerializeField, Tooltip("该关卡对应的运行时场景路径。关卡编辑器和构建工具使用此字段")]
        string scenePath = "Assets/Scenes/Level001.unity";

        [Header("能量")]
        [SerializeField, Min(0f)] float energyStart = 100f;
        [SerializeField, Min(1f)] float energyMax = 200f;
        [SerializeField, Min(0f), Tooltip("每秒恢复的能量")]
        float energyRegenPerSecond = 8f;

        [Header("玩家部署")]
        [SerializeField, Tooltip("点击部署时选择当前能量可负担的最高阶兵种；新增兵种只需追加资产")]
        List<SoldierData> deployableSoldiers = new List<SoldierData>();

        [Header("敌方布阵")]
        [SerializeField] BaseDefinition baseDefinition;
        [SerializeField] Vector3 basePosition;
        [SerializeField, Tooltip("防御塔列表，position 为世界坐标；列表顺序即编辑器中的布阵顺序")]
        List<TowerPlacement> towers = new List<TowerPlacement>();

        public float TimeLimitSeconds => timeLimitSeconds;
        public string ScenePath => scenePath;
        public float EnergyStart => energyStart;
        public float EnergyMax => energyMax;
        public float EnergyRegenPerSecond => energyRegenPerSecond;
        public IReadOnlyList<SoldierData> DeployableSoldiers => deployableSoldiers;
        public BaseDefinition BaseDefinition => baseDefinition;
        public Vector3 BasePosition => basePosition;
        public IReadOnlyList<TowerPlacement> Towers => towers;
    }
}
