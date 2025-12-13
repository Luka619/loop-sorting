using System;

namespace LoopSorting.Motion
{
    /// <summary>
    /// 动效事件 ID（建议用于埋点、播放路由、A/B 试验）。
    /// </summary>
    public enum MotionId
    {
        Conveyor_Tick_Move,
        Conveyor_Loop_Wrap,
        Conveyor_Panic_FastForward,

        Box_Interact_Hover,
        Box_Ship_Run,
        Box_Busy_On,
        Box_Busy_Off,
        Box_Complete_Seal,
        Box_Locked_Idle,
        Box_Locked_Unlock,

        Block_Spawn_ToBelt,
        Block_Tick_FollowSlot,
        Block_EnterBox_Success,
        Block_EnterBox_Fail_Busy,
        Block_EnterBox_Fail_Locked,
        Block_EnterBox_Fail_Mismatch,
        Block_EnterBox_Fail_EmptyDeferred,
        Block_Hidden_Reveal,

        Booster_FillSort,
        Booster_Shuffle,

        Flow_Win,
        Flow_Lose,
    }
}
