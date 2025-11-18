using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

internal struct Shop
{
    public string ShopId { get; }
    public Good[] Goods { get; }

    public Shop(string shopId, ShopData shop)
    {
        ShopId = shopId;
        var r = new Random();
        var itemQueryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, r, "shop '" + shopId + "'");
        var items = shop.Items;
        var currency = shop.Currency switch
        {
            1 => Currency.StarToken,
            2 => Currency.QiCoin,
            4 => Currency.Gem,
            _ => Currency.Money
        };

        if (items.Any())
        {
            items.ForEach(d => d.MaxItems = null);
            Goods = items
                .Select(shopItemData => ItemQueryResolver.TryResolve(shopItemData, itemQueryContext)
                    .Select(g => new Good(g, shopId, shop, shopItemData, currency)))
                .SelectMany(x => x)
                .ToArray();
        }
        else
        {
            Goods = Array.Empty<Good>();
        }
    }
}

internal static class Shops
{
    public static readonly Dictionary<string, Shop> AllShops;

    static Shops()
    {
        var shops = DataLoader.Shops(Game1.content);
        AllShops = shops
            .ToDictionary(kvp => kvp.Key, kvp => new Shop(kvp.Key, kvp.Value));
    }

    public static Dictionary<string, List<PriceData>> GetAllPriceDataOfThisItem(string qualifiedItemID) => AllShops
        .Where(kvp => kvp.Value.Goods.Any(g => g.QualifiedItemID == qualifiedItemID))
        .Select(kvp => kvp.Value)
        .ToDictionary(
            s => s.ShopId,
            s => s.Goods
                .Where(g => g.QualifiedItemID == qualifiedItemID)
                .Select(g => g.PriceData)
                .ToList()
        );
}