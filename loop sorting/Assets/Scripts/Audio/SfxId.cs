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

        // Gameplay
        BoxSelect,
        BoxLockedThunk,
        RunShipStart,
        BlockEject,
        BlockInsert,
        BlockReject,
        BoxComplete,
        BoxUnlock,
        HiddenReveal,

        // Conveyor
        ConveyorTick,
        ConveyorLoop,
        ConveyorSpeedup,
        ConveyorFullWarning,

        // Boosters
        BoosterActivate,
        BoosterFillSort,
        BoosterShuffle,
        BoosterFail,

        // Level states
        LevelStart,
        LevelWin,
        LevelLose
    }
}
