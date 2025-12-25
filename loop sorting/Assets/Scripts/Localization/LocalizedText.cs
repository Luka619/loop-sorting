namespace LoopSorting
{
    public static class LocalizedText
    {
        public const string TutorialClickBox = "点击箱子来把混乱的积木放在传送带上";
        public const string TutorialWinToast = "所有箱子都被摆放整齐了！";

        public const string MainMenuPlay = "开始游戏";
        public const string MainMenuTitle = "循环\n排序";

        public static string LevelLabel(int levelNumber)
        {
            return $"第 {levelNumber} 关";
        }

        public const string HudFast = "加速";
        public const string SpeedMultiplierSuffix = "倍";

        public static string HudFastMultiplier(string speedLabel)
        {
            return $"{HudFast} {speedLabel}";
        }

        public const string ResultVictory = "胜利";
        public const string ResultFailed = "失败";
        public const string ResultRevive = "复活";
        public const string ResultNext = "下一关";

        public static string ResultReviveCost(int cost)
        {
            return $"复活 {cost}";
        }

        public const string SettingsTitle = "设置";
        public const string SettingsMusic = "音乐";
        public const string SettingsSfx = "音效";
        public const string SettingsVibration = "震动";
        public const string SettingsRetry = "重试";

        public const string ShopTitle = "商店";
        public const string ShopMoreLives = "更多生命";
        public const string ShopSectionCoins = "金币";
        public const string ShopSectionLives = "生命";
        public const string CurrencyTenThousandSuffix = "万";
        public const string CurrencyHundredMillionSuffix = "亿";

        public static string ShopCoinPackTitle(int amount)
        {
            return $"{amount} 金币";
        }

        public static string ShopLifePackTitle(int amount)
        {
            return $"获得 +{amount} 生命";
        }

        public const string ShopLifeRefillTitle = "补满 5 生命";

        public const string BoosterTitle = "道具";
        public const string BoosterPurchaseTitle = "购买道具";
        public const string BoosterShuffle = "洗牌";
        public const string BoosterSort = "排序";
        public const string BoosterFree = "免费";
        public const string BoosterShuffleDesc = "打乱未完成箱子里的积木顺序";
        public const string BoosterSortDesc = "随机选一种颜色，自动集中填满一箱";

        public static string BoosterPurchaseSpecific(string title)
        {
            return $"购买{title}";
        }

        public static string DebugCapacityLine(int capacity)
        {
            return $"容量:{capacity}";
        }

        public static string DebugBoxOpenLine(int opening)
        {
            return $"开口:{opening}";
        }
    }
}
