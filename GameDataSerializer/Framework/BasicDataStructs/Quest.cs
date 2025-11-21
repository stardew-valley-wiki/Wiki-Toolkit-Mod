using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace WikiInGameTools.GameDataSerializer.Framework.BasicDataStructs;

internal static class Quests
{
    /// <summary>
    /// 解析得到的所有附带打造配方的特别任务。
    /// </summary>
    /// <remarks>
    /// <c>key</c>：配方 ID
    /// <c>value</c>：相关任务名
    /// </remarks>
    public static readonly Dictionary<string, string> AllSpecialOrdersWithRecipe = new();

    static Quests()
    {
        var mails = DataLoader.Mail(Game1.content);
        var orders = DataLoader.SpecialOrders(Game1.content);
        foreach (var order in orders.Values)
        {
            var orderDisplayName = order.Name;
            var mailRewards = order.Rewards.FirstOrDefault(r => r.Type == "Mail");
            if (mailRewards is null) continue;

            if (!mailRewards.Data.TryGetValue("MailReceived", out var mailID) ||
                !mails.TryGetValue(mailID, out var mailData) ||
                !mailData.Contains("%item craftingRecipe")) continue;

            var recipeName = mailData.Split("%")[1].Replace("item craftingRecipe ", "").Replace("_", " ").Trim();
            AllSpecialOrdersWithRecipe.TryAdd(recipeName, orderDisplayName);
        }
    }
}