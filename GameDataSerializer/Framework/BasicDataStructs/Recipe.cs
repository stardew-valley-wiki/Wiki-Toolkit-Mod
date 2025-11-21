#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

    /// <summary>
    /// 打造产出的物品。
    /// </summary>
    [JsonIgnore]
    public string ProduceItemID { get; }

    /// <summary>
    /// 解析后的配方来源信息
    /// </summary>
    public Dictionary<RecipeSourceType, string>? RecipeSource { get; }

    /// <summary>
    /// 构造函数，创建配方实例。
    /// </summary>
    /// <param name="name">配方在 <c>XXRecipes.json</c> 中的键名。</param>
    /// <param name="type">配方类型。</param>
    public Recipe(string name, RecipeType type)
    {
        // 使用游戏内方法获取 CraftingRecipe 实例
        var isCookingRecipe = type == RecipeType.Cooking;
        var recipe = new CraftingRecipe(name, isCookingRecipe);

        // 检查配方是否存在
        var available = isCookingRecipe
            ? CraftingRecipe.cookingRecipes.TryGetValue(name, out var raw)
            : CraftingRecipe.craftingRecipes.TryGetValue(name, out raw);
        if (!available || raw is null)
        {
            ModEntry.Log($"配方{name}看起来不是合法的配方。");
            Ingredients = Array.Empty<Item>();
            Produce = -1;
            ProduceItemID = "错误配方";
            RecipeSource = null;
            return;
        }

        // 设置基础值
        Ingredients = recipe.recipeList
            .Select(kvp => new Item(kvp.Key, kvp.Value, recipe.getNameFromIndex(kvp.Key)))
            .ToArray();
        Produce = recipe.numberProducedPerCraft;
        ProduceItemID = recipe.ProduceItemId();

        // 解析配方来源信息
        var recipeSource = new List<RecipeSource>();
        var otherInfo = new List<string>();
        if (ArgUtility.TryGet(raw.Split('/'), isCookingRecipe ? 3 : 4, out var rawOtherInfo, out _, false))
            otherInfo.AddRange(rawOtherInfo.Trim().Split(" "));

        // 配方其他信息长度为 1 且值为 default，解析为初始拥有的配方。
        if (otherInfo.Count == 1 && otherInfo[0] == "default")
            recipeSource.Add(RecipeFromInitial());

        // 配方其他信息长度为 3，尝试直接解析。
        else if (otherInfo.Count == 3)
        {
            switch (otherInfo[0])
            {
                case "f":
                    recipeSource.Add(RecipeFromFriendShip(otherInfo[1], int.Parse(otherInfo[2])));
                    break;
                case "s":
                    recipeSource.Add(RecipeFromSkill(otherInfo[1], int.Parse(otherInfo[2])));
                    break;
            }
        }

        // 其它情况，四处搜罗解析。
        var qualifiedItemID = ProduceItemID;

        // 先尝试在商店中寻找
        var priceData = Shops.AllShops
            .Select(kvp => new 
            {
                Shop = kvp.Value.ShopId,
                Good = kvp.Value.Goods.FirstOrDefault(g => g.QualifiedItemID == qualifiedItemID && g.Recipe is true)
            })
            .Where(x => x.Good is not null)
            .Select(x => new { x.Shop, x.Good!.PriceData} )
            .ToList();
        if (priceData.Count == 1)
            recipeSource.Add(RecipeFromShop(priceData[0].Shop, priceData[0].PriceData));

        // 对于菜品，尝试在电视节目中寻找
        if (isCookingRecipe)
        {
            if (Recipes.AllTvRecipes.TryGetValue(name, out var whichWeek))
            {
                var week = int.Parse(whichWeek) - 1;
                var year = ((week >> 4) & 1) switch
                {
                    0 => "奇数年",
                    1 => "偶数年",
                    _ => "错误年"
                };
                var season = ((week >> 2) & 3) switch
                {
                    0 => "春季",
                    1 => "夏季",
                    2 => "秋季",
                    3 => "冬季",
                    _ => "错误季节"
                };
                var day = (week & 3) switch
                {
                    0 => " 7 日",
                    1 => " 14 日",
                    2 => " 21 日",
                    3 => " 28 日",
                    _ => "错误日期"
                };
                var date = string.Concat(year, season, day);
                recipeSource.Add(RecipeFromCookingChannel(date));
            }
        }
        // 对于打造物，尝试解析特别任务
        else if (Quests.AllSpecialOrdersWithRecipe.TryGetValue(name, out var questName))
            recipeSource.Add(RecipeFromQuest(questName));

        // 加入硬编码的来源
        switch (name)
        {
            case "Drum Block": // 鼓块
            case "Flute Block": // 长笛块
                recipeSource.Add(RecipeFromHeartEvent("Robin", 6));
                break;
            case "Mini-Jukebox": // 迷你点唱机
                recipeSource.Add(RecipeFromHeartEvent("Gus", 5));
                break;
            case "Wild Bait": // 万能鱼饵
                recipeSource.Add(RecipeFromHeartEvent("Linus", 4));
                break;
            case "Cookies": // 饼干
                recipeSource.Add(RecipeFromHeartEvent("Evelyn", 4));
                break;
            case "Tea Sapling": // 茶苗
                recipeSource.Add(RecipeFromFriendShip("Caroline", 2, " 事件后的次日"));
                break;
            case "Fairy Dust": // 仙尘
                recipeSource.Add(RecipeFromQuest("海盗的妻子"));
                break;
            case "Furnace": // 熔炉
                recipeSource.Add(RecipeFromOtherSource("获取一颗铜矿石后由克林特提供"));
                break;
            case "Garden Pot": // 花盆
                recipeSource.Add(RecipeFromOtherSource("修复温室后由艾芙琳提供"));
                break;
            case "Cask": // 木桶
                recipeSource.Add(RecipeFromOtherSource("第二次农舍升级"));
                break;
            case "Ancient Seeds": // 古代种子
                recipeSource.Add(RecipeFromOtherSource("向博物馆"));
                break;
            case "Deluxe Scarecrow": // 豪华稻草人
                recipeSource.Add(RecipeFromOtherSource("集齐所有稀有稻草人后通过邮件获得"));
                break;
            case "Ostrich Incubator": // 鸵鸟孵化器
                recipeSource.Add(RecipeFromOtherSource("完成岛屿办事处的所有调查和捐赠任务"));
                break;
            case "Statue Of Blessings": // 祝福雕像
                recipeSource.Add(RecipeFromSkill("Farming", 77));
                break;
            case "Heavy Furnace": // 重型熔炉
            case "Statue Of The Dwarf King": // 矮人之王雕像
                recipeSource.Add(RecipeFromSkill("Mining", 77));
                break;
            case "Challenge Bait": // 挑战鱼饵
                recipeSource.Add(RecipeFromSkill("Fishing", 77));
                break;
            case "Treasure Totem": // 宝藏图腾
            case "Mystic Tree Seed": // 神秘树种子
                recipeSource.Add(RecipeFromSkill("Foraging", 77));
                break;
            case "Anvil": // 铁砧
            case "Mini-Forge": // 迷你锻造台
                recipeSource.Add(RecipeFromSkill("Combat", 77));
                break;
        }

        // 若没有任何来源，将其置空，然后打印警告。
        RecipeSource = recipeSource.Any() 
            ? recipeSource.ToDictionary(r => r.Type, r => r.ToString())
            : null;
    }

    /// <summary>
    /// 处理来自好感度信件的配方。
    /// </summary>
    /// <param name="npc">相关 NPC</param>
    /// <param name="value">需要的好感度等级</param>
    /// <param name="desc">额外说明</param>
    private static RecipeSource RecipeFromFriendShip(string npc, int value, string desc="")
        => new() { Type = RecipeSourceType.FriendShip, RelativeNPC = npc, Value = value, Desc = desc};

    /// <summary>
    /// 处理来自技能等级的配方。
    /// </summary>
    /// <param name="skill">相关的技能名称</param>
    /// <param name="value">需要的技能等级，77 代表精通</param>
    private static RecipeSource RecipeFromSkill(string skill, int value)
        => new() { Type = RecipeSourceType.SkillLevel, RelativeSkill = skill, Value = value };

    /// <summary>
    /// 处理来自爱心事件的配方。
    /// </summary>
    /// <param name="npc">相关 NPC</param>
    /// <param name="value">需要的好感度等级</param>
    private static RecipeSource RecipeFromHeartEvent(string npc, int value)
        => new() { Type = RecipeSourceType.Event, RelativeNPC = npc, Value = value };

    /// <summary>
    /// 处理来自特别任务的配方。
    /// </summary>
    /// <param name="quest">相关任务</param>
    private static RecipeSource RecipeFromQuest(string quest)
        => new() { Type = RecipeSourceType.Quest, RelativeQuest = quest };

    /// <summary>
    /// 处理来自酱料女皇的配方。
    /// </summary>
    /// <param name="date">播出的日期</param>
    private static RecipeSource RecipeFromCookingChannel(string date)
        => new() { Type = RecipeSourceType.CookingChannel, Date = date };

    /// <summary>
    /// 处理来自商店购买的配方。
    /// </summary>
    /// <param name="shop">在何商店购买</param>
    /// <param name="priceData">以何价格购买</param>
    private static RecipeSource RecipeFromShop(string shop, PriceData priceData)
        => new() { Type = RecipeSourceType.BuyInShop, RelativeShop = shop, Price = priceData };

    /// <summary>
    /// 处理其他来源的配方。
    /// </summary>
    /// <param name="description">配方说明</param>
    private static RecipeSource RecipeFromOtherSource(string description)
        => new() { Type = RecipeSourceType.Other, Desc = description };

    /// <summary>
    /// 处理初始拥有的配方。
    /// </summary>
    private static RecipeSource RecipeFromInitial()
        => new() { Type = RecipeSourceType.Default };
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

    /// <summary>
    /// 所有酱料女皇的配方。
    /// </summary>
    public static readonly Dictionary<string, string> AllTvRecipes;

    static Recipes()
    {
        AllTvRecipes = DataLoader.Tv_CookingChannel(Game1.content)
            .ToDictionary(kvp => kvp.Value.Split("/")[0], kvp => kvp.Key);

        var cookingRecipes = CraftingRecipe.cookingRecipes
            .Select(kvp => new CraftingRecipe(kvp.Key))
            .Select(r => new Recipe(r.name, RecipeType.Cooking))
            .ToDictionary(r => r.ProduceItemID, r => r );

        var craftingRecipes = CraftingRecipe.craftingRecipes
            .Select(kvp => new CraftingRecipe(kvp.Key))
            .Select(r => new Recipe(r.name, RecipeType.Crafting))
            .ToDictionary(r => r.ProduceItemID, r => r );

        AllRecipes = cookingRecipes
            .Concat(craftingRecipes)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 获取产出物品的 QualifiedItemId。
    /// </summary>
    public static string ProduceItemId(this CraftingRecipe recipe)
    {
        var itemID = recipe.itemToProduce.First();
        return recipe.bigCraftable 
            ? ItemRegistry.ManuallyQualifyItemId(itemID, "(BC)") 
            : ItemRegistry.QualifyItemId(itemID);
    }

    public static bool TryGetValue(string qualifyItemId, out Recipe recipe) => 
        AllRecipes.TryGetValue(qualifyItemId, out recipe);
}

internal enum RecipeType
{
    Cooking,
    Crafting
}