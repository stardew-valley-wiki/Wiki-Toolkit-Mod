using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CJBItemSpawner.Framework.ItemData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Tools;
using WikiInGameTools._Framework;
using WikiInGameTools._Framework.ConfigurationService;
using WikiInGameTools.getItemInfo.Framework;

namespace WikiInGameTools.GetItemInfo;

public class GetItemInfo : IModule
{
    private static readonly Dictionary<string, (string zh, string en)> ItemNameMappings = new()
    {
        { "(O)168", ("垃圾（物品）", "Trash (item)") },
        { "(O)685", ("鱼饵（物品）", "Bait (item)") },
        { "(TR)FrogEgg", ("青蛙蛋", "Frog Egg") },
        { "(O)113", ("鸡雕像", "Chicken Statue") },
        { "(BC)31", ("鸡雕像（家具）", "Chicken Statue (furniture)") },
        { "(F)1305", ("鸡雕像（家具）", "Chicken Statue (furniture)") },
        { "(O)126", ("诡异玩偶（绿）", "Strange Doll (green)") },
        { "(O)127", ("诡异玩偶（黄）", "Strange Doll (yellow)") },
        { "(O)117", ("锚", "Anchor") },
        { "(F)1675", ("锚（家具）", "Anchor (furniture)") },
        { "(O)590", ("远古斑点", "Artifact Spot") },
        { "(O)SeedSpot", ("绿色斑点", "SeedSpot") },
        { "(BC)54", ("石猫头鹰", "Stone Owl") },
        { "(BC)95", ("石猫头鹰（随机事件）", "Stone Owl (random event)") }, // Random Events#Stone Owl
        { "(BC)163", ("木桶", "Cask") },
        { "(F)2398", ("树桩火炬（装饰）", "Stump Torch") },
        { "(BC)147", ("树桩火炬", "Stump Brazier") },
        { "(F)70", ("餐椅（红）", "Dining Chair (red)") },
        { "(F)67", ("餐椅（黄）", "Dining Chair (yellow)") },
        { "(F)1134", ("酒桌", "Pub Table") },
        { "(F)WineTable", ("酒桶（长）", "Wine Table") },
        { "(F)MidnightBeachBed", ("午夜沙滩床", "Midnight Beach Bed") },
        { "(F)MidnightBeachDoubleBed", ("午夜沙滩双人床", "Midnight Beach Double Bed") },
        { "(H)58", ("派对帽（蓝色）", "Party Hat (blue)") },
        { "(H)59", ("派对帽（绿色）", "Party Hat (green)") },
        { "(H)57", ("派对帽（红色）", "Party Hat (red)") },
        { "(F)J", ("J（标志）", "J (sign)") },
        { "(O)124", ("黄金面具", "Golden Mask") },
        { "(H)67", ("金色面具", "Golden Mask (hat)") }
    };

    public GetItemInfo()
    {
        ModEntry.ModHelper.ConsoleCommands.Add("Get_All_Item_Info",
            "输出所有物品相关数据。", SerializeAll);
    }

    private List<ItemInfo> ItemInfos { get; set; }
    public bool IsActive { get; private set; }

    public IConfig Config => ModEntry.Config.GetItemInfoModConfig;

    public void Activate()
    {
        IsActive = true;
        ItemInfos = ItemRegistry.ItemTypes
            .SelectMany(r => r.GetAllData().Select(r.CreateItem))
            .Select(i => new ItemInfo(i))
            .ToList();
    }

    public void Deactivate()
    {
        IsActive = false;
        ItemInfos = null;
    }

    private void SerializeAll(string command, string[] args)
    {
        if (!IsActive)
        {
            ModEntry.Log("模块未被启用！", LogLevel.Error);
            return;
        }

        var lang = ModEntry.ModHelper.Translation.Locale.Contains("zh") ? "zh" : "en";

        var itemRepository = new ItemRepository();
        var all = itemRepository.GetAll();

        // 注：需要更改当前显示的语言来导出数据。

        var floorDividers = new List<string> { "Floor Divider R", "Floor Divider L", "地板分隔条（右）", "地板分隔条（左）" };
        var mannequins = new List<string> { "Floor Divider R", "Floor Divider L", "地板分隔条（右）", "地板分隔条（左）" };
        var cursedMannequins = new List<string> { "Floor Divider R", "Floor Divider L", "地板分隔条（右）", "地板分隔条（左）" };
        var id2Desc = new Dictionary<string, string>(); // Module:Description/data/id（仅限 Object，历史遗留兼容性处理）
        var displayName2Desc = new Dictionary<string, string>(); // Module:Description/data/zh（en 下此项不使用）
        var fullId2DisplayName = new Dictionary<string, string>(); // Module:ItemNames/data/[zh/en]
        var displayName2FullId = new Dictionary<string, string>(); // Module:ID/data/[zh/en]


        var dictId2Desc = ItemInfos.ToDictionary(i => i.QualifiedItemID, i => i.Description);

        foreach (var item in all)
        {
            var internalName = item.Name; // 物品内部名称
            var displayName = ContainsChinese(item.DisplayName)
                ? ApplyReplacements(item.DisplayName.Replace(" ", ""))
                : item.DisplayName; // 物品显示名称（可能包含中文翻译）
            if (lang == "en" && displayName.Contains(": ")) displayName = displayName.Replace(": ", " ");
            var itemId = item.Id; // 简短ID
            var itemFullId = item.QualifiedItemId; // 完整ID
            switch (displayName)
            {
                case "木材":
                case "Wood":
                    if (itemId != "388") continue;
                    break;
                case "石头":
                case "Stone":
                    if (itemId != "390") continue;
                    break;
                case "篝火":
                case "Campfire":
                    if (itemFullId != "(BC)146") continue;
                    break;
            }

            if (ItemNameMappings.TryGetValue(itemFullId, out var names))
                displayName = lang == "zh" ? names.zh : names.en;
            if (floorDividers.Contains(displayName)) displayName = lang == "zh" ? "地板分隔条" : "Floor Divider";
            if (mannequins.Contains(displayName)) displayName = lang == "zh" ? "假人模特" : "Mannequin";
            if (cursedMannequins.Contains(displayName)) displayName = lang == "zh" ? "被诅咒的假人模特" : "Cursed Mannequin";

            var itemType = item.Type; // 物品类型标识，如 "(O)" 表示 Object
            var itemPrototype = item.Item;
            var separators = new[] { '\n', '\r' };
            var desc = GetDescription(itemPrototype)?.Split(separators);

            var itemDesc = (desc ?? Array.Empty<string>()).Where(j => !j.StartsWith("等级"))
                .Aggregate("", (current, j) => current + j);
            if (itemFullId.Contains("(TR)"))
                itemDesc = dictId2Desc[itemFullId].Replace("{0}", "X").Replace("有{1}概率", "有 Y 概率").Replace("{1}", "Y")
                    .Replace("{2}", "Z");

            if (!IsInvalidString(itemDesc))
                if (!(displayName == "杂草" && itemId != "0") || !ContainsChinese(itemDesc))
                {
                    if (itemType == "(O)") TryAddOrUpdateIfChinese(id2Desc, itemId, itemDesc);
                    TryAddOrUpdateIfChinese(displayName2Desc, displayName, itemDesc);
                }


            TryAddOrUpdateIfChinese(fullId2DisplayName, itemFullId, displayName);
            if (!displayName2FullId.TryAdd(displayName, itemFullId))
                displayName2FullId[displayName] = $"{displayName2FullId[displayName]}\\{itemFullId}";
        }

        if (lang == "zh")
        {
            WriteJsonFile(displayName2Desc, nameof(displayName2Desc));
            WriteJsonFile(id2Desc, nameof(id2Desc));
        }

        WriteJsonFile(fullId2DisplayName, nameof(fullId2DisplayName));
        var fullId2DisplayName2 = displayName2FullId.ToDictionary(pair => pair.Value, pair => pair.Key);
        WriteJsonFile(fullId2DisplayName2, nameof(fullId2DisplayName2));
        WriteJsonFile(displayName2FullId, nameof(displayName2FullId));
        if (lang == "en")
        {
            var displayName2FullId2 = displayName2FullId.ToDictionary(pair => pair.Key.ToLower(), pair => pair.Value);
            WriteJsonFile(displayName2FullId2, nameof(displayName2FullId2));
        }

        return;

        static void WriteJsonFile(Dictionary<string, string> dictionary, string name)
        {
            var lang = ModEntry.ModHelper.Translation.Locale.Contains("zh") ? "zh" : "en";
            ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", lang, name + ".json"), dictionary);
        }

        static bool IsInvalidString(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (Regex.IsMatch(input, @"[\u4e00-\u9fa5]")) return false;

            var englishCharCount = 0;
            var nonPunctuationCharCount = 0;

            foreach (var c in input)
            {
                if (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z') englishCharCount++;

                if (!char.IsPunctuation(c) && !char.IsSymbol(c) && !char.IsSeparator(c)) nonPunctuationCharCount++;
            }

            if (englishCharCount == 0 || nonPunctuationCharCount == 0) return false;

            var percentage = (double)englishCharCount / nonPunctuationCharCount;

            return percentage >= 0.7;
        }

        string ApplyReplacements(string text)
        {
            var replacements = new (string Old, string New)[]
            {
                ("《风筝大师'95》", "《风筝大师 '95》"),
                ("《高速公路89》", "《高速公路 89》"),
                ("风之道第一部分", "风之道 第一部分"),
                ("风之道第二部分", "风之道 第二部分"),
                ("矮人卷轴I", "矮人卷轴 I")
            };

            return replacements.Aggregate(text, (current, r) => current.Replace(r.Old, r.New));
        }

        static string GetDescription(Item item)
        {
            try
            {
                _ = item.DisplayName; // force display name to load, which is needed to get the description outside the inventory for some reason
                return item is MeleeWeapon weapon && !weapon.isScythe()
                    ? weapon.Description
                    : item.getDescription();
            }
            catch (KeyNotFoundException)
            {
                return null; // e.g. incubator
            }
        }

        static bool ContainsChinese(string input)
        {
            return input.Any(c => c >= 0x4e00 && c <= 0x9fff);
        }

        static void TryAddOrUpdateIfChinese(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.TryAdd(key, value) && ContainsChinese(value)) dict[key] = value;
        }

        // 如果限定 SVE 范围，则需要判断 FlashShifter.StardewValleyExpandedCP_
        // 更简洁的逻辑，但不适用于 WIKI
        /*
        var dictId2Desc = ItemInfos
            .Where(i => i.QualifiedItemID.StartsWith("(O)"))
            .DistinctBy(i => i.QualifiedItemID[3..])
            .ToDictionary(i => i.QualifiedItemID[3..], i => i.Description);

        var dictId2Zh = ItemInfos
            .Where(i => i.QualifiedItemID.StartsWith("(O)"))
            .DistinctBy(i => i.QualifiedItemID[3..])
            .ToDictionary(i => i.QualifiedItemID[3..], i => i.DisplayName);

        var dictId2En = ItemInfos
            .Where(i => i.QualifiedItemID.StartsWith("(O)"))
            .DistinctBy(i => i.QualifiedItemID[3..])
            .ToDictionary(i => i.QualifiedItemID[3..], i => i.Name);

        var dictEn2Desc = ItemInfos
            .DistinctBy(i => i.Name)
            .ToDictionary(i => i.Name, i => i.Description);

        var dictEn2ID = ItemInfos
            .DistinctBy(i => i.Name)
            .ToDictionary(i => i.Name, i => i.QualifiedItemID);

        var dictEn2Zh = ItemInfos
            .DistinctBy(i => i.Name)
            .ToDictionary(i => i.Name, i => i.DisplayName);

        var dictZh2Desc = ItemInfos
            .DistinctBy(i => i.DisplayName)
            .ToDictionary(i => i.DisplayName, i => i.Description);

        var dictZh2ID = ItemInfos
            .DistinctBy(i => i.DisplayName)
            .ToDictionary(i => i.DisplayName, i => i.QualifiedItemID);

        var dictZh2En = ItemInfos
            .DistinctBy(i => i.DisplayName)
            .ToDictionary(i => i.DisplayName, i => i.Name);

        var dictId2Tags = ItemInfos
            .ToDictionary(i => i.QualifiedItemID, i => i.Tags.Where(t => !t.StartsWith("id_")));

        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Desc.json"), dictId2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Zh.json"), dictId2Zh);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2En.json"), dictId2En);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Desc.json"), dictEn2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2ID.json"), dictEn2ID);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Zh.json"), dictEn2Zh);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2Desc.json"), dictZh2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2ID.json"), dictZh2ID);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2En.json"), dictZh2En);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Tags.json"), dictId2Tags);
        */
    }

    // 先分别使用中文和英语导出一次（回到主标题切换语言）
    // 重名物品 & 特例物品

    // 针对性鱼饵 = (O)SpecificBait = Targeted Bait 【手动】
    // 熏鱼 = (O)Smoked = Smoked Fish【手动】
    // 果干 = (O)DriedFruit = Dried Fruit【手动】
    // 蘑菇干 = (O)DriedMushrooms = Dried Mushrooms【手动】

    // 季节性植物
    // 蛋
    // 大鸡蛋
    // 腐烂的植物
    // 日记残页
    // 补给箱
    // ???
    // 绿雨杂草
    // 家居植物
    // 木椅
    // 邪恶雕像
    // 树懒骨架（左、中、右）
    // 直立的晶洞、黑曜石花瓶、唱歌的石头
    // 珍奇乌鸦
    // 风干太阳花
    // 丛林贴纸、圆木镶板、天花板垂叶、云朵贴纸
    // 小祝尼魔毛绒玩具（四种颜色）
    // 所有淘盘、所有上衣、水手服、爱心T恤等（除v1.6外，旧版本 (S) 服饰物品没有独立页面，不列出）
    // 所有地板、所有壁纸
}