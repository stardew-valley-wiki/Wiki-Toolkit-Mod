namespace WikiInGameTools.GameDataSerializer.Framework;

/// <summary>
/// 定义基础物品信息格式的接口。
/// </summary>
internal interface IObject
{
    /// <summary>
    /// 物品的唯一标识符，序列化时应当优先作为字典键使用。
    /// </summary>
    public string QualifiedItemID { get; }

    /// <summary>
    /// 物品的内部名称，英文。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 物品的显示名称，中文。应当在中文环境下运行 Mod 以获取正确的中文名称。
    /// </summary>
    public string DisplayName { get; }
}