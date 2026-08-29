using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 兼容旧资源名的士兵数据类型。新代码依赖 SoldierData；旧的 SoldierDefinition 资产无需迁移即可继续使用。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Soldier Definition", fileName = "SoldierDefinition")]
    [System.Obsolete("SoldierDefinition 仅为旧资源兼容类型，请新建 SoldierData 资产。")]
    public class SoldierDefinition : SoldierData { }
}
