#nullable enable
using Newtonsoft.Json;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

internal readonly struct RecipeSource
{
    private const int MASTERY_VALUE = 77;

    public RecipeSourceType Type { get; init; }

    /// <summary>
    /// 相关的 NPC 名称。
    /// </summary>
    [JsonIgnore]
    public string? RelativeNPC { get; init; }

    /// <summary>
    /// 相关的技能名称。
    /// </summary>
    [JsonIgnore]
    public string? RelativeSkill { get; init; }

    /// <summary>
    /// 相关的特别任务名称。
    /// </summary>
    [JsonIgnore]
    public string? RelativeQuest { get; init; }

    /// <summary>
    /// 相关的商店名称。
    /// </summary>
    [JsonIgnore]
    public string? RelativeShop { get; init; }

    /// <summary>
    /// 相关的数字。对于技能相关，此项代表所需的技能等级；对于友谊和爱心事件相关，此项代表需要的友谊等级。其中 77 代表精通技能。
    /// </summary>
    [JsonIgnore]
    public int? Value { get; init; }

    /// <summary>
    /// 仅适用于酱料女皇。节目播出的日期。
    /// </summary>
    [JsonIgnore]
    public string? Date { get; init; }

    /// <summary>
    /// 补充描述，适用于其他较复杂的情况。
    /// </summary>
    [JsonIgnore]
    public string? Desc { get; init; }

    /// <summary>
    /// 仅适用于商店出售的配方。价格信息。
    /// </summary>
    [JsonIgnore]
    public PriceData? Price { get; init; }

    public override string ToString()
    {
        return Type switch
        {
            RecipeSourceType.FriendShip
                => $"{{{{NPC|{RelativeNPC}|邮件 - {Value} [[File:HeartIconLarge.png|16px|link=]]{Desc}}}}}",
            RecipeSourceType.SkillLevel
                => $"{{{{Skill level|{RelativeSkill}|{(Value == MASTERY_VALUE ? "m" : Value.ToString())}}}}}",
            RecipeSourceType.Event
                => $"{{{{NPC|{RelativeNPC}|{Value} [[File:HeartIconLarge.png|16px|link=]] 事件}}}}",
            RecipeSourceType.Quest => $"特别任务：“{RelativeQuest}”",
            RecipeSourceType.CookingChannel => Date ?? "未知日期",
            RecipeSourceType.BuyInShop => $"{RelativeShop}以{Price} 购买",
            RecipeSourceType.Other => Desc ?? "",
            RecipeSourceType.Default => "初始拥有",
            _ => "解析错误！"
        };
    }
}

internal enum RecipeSourceType
{
    /// <summary>好感度</summary>
    FriendShip,

    /// <summary>技能等级</summary>
    SkillLevel,

    /// <summary>事件（需要自己硬编码）</summary>
    Event,

    /// <summary>任务</summary>
    Quest,

    /// <summary>酱料女皇</summary>
    CookingChannel,

    /// <summary>商店购买</summary>
    BuyInShop,

    /// <summary>其它来源</summary>
    Other,

    /// <summary>初始拥有</summary>
    Default
}