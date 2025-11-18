#nullable enable
using System;
using StardewValley.GameData.Weapons;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 装备的属性。使用游戏内逻辑获取。
/// </summary>
[Serializable]
internal struct EquipStat
{
    /// <summary>
    /// 攻击速度。
    /// </summary>
    public string? Speed { get; } = null;

    /// <summary>
    /// 防御值。
    /// </summary>
    public string? Defense { get; } = null;

    /// <summary>
    /// 暴击率。
    /// </summary>
    public string? CritChance { get; } = null;

    /// <summary>
    /// 暴击力量。
    /// </summary>
    public string? CritPower { get; } = null;

    /// <summary>
    /// 重量。
    /// </summary>
    public string? Weight { get; } = null;

    /// <summary>
    /// 包含反编译后的相关代码。与游戏内获取基础信息的方法一致。
    /// </summary>
    public EquipStat(WeaponData data)
    {
        if (data.Speed != (data.Type == 2 ? -8 : 0))
            Speed = ((data.Type == 2 ? data.Speed + 8 : data.Speed) > 0 ? "+" : "") + 
                    (data.Type == 2 ? data.Speed + 8 : data.Speed) / 2;

        if (data.Defense > 0)
            Defense = data.Defense.ToString();

        var effectiveCritChance = data.CritChance;
        if (data.Type == 1)
        {
            effectiveCritChance += 0.005f;
            effectiveCritChance *= 1.12f;
        }

        if (effectiveCritChance / 0.02 >= 1.1)
            CritChance = ((int)Math.Round((effectiveCritChance - 0.001f) / 0.02)).ToString();

        if ((data.CritMultiplier - 3f) / 0.02 >= 1.0)
            CritPower = ((int)((data.CritMultiplier - 3f) / 0.02)).ToString();

        // Resharper disable once CompareOfFloatsByEqualityOperator
        if (data.Knockback != DefaultKnockBack(data.Type))
            Weight = GetKnockbackDisplayText(data);
    }

    private static string GetKnockbackDisplayText(WeaponData data)
    {
        var knockbackDiff = Math.Abs(data.Knockback - DefaultKnockBack(data.Type));
        var displayValue = (int)Math.Ceiling(knockbackDiff * 10f);
        var defaultKnockback = DefaultKnockBack(data.Type);
        var sign = displayValue > defaultKnockback ? "+" : "";

        return sign + displayValue;
    }

    public static float DefaultKnockBack(int type) => type switch 
    {
        1 => 0.5f,
        0 or 3 => 1f,
        2 => 1.5f,
        _ => -1f
    };
}