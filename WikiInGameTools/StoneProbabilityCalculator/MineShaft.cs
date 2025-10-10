using System;

namespace StoneProbabilityCalculator;

public class MineShaft
{
    protected const double DiamondChance = 0.00025;
    protected const double GemStoneChance = 0.0015;
    protected const double MysticStoneChance = 0.00005;
    protected const double PurpleStoneChance = 0.001;
    protected readonly Random Random = new();

    public double DailyLuck;
    public int Difficulty;
    public int MineLevel;

    protected bool DesertFestival;
    protected int LuckLevel;
    protected int MiningLevel;

    protected int GetMineArea() =>
        MineLevel switch
        {
            > 10 and < 30 => 10,
            >= 40 and < 80 => 40,
            >= 80 and <= 120 => 80,
            77377 => 77377,
            > 120 => 121,
            _ => 0
        };

    protected bool IsDarkArea() => MineLevel % 40 >= 30;

    protected string GetRandomGemRichStoneForThisLevel(int level, bool reachedMineBottom = true)
    {
        var whichGem = Random.Next(59, 70);
        whichGem += whichGem % 2;

        if (!reachedMineBottom)
            whichGem = level switch
            {
                < 40 when whichGem is not (66 or 68) => Random.Choose(66, 68),
                < 80 when whichGem is 64 or 60 => Random.Choose(66, 70, 68, 62),
                _ => whichGem
            };

        return whichGem switch
        {
            66 => "8",
            68 => "10",
            60 => "12",
            70 => "6",
            64 => "4",
            62 => "14",
            _ => 40.ToString()
        };
    }
}