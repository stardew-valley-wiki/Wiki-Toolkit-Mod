#nullable enable
using System;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 存储与物品的购买价格相关的数据。
/// </summary>
[Serializable]
internal struct PriceData
{
    /// <summary>
    /// 物品的购买价格或以物易物所需的物品数量。
    /// </summary>
    public int Price { get; private set; }

    /// <summary>
    /// 购买需要用到的货币。
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// 以物易物所用的物品，若是以物易物。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Item? TradeInItem { get; }

    /// <summary>
    /// 物品的库存。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Stock { get; }

    /// <summary>
    /// 每次购买获得的数量。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Stack { get; }

    public PriceData(ItemQueryResult result, string shopID, ShopData shop, ShopItemData data, Currency currency)
    {
        Currency = data.TradeItemId switch
        {
            null => currency,
            "(O)858" => Currency.Gem,
            _ => Currency.TradeIn
        };
        TradeInItem = data.TradeItemId switch
        {
            null => null,
            "(O)858" => null,
            _ => new Item(data.TradeItemId, data.TradeItemAmount)
        };
        Stock = data.AvailableStock == -1 ? null : data.AvailableStock;
        Stack = data.MinStack == -1 ? null : data.MinStack;

        if (result.Item is not StardewValley.Item item)
        {
            Price = -1;
            return;
        }

        var price = data.TradeItemId == "(O)858" 
            ? data.TradeItemAmount
            : ShopBuilder.GetBasePrice(result, shop, data, item, false, data.UseObjectDataPrice);

        Price = shopID == "Traveler" || data.IgnoreShopPriceModifiers
            ? price
            : (int)Utility.ApplyQuantityModifiers(price, shop.PriceModifiers, shop.PriceModifierMode);
    }

    public override string ToString()
    {
        var currency = Currency switch
        {
            Currency.StarToken => "Token",
            Currency.QiCoin => "Qi",
            Currency.TradeIn => TradeInItem?.Name ?? "TradeIn",
            Currency.Gem => "Gem",
            _ => ""
        };
        var price = Currency == Currency.TradeIn ? TradeInItem?.Amount ?? 0 : Price;
        return $"{{{{Price|{price}|{currency}}}}}";
    }
}

public enum Currency
{
    /// <summary>金币</summary>
    Money,

    /// <summary>星星币</summary>
    StarToken,

    /// <summary>齐币</summary>
    QiCoin,

    /// <summary>以物易物</summary>
    TradeIn,

    /// <summary>齐钻</summary>
    Gem
}