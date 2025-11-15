using System;
using StardewValley.Tools;

namespace WikiInGameTools.getItemInfo.Framework;

[Serializable]
public struct WeaponInfo
{
    [NonSerialized]
    public string QualifiedItemID;

    public string Name;
    public string DisplayName;
    public int SellPrice;
    public int Level;
    public string Type;
    public string Damage;
    public string CritChance;
    public string CritMultiplier;
    public Stats Statistics;

    public WeaponInfo(MeleeWeapon weapon)
    {
        QualifiedItemID = weapon.QualifiedItemId;
        Name = weapon.Name;
        DisplayName = weapon.DisplayName;
        SellPrice = weapon.sellToStorePrice();
        Level = weapon.getItemLevel();
        Type = weapon.type.Value switch
        {
            1 => "匕首",
            2 => "锤",
            _ => "剑"
        };
        Damage = $"{weapon.minDamage}-{weapon.maxDamage}";
        CritChance = weapon.critChance.Value.ToString("F");
        CritMultiplier = weapon.critMultiplier.Value.ToString("F1");
        Statistics = new Stats(weapon);
    }

    #nullable enable
    [Serializable]
    public class Stats
    {
        public string? Speed;
        public string? Defense;
        public string? CritChance;
        public string? CritPower;
        public string? Weight;

        public Stats(MeleeWeapon weapon)
        {
            if (weapon.speed.Value != (weapon.type.Value == 2 ? -8 : 0))
                Speed = ((weapon.type.Value == 2 ? weapon.speed.Value + 8 : weapon.speed.Value) > 0 ? "+" : "") + 
                        (weapon.type.Value == 2 ? weapon.speed.Value + 8 : weapon.speed.Value) / 2;

            if (weapon.addedDefense.Value > 0)
                Defense = weapon.addedDefense.Value.ToString();

            var effectiveCritChance = weapon.critChance.Value;
            if (weapon.type.Value == 1)
            {
                effectiveCritChance += 0.005f;
                effectiveCritChance *= 1.12f;
            }

            if (effectiveCritChance / 0.02 >= 1.1)
                CritChance = ((int)Math.Round((effectiveCritChance - 0.001f) / 0.02)).ToString();

            if ((weapon.critMultiplier.Value - 3f) / 0.02 >= 1.0)
                CritPower = ((int)((weapon.critMultiplier.Value - 3f) / 0.02)).ToString();

            // Resharper disable once CompareOfFloatsByEqualityOperator
            if (weapon.knockback.Value != weapon.defaultKnockBackForThisType(weapon.type.Value))
                Weight = GetKnockbackDisplayText(weapon);
        }

        private static string GetKnockbackDisplayText(MeleeWeapon weapon)
        {
            var knockbackDiff = Math.Abs(weapon.knockback.Value - weapon.defaultKnockBackForThisType(weapon.type.Value));
            var displayValue = (int)Math.Ceiling(knockbackDiff * 10f);
            var defaultKnockback = weapon.defaultKnockBackForThisType(weapon.type.Value);
            var sign = displayValue > defaultKnockback ? "+" : "";

            return sign + displayValue;
        }
    }
}