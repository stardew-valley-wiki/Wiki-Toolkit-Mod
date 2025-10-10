using System;
using static System.Math;

namespace StoneProbabilityCalculator;

public class Calculator : MineShaft
{
    public Calculator(double dailyLuck, int mineLevel, int luckLevel, int miningLevel, 
        int difficulty=0, bool desertFestival=false)
    {
        DailyLuck = dailyLuck;
        MineLevel = mineLevel;
        LuckLevel = luckLevel;
        MiningLevel = miningLevel;
        Difficulty = difficulty;
        DesertFestival = desertFestival;
    }

    public void SkullCavern()
    {
        var level = MineLevel - 120;
        var raChance = Difficulty > 0 
            ? Min(1.0, Difficulty * 0.001 + MineLevel / 100000d + DailyLuck / 13.0 + LuckLevel * 0.00015)
            : 0;

        var oreChance = Min(1.0, 0.02 + level * 0.0005 + Max(0.0, 0.001 * (Min(level, 100) - 10))) * (1 - raChance);
        var otherChance = (1 - oreChance) * (1 - raChance);

        var irBoost = Min(0.004, Max(0.0, 0.0001 * (level - 10))) + (level <= 100 ? 0d : level / 1_000_000d);

        var irChance = Min(1.0, Min(100, level) * (0.0003 + irBoost));
        var auChance = Min(1.0, 0.01 + (MineLevel - Min(150, level)) * 0.0005);

        var irProportion = oreChance * irChance;
        var auProportion = oreChance * (1 - irChance) * auChance;
        var feProportion = oreChance * (1 - irChance) * (1 - auChance) * 0.5;

        var diamondChance = DiamondChance * (1 + DailyLuck + MiningLevel * 0.005) + MineLevel / 120000d;
        var gemNodeChance = GemStoneChance * (1 + DailyLuck + MiningLevel * 0.005) + MineLevel / 24000d;
        var purpleStoneChance = PurpleStoneChance * (1 + DailyLuck + MiningLevel * 0.016) / 2;
        var mysticStoneChance = MysticStoneChance * (1 + DailyLuck / 2 + MiningLevel * 0.008);

        var gemProportion = otherChance * (1 - diamondChance) * gemNodeChance;
        var diamondProportion = otherChance * diamondChance * (1 - purpleStoneChance) * (1 - mysticStoneChance);
        var specialChance = otherChance * (1 - (1 - diamondChance) * gemNodeChance);
        var mysticProportion = specialChance * mysticStoneChance;
        var purpleProportion = specialChance * (1 - mysticStoneChance) * purpleStoneChance;

        if (raChance > 0) Console.WriteLine($"Radioactive Chance {MineLevel - 120}: {raChance}");
        Console.WriteLine($"Iridium Chance {MineLevel - 120}: {irProportion:P3}");
        Console.WriteLine($"Gold Chance {MineLevel - 120}: {auProportion:P3}");
        Console.WriteLine($"Iron Chance {MineLevel - 120}: {feProportion:P3}");
        Console.WriteLine($"Copper Chance {MineLevel - 120}: {feProportion:P3}");
        Console.WriteLine($"Diamond Chance {MineLevel - 120}: {diamondProportion:P3}");
        Console.WriteLine($"Gem Node Chance {MineLevel - 120}: {gemProportion:P3}");
        Console.WriteLine($"Purple Stone Chance {MineLevel - 120}: {purpleProportion:P3}, {purpleStoneChance:P3}");
        Console.WriteLine($"Mistic Stone Chance {MineLevel - 120}: {mysticProportion:P5}, {mysticStoneChance:P5}");
    }

    private void EggStone(ref int oreChance)
    {
        var eggScore = MineLevel / 5;
        var eggChanceMin = Min(1.0, DesertFestival ? 0.13 + eggScore * 5 / 1000f : 0);
        var eggChanceMax = Min(1.0, DesertFestival ? 0.13 + (eggScore * 5 + MineLevel * 2) / 1000f : 0);

        var eggProportionMin = oreChance * eggChanceMin;
        var eggProportionMax = oreChance * eggChanceMax;

        if (eggProportionMin > 0) Console.WriteLine($"Egg Chance: {eggProportionMin:P} ~ {eggProportionMax:P}");
    }
}