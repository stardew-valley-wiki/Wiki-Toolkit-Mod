using System;
using System.Collections.Generic;

namespace StoneProbabilityCalculator;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("请选择计算模式：");
        Console.WriteLine("1. 暴力模拟");
        Console.WriteLine("2. 公式计算");
        switch (Console.ReadLine())
        {
            case "1":
                Console.WriteLine("请输入模拟次数（单位：千万）：");
                var arg1 = Console.ReadLine();
                if (!int.TryParse(arg1, out var times)) goto default;
                Simulate(times);
                Console.WriteLine("\n模拟完毕，按任意键退出！");
                break;
            case "2":
                Calculate();
                Console.WriteLine("\n计算完毕，按任意键退出！");
                break;
            default:
                Console.WriteLine("错误！");
                break;
        }

        Console.ReadKey();
    }

    private static void Simulate(int times)
    {
        var threads = new List<SimulateThread>();
        
        int[] mineLevels = { 20, 40, 60, 80, 100, 120 };
        // int[] mineLevels = { 121, 220, 320, 520 };
        // int[] mineLevels = { 920, 1720, 3320 };
        double[] dailyLucks = { -0.1, 0.125 };
        
        foreach (var mineLevel in mineLevels)
        foreach (var dailyLuck in dailyLucks)
        {
            threads.Add(new SimulateThread(dailyLuck, mineLevel, 0, 10));
            threads.Add(new SimulateThread(dailyLuck, mineLevel, 0, 10, 1));
            threads.Add(new SimulateThread(dailyLuck, mineLevel, 0, 10, 2));
        }

        Utilities.ParallelRun(threads, t => t.Run(times));
    }

    private static void Calculate()
    {
        var calculators = new List<Calculator>();

        int[] mineLevels = { 121, 220, 320, 420, 520, 920, 1320, 1720 };
        double[] dailyLucks = { -0.1, 0.125 };

        foreach (var mineLevel in mineLevels)
        foreach (var dailyLuck in dailyLucks)
        {
            calculators.Add(new Calculator(dailyLuck, mineLevel, 0, 10));
            calculators.Add(new Calculator(dailyLuck, mineLevel, 0, 10, 1));
            calculators.Add(new Calculator(dailyLuck, mineLevel, 0, 10, 2));
        }

        foreach (var calculator in calculators) calculator.SkullCavern();
    }
}