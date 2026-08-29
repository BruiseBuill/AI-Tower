using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 兼容旧资源名的防御塔数据类型。新代码依赖 TowerData；旧的 TowerDefinition 资产无需迁移即可继续使用。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Tower Definition", fileName = "TowerDefinition")]
    [System.Obsolete("TowerDefinition 仅为旧资源兼容类型，请新建 TowerData 资产。")]
    public class TowerDefinition : TowerData { }
}
