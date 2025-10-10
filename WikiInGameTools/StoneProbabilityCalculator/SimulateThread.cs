using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StoneProbabilityCalculator;

public class SimulateThread : MineShaft
{
    public readonly Dictionary<string, int> Stats = new();

    public SimulateThread(double dailyLuck, int mineLevel, int luckLevel, int miningLevel, 
        int difficulty=0, bool desertFestival=false)
    {
        DailyLuck = dailyLuck;
        MineLevel = mineLevel;
        LuckLevel = luckLevel;
        MiningLevel = miningLevel;
        Difficulty = difficulty;
        DesertFestival = desertFestival;
    }

    public void Run(int x)
    {
        var percentage = 10;
        for (var i = 1; i <= 1000_0000 * x; i++)
        {
            var obj = GenerateRandomStone();
            Stats.IncrementValue(obj);
            if (i % (100_0000 * x) == 0)
            {
                Console.WriteLine($"{MineLevel},{DailyLuck},{Difficulty}: {percentage} %");
                percentage += 10;
            }
        }
    }

    private string GenerateRandomStone()
    {
        var stoneColor = Color.White;
        // 放射性矿
        if (Difficulty > 0 && 
            Random.NextDouble() < Difficulty * 0.001 + MineLevel / 100000f + DailyLuck / 13.0 + LuckLevel * 0.00015)
            return "95";

        int whichStone;
        // 矿井 1~40 层
        if (GetMineArea() == 0 || GetMineArea() == 10)
        {
            whichStone = Random.Next(31, 42);
            if (!IsDarkArea() && whichStone is >= 33 and < 38)
                whichStone = Random.Choose(32, 38);
            else if (IsDarkArea()) 
                whichStone = Random.Choose(34, 36);
            if (Difficulty > 0)
            {
                whichStone = Random.Next(33, 37);
                if (Random.NextDouble() < 0.33)
                    whichStone = 846;
                else
                    stoneColor = Color.SkyBlue;
                if (IsDarkArea())
                {
                    whichStone = Random.Next(32, 39);
                    stoneColor = Color.DarkGray;
                }

                if (MineLevel != 1 && Random.NextDouble() < 0.029)
                    return "849";
                if (stoneColor.Equals(Color.White))
                    return whichStone.ToString();
            }
            else if (MineLevel != 1 &&  Random.NextDouble() < 0.029)
            {
                return "751";
            }
        }
        // 矿井 41~80 层
        else if (GetMineArea() == 40)
        {
            whichStone = Random.Next(47, 54);
            if (Difficulty > 0 && !IsDarkArea())
            {
                whichStone = Random.Next(39, 42);
                stoneColor = new Color(170, 255, 160);
                if (IsDarkArea())
                {
                    whichStone = Random.Next(32, 39);
                    stoneColor = Color.DarkGray;
                }

                if (Random.NextDouble() < 0.15)
                    return (294 + Random.Choose(1, 0)).ToString();
                if (MineLevel != 1 && Random.NextDouble() < 0.029)
                    return "290";
                if (stoneColor.Equals(Color.White))
                    return whichStone.ToString();
            }
            else if (Random.NextDouble() < 0.029)
            {
                return "290";
            }
        }
        // 矿井 81~120 层
        else if (GetMineArea() == 80)
        {
            whichStone = Random.NextDouble() < 0.3 && !IsDarkArea() ? !Random.NextBool() ? 32 : 38 :
                Random.NextDouble() < 0.3 ? Random.Next(55, 58) : !Random.NextBool() ? 762 : 760;
            if (Difficulty > 0)
            {
                whichStone = !Random.NextBool() ? 32 : 38;
                stoneColor = Color.MediumPurple;
                if (IsDarkArea())
                {
                    whichStone = Random.Next(32, 39);
                    stoneColor = Color.DarkGray;
                }

                if (MineLevel != 1 && Random.NextDouble() < 0.029)
                    return "764";
                if (stoneColor.Equals(Color.White))
                    return whichStone.ToString();
            }
            else if (Random.NextDouble() < 0.029)
                return "764";
        }
        // 骷髅洞穴和采石场矿井
        else
        {
            // 采石场矿井，此处不考虑
            // if (GetMineArea() == 77377)
            // {
            //     var foundSomething = false;
            //     foreach (var v in Utility.getAdjacentTileLocations(tile))
            //         if (objects.ContainsKey(v))
            //         {
            //             foundSomething = true;
            //             break;
            //         }
            //
            //     if (!foundSomething && Random.NextDouble() < 0.45) return null;
            //     var brownSpot = false;
            //     for (var i = 0; i < brownSpots.Count; i++)
            //     {
            //         if (Vector2.Distance(tile, brownSpots[i]) < 4f)
            //         {
            //             brownSpot = true;
            //             break;
            //         }
            //
            //         if (Vector2.Distance(tile, brownSpots[i]) < 6f) return null;
            //     }
            //
            //     if (tile.X > 50f)
            //     {
            //         whichStone = _game1Random.Choose(668, 670);
            //         if (Random.NextDouble() < 0.09 + DailyLuck / 2.0)
            //             return _game1Random.Choose("BasicCoalNode0", "BasicCoalNode1");
            //         if (Random.NextDouble() < 0.25) return null;
            //     }
            //     else if (brownSpot)
            //     {
            //         whichStone = Random.Choose(32, 38);
            //         if (Random.NextDouble() < 0.01)
            //             return "751";
            //     }
            //     else
            //     {
            //         whichStone = Random.Choose(34, 36);
            //         if (Random.NextDouble() < 0.01)
            //             return "290";
            //     }
            //
            //     return whichStone.ToString();
            // }

            whichStone = Random.NextBool() ? !Random.NextBool() ? 32 : 38 : !Random.NextBool() ? 42 : 40;
            var skullCavernMineLevel = MineLevel - 120;
            var chanceForOre = 0.02 + skullCavernMineLevel * 0.0005;

            if (MineLevel >= 130)
                chanceForOre += 0.01 * ((Math.Min(100, skullCavernMineLevel) - 10) / 10f);

            var iridiumBoost = 0.0;
            if (MineLevel >= 130) iridiumBoost += 0.001 * ((skullCavernMineLevel - 10) / 10f);
            iridiumBoost = Math.Min(iridiumBoost, 0.004);

            if (skullCavernMineLevel > 100)
                iridiumBoost += skullCavernMineLevel / 1000000.0;

            if (Random.NextDouble() < chanceForOre)
            {
                var chanceForIridium = Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost);
                var chanceForGold = 0.01 + (MineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
                var chanceForIron = Math.Min(0.5, 0.1 + (MineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);

                if (DesertFestival && Random.NextBool(0.13 + (MineLevel - MiningLevel % 5) * 1.25 / 1000f))
                    return "CalicoEggStone";

                if (Random.NextDouble() < chanceForIridium)
                    return "765";

                if (Random.NextDouble() < chanceForGold)
                    return "764";

                if (Random.NextDouble() < chanceForIron)
                    return "290";

                return "751";
            }
        }

        var chanceModifier = DailyLuck + MiningLevel * 0.005;
        // 钻石矿
        // 0.00025 * (1 + 每日运气 + 采矿等级 * 0.005) + 矿井层数 / 120000
        if (MineLevel > 50 && Random.NextDouble() < DiamondChance + MineLevel / 120000d + 0.0005 * chanceModifier / 2.0)
            whichStone = 2;

        //  0.0015 * (1 + 每日运气 + 采矿等级 * 0.005) + 矿井层数 / 24000，直接返回
        else if (Random.NextDouble() < GemStoneChance + GemStoneChance * chanceModifier + MineLevel / 24000d)
            return GetRandomGemRichStoneForThisLevel(MineLevel);

        // 紫色宝石矿：
        // 0.001 (1 + 每日运气 + 采矿等级 * 0.016)
        if (Random.NextDouble() <
            PurpleStoneChance / 2.0 + PurpleStoneChance * MiningLevel * 0.008 + PurpleStoneChance * (DailyLuck / 2.0))
            whichStone = 44;

        // 神秘石：
        // 0.00005 (1 + 每日运气 / 2 + 采矿等级 * 0.008)
        if (MineLevel > 100 && Random.NextDouble() <
            MysticStoneChance + MysticStoneChance * MiningLevel * 0.008 + MysticStoneChance * (DailyLuck / 2.0))
            whichStone = 46;

        whichStone += whichStone % 2;
        if (Random.NextDouble() < 0.1 && GetMineArea() != 40)
            return Random.Choose("668", "670");

        return whichStone.ToString();
    }
}