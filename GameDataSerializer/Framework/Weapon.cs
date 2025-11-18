#nullable enable
using System;
using Newtonsoft.Json;
using StardewValley.Tools;
using WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

namespace WikiInGameTools.GameDataSerializer.Framework;

/// <summary>
/// 序列化武器数据。
/// </summary>
[Serializable]
internal struct Weapon : IObject
{
    /// <inheritdoc/>
    [JsonIgnore]
    public string QualifiedItemID { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string DisplayName { get; }

    /// <summary>
    /// 武器的售出价格，若有。
    /// </summary>
    public int SellPrice { get; }

    /// <summary>
    /// 武器等级。
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// 武器类型。直接映射为常规的文字，例如"剑"。
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// 武器伤害范围。格式为"最小伤害-最大伤害"，例如"80-100"。
    /// </summary>
    public string Damage { get; }

    /// <summary>
    /// 武器暴击率。格式为 P1，例如 1.0%。
    /// </summary>
    public string CritChance { get; }

    /// <summary>
    /// 武器伤害倍率。格式为 F1，例如 3.0。
    /// </summary>
    public string CritMultiplier { get; }

    /// <inheritdoc cref="EquipStat"/>
    public EquipStat Statistics { get; }

    /// <summary>
    /// 直接读取原始字段。
    /// </summary>
    public Weapon(MeleeWeapon weapon)
    {
        var data = weapon.GetData();
        QualifiedItemID = weapon.QualifiedItemId;
        Name = data.Name;
        DisplayName = data.DisplayName;
        SellPrice = weapon.sellToStorePrice();
        Level = weapon.getItemLevel();
        Type = data.Type switch
        {
            1 => "匕首",
            2 => "锤",
            4 => "弹弓",
            _ => "剑"
        };
        Damage = $"{data.MinDamage}-{data.MaxDamage}";
        CritChance = data.CritChance.ToString("P1").Replace(" ", "");
        CritMultiplier = data.CritMultiplier.ToString("F1");
        Statistics = new EquipStat(data);
    }
}