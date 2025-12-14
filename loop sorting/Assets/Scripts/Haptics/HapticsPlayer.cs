using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    public sealed class HapticsPlayer : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float intensity = 1f;

        public bool Enabled { get; private set; } = true;

        private readonly Dictionary<HapticsId, float> _lastTime = new Dictionary<HapticsId, float>();
        private Coroutine _patternRoutine;

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (!Enabled && _patternRoutine != null)
            {
                StopCoroutine(_patternRoutine);
                _patternRoutine = null;
            }
        }

        public void PlayFromSfx(SfxId sfxId)
        {
            if (HapticsCatalog.TryMapSfxToHaptics(sfxId, out var hapticsId))
            {
                Play(hapticsId);
            }
        }

        public void Play(HapticsId id)
        {
            if (!Enabled) return;

            var profile = HapticsCatalog.GetProfile(id);
            if (profile == null || profile.Steps == null || profile.Steps.Length == 0)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (profile.CooldownSeconds > 0f &&
                _lastTime.TryGetValue(id, out var last) &&
                now - last < profile.CooldownSeconds)
            {
                return;
            }
            _lastTime[id] = now;

            // Single-step fast path.
            if (profile.Steps.Length == 1 && profile.Steps[0].DelaySeconds <= 0f)
            {
                Pulse(profile.Steps[0].Pulse);
                return;
            }

            if (_patternRoutine != null)
            {
                StopCoroutine(_patternRoutine);
            }
            _patternRoutine = StartCoroutine(PlayPattern(profile.Steps));
        }

        private IEnumerator PlayPattern(HapticsStep[] steps)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                Pulse(steps[i].Pulse);
                float d = Mathf.Max(0f, steps[i].DelaySeconds);
                if (d > 0f)
                {
                    yield return new WaitForSecondsRealtime(d);
                }
            }
            _patternRoutine = null;
        }

        private void Pulse(HapticsPulse pulse)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TryWeChatPulse(pulse);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (TryAndroidPulse(pulse)) return;
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#else
            // Editor / unsupported: no-op.
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static void TryWeChatPulse(HapticsPulse pulse)
        {
            try
            {
                if (pulse == HapticsPulse.Long)
                {
                    WeChatWASM.WX.VibrateLong(new WeChatWASM.VibrateLongOption());
                    return;
                }

                string type = pulse switch
                {
                    HapticsPulse.Heavy => "heavy",
                    HapticsPulse.Medium => "medium",
                    _ => "light"
                };
                var opt = new WeChatWASM.VibrateShortOption
                {
                    type = type
                };
                WeChatWASM.WX.VibrateShort(opt);
            }
            catch
            {
                // Best-effort only.
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private bool TryAndroidPulse(HapticsPulse pulse)
        {
            try
            {
                int durationMs;
                int amplitude; // 1~255
                switch (pulse)
                {
                    case HapticsPulse.Light:
                        durationMs = 18;
                        amplitude = 80;
                        break;
                    case HapticsPulse.Medium:
                        durationMs = 28;
                        amplitude = 140;
                        break;
                    case HapticsPulse.Heavy:
                        durationMs = 45;
                        amplitude = 220;
                        break;
                    case HapticsPulse.Long:
                        durationMs = 120;
                        amplitude = 200;
                        break;
                    default:
                        durationMs = 28;
                        amplitude = 140;
                        break;
                }

                amplitude = Mathf.Clamp(Mathf.RoundToInt(amplitude * Mathf.Clamp01(intensity)), 1, 255);

                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null) return false;
                var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null) return false;

                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                int sdk = version.GetStatic<int>("SDK_INT");
                if (sdk >= 26)
                {
                    using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", (long)durationMs);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
#endif
    }
}

