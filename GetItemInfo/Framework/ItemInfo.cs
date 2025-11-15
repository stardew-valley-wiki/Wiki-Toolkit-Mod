using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace WikiInGameTools.getItemInfo.Framework;

[Serializable]
public struct ItemInfo
{
    public string QualifiedItemID;
    public string Name;
    public string DisplayName;
    public string Description;
    public List<string> Tags = new();

    public ItemInfo(Item item)
    {
        QualifiedItemID = item.QualifiedItemId;
        Name = item.Name;
        DisplayName = item.DisplayName;
        Description = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId).Description;
        Tags = item.GetContextTags().ToList();
    }
}