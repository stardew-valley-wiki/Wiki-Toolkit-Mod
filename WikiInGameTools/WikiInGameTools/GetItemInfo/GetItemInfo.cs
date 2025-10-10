using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public bool IsActive { get; private set; }
    private List<ItemInfo> ItemInfos { get; set; }

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
        
        var id2Desc = new Dictionary<string, string>(); // Module:Description/data/id（仅限 Object，历史遗留兼容性处理）
        var displayName2Desc = new Dictionary<string, string>(); // Module:Description/data/zh（en 下此项不使用）
        var fullId2DisplayName = new Dictionary<string, string>(); // Module:ItemNames/data/[zh/en]
        var displayName2FullId = new Dictionary<string, string>(); // Module:ID/data/[zh/en]
        
        foreach (var item in all)
        {
            var internalName = item.Name; // 物品内部名称
            var displayName = ContainsChinese(item.DisplayName) 
                ? ApplyReplacements(item.DisplayName.Replace(" ", ""))
                : item.DisplayName; // 物品显示名称（可能包含中文翻译）
            var itemId = item.Id; // 简短ID
            if ((displayName == "木材" && itemId != "388") || (displayName == "石头" && itemId != "390"))
            {
                continue;
            }
            var itemType = item.Type; // 物品类型标识，如 "(O)" 表示 Object
            var itemFullId = item.QualifiedItemId; // 完整ID
            var itemPrototype = item.Item;
            var separators = new char[] { '\n', '\r' };
            var desc = GetDescription(itemPrototype)?.Split(separators);
            var itemDesc = (desc ?? Array.Empty<string>()).Where(j => !j.StartsWith("等级")).Aggregate("", (current, j) => current + j);
            
            if (!(displayName == "杂草" && itemId != "0") || !ContainsChinese(itemDesc))
            {
                if (itemType == "(O)")
                {
                    TryAddOrUpdateIfChinese(id2Desc, itemId, itemDesc);
                }
                TryAddOrUpdateIfChinese(displayName2Desc, displayName, itemDesc);
            }
            TryAddOrUpdateIfChinese(fullId2DisplayName, itemFullId, displayName);
            if (!displayName2FullId.TryAdd(displayName, itemFullId))
            {
                displayName2FullId[displayName] = $"{displayName2FullId[displayName]}\\{itemFullId}";
            }
        }

        if (lang == "zh")
        {
            WriteJsonFile(displayName2Desc, nameof(displayName2Desc));
            WriteJsonFile(id2Desc, nameof(id2Desc));
        }
        WriteJsonFile(fullId2DisplayName, nameof(fullId2DisplayName));
        WriteJsonFile(displayName2FullId, nameof(displayName2FullId));
        return;

        static void WriteJsonFile(Dictionary<string, string> dictionary, string name)
        {
            var lang = ModEntry.ModHelper.Translation.Locale.Contains("zh") ? "zh" : "en";
            ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", lang, name + ".json"), dictionary);
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
            return input.Any(c => (int)c >= 0x4e00 && (int)c <= 0x9fff);
        }
        
        static void TryAddOrUpdateIfChinese(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.TryAdd(key, value) && ContainsChinese(value))
            {
                dict[key] = value;
            }
        }
        
        // 如果限定 SVE 范围，则需要判断 FlashShifter.StardewValleyExpandedCP_
        // 更简洁的逻辑，但不适用于 WIKI
        /*
        var dictId2Desc = ItemInfos
            .Where(i => i.QualifiedItemID.StartsWith("(O)"))
            .DistinctBy(i => i.QualifiedItemID[3..])
            .ToDictionary(i => i.QualifiedItemID[3..], i => i.Description);
        
        var dictEn2Desc = ItemInfos
            .DistinctBy(i => i.Name)
            .ToDictionary(i => i.Name, i => i.Description);
        
        var dictZh2Id = ItemInfos
            .DistinctBy(i => i.DisplayName)
            .ToDictionary(i => i.DisplayName, i => i.QualifiedItemID);
        
        var dictZh2Desc = ItemInfos
            .DistinctBy(i => i.DisplayName)
            .ToDictionary(i => i.DisplayName, i => i.Description);
        
        var dictEn2Name = ItemInfos
            .DistinctBy(i => i.Name)
            .ToDictionary(i => i.Name, i => i.DisplayName);
        
        var dictId2Tags = ItemInfos
            .ToDictionary(i => i.QualifiedItemID, i => i.Tags.Where(t => !t.StartsWith("id_")));
        
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Desc.json"), dictId2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Desc.json"), dictEn2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2Id.json"), dictZh2Id);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2Desc.json"), dictZh2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Name.json"), dictEn2Name);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Tags.json"), dictId2Tags);
        */
    }
    
    public GetItemInfo()
    {
        ModEntry.ModHelper.ConsoleCommands.Add("Get_All_Item_Info",
            "输出所有物品相关数据。", SerializeAll);
    }
    
    // 先分别使用中文和英语导出一次（回到主标题切换语言）
    // 重名物品 & 特例物品
    
    // 针对性鱼饵 = (O)SpecificBait = Targeted Bait 【手动】
    // 熏鱼 = (O)Smoked = Smoked Fish【手动】
    // 果干 = (O)DriedFruit = Dried Fruit【手动】
    // 蘑菇干 = (O)DriedMushrooms = Dried Mushrooms【手动】
    
    // 垃圾（物品） = (O)168 = Trash (item)
    // 鱼饵（物品） = (O)685 = Bait (item)
    // 青蛙蛋 = (TR)FrogEgg = Frog Egg
    
    // 鸡雕像 = (O)113 = Chicken Statue
    // 鸡雕像（家具） = (BC)31 & (F)1305 = Chicken Statue (furniture)
    // 诡异玩偶（绿） = (O)126 = Strange Doll (green)
    // 诡异玩偶（黄） = (O)127 = Strange Doll (yellow)
    // 锚 = (O)117 = Anchor
    // 锚（家具） = (F)1675 = Anchor (furniture)
    // 蛋
    // 大鸡蛋
    // 远古斑点 = (O)590 = Artifact Spot
    // 绿色斑点 = (O)SeedSpot = SeedSpot
    // 腐烂的植物
    // 日记残页
    // 补给箱
    // ???
    // 绿雨杂草
    // 家居植物
    // 木椅
    // 石猫头鹰 = (BC)54 = Stone Owl
    // 石猫头鹰（随机事件） = (BC)95 = [Random Events#Stone_Owl]
    // 篝火 = (BC)146 = Campfire 【三个重名物品，只有 146 为正常物品】
    // 邪恶雕像
    // 树懒骨架（左、中、右）
    // 直立的晶洞、黑曜石花瓶、唱歌的石头
    // 珍奇乌鸦
    // 风干太阳花
    // 木桶 = (BC)163 = Cask
    // 树桩火炬（装饰） = (F)2398 = Stump Torch
    // 树桩火炬 = (BC)147 = Stump Brazier
    // 季节性植物
    // 餐椅（红） = (F)70 = Dining Chair (red)
    // 餐椅（黄） = (F)67 = Dining Chair (yellow)
    // 酒桌 = (F)1134 = Pub Table
    // 酒桌（长） = (F)WineTable = Wine Table
    // 小祝尼魔毛绒玩具（四种颜色）
    // 午夜沙滩床 = (F)MidnightBeachBed = Midnight Beach Bed
    // 午夜沙滩双人床 = (F)MidnightBeachDoubleBed = Midnight Beach Double Bed
    // 丛林贴纸、圆木镶板、天花板垂叶、云朵贴纸
    // 地板分隔条 = 地板分隔条（左） + 地板分隔条（右） = Floor Divider【此项需要合并】
    // 派对帽（蓝色） = (H)58 = Party Hat (blue)
    // 派对帽（绿色） = (H)59 = Party Hat (green)
    // 派对帽（红色） = (H)57 = Party Hat (red)
    // 所有淘盘、所有上衣、水手服、爱心T恤等（除v1.6外，旧版本 (S) 服饰物品没有独立页面，不列出）
    // 所有地板、所有壁纸
}