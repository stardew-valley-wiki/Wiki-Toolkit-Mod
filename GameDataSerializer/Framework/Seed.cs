#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.TerrainFeatures;
using WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;
using Item = WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs.Item;
using Object = StardewValley.Object;

namespace WikiInGameTools.GameDataSerializer.Framework;

[Serializable]
internal struct Seed : IObject
{
    /// <inheritdoc/>
    [JsonIgnore]
    public string QualifiedItemID { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string DisplayName { get; }

    public int SellPrice { get; }

    public int Growth { get; } = -1;
    public int Xp { get; } = 0;
    public Item? HarvestItem { get; }
    public Season[] Seasons { get; }
    public Dictionary<string, List<PriceData>>? Price { get; } = null;

    /// <summary>
    /// 种子的配方信息，若有。
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Recipe? RecipeData { get; } = null;

    public Seed(Object obj)
    {
        QualifiedItemID = obj.QualifiedItemId;
        Name = obj.Name;
        DisplayName = obj.DisplayName;
        SellPrice = obj.sellToStorePrice();

        if (Crop.TryGetData(obj.ItemId, out var cropData))
        {
            var crop = ItemRegistry.Create<Object>(cropData.HarvestItemId);
            HarvestItem = new Item(crop.ItemId, null);
            Xp = GetXp(crop.Price);
            Growth = cropData.DaysInPhase.Sum();
            Seasons = cropData.Seasons.ToArray();
        }
        else if (FruitTree.TryGetData(obj.ItemId, out var fruitData))
        {
            var fruit = ItemRegistry.Create<Object>(fruitData.Fruit.First().ItemId);
            HarvestItem = new Item(fruit.ItemId, null);
            Growth = 28;
            Seasons = fruitData.Seasons.ToArray();
        }
        else
        {
            HarvestItem = null;
            Seasons = Array.Empty<Season>();
        }

        if (Recipes.TryGetValue(obj.QualifiedItemId, out var recipe))
            RecipeData = recipe;

        Price = Shops.GetAllPriceDataOfThisItem(QualifiedItemID);
    }

    public static int GetXp(int price) => (int)Math.Round((float)(16.0 * Math.Log(0.018 * price + 1.0, Math.E)));

}