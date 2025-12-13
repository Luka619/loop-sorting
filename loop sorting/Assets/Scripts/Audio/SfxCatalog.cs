namespace LoopSorting
{
    using UnityEngine;

    public static class SfxCatalog
    {
        private const string Root = "Audio/LoopSorting_SFX_Pack_v3/SFX";

        public static int GetVariantCount(SfxId id)
        {
            return id switch
            {
                SfxId.UiClick => 3,
                SfxId.BoxSelect => 2,
                SfxId.BlockEject => 3,
                SfxId.BlockLand => 2,
                SfxId.BlockInsert => 3,
                SfxId.BlockReject => 2,
                SfxId.BlockRejectLocked => 2,
                SfxId.BlockRejectBusy => 2,
                SfxId.BlockRejectFull => 2,
                SfxId.BlockRejectMismatch => 2,
                SfxId.ConveyorTick => 3,
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
                SfxId.UiHover => $"{Root}/UI/ui_hover",

                // Gameplay
                SfxId.BoxSelect => v == 2 ? $"{Root}/Gameplay/box_select_02" : $"{Root}/Gameplay/box_select",
                SfxId.BoxLockedThunk => $"{Root}/Gameplay/box_locked_thunk",
                SfxId.BoxBusyDenied => $"{Root}/Gameplay/box_busy_denied",
                SfxId.RunShipStart => $"{Root}/Gameplay/run_ship_start",
                SfxId.RunShipEnd => $"{Root}/Gameplay/run_ship_end",
                SfxId.BlockEject => $"{Root}/Gameplay/block_eject{suffix}",
                SfxId.BlockLand => $"{Root}/Gameplay/block_land{suffix}",
                SfxId.BlockInsert => $"{Root}/Gameplay/block_insert{suffix}",
                SfxId.BlockReject => $"{Root}/Gameplay/block_reject{suffix}",
                SfxId.BlockRejectLocked => $"{Root}/Gameplay/block_reject_locked{suffix}",
                SfxId.BlockRejectBusy => $"{Root}/Gameplay/block_reject_busy{suffix}",
                SfxId.BlockRejectFull => $"{Root}/Gameplay/block_reject_full{suffix}",
                SfxId.BlockRejectMismatch => $"{Root}/Gameplay/block_reject_mismatch{suffix}",
                SfxId.BlockSkipEmptyBox => $"{Root}/Gameplay/block_skip_empty_box",
                SfxId.BoxComplete => $"{Root}/Gameplay/box_complete",
                SfxId.BoxUnlock => $"{Root}/Gameplay/box_unlock",
                SfxId.HiddenReveal => $"{Root}/Gameplay/hidden_reveal",

                // Conveyor
                SfxId.ConveyorTick => $"{Root}/Conveyor/conveyor_tick{suffix}",
                SfxId.ConveyorLoop => $"{Root}/Conveyor/conveyor_loop",
                SfxId.ConveyorSpeedup => $"{Root}/Conveyor/conveyor_speedup",
                SfxId.ConveyorSpeeddown => $"{Root}/Conveyor/conveyor_speeddown",
                SfxId.ConveyorFullWarning => $"{Root}/Conveyor/belt_full_warning",
                SfxId.ConveyorFullFail => $"{Root}/Conveyor/belt_full_fail",

                // Boosters
                SfxId.BoosterActivate => $"{Root}/Boosters/booster_activate",
                SfxId.BoosterFillSort => $"{Root}/Boosters/booster_fill",
                SfxId.BoosterShuffle => $"{Root}/Boosters/booster_shuffle",
                SfxId.BoosterFail => $"{Root}/Boosters/booster_fail",
                SfxId.BoosterUiOpen => $"{Root}/Boosters/booster_ui_open",
                SfxId.BoosterUiClose => $"{Root}/Boosters/booster_ui_close",

                // States
                SfxId.LevelStart => $"{Root}/States/level_start",
                SfxId.LevelNext => $"{Root}/States/level_next",
                SfxId.LevelRetry => $"{Root}/States/level_retry",
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
                SfxId.UiHover => new SfxProfile(volume: 0.22f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.05f),

                // Gameplay
                SfxId.BoxSelect => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.05f, cooldownSeconds: 0.04f),
                SfxId.BoxLockedThunk => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.RunShipStart => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.BoxBusyDenied => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.RunShipEnd => new SfxProfile(volume: 0.6f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.BlockEject => new SfxProfile(volume: 0.65f, pitch: 1f, pitchRandom: 0.07f, cooldownSeconds: 0.02f),
                SfxId.BlockLand => new SfxProfile(volume: 0.5f, pitch: 1f, pitchRandom: 0.04f, cooldownSeconds: 0.02f),
                SfxId.BlockInsert => new SfxProfile(volume: 0.65f, pitch: 1f, pitchRandom: 0.06f, cooldownSeconds: 0.06f),
                SfxId.BlockReject => new SfxProfile(volume: 0.7f, pitch: 1f, pitchRandom: 0.04f, cooldownSeconds: 0.08f),
                SfxId.BlockRejectLocked => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.1f),
                SfxId.BlockRejectBusy => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.1f),
                SfxId.BlockRejectFull => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.1f),
                SfxId.BlockRejectMismatch => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.1f),
                SfxId.BlockSkipEmptyBox => new SfxProfile(volume: 0.35f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.BoxComplete => new SfxProfile(volume: 0.9f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoxUnlock => new SfxProfile(volume: 0.9f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.HiddenReveal => new SfxProfile(volume: 0.8f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.25f),

                // Conveyor
                SfxId.ConveyorTick => new SfxProfile(volume: 0.22f, pitch: 1f, pitchRandom: 0.03f, cooldownSeconds: 0.08f),
                SfxId.ConveyorLoop => new SfxProfile(volume: 0.22f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0f),
                SfxId.ConveyorSpeedup => new SfxProfile(volume: 0.6f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.35f),
                SfxId.ConveyorFullWarning => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.4f),
                SfxId.ConveyorSpeeddown => new SfxProfile(volume: 0.55f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.35f),
                SfxId.ConveyorFullFail => new SfxProfile(volume: 0.85f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),

                // Boosters
                SfxId.BoosterActivate => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.2f),
                SfxId.BoosterFillSort => new SfxProfile(volume: 0.85f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoosterShuffle => new SfxProfile(volume: 0.85f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoosterFail => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.25f),
                SfxId.BoosterUiOpen => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),
                SfxId.BoosterUiClose => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0.02f, cooldownSeconds: 0.12f),

                // States
                SfxId.LevelStart => new SfxProfile(volume: 0.75f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                SfxId.LevelWin => new SfxProfile(volume: 0.95f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                SfxId.LevelLose => new SfxProfile(volume: 0.95f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.5f),
                SfxId.LevelNext => new SfxProfile(volume: 0.8f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.25f),
                SfxId.LevelRetry => new SfxProfile(volume: 0.8f, pitch: 1f, pitchRandom: 0f, cooldownSeconds: 0.25f),
                _ => new SfxProfile(volume: 0.7f)
            };
        }
    }
}
