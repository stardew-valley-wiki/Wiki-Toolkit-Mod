#nullable enable
using System;
using Newtonsoft.Json;
using StardewValley;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 存储物品基本信息的数据结构。
/// </summary>
[Serializable]
internal struct Item : IObject
{
    /// <inheritdoc/>
    public string QualifiedItemID { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string DisplayName { get; }

    /// <summary>
    /// 物品的数量。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Amount { get; }

    public Item(string itemID, int? amount = 1, string? displayName = null)
    {
        var qualifiedItemID = ItemRegistry.QualifyItemId(itemID) ?? itemID;
        var item = ItemRegistry.Create(qualifiedItemID);

        QualifiedItemID = qualifiedItemID;
        DisplayName = displayName ?? item.DisplayName;
        Amount = amount;
        Name = item.Name == "Error Item"
            ? qualifiedItemID
            : item.Name;
    }
}