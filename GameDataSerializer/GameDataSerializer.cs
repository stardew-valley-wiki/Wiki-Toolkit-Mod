using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Tools;
using WikiInGameTools._Framework;
using WikiInGameTools._Framework.ConfigurationService;
using WikiInGameTools.GameDataSerializer.Framework;
using WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;
using Item = StardewValley.Item;
using Object = StardewValley.Object;

namespace WikiInGameTools.GameDataSerializer;

internal class GameDataSerializer : IModule
{
    public GameDataSerializer()
    {
        _allItems = ItemRegistry.ItemTypes
            .SelectMany(r => r.GetAllData().Select(r.CreateItem));

        ModEntry.ModHelper.ConsoleCommands.Add("Get_All_Game_Data",
            "输出所有物品相关数据。", SerializeAll);
    }

    #region 数据列表

    private readonly IEnumerable<Item> _allItems;
    private List<Cooking> CookingInfos { get; set; }
    private List<Seed> SeedInfos { get; set; }
    private List<Weapon> WeaponInfos { get; set; }

    #endregion

    public bool IsActive { get; private set; }
    public IConfig Config => ModEntry.Config.GameDataSerializerModConfig;
    private readonly Harmony _harmony = new (ModEntry.Manifest.UniqueID + ".GameDataParser");

    public void Activate()
    {
        IsActive = true;
        RegisterHarmonyPatch();
        try
        {
            CookingInfos = _allItems
                .OfType<Object>()
                .Where(i => i.Category == Object.CookingCategory)
                .Select(i => new Cooking(i))
                .ToList();

            SeedInfos = _allItems
                .OfType<Object>()
                .Where(i => i.Category == Object.SeedsCategory)
                .Select(i => new Seed(i))
                .ToList();

            WeaponInfos = _allItems
                .OfType<MeleeWeapon>()
                .Select(i => new Weapon(i))
                .ToList();
        }
        catch (Exception e)
        {
            ModEntry.Log("获取数据失败，已自动关闭模块。\n错误信息：", LogLevel.Error);
            ModEntry.Log(e.ToString(), LogLevel.Error);
            Deactivate();
        }
    }

    public void Deactivate()
    {
        _harmony.UnpatchAll(_harmony.Id);
        IsActive = false;
        CookingInfos = null;
        SeedInfos = null;
        WeaponInfos = null;
    }

    /// <summary>
    /// 禁用价格修饰，防止来自技能的售价加成影响结果。
    /// </summary>
    public static void Patch_getPriceAfterMultipliers(float startPrice, ref float __result) => __result = startPrice;

    private void RegisterHarmonyPatch()
    {
        var original = AccessTools.Method(typeof(Object), "getPriceAfterMultipliers");
        var postfix = AccessTools.Method(
            typeof(GameDataSerializer), nameof(Patch_getPriceAfterMultipliers));
        _harmony.Patch(original: original, postfix: new HarmonyMethod(postfix));
        ModEntry.Log("Patched Object.getPriceAfterMultipliers successfully.");
    }

    private void SerializeAll(string command, string[] args)
    {
        if (!IsActive)
        {
            ModEntry.Log("模块未被启用！", LogLevel.Error);
            return;
        }

        var dictCooking = CookingInfos
            .ToDictionary(i => i.QualifiedItemID, i => i);

        var dictSeed = SeedInfos
            .ToDictionary(i => i.QualifiedItemID, i => i);

        var dictWeapon = WeaponInfos
            .ToDictionary(i => i.QualifiedItemID, i => i);

        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "data", "Cooking.json"), dictCooking);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "data", "Seed.json"), dictSeed);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "data", "Weapon.json"), dictWeapon);

        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "data", "Recipe.json"), Recipes.AllRecipes);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "data", "Shop.json"), Shops.AllShops);
    }
}