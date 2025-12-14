namespace LoopSorting
{
    public static class BgmCatalog
    {
        private const string Root = "LoopSorting_BGM_Pack_v1";

        public static string GetResourcesPath(BgmLoopId id)
        {
            return id switch
            {
                BgmLoopId.Menu => $"{Root}/BGM/Loops/Menu/bgm_menu_loop",
                BgmLoopId.GameplayBase => $"{Root}/BGM/Loops/Gameplay/bgm_gameplay_base_loop",
                BgmLoopId.GameplayPressure => $"{Root}/BGM/Loops/Gameplay/bgm_gameplay_pressure_loop",
                _ => null
            };
        }

        public static string GetResourcesPath(BgmStemId id)
        {
            return id switch
            {
                BgmStemId.MenuPad => $"{Root}/BGM/Stems/Menu/menu_pad",
                BgmStemId.MenuArp => $"{Root}/BGM/Stems/Menu/menu_arp",
                BgmStemId.MenuPerc => $"{Root}/BGM/Stems/Menu/menu_perc",

                BgmStemId.GameplayPad => $"{Root}/BGM/Stems/Gameplay/gameplay_pad",
                BgmStemId.GameplayArp => $"{Root}/BGM/Stems/Gameplay/gameplay_arp",
                BgmStemId.GameplayPerc => $"{Root}/BGM/Stems/Gameplay/gameplay_perc",
                BgmStemId.GameplayPressure => $"{Root}/BGM/Stems/Gameplay/gameplay_pressure",

                _ => null
            };
        }

        public static string GetResourcesPath(BgmStingerId id)
        {
            return id switch
            {
                BgmStingerId.FullWarning => $"{Root}/BGM/Stingers/stinger_full_warning",
                BgmStingerId.Speedup => $"{Root}/BGM/Stingers/stinger_speedup",
                BgmStingerId.Speeddown => $"{Root}/BGM/Stingers/stinger_speeddown",
                BgmStingerId.Unlock => $"{Root}/BGM/Stingers/stinger_unlock",
                BgmStingerId.BoxComplete => $"{Root}/BGM/Stingers/stinger_box_complete",
                BgmStingerId.Win => $"{Root}/BGM/Stingers/stinger_win",
                BgmStingerId.Lose => $"{Root}/BGM/Stingers/stinger_lose",
                BgmStingerId.BoosterActivate => $"{Root}/BGM/Stingers/stinger_booster_activate",
                _ => null
            };
        }
    }
}
