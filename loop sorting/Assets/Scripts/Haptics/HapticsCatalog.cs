using System;
using System.Collections.Generic;

namespace LoopSorting
{
    public static class HapticsCatalog
    {
        private static readonly Dictionary<HapticsId, HapticsProfile> Profiles = new Dictionary<HapticsId, HapticsProfile>
        {
            // UI
            { HapticsId.UiTap, new HapticsProfile(0.04f, new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.UiConfirm, new HapticsProfile(0.06f, new HapticsStep(HapticsPulse.Medium, 0f)) },
            { HapticsId.UiCancel, new HapticsProfile(0.10f, new HapticsStep(HapticsPulse.Light, 0.05f), new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.UiDenied, new HapticsProfile(0.14f, new HapticsStep(HapticsPulse.Medium, 0.06f), new HapticsStep(HapticsPulse.Light, 0f)) },

            // Gameplay
            { HapticsId.GameplaySelect, new HapticsProfile(0.06f, new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.GameplayInsert, new HapticsProfile(0.10f, new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.GameplayReject, new HapticsProfile(0.18f, new HapticsStep(HapticsPulse.Heavy, 0f)) },
            { HapticsId.GameplayLocked, new HapticsProfile(0.22f, new HapticsStep(HapticsPulse.Heavy, 0.07f), new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.BoxComplete, new HapticsProfile(0.35f, new HapticsStep(HapticsPulse.Medium, 0.06f), new HapticsStep(HapticsPulse.Light, 0f)) },
            { HapticsId.BoxUnlock, new HapticsProfile(0.35f, new HapticsStep(HapticsPulse.Light, 0.06f), new HapticsStep(HapticsPulse.Medium, 0f)) },
            { HapticsId.HiddenReveal, new HapticsProfile(0.12f, new HapticsStep(HapticsPulse.Light, 0f)) },

            // Conveyor
            { HapticsId.ConveyorFullWarning, new HapticsProfile(0.70f, new HapticsStep(HapticsPulse.Medium, 0.08f), new HapticsStep(HapticsPulse.Medium, 0f)) },
            // Keep fail vibration subtle per motion design (panic stop + restrained buzz).
            { HapticsId.ConveyorFullFail, new HapticsProfile(1.20f, new HapticsStep(HapticsPulse.Medium, 0.10f), new HapticsStep(HapticsPulse.Heavy, 0f)) },

            // Boosters
            { HapticsId.BoosterActivate, new HapticsProfile(0.20f, new HapticsStep(HapticsPulse.Medium, 0f)) },
            { HapticsId.BoosterSuccess, new HapticsProfile(0.40f, new HapticsStep(HapticsPulse.Medium, 0.07f), new HapticsStep(HapticsPulse.Medium, 0f)) },
            { HapticsId.BoosterFail, new HapticsProfile(0.50f, new HapticsStep(HapticsPulse.Heavy, 0f)) },

            // Level states
            { HapticsId.LevelWin, new HapticsProfile(2.0f, new HapticsStep(HapticsPulse.Light, 0.06f), new HapticsStep(HapticsPulse.Light, 0.08f), new HapticsStep(HapticsPulse.Medium, 0f)) },
            { HapticsId.LevelLose, new HapticsProfile(2.0f, new HapticsStep(HapticsPulse.Heavy, 0.10f), new HapticsStep(HapticsPulse.Long, 0f)) },
        };

        public static HapticsProfile GetProfile(HapticsId id)
        {
            return Profiles.TryGetValue(id, out var p) ? p : null;
        }

        public static bool TryMapSfxToHaptics(SfxId sfxId, out HapticsId hapticsId)
        {
            switch (sfxId)
            {
                case SfxId.UiClick:
                case SfxId.UiHover:
                case SfxId.UiPopupOpen:
                case SfxId.UiPopupClose:
                    hapticsId = HapticsId.UiTap;
                    return true;

                case SfxId.UiConfirm:
                case SfxId.LevelNext:
                case SfxId.LevelRetry:
                    hapticsId = HapticsId.UiConfirm;
                    return true;

                case SfxId.UiCancel:
                    hapticsId = HapticsId.UiCancel;
                    return true;

                case SfxId.UiDenied:
                case SfxId.BoxBusyDenied:
                    hapticsId = HapticsId.UiDenied;
                    return true;

                case SfxId.BoxSelect:
                    hapticsId = HapticsId.GameplaySelect;
                    return true;

                case SfxId.BlockInsert:
                case SfxId.BlockLand:
                    hapticsId = HapticsId.GameplayInsert;
                    return true;

                case SfxId.BlockReject:
                case SfxId.BlockRejectLocked:
                case SfxId.BlockRejectBusy:
                case SfxId.BlockRejectFull:
                case SfxId.BlockRejectMismatch:
                    hapticsId = HapticsId.GameplayReject;
                    return true;

                case SfxId.BoxLockedThunk:
                    hapticsId = HapticsId.GameplayLocked;
                    return true;

                case SfxId.BoxComplete:
                    hapticsId = HapticsId.BoxComplete;
                    return true;

                case SfxId.BoxUnlock:
                    hapticsId = HapticsId.BoxUnlock;
                    return true;

                case SfxId.HiddenReveal:
                    hapticsId = HapticsId.HiddenReveal;
                    return true;

                case SfxId.ConveyorFullWarning:
                    hapticsId = HapticsId.ConveyorFullWarning;
                    return true;

                case SfxId.ConveyorFullFail:
                    hapticsId = HapticsId.ConveyorFullFail;
                    return true;

                case SfxId.BoosterActivate:
                    hapticsId = HapticsId.BoosterActivate;
                    return true;

                case SfxId.BoosterFillSort:
                case SfxId.BoosterShuffle:
                    hapticsId = HapticsId.BoosterSuccess;
                    return true;

                case SfxId.BoosterFail:
                    hapticsId = HapticsId.BoosterFail;
                    return true;

                case SfxId.LevelWin:
                    hapticsId = HapticsId.LevelWin;
                    return true;

                case SfxId.LevelLose:
                    hapticsId = HapticsId.LevelLose;
                    return true;

                default:
                    hapticsId = default;
                    return false;
            }
        }
    }
}

