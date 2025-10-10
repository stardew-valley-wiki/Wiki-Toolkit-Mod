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
    
    // 特例物品
    // ["诡异玩偶（绿）"] = "126"
    // ["诡异玩偶（黄）"] = "127"
    // ["Joja可乐"] = "167"
    // ["垃圾（物品）"] = "168"
    // ["破损的CD"] = "171"
    // ["鱼饵（物品）"] = "685"
    // ["针对性鱼饵"] = "SpecificBait"
    // ["熏鱼"] = "Smoked"
    // ["果干"] = "DriedFruit"
    // ["蘑菇干"] = "DriedMushrooms"
    // ["青蛙蛋"] = "FrogEgg"
    // 重名物品
    // 蛋、鸡雕像、诡异玩偶

    private void SerializeAll(string command, string[] args)
    {
        if (!IsActive)
        {
            ModEntry.Log("模块未被启用！", LogLevel.Error);
            return;
        }
        
        var itemRepository = new ItemRepository();
        var all = itemRepository.GetAll();
        
        // 注：需要更改当前显示的语言来导出数据。
        
        var internalName2ZhName = new Dictionary<string, string>(); // Module:Name/data/en
        var internalName2Desc = new Dictionary<string, string>(); // Module:Description/data/en
        var displayName2Desc = new Dictionary<string, string>(); // Module:Description/data/zh
        var id2Desc = new Dictionary<string, string>(); // Module:Description/data/id
        var fullId2DisplayName = new Dictionary<string, string>(); // Module:ItemNames/data/zh
        var fullId2InternalName = new Dictionary<string, string>(); // Module:ItemNames/data/en
        var displayName2Id = new Dictionary<string, string>(); // Module:ID/data/zh
        var internalName2Id = new Dictionary<string, string>(); // Module:ID/data/en
        
        foreach (var item in all)
        {
            var internalName = item.Name; // 物品英文名称
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
                TryAddOrUpdateIfChinese(internalName2Desc, internalName, itemDesc);
                TryAddOrUpdateIfChinese(displayName2Desc, displayName, itemDesc);
            }
            TryAddOrUpdateIfChinese(internalName2ZhName, internalName, displayName);
            TryAddOrUpdateIfChinese(fullId2DisplayName, itemFullId, displayName);
            fullId2InternalName.TryAdd(itemFullId, internalName);
            if (!displayName2Id.TryAdd(displayName, itemFullId))
            {
                displayName2Id[displayName] = $"{displayName2Id[displayName]}<br />{itemFullId}";
            }
            if (!internalName2Id.TryAdd(internalName, itemFullId))
            {
                internalName2Id[internalName] = $"{internalName2Id[internalName]}<br />{itemFullId}";
            }
        }
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "internalName2ZhName.json"), internalName2ZhName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "internalName2Desc.json"), internalName2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "displayName2Desc.json"), displayName2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "id2Desc.json"), id2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "fullId2DisplayName.json"), fullId2DisplayName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "fullId2InternalName.json"), fullId2InternalName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "displayName2Id.json"), displayName2Id);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "internalName2Id.json"), internalName2Id);

        return;

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
    
    // 先分别使用中文和英语导出一次（回到主标题切换语言；放到不同文件夹下，例如 zh 和 en）
    // 二者的 internalName2Desc、internalName2Id、internalName2ZhName、fullId2InternalName 均不使用
    // en 下的 displayName2Desc、id2Desc 不使用
    
}