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
        
        var itemRepository = new ItemRepository();
        var all = itemRepository.GetAll();
        
        var enName2ZhName = new Dictionary<string, string>(); // Module:Name/data/en
        var enName2Desc = new Dictionary<string, string>(); // Module:Description/data/en
        var zhName2Desc = new Dictionary<string, string>(); // Module:Description/data/zh
        var id2Desc = new Dictionary<string, string>(); // Module:Description/data/id
        var fullId2ZhName = new Dictionary<string, string>(); // Module:ItemNames/data/zh
        var fullId2EnName = new Dictionary<string, string>(); // Module:ItemNames/data/en
        var zhName2Id = new Dictionary<string, string>(); // Module:ID/data/zh
        var enName2Id = new Dictionary<string, string>(); // Module:ID/data/en
        
        foreach (var item in all)
        {
            var enName = item.Name; // 物品英文名称
            var zhName = ContainsChinese(item.DisplayName) 
                ? ApplyReplacements(item.DisplayName.Replace(" ", ""))
                : item.DisplayName; // 物品显示名称（可能包含中文翻译）
            var itemId = item.Id; // 简短ID
            if ((zhName == "木材" && itemId != "388") || (zhName == "石头" && itemId != "390"))
            {
                continue;
            }
            var itemType = item.Type; // 物品类型标识，如 "(O)" 表示 Object
            var itemFullId = item.QualifiedItemId; // 完整ID
            var itemPrototype = item.Item;
            var separators = new char[] { '\n', '\r' };
            var desc = GetDescription(itemPrototype)?.Split(separators);
            var itemDesc = (desc ?? Array.Empty<string>()).Where(j => !j.StartsWith("等级")).Aggregate("", (current, j) => current + j);
            if (itemType == "(O)")
            {
                TryAddOrUpdateIfChinese(id2Desc, itemId, itemDesc);
            }
            if (!(zhName == "杂草" && itemId != "0"))
            {
                TryAddOrUpdateIfChinese(enName2Desc, enName, itemDesc);
                TryAddOrUpdateIfChinese(zhName2Desc, zhName, itemDesc);
            }
            TryAddOrUpdateIfChinese(enName2ZhName, enName, zhName);
            TryAddOrUpdateIfChinese(fullId2ZhName, itemFullId, zhName);
            fullId2EnName.TryAdd(itemFullId, enName);
            if (!zhName2Id.TryAdd(zhName, itemFullId))
            {
                zhName2Id[zhName] = $"{zhName2Id[zhName]}<br />{itemFullId}";
            }
            if (!enName2Id.TryAdd(enName, itemFullId))
            {
                enName2Id[enName] = $"{enName2Id[enName]}<br />{itemFullId}";
            }
        }
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "enName2ZhName.json"), enName2ZhName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "enName2Desc.json"), enName2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "zhName2Desc.json"), zhName2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "id2Desc.json"), id2Desc);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "fullId2ZhName.json"), fullId2ZhName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "fullId2EnName.json"), fullId2EnName);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "zhName2Id.json"), zhName2Id);
        ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "enName2Id.json"), enName2Id);

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
        
        // 更简洁的逻辑，但目前不适用于 WIKI
        
        // var dictId2Desc = ItemInfos
        //     .Where(i => i.QualifiedItemID.StartsWith("(O)"))
        //     .DistinctBy(i => i.QualifiedItemID[3..])
        //     .ToDictionary(i => i.QualifiedItemID[3..], i => i.Description);
        //
        // var dictEn2Desc = ItemInfos
        //     .DistinctBy(i => i.Name)
        //     .ToDictionary(i => i.Name, i => i.Description);
        //
        // var dictZh2Id = ItemInfos
        //     .DistinctBy(i => i.DisplayName)
        //     .ToDictionary(i => i.DisplayName, i => i.QualifiedItemID);
        //
        // var dictZh2Desc = ItemInfos
        //     .DistinctBy(i => i.DisplayName)
        //     .ToDictionary(i => i.DisplayName, i => i.Description);
        //
        // var dictEn2Name = ItemInfos
        //     .DistinctBy(i => i.Name)
        //     .ToDictionary(i => i.Name, i => i.DisplayName);
        //
        // var dictId2Tags = ItemInfos
        //     .ToDictionary(i => i.QualifiedItemID, i => i.Tags.Where(t => !t.StartsWith("id_")));
        //
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Desc.json"), dictId2Desc);
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Desc.json"), dictEn2Desc);
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2Id.json"), dictZh2Id);
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictZh2Desc.json"), dictZh2Desc);
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictEn2Name.json"), dictEn2Name);
        // ModEntry.ModHelper.Data.WriteJsonFile(Path.Combine("output", "dictId2Tags.json"), dictId2Tags);
    }
    


    public GetItemInfo()
    {
        ModEntry.ModHelper.ConsoleCommands.Add("Get_All_Item_Info",
            "输出所有物品相关数据。", SerializeAll);
    }
    
    
}