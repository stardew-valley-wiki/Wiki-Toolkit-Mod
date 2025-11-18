using System;
using StardewValley;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

/// <summary>
/// 存储增益效果信息的类。
/// </summary>
[Serializable]
internal struct Buff
{
    /// <summary>
    /// 增益效果的标识符。
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// 战斗技能等级加成。
    /// </summary>
    public float CombatLevel { get; }

    /// <summary>
    /// 耕种技能等级加成。
    /// </summary>
    public float FarmingLevel { get; }

    /// <summary>
    /// 钓鱼技能等级加成。
    /// </summary>
    public float FishingLevel { get; }

    /// <summary>
    /// 采矿技能等级加成。
    /// </summary>
    public float MiningLevel { get; }

    /// <summary>
    /// 运气等级加成。
    /// </summary>
    public float LuckLevel { get; }

    /// <summary>
    /// 采集技能等级加成。
    /// </summary>
    public float ForagingLevel { get; }

    /// <summary>
    /// 最大体力加成。
    /// </summary>
    public float MaxStamina { get; }

    /// <summary>
    /// 磁力半径加成。
    /// </summary>
    public float MagneticRadius { get; }

    /// <summary>
    /// 移动速度加成。
    /// </summary>
    public float Speed { get; }

    /// <summary>
    /// 防御加成。
    /// </summary>
    public float Defense { get; }

    /// <summary>
    /// 攻击加成。
    /// </summary>
    public float Attack { get; }

    /// <summary>
    /// 增益效果的持续时间。
    /// </summary>
    public string Duration { get; }

    public Buff(StardewValley.Buff buff)
    {
        ID = buff.id;
        CombatLevel = buff.effects.CombatLevel.Value;
        FarmingLevel = buff.effects.FarmingLevel.Value;
        FishingLevel = buff.effects.FishingLevel.Value;
        MiningLevel = buff.effects.MiningLevel.Value;
        LuckLevel = buff.effects.LuckLevel.Value;
        ForagingLevel = buff.effects.ForagingLevel.Value;
        MaxStamina = buff.effects.MaxStamina.Value;
        MagneticRadius = buff.effects.MagneticRadius.Value;
        Speed = buff.effects.Speed.Value;
        Defense = buff.effects.Defense.Value;
        Attack = buff.effects.Attack.Value;
        Duration = Utility.getMinutesSecondsStringFromMilliseconds(buff.millisecondsDuration);
    }
}