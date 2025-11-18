using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 存储配方信息的数据结构。
/// </summary>
[Serializable]
internal struct Recipe
{
    /// <summary>
    /// 制作所需的原材料。
    /// </summary>
    public Item[] Ingredients { get; }

    /// <summary>
    /// 每次打造产出的数量。
    /// </summary>
    public int Produce { get; }

    public Recipe(string name)
    {
        var recipe = new CraftingRecipe(name, true);
        Ingredients = recipe.recipeList
            .Select(kvp => new Item(kvp.Key, kvp.Value, recipe.getNameFromIndex(kvp.Key)))
            .ToArray();
        Produce = recipe.numberProducedPerCraft;
    }
}

/// <summary>
/// 存储游戏内全部配方的静态辅助类。
/// </summary>
internal static class Recipes
{
    /// <summary>
    /// 所有可制作的配方列表。
    /// </summary>
    public static readonly Dictionary<string, Recipe> AllRecipes;

    static Recipes()
    {
        var cookingRecipes = CraftingRecipe.cookingRecipes
            .Select(kvp => new CraftingRecipe(kvp.Key, true))
            .ToDictionary(r => r.ProduceItemId(), r => new Recipe(r.name));

        var craftingRecipes = CraftingRecipe.craftingRecipes
            .Select(kvp => new CraftingRecipe(kvp.Key, false))
            .ToDictionary(r => r.ProduceItemId(), r => new Recipe(r.name));

        AllRecipes = cookingRecipes
            .Concat(craftingRecipes)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 获取产出物品的 QualifiedItemId。
    /// </summary>
    private static string ProduceItemId(this CraftingRecipe recipe)
    {
        var itemID = recipe.itemToProduce.First();
        return recipe.bigCraftable 
            ? ItemRegistry.ManuallyQualifyItemId(itemID, "(BC)") 
            : ItemRegistry.QualifyItemId(itemID);
    }

    public static bool TryGetValue(string qualifyItemId, out Recipe recipe) => 
        AllRecipes.TryGetValue(qualifyItemId, out recipe);
}