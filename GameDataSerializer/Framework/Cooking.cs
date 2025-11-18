#nullable enable
using System;
using System.Linq;
using Newtonsoft.Json;
using WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;
using Object = StardewValley.Object;

namespace WikiInGameTools.GameDataSerializer.Framework;

/// <summary>
/// 序列化菜品数据。
/// </summary>
[Serializable]
internal struct Cooking : IObject
{
    /// <inheritdoc/>
    [JsonIgnore]
    public string QualifiedItemID { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string DisplayName { get; }

    /// <summary>
    /// 菜品的售出价格，若有。
    /// </summary>
    public int SellPrice { get; }

    /// <summary>
    /// 菜品的可食用性。
    /// </summary>
    public int Edibility { get; }

    /// <summary>
    /// 菜品的增益列表。
    /// </summary>
    public Buff[]? Buffs { get; }

    /// <summary>
    /// 菜品的配方信息。
    /// </summary>
    public Recipe? RecipeData { get; } = null;

    public Cooking(Object obj)
    {
        QualifiedItemID = obj.QualifiedItemId;
        Name = obj.Name;
        DisplayName = obj.DisplayName;
        SellPrice = obj.sellToStorePrice();
        Edibility = obj.Edibility;

        var buffs = obj.GetFoodOrDrinkBuffs()
            .Select(b => new Buff(b))
            .ToArray();
        Buffs = buffs.Any() ? buffs : null;

        if (Recipes.TryGetValue(obj.QualifiedItemId, out var recipe))
            RecipeData = recipe;
    }
}