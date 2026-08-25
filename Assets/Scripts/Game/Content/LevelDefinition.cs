using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 关卡定义：时限、能量规则、可部署士兵、敌方布阵与进军路线。
    /// 世界坐标以战斗相机位于原点、正交尺寸 5 为参考（16:9 下水平约 ±8.9）。
    /// 未来扩展（多路径、部署区、地形机制等）在此资产上新增字段，避免场景脚本写死数值。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Level Definition", fileName = "LevelDefinition")]
    public class LevelDefinition : ContentDefinition
    {
        [Serializable]
        public class TowerPlacement
        {
            public TowerDefinition definition;
            public Vector3 position;
        }

        [Header("时限")]
        [SerializeField, Min(1f), Tooltip("倒计时秒数。归零前未摧毁大本营则失败")]
        float timeLimitSeconds = 99f;

        [Header("能量")]
        [SerializeField, Min(0f)] float energyStart = 100f;
        [SerializeField, Min(1f)] float energyMax = 200f;
        [SerializeField, Min(0f), Tooltip("每秒恢复的能量")]
        float energyRegenPerSecond = 8f;

        [Header("玩家部署")]
        [SerializeField, Tooltip("部署按钮顺序 = 列表顺序。新增兵种只需在这里追加资产")]
        List<SoldierDefinition> deployableSoldiers = new List<SoldierDefinition>();

        [Header("敌方布阵")]
        [SerializeField] BaseDefinition baseDefinition;
        [SerializeField] Vector3 basePosition;
        [SerializeField, Tooltip("防御塔列表，position 为世界坐标")]
        List<TowerPlacement> towers = new List<TowerPlacement>();

        [Header("进军路线")]
        [SerializeField, Tooltip("士兵行进路径点。[0] 为出生点，最后一点须位于大本营攻击距离内")]
        List<Vector3> soldierPath = new List<Vector3>();

        public float TimeLimitSeconds => timeLimitSeconds;
        public float EnergyStart => energyStart;
        public float EnergyMax => energyMax;
        public float EnergyRegenPerSecond => energyRegenPerSecond;
        public IReadOnlyList<SoldierDefinition> DeployableSoldiers => deployableSoldiers;
        public BaseDefinition BaseDefinition => baseDefinition;
        public Vector3 BasePosition => basePosition;
        public IReadOnlyList<TowerPlacement> Towers => towers;
        public IReadOnlyList<Vector3> SoldierPath => soldierPath;
    }
}
