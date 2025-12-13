namespace LoopSorting
{
    using UnityEngine;

    public static class SfxCatalog
    {
        private const string Root = "Audio/LoopSorting_SFX_Pack/SFX";

        public static int GetVariantCount(SfxId id)
        {
            return id switch
            {
                SfxId.UiClick => 3,
                SfxId.BlockEject => 2,
                SfxId.BlockInsert => 2,
                SfxId.BlockReject => 2,
                SfxId.ConveyorTick => 2,
                _ => 1
            };
        }

        public static string GetResourcesPath(SfxId id, int variantIndex1Based = 1)
        {
            int variants = GetVariantCount(id);
            int v = Mathf.Clamp(variantIndex1Based, 1, Mathf.Max(1, variants));
            string suffix = variants > 1 ? $"_{v:00}" : string.Empty;

            return id switch
            {
                // UI
                SfxId.UiClick => $"{Root}/UI/ui_click{suffix}",
                SfxId.UiConfirm => $"{Root}/UI/ui_confirm",
                SfxId.UiCancel => $"{Root}/UI/ui_cancel",
                SfxId.UiPopupOpen => $"{Root}/UI/ui_popup_open",
                SfxId.UiPopupClose => $"{Root}/UI/ui_popup_close",
                SfxId.UiDenied => $"{Root}/UI/ui_denied",

                // Gameplay
                SfxId.BoxSelect => $"{Root}/Gameplay/box_select",
                SfxId.BoxLockedThunk => $"{Root}/Gameplay/box_locked_thunk",
                SfxId.RunShipStart => $"{Root}/Gameplay/run_ship_start",
                SfxId.BlockEject => $"{Root}/Gameplay/block_eject{suffix}",
                SfxId.BlockInsert => $"{Root}/Gameplay/block_insert{suffix}",
                SfxId.BlockReject => $"{Root}/Gameplay/block_reject{suffix}",
                SfxId.BoxComplete => $"{Root}/Gameplay/box_complete",
                SfxId.BoxUnlock => $"{Root}/Gameplay/box_unlock",
                SfxId.HiddenReveal => $"{Root}/Gameplay/hidden_reveal",

                // Conveyor
                SfxId.ConveyorTick => $"{Root}/Conveyor/conveyor_tick{suffix}",
                SfxId.ConveyorLoop => $"{Root}/Conveyor/conveyor_loop",
                SfxId.ConveyorSpeedup => $"{Root}/Conveyor/conveyor_speedup",
                SfxId.ConveyorFullWarning => $"{Root}/Conveyor/belt_full_warning",

                // Boosters
                SfxId.BoosterActivate => $"{Root}/Boosters/booster_activate",
                SfxId.BoosterFillSort => $"{Root}/Boosters/booster_fill",
                SfxId.BoosterShuffle => $"{Root}/Boosters/booster_shuffle",
                SfxId.BoosterFail => $"{Root}/Boosters/booster_fail",

                // States
                SfxId.LevelStart => $"{Root}/States/level_start",
                SfxId.LevelWin => $"{Root}/States/win_jingle",
                SfxId.LevelLose => $"{Root}/States/lose_jingle",

                _ => null
            };
        }

        public static SfxProfile GetProfile(SfxId id)
        {
            return id switch
            {
                // UI
                SfxId.UiClick => new SfxProfile(volume: 0.55f, pitch: 1f, pitchRandom: 0.06f, cooldownSeconds: 0.03f),
                SfxId.UiConfirm => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.06f),
                SfxId.UiCancel => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.06f),
                SfxId.UiPopupOpen => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.08f),
                SfxId.UiPopupClose => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.08f),
                SfxId.UiDenied => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.15f),

                // Gameplay
                SfxId.BoxSelect => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.05f, cooldownSeconds: 0.04f),
                SfxId.BoxLockedThunk => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.RunShipStart => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.BlockEject => new SfxProfile(volume: 0.65f, pitch: 1f, pitchRandom: 0.07f, cooldownSeconds: 0.02f),
                SfxId.BlockInsert => new SfxProfile(volume: 0.65f, pitch: 1f, pitchRandom: 0.06f, cooldownSeconds: 0.06f),
                SfxId.BlockReject => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.04f, cooldownSeconds: 0.08f),
                SfxId.BoxComplete => new SfxProfile(volume: 0.9f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoxUnlock => new SfxProfile(volume: 0.9f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.HiddenReveal => new SfxProfile(volume: 0.8f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.25f),

                // Conveyor
                SfxId.ConveyorTick => new SfxProfile(volume: 0.22f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.08f),
                SfxId.ConveyorLoop => new SfxProfile(volume: 0.22f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0f),
                SfxId.ConveyorSpeedup => new SfxProfile(volume: 0.6f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.35f),
                SfxId.ConveyorFullWarning => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.4f),

                // Boosters
                SfxId.BoosterActivate => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.2f),
                SfxId.BoosterFillSort => new SfxProfile(volume: 0.85f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoosterShuffle => new SfxProfile(volume: 0.85f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoosterFail => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),

                // States
                SfxId.LevelStart => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                SfxId.LevelWin => new SfxProfile(volume: 0.95f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                SfxId.LevelLose => new SfxProfile(volume: 0.95f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                _ => new SfxProfile(volume: 0.7f)
            };
        }
    }
}
