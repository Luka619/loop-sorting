using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    public sealed class SfxPlayer : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Min(1)]
        public int poolSize = 8;

        public bool Enabled { get; private set; } = true;

        private readonly Dictionary<SfxId, AudioClip[]> _clips = new Dictionary<SfxId, AudioClip[]>();
        private readonly Dictionary<SfxId, float> _lastTime = new Dictionary<SfxId, float>();
        private readonly HashSet<SfxId> _missingLogged = new HashSet<SfxId>();

        private AudioSource[] _sources;
        private int _nextSource;

        private AudioSource _loopSource;

#if UNITY_WEBGL && !UNITY_EDITOR
        private static bool _wxVisibilityHooksRegistered;
        private static float _wxVisibilityHooksRetryAt;
        private static bool _wxIsHidden;
#endif

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (!Enabled)
            {
                StopLoop();
            }
        }

        public void Play(SfxId id, float volumeMultiplier = 1f)
        {
            if (!Enabled)
            {
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!CanOperateAudioNow())
            {
                return;
            }
#endif

            EnsurePool();

            var clips = GetClips(id);
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            var profile = SfxCatalog.GetProfile(id);

            float now = Time.unscaledTime;
            if (profile.CooldownSeconds > 0f &&
                _lastTime.TryGetValue(id, out var last) &&
                now - last < profile.CooldownSeconds)
            {
                return;
            }
            _lastTime[id] = now;

            var src = GetNextSource();
            float pitch = profile.Pitch;
            if (profile.PitchRandom > 0f)
            {
                pitch += Random.Range(-profile.PitchRandom, profile.PitchRandom);
            }
            src.pitch = Mathf.Clamp(pitch, 0.25f, 3f);
            src.volume = 1f;

            float vol = Mathf.Clamp01(masterVolume * profile.Volume * Mathf.Max(0f, volumeMultiplier));
            var clip = clips.Length == 1 ? clips[0] : clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // WeChat WebGL audio backend can behave inconsistently with PlayOneShot; use explicit clip playback.
                src.Stop();
                src.clip = clip;
                src.loop = false;
                src.volume = vol;
                src.Play();
#else
                src.PlayOneShot(clip, vol);
#endif
            }
        }

        public void StartLoop(SfxId id, float volumeMultiplier = 1f, float pitch = 1f)
        {
            if (!Enabled)
            {
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!CanOperateAudioNow())
            {
                return;
            }
#endif

            EnsurePool();
            EnsureLoopSource();

            var clips = GetClips(id);
            var clip = clips != null && clips.Length > 0 ? clips[0] : null;
            if (clip == null)
            {
                return;
            }

            _loopSource.clip = clip;
            _loopSource.loop = true;
            _loopSource.pitch = Mathf.Clamp(pitch, 0.25f, 3f);

            var profile = SfxCatalog.GetProfile(id);
            float vol = Mathf.Clamp01(masterVolume * profile.Volume * Mathf.Max(0f, volumeMultiplier));
            _loopSource.volume = vol;

            if (!_loopSource.isPlaying)
            {
                _loopSource.Play();
            }
        }

        public void StopLoop()
        {
            if (_loopSource != null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (!CanOperateAudioNow())
                {
                    return;
                }
#endif
                _loopSource.Stop();
                _loopSource.clip = null;
            }
        }

        public void SetLoopPitch(float pitch)
        {
            if (_loopSource == null) return;
            if (!_loopSource.isPlaying) return;
            _loopSource.pitch = Mathf.Clamp(pitch, 0.25f, 3f);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static bool CanOperateAudioNow()
        {
            EnsureWeChatVisibilityHooks();
            if (!_wxVisibilityHooksRegistered)
            {
                // Best-effort fallback when WX hooks aren't available (e.g. running the WebGL build in a normal browser).
                return Application.isFocused;
            }

            return !_wxIsHidden;
        }

        private static void EnsureWeChatVisibilityHooks()
        {
            if (_wxVisibilityHooksRegistered) return;
            if (_wxVisibilityHooksRetryAt > 0f && Time.realtimeSinceStartup < _wxVisibilityHooksRetryAt) return;

            try
            {
                WeChatWASM.WX.OnHide(_ => { _wxIsHidden = true; });
                WeChatWASM.WX.OnShow(_ => { _wxIsHidden = false; });
                _wxVisibilityHooksRegistered = true;
            }
            catch
            {
                // WX SDK can be unavailable early during boot; retry later.
                _wxVisibilityHooksRetryAt = Time.realtimeSinceStartup + 2f;
            }
        }
#endif

        private AudioClip[] GetClips(SfxId id)
        {
            if (_clips.TryGetValue(id, out var cached))
            {
                return cached;
            }

            int variants = Mathf.Max(1, SfxCatalog.GetVariantCount(id));
            var list = new List<AudioClip>(variants);
            for (int i = 1; i <= variants; i++)
            {
                var path = SfxCatalog.GetResourcesPath(id, i);
                if (string.IsNullOrEmpty(path)) continue;

                var clip = Resources.Load<AudioClip>(path);
                if (clip != null) list.Add(clip);
            }

            var arr = list.ToArray();
            _clips[id] = arr;

            if (arr.Length == 0 && !_missingLogged.Contains(id))
            {
                _missingLogged.Add(id);
                var example = SfxCatalog.GetResourcesPath(id, 1);
                Debug.LogWarning($"SfxPlayer: missing AudioClip(s) in Resources at '{example}' for {id}.");
            }

            return arr;
        }

        private void EnsurePool()
        {
            if (_sources != null && _sources.Length == poolSize)
            {
                return;
            }

            var existing = GetComponents<AudioSource>();
            var list = new List<AudioSource>(existing);

            for (int i = list.Count; i < poolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 0f;
                src.dopplerLevel = 0f;
                src.rolloffMode = AudioRolloffMode.Linear;
                list.Add(src);
            }

            _sources = list.ToArray();
            _nextSource = 0;

            EnsureAudioListener();
        }

        private void EnsureLoopSource()
        {
            if (_loopSource != null)
            {
                return;
            }

            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.playOnAwake = false;
            _loopSource.loop = true;
            _loopSource.spatialBlend = 0f;
            _loopSource.dopplerLevel = 0f;
            _loopSource.rolloffMode = AudioRolloffMode.Linear;
        }

        private AudioSource GetNextSource()
        {
            if (_sources == null || _sources.Length == 0)
            {
                EnsurePool();
            }

            for (int i = 0; i < _sources.Length; i++)
            {
                int idx = (_nextSource + i) % _sources.Length;
                var s = _sources[idx];
                if (s != null && !s.isPlaying)
                {
                    _nextSource = (idx + 1) % _sources.Length;
                    return s;
                }
            }

            var fallback = _sources[_nextSource % _sources.Length];
            _nextSource = (_nextSource + 1) % _sources.Length;
            return fallback;
        }

        private void EnsureAudioListener()
        {
            if (FindObjectOfType<AudioListener>() != null)
            {
                return;
            }

            var cam = Camera.main;
            if (cam != null && cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
                return;
            }

            gameObject.AddComponent<AudioListener>();
        }
    }
}
