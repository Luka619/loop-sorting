namespace LoopSorting
{
    public enum HapticsPulse
    {
        Light,
        Medium,
        Heavy,
        Long
    }

    public readonly struct HapticsStep
    {
        public readonly HapticsPulse Pulse;
        public readonly float DelaySeconds;

        public HapticsStep(HapticsPulse pulse, float delaySeconds)
        {
            Pulse = pulse;
            DelaySeconds = delaySeconds;
        }
    }

    public sealed class HapticsProfile
    {
        public readonly float CooldownSeconds;
        public readonly HapticsStep[] Steps;

        public HapticsProfile(float cooldownSeconds, params HapticsStep[] steps)
        {
            CooldownSeconds = cooldownSeconds;
            Steps = steps;
        }
    }
}

