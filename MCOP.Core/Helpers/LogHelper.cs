using System.Text;
using Humanizer;
using MCOP.Data.Models;
using Serilog;

namespace MCOP.Core.Helpers
{
    public class LogHelper
    {
        public static Dictionary<string, object?> GetClassProperties<T>(T userStats)
        {
            return typeof(T)
                .GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p.GetValue(userStats));
        }

        public static void LogChangedProperties(Dictionary<string, object?> originalValues, Dictionary<string, object?> updatedValues, ulong guildId, ulong userId)
        {
            var changes = new List<string>();

            foreach (var key in originalValues.Keys)
            {
                var originalValue = originalValues[key];
                var updatedValue = updatedValues[key];

                if (!Equals(originalValue, updatedValue))
                {
                    changes.Add($"{key}: {originalValue} -> {updatedValue}");
                }
            }

            if (changes.Count != 0)
            {
                Log.Information(
                    "ModifyUserStatsAsync: Changes detected for guildId: {guildId}, userId: {userId}. \n{changes}",
                    guildId, userId, string.Join(Environment.NewLine, changes));
            }
            else
            {
                Log.Information(
                    "ModifyUserStatsAsync: No changes detected for guildId: {guildId}, userId: {userId}",
                    guildId, userId);
            }
        }

    }
}
