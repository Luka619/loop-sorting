namespace LoopSorting
{
    public readonly struct SfxProfile
    {
        public readonly float Volume;
        public readonly float Pitch;
        public readonly float PitchRandom;
        public readonly float CooldownSeconds;

        public SfxProfile(float volume, float pitch = 1f, float pitchRandom = 0.03f, float cooldownSeconds = 0.02f)
        {
            Volume = volume;
            Pitch = pitch;
            PitchRandom = pitchRandom;
            CooldownSeconds = cooldownSeconds;
        }
    }
}

