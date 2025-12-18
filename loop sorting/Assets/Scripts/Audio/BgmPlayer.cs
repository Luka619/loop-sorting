using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    public sealed class BgmPlayer : MonoBehaviour
    {
        public enum Implementation
        {
            CrossfadeLoops = 0,
            VerticalLayering = 1,
        }

        public Implementation implementation =
#if UNITY_WEBGL && !UNITY_EDITOR
            Implementation.CrossfadeLoops;
#else
            Implementation.VerticalLayering;
#endif

        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Tooltip("Recommended mix gain from BGM_Metadata.json (default -12 dB).")]
        [Range(0f, 1f)]
        public float recommendedGain = 0.251f;

        [Tooltip("Sidechain duck amount when playing stingers (default -6 dB).")]
        [Range(0f, 1f)]
        public float duckMultiplier = 0.501f;

        [Tooltip("Duck time in seconds (default 0.4s).")]
        public float duckSeconds = 0.4f;

        [Header("Vertical Layering Gains")]
        [Range(0f, 1f)]
        public float menuPadGain = 0.34f;
        [Range(0f, 1f)]
        public float menuArpGain = 0.34f;
        [Range(0f, 1f)]
        public float menuPercGain = 0.34f;

        [Range(0f, 1f)]
        public float gameplayPadGain = 0.32f;
        [Range(0f, 1f)]
        public float gameplayArpGain = 0.32f;
        [Range(0f, 1f)]
        public float gameplayPercGain = 0.32f;
        [Range(0f, 1f)]
        public float gameplayPressureGain = 0.28f;

        public bool Enabled { get; private set; } = true;

        private enum StemGroup
        {
            None = 0,
            Menu = 1,
            Gameplay = 2,
        }

        private AudioSource _loopA;
        private AudioSource _loopB;
        private AudioSource _stinger;

        private AudioSource _activeLoop;
        private BgmLoopId? _activeLoopId;
        private float _loopAMix;
        private float _loopBMix;

        private AudioSource _menuPad;
        private AudioSource _menuArp;
        private AudioSource _menuPerc;
        private AudioSource _gameplayPad;
        private AudioSource _gameplayArp;
        private AudioSource _gameplayPerc;
        private AudioSource _gameplayPressure;

        private StemGroup _stemGroup;
        private bool _stemPressure;
        private float _menuPadMix;
        private float _menuArpMix;
        private float _menuPercMix;
        private float _gameplayPadMix;
        private float _gameplayArpMix;
        private float _gameplayPercMix;
        private float _gameplayPressureMix;

        private Coroutine _fadeRoutine;
        private Coroutine _duckRoutine;
        private float _duck = 1f;

        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (!Enabled)
            {
                StopAll(fadeSeconds: 0f);
            }
        }

        public void PlayLoop(BgmLoopId id, float fadeSeconds = 0.9f)
        {
            EnsureSources();
            if (!Enabled) return;

            if (implementation == Implementation.VerticalLayering)
            {
                PlayLoop_VerticalLayering(id, fadeSeconds);
                return;
            }

            PlayLoop_CrossfadeLoops(id, fadeSeconds);
        }

        public void FadeOutLoops(float fadeSeconds = 0.9f)
        {
            EnsureSources();
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (implementation == Implementation.VerticalLayering)
            {
                _fadeRoutine = StartCoroutine(FadeOutAllStems(Mathf.Max(0f, fadeSeconds)));
            }
            else
            {
                _fadeRoutine = StartCoroutine(FadeOutAllLoops(Mathf.Max(0f, fadeSeconds)));
            }
        }

        public void StopAll(float fadeSeconds = 0.2f)
        {
            EnsureSources();
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (_duckRoutine != null) StopCoroutine(_duckRoutine);
            _fadeRoutine = null;
            _duckRoutine = null;
            _duck = 1f;
            _loopAMix = 0f;
            _loopBMix = 0f;
            ClearStemMixes();

            if (fadeSeconds <= 0f)
            {
                StopLoopSource(_loopA);
                StopLoopSource(_loopB);
                StopStemGroup(StemGroup.Menu);
                StopStemGroup(StemGroup.Gameplay);
                if (_stinger != null) _stinger.Stop();
                _activeLoop = null;
                _activeLoopId = null;
                _stemGroup = StemGroup.None;
                _stemPressure = false;
                ApplyAllVolumes();
                return;
            }

            _fadeRoutine = StartCoroutine(FadeOutAll(Mathf.Max(0f, fadeSeconds)));
        }

        public void PlayStinger(BgmStingerId id, float volume = 1f)
        {
            EnsureSources();
            if (!Enabled) return;

            var path = BgmCatalog.GetResourcesPath(id);
            var clip = !string.IsNullOrEmpty(path) ? Resources.Load<AudioClip>(path) : null;
            if (clip == null)
            {
                Debug.LogWarning($"BgmPlayer: missing stinger AudioClip in Resources at '{path}' for {id}.");
                return;
            }

            float vol = Mathf.Clamp01(masterVolume * recommendedGain * Mathf.Clamp01(volume));
            _stinger.pitch = 1f;
            _stinger.volume = 1f;
            _stinger.PlayOneShot(clip, vol);

            if (_duckRoutine != null) StopCoroutine(_duckRoutine);
            _duckRoutine = StartCoroutine(DuckForSeconds(duckSeconds));
        }

        private void EnsureSources()
        {
            if (_loopA != null && _loopB != null && _stinger != null)
            {
                if (implementation == Implementation.VerticalLayering)
                {
                    EnsureStemSources();
                }
                return;
            }

            _stinger ??= CreateOneShotSource("BGM_Stinger");
            _loopA ??= CreateLoopSource("BGM_Loop_A");
            _loopB ??= CreateLoopSource("BGM_Loop_B");
            _activeLoop ??= _loopA;

            if (implementation == Implementation.VerticalLayering)
            {
                EnsureStemSources();
            }
        }

        private void EnsureStemSources()
        {
            _menuPad ??= CreateStemSource("BGM_Menu_Pad");
            _menuArp ??= CreateStemSource("BGM_Menu_Arp");
            _menuPerc ??= CreateStemSource("BGM_Menu_Perc");

            _gameplayPad ??= CreateStemSource("BGM_Gameplay_Pad");
            _gameplayArp ??= CreateStemSource("BGM_Gameplay_Arp");
            _gameplayPerc ??= CreateStemSource("BGM_Gameplay_Perc");
            _gameplayPressure ??= CreateStemSource("BGM_Gameplay_Pressure");
        }

        private AudioSource CreateLoopSource(string name)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.name = name;
            return src;
        }

        private AudioSource CreateStemSource(string name)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.name = name;
            return src;
        }

        private AudioSource CreateOneShotSource(string name)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.name = name;
            return src;
        }

        private void PlayLoop_CrossfadeLoops(BgmLoopId id, float fadeSeconds)
        {
            // If switching from vertical layering back to loop mode, stop stems to avoid double playback.
            StopStemGroup(StemGroup.Menu);
            StopStemGroup(StemGroup.Gameplay);
            _stemGroup = StemGroup.None;
            _stemPressure = false;
            ClearStemMixes();

            if (_activeLoopId.HasValue && _activeLoopId.Value == id && _activeLoop != null && _activeLoop.isPlaying)
            {
                ApplyAllVolumes();
                return;
            }

            var path = BgmCatalog.GetResourcesPath(id);
            var clip = LoadClip(path);
            if (clip == null)
            {
                Debug.LogWarning($"BgmPlayer: missing loop AudioClip in Resources at '{path}' for {id}.");
                return;
            }

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeToLoop(id, clip, Mathf.Max(0f, fadeSeconds)));
        }

        private void PlayLoop_VerticalLayering(BgmLoopId id, float fadeSeconds)
        {
            // If switching from loop mode to vertical layering, stop loops to avoid double playback.
            StopLoopSource(_loopA);
            StopLoopSource(_loopB);
            _activeLoop = null;
            _activeLoopId = null;
            _loopAMix = 0f;
            _loopBMix = 0f;

            StemGroup targetGroup = id == BgmLoopId.Menu ? StemGroup.Menu : StemGroup.Gameplay;
            bool targetPressure = id == BgmLoopId.GameplayPressure;

            if (_stemGroup != targetGroup)
            {
                if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
                _fadeRoutine = StartCoroutine(CrossfadeStemGroups(_stemGroup, targetGroup, targetPressure, Mathf.Max(0f, fadeSeconds)));
                return;
            }

            if (targetGroup == StemGroup.Gameplay && targetPressure != _stemPressure)
            {
                if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
                _fadeRoutine = StartCoroutine(FadeGameplayPressure(targetPressure, Mathf.Max(0f, fadeSeconds)));
                return;
            }

            EnsureStemGroupPlaying(targetGroup);
            if (targetGroup == StemGroup.Menu && _menuPadMix <= 0.0001f && _menuArpMix <= 0.0001f && _menuPercMix <= 0.0001f)
            {
                _menuPadMix = menuPadGain;
                _menuArpMix = menuArpGain;
                _menuPercMix = menuPercGain;
            }
            if (targetGroup == StemGroup.Gameplay && _gameplayPadMix <= 0.0001f && _gameplayArpMix <= 0.0001f && _gameplayPercMix <= 0.0001f)
            {
                _gameplayPadMix = gameplayPadGain;
                _gameplayArpMix = gameplayArpGain;
                _gameplayPercMix = gameplayPercGain;
                _gameplayPressureMix = _stemPressure ? gameplayPressureGain : 0f;
            }

            ApplyAllVolumes();
        }

        private IEnumerator CrossfadeToLoop(BgmLoopId id, AudioClip clip, float fadeSeconds)
        {
            var from = _activeLoop != null && _activeLoop.isPlaying ? _activeLoop : null;
            var to = from == _loopA ? _loopB : _loopA;
            if (to == null) yield break;

            to.clip = clip;
            to.loop = true;
            to.pitch = 1f;

            if (from != null && from.clip != null)
            {
                int samples = Mathf.Min(from.timeSamples, Mathf.Max(0, clip.samples - 1));
                to.timeSamples = samples;
            }
            else
            {
                to.timeSamples = 0;
            }

            float targetMix = 1f;
            if (fadeSeconds <= 0f || from == null)
            {
                if (!to.isPlaying) to.Play();
                _loopAMix = to == _loopA ? targetMix : 0f;
                _loopBMix = to == _loopB ? targetMix : 0f;
                ApplyAllVolumes();
                if (from != null) StopLoopSource(from);
                _activeLoop = to;
                _activeLoopId = id;
                _fadeRoutine = null;
                yield break;
            }

            if (!to.isPlaying) to.Play();
            float fromStart = from == _loopA ? _loopAMix : _loopBMix;
            float toStart = to == _loopA ? _loopAMix : _loopBMix;
            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                float fromMix = Mathf.Lerp(fromStart, 0f, a);
                float toMix = Mathf.Lerp(toStart, targetMix, a);

                if (from == _loopA) _loopAMix = fromMix; else _loopBMix = fromMix;
                if (to == _loopA) _loopAMix = toMix; else _loopBMix = toMix;
                ApplyAllVolumes();
                yield return null;
            }

            if (from == _loopA) _loopAMix = 0f; else _loopBMix = 0f;
            if (to == _loopA) _loopAMix = targetMix; else _loopBMix = targetMix;
            ApplyAllVolumes();
            StopLoopSource(from);
            _activeLoop = to;
            _activeLoopId = id;
            _fadeRoutine = null;
        }

        private IEnumerator CrossfadeStemGroups(StemGroup from, StemGroup to, bool toPressure, float fadeSeconds)
        {
            EnsureStemSources();
            EnsureStemGroupPlaying(to);

            float startMenuPad = _menuPadMix;
            float startMenuArp = _menuArpMix;
            float startMenuPerc = _menuPercMix;
            float startGamePad = _gameplayPadMix;
            float startGameArp = _gameplayArpMix;
            float startGamePerc = _gameplayPercMix;
            float startGamePressure = _gameplayPressureMix;

            float targetMenuPad = to == StemGroup.Menu ? menuPadGain : 0f;
            float targetMenuArp = to == StemGroup.Menu ? menuArpGain : 0f;
            float targetMenuPerc = to == StemGroup.Menu ? menuPercGain : 0f;
            float targetGamePad = to == StemGroup.Gameplay ? gameplayPadGain : 0f;
            float targetGameArp = to == StemGroup.Gameplay ? gameplayArpGain : 0f;
            float targetGamePerc = to == StemGroup.Gameplay ? gameplayPercGain : 0f;
            float targetGamePressure = to == StemGroup.Gameplay && toPressure ? gameplayPressureGain : 0f;

            if (fadeSeconds <= 0f)
            {
                StopStemGroup(from);
                _menuPadMix = targetMenuPad;
                _menuArpMix = targetMenuArp;
                _menuPercMix = targetMenuPerc;
                _gameplayPadMix = targetGamePad;
                _gameplayArpMix = targetGameArp;
                _gameplayPercMix = targetGamePerc;
                _gameplayPressureMix = targetGamePressure;
                _stemGroup = to;
                _stemPressure = toPressure && to == StemGroup.Gameplay;
                ApplyAllVolumes();
                _fadeRoutine = null;
                yield break;
            }

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                _menuPadMix = Mathf.Lerp(startMenuPad, targetMenuPad, a);
                _menuArpMix = Mathf.Lerp(startMenuArp, targetMenuArp, a);
                _menuPercMix = Mathf.Lerp(startMenuPerc, targetMenuPerc, a);
                _gameplayPadMix = Mathf.Lerp(startGamePad, targetGamePad, a);
                _gameplayArpMix = Mathf.Lerp(startGameArp, targetGameArp, a);
                _gameplayPercMix = Mathf.Lerp(startGamePerc, targetGamePerc, a);
                _gameplayPressureMix = Mathf.Lerp(startGamePressure, targetGamePressure, a);
                ApplyAllVolumes();
                yield return null;
            }

            if (targetMenuPad <= 0.0001f && targetMenuArp <= 0.0001f && targetMenuPerc <= 0.0001f)
            {
                StopStemGroup(StemGroup.Menu);
            }
            if (targetGamePad <= 0.0001f && targetGameArp <= 0.0001f && targetGamePerc <= 0.0001f && targetGamePressure <= 0.0001f)
            {
                StopStemGroup(StemGroup.Gameplay);
            }

            _menuPadMix = targetMenuPad;
            _menuArpMix = targetMenuArp;
            _menuPercMix = targetMenuPerc;
            _gameplayPadMix = targetGamePad;
            _gameplayArpMix = targetGameArp;
            _gameplayPercMix = targetGamePerc;
            _gameplayPressureMix = targetGamePressure;
            _stemGroup = to;
            _stemPressure = toPressure && to == StemGroup.Gameplay;
            ApplyAllVolumes();
            _fadeRoutine = null;
        }

        private IEnumerator DuckForSeconds(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            _duck = Mathf.Clamp01(duckMultiplier <= 0f ? 1f : duckMultiplier);
            ApplyAllVolumes();
            if (seconds > 0f)
            {
                yield return new WaitForSecondsRealtime(seconds);
            }
            _duck = 1f;
            ApplyAllVolumes();
            _duckRoutine = null;
        }

        private IEnumerator FadeGameplayPressure(bool pressure, float fadeSeconds)
        {
            EnsureStemSources();
            EnsureStemGroupPlaying(StemGroup.Gameplay);
            if (_gameplayPadMix <= 0.0001f && _gameplayArpMix <= 0.0001f && _gameplayPercMix <= 0.0001f)
            {
                _gameplayPadMix = gameplayPadGain;
                _gameplayArpMix = gameplayArpGain;
                _gameplayPercMix = gameplayPercGain;
            }

            float from = _gameplayPressureMix;
            float to = pressure ? gameplayPressureGain : 0f;

            if (fadeSeconds <= 0f)
            {
                _gameplayPressureMix = to;
                _stemPressure = pressure;
                ApplyAllVolumes();
                _fadeRoutine = null;
                yield break;
            }

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                _gameplayPressureMix = Mathf.Lerp(from, to, a);
                ApplyAllVolumes();
                yield return null;
            }

            _gameplayPressureMix = to;
            _stemPressure = pressure;
            ApplyAllVolumes();
            _fadeRoutine = null;
        }

        private IEnumerator FadeOutAll(float fadeSeconds)
        {
            if (fadeSeconds <= 0f)
            {
                StopAll(fadeSeconds: 0f);
                yield break;
            }

            float loopAStart = _loopAMix;
            float loopBStart = _loopBMix;
            float menuPadStart = _menuPadMix;
            float menuArpStart = _menuArpMix;
            float menuPercStart = _menuPercMix;
            float gamePadStart = _gameplayPadMix;
            float gameArpStart = _gameplayArpMix;
            float gamePercStart = _gameplayPercMix;
            float gamePressureStart = _gameplayPressureMix;

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                _loopAMix = Mathf.Lerp(loopAStart, 0f, a);
                _loopBMix = Mathf.Lerp(loopBStart, 0f, a);
                _menuPadMix = Mathf.Lerp(menuPadStart, 0f, a);
                _menuArpMix = Mathf.Lerp(menuArpStart, 0f, a);
                _menuPercMix = Mathf.Lerp(menuPercStart, 0f, a);
                _gameplayPadMix = Mathf.Lerp(gamePadStart, 0f, a);
                _gameplayArpMix = Mathf.Lerp(gameArpStart, 0f, a);
                _gameplayPercMix = Mathf.Lerp(gamePercStart, 0f, a);
                _gameplayPressureMix = Mathf.Lerp(gamePressureStart, 0f, a);
                ApplyAllVolumes();
                yield return null;
            }

            StopAll(fadeSeconds: 0f);
            _fadeRoutine = null;
        }

        private IEnumerator FadeOutAllLoops(float fadeSeconds)
        {
            if (fadeSeconds <= 0f)
            {
                StopLoopSource(_loopA);
                StopLoopSource(_loopB);
                _loopAMix = 0f;
                _loopBMix = 0f;
                ApplyAllVolumes();
                _activeLoop = null;
                _activeLoopId = null;
                _fadeRoutine = null;
                yield break;
            }

            float aStart = _loopAMix;
            float bStart = _loopBMix;
            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                _loopAMix = Mathf.Lerp(aStart, 0f, a);
                _loopBMix = Mathf.Lerp(bStart, 0f, a);
                ApplyAllVolumes();
                yield return null;
            }

            StopLoopSource(_loopA);
            StopLoopSource(_loopB);
            _loopAMix = 0f;
            _loopBMix = 0f;
            ApplyAllVolumes();
            _activeLoop = null;
            _activeLoopId = null;
            _fadeRoutine = null;
        }

        private IEnumerator FadeOutAllStems(float fadeSeconds)
        {
            if (fadeSeconds <= 0f)
            {
                StopStemGroup(StemGroup.Menu);
                StopStemGroup(StemGroup.Gameplay);
                _stemGroup = StemGroup.None;
                _stemPressure = false;
                ClearStemMixes();
                ApplyAllVolumes();
                _fadeRoutine = null;
                yield break;
            }

            float menuPadStart = _menuPadMix;
            float menuArpStart = _menuArpMix;
            float menuPercStart = _menuPercMix;
            float gamePadStart = _gameplayPadMix;
            float gameArpStart = _gameplayArpMix;
            float gamePercStart = _gameplayPercMix;
            float gamePressureStart = _gameplayPressureMix;

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / fadeSeconds);
                float a = Mathf.SmoothStep(0f, 1f, u);
                _menuPadMix = Mathf.Lerp(menuPadStart, 0f, a);
                _menuArpMix = Mathf.Lerp(menuArpStart, 0f, a);
                _menuPercMix = Mathf.Lerp(menuPercStart, 0f, a);
                _gameplayPadMix = Mathf.Lerp(gamePadStart, 0f, a);
                _gameplayArpMix = Mathf.Lerp(gameArpStart, 0f, a);
                _gameplayPercMix = Mathf.Lerp(gamePercStart, 0f, a);
                _gameplayPressureMix = Mathf.Lerp(gamePressureStart, 0f, a);
                ApplyAllVolumes();
                yield return null;
            }

            StopStemGroup(StemGroup.Menu);
            StopStemGroup(StemGroup.Gameplay);
            _stemGroup = StemGroup.None;
            _stemPressure = false;
            ClearStemMixes();
            ApplyAllVolumes();
            _fadeRoutine = null;
        }

        private void ApplyAllVolumes()
        {
            float gain = Mathf.Clamp01(masterVolume * recommendedGain);
            float mul = Mathf.Clamp01(_duck);

            if (_loopA != null) _loopA.volume = (_loopA.isPlaying ? gain * _loopAMix * mul : 0f);
            if (_loopB != null) _loopB.volume = (_loopB.isPlaying ? gain * _loopBMix * mul : 0f);

            if (_menuPad != null) _menuPad.volume = (_menuPad.isPlaying ? gain * _menuPadMix * mul : 0f);
            if (_menuArp != null) _menuArp.volume = (_menuArp.isPlaying ? gain * _menuArpMix * mul : 0f);
            if (_menuPerc != null) _menuPerc.volume = (_menuPerc.isPlaying ? gain * _menuPercMix * mul : 0f);

            if (_gameplayPad != null) _gameplayPad.volume = (_gameplayPad.isPlaying ? gain * _gameplayPadMix * mul : 0f);
            if (_gameplayArp != null) _gameplayArp.volume = (_gameplayArp.isPlaying ? gain * _gameplayArpMix * mul : 0f);
            if (_gameplayPerc != null) _gameplayPerc.volume = (_gameplayPerc.isPlaying ? gain * _gameplayPercMix * mul : 0f);
            if (_gameplayPressure != null) _gameplayPressure.volume = (_gameplayPressure.isPlaying ? gain * _gameplayPressureMix * mul : 0f);
        }

        private AudioClip LoadClip(string resourcesPathNoExt)
        {
            if (string.IsNullOrWhiteSpace(resourcesPathNoExt)) return null;
            if (_clipCache.TryGetValue(resourcesPathNoExt, out var cached) && cached != null) return cached;
            var clip = Resources.Load<AudioClip>(resourcesPathNoExt);
            _clipCache[resourcesPathNoExt] = clip;
            return clip;
        }

        private void EnsureStemGroupPlaying(StemGroup group)
        {
            EnsureStemSources();
            if (group == StemGroup.Menu)
            {
                if (_menuPad != null && _menuPad.isPlaying) return;
                StartStemGroup(StemGroup.Menu, startSamples: 0);
                return;
            }

            if (group == StemGroup.Gameplay)
            {
                if (_gameplayPad != null && _gameplayPad.isPlaying) return;
                StartStemGroup(StemGroup.Gameplay, startSamples: 0);
            }
        }

        private void StartStemGroup(StemGroup group, int startSamples)
        {
            EnsureStemSources();

            if (group == StemGroup.Menu)
            {
                StartStem(_menuPad, BgmStemId.MenuPad, startSamples);
                StartStem(_menuArp, BgmStemId.MenuArp, startSamples);
                StartStem(_menuPerc, BgmStemId.MenuPerc, startSamples);
                return;
            }

            if (group == StemGroup.Gameplay)
            {
                StartStem(_gameplayPad, BgmStemId.GameplayPad, startSamples);
                StartStem(_gameplayArp, BgmStemId.GameplayArp, startSamples);
                StartStem(_gameplayPerc, BgmStemId.GameplayPerc, startSamples);
                StartStem(_gameplayPressure, BgmStemId.GameplayPressure, startSamples);
            }
        }

        private void StartStem(AudioSource src, BgmStemId id, int startSamples)
        {
            if (src == null) return;
            var path = BgmCatalog.GetResourcesPath(id);
            var clip = LoadClip(path);
            if (clip == null)
            {
                Debug.LogWarning($"BgmPlayer: missing stem AudioClip in Resources at '{path}' for {id}.");
                return;
            }

            src.clip = clip;
            src.loop = true;
            src.pitch = 1f;
            src.timeSamples = Mathf.Clamp(startSamples, 0, Mathf.Max(0, clip.samples - 1));
            if (!src.isPlaying) src.Play();
        }

        private void StopStemGroup(StemGroup group)
        {
            if (group == StemGroup.Menu)
            {
                StopStemSource(_menuPad);
                StopStemSource(_menuArp);
                StopStemSource(_menuPerc);
                return;
            }

            if (group == StemGroup.Gameplay)
            {
                StopStemSource(_gameplayPad);
                StopStemSource(_gameplayArp);
                StopStemSource(_gameplayPerc);
                StopStemSource(_gameplayPressure);
            }
        }

        private static void StopStemSource(AudioSource src)
        {
            if (src == null) return;
            src.Stop();
            src.clip = null;
            src.volume = 0f;
        }

        private void ClearStemMixes()
        {
            _menuPadMix = 0f;
            _menuArpMix = 0f;
            _menuPercMix = 0f;
            _gameplayPadMix = 0f;
            _gameplayArpMix = 0f;
            _gameplayPercMix = 0f;
            _gameplayPressureMix = 0f;
        }

        private static void StopLoopSource(AudioSource src)
        {
            if (src == null) return;
            src.Stop();
            src.clip = null;
            src.volume = 0f;
        }
    }
}
