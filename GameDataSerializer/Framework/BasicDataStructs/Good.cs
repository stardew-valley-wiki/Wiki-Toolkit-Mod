#nullable enable
using System;
using Newtonsoft.Json;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 存储作为商品出售的物品的基本信息的数据结构。
/// </summary>
[Serializable]
internal class Good : IObject
{
    /// <inheritdoc/>
    public string QualifiedItemID { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string DisplayName { get; }

    /// <inheritdoc cref="PriceData"/>
    public PriceData PriceData { get; }

    /// <summary>
    /// 物品是否是一个配方。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? Recipe { get; }

    /// <summary>
    /// 商店解锁该物品的额外条件。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? ExtraCondition { get; }

    public Good(ItemQueryResult result, string shopID, ShopData shop, ShopItemData data, Currency currency)
    {
        QualifiedItemID = result.Item?.QualifiedItemId ?? "Error Item";
        Name = result.Item?.Name ?? "Error Item";
        DisplayName = result.Item?.DisplayName ?? "Error Item";
        PriceData = new PriceData(result, shopID, shop, data, currency);
        Recipe = result.Item?.IsRecipe == true ? true : null;
        ExtraCondition = data.Condition;
    }
}