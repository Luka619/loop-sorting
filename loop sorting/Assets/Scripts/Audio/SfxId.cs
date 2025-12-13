namespace LoopSorting
{
    public enum SfxId
    {
        // UI
        UiClick,
        UiConfirm,
        UiCancel,
        UiPopupOpen,
        UiPopupClose,
        UiDenied,
        UiHover,

        // Gameplay
        BoxSelect,
        BoxLockedThunk,
        BoxBusyDenied,
        RunShipStart,
        RunShipEnd,
        BlockEject,
        BlockLand,
        BlockInsert,
        BlockReject,
        BlockRejectLocked,
        BlockRejectBusy,
        BlockRejectFull,
        BlockRejectMismatch,
        BlockSkipEmptyBox,
        BoxComplete,
        BoxUnlock,
        HiddenReveal,

        // Conveyor
        ConveyorTick,
        ConveyorLoop,
        ConveyorSpeedup,
        ConveyorSpeeddown,
        ConveyorFullWarning,
        ConveyorFullFail,

        // Boosters
        BoosterActivate,
        BoosterFillSort,
        BoosterShuffle,
        BoosterFail,
        BoosterUiOpen,
        BoosterUiClose,

        // Level states
        LevelStart,
        LevelNext,
        LevelRetry,
        LevelWin,
        LevelLose
    }
}
