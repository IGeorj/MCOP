using System.Text;
using Humanizer;

namespace MCOP.Core.Helpers
{
    public static class LevelingHelper
    {
        private static readonly double a = 1.6665;
        private static readonly double b = 22.5;
        private static readonly double c = 75.8335;

        public static string GenerateLevelString(int currentLevel, int totalExp, int rank)
        {
            double currentLevelXP = GetTotalXPForLevel(currentLevel);

            double nextLevelXP = GetTotalXPForLevel(currentLevel + 1);

            int xpEarnedInCurrentLevel = (int)(totalExp - currentLevelXP);

            int xpRequiredForNextLevel = (int)(nextLevelXP - currentLevelXP);

            double progress = (double)xpEarnedInCurrentLevel / xpRequiredForNextLevel * 100;
            int progressPercentage = (int)Math.Min(Math.Max(progress, 0), 100);

            string progressBar = GenerateProgressBar(progressPercentage);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-# **Уровень:** {currentLevel} (#{rank})");
            sb.AppendLine($"-# **Опыт:** {xpEarnedInCurrentLevel.ToMetric()} / {xpRequiredForNextLevel.ToMetric()}");
            sb.AppendLine(progressBar);

            return sb.ToString();
        }

        private static string GenerateProgressBar(int progressPercentage)
        {
            const int barLength = 20;
            int filledLength = (int)(barLength * (progressPercentage / 100.0));

            string progressBar = new string('█', filledLength).PadRight(barLength, '░');
            return $"-# {progressBar} {progressPercentage}%";
        }

        public static int GetLevelFromTotalExp(int totalExp)
        {
            int low = 0;
            int high = 1000;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                double xp = GetTotalXPForLevel(mid);

                if (xp < totalExp)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return high < 0 ? 0 : high;
        }

        public static int GetExpToLvlUp(int level, int totalExp)
        {
            double nextLevelXP = GetTotalXPForLevel(level + 1);

            int remainingXP = (int)(nextLevelXP - totalExp);

            return Math.Max(remainingXP, 0);
        }

        public static double GetTotalXPForLevel(int level)
        {
            return a * Math.Pow(level, 3) + b * Math.Pow(level, 2) + c * level;
        }
    }
}
