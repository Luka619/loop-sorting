using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    public sealed class SfxPlayer : MonoBehaviour
    {
        private const int WebglWeChatPoolSize = 2;
        private const int WebglMaxQueuedSfx = 24;
        private const float WebglQueueMaxSecondsHigh = 0.25f;
        private const float WebglQueueMaxSecondsDefault = 0.35f;
        private const float WebglQueueMaxSecondsLow = 0.15f;

        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Min(1)]
        public int poolSize = 8;

        [Header("Debug")]
        public bool debugLog;

        public bool Enabled { get; private set; } = true;

        private readonly Dictionary<SfxId, AudioClip[]> _clips = new Dictionary<SfxId, AudioClip[]>();
        private readonly Dictionary<SfxId, float> _lastTime = new Dictionary<SfxId, float>();
        private readonly HashSet<SfxId> _missingLogged = new HashSet<SfxId>();

        private AudioSource[] _sources;
        private float[] _sourceBusyUntil;
        private int _nextSource;

        private AudioSource _loopSource;
        private float _webglLoopRetryAt;
        private bool _weChatConveyorLoopSuppressed;

#if UNITY_WEBGL && !UNITY_EDITOR
        private struct WebglQueuedSfx
        {
            public SfxId id;
            public AudioClip clip;
            public float volume;
            public float pitch;
            public float enqueuedAt;
            public int priority;
        }

        private readonly List<WebglQueuedSfx> _webglQueue = new List<WebglQueuedSfx>(WebglMaxQueuedSfx);
        private int[] _webglSourcePriority;

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
#if UNITY_WEBGL && !UNITY_EDITOR
                _webglQueue.Clear();
#endif
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void Update()
        {
            if (!Enabled) return;
            if (_webglQueue.Count == 0) return;
            if (!CanOperateAudioNow()) return;

            EnsurePool();

            float now = Time.realtimeSinceStartup;
            for (int i = _webglQueue.Count - 1; i >= 0; i--)
            {
                var req = _webglQueue[i];
                float maxAge = req.priority >= 90
                    ? WebglQueueMaxSecondsHigh
                    : (req.priority <= 20 ? WebglQueueMaxSecondsLow : WebglQueueMaxSecondsDefault);
                if (now - req.enqueuedAt > maxAge)
                {
                    _webglQueue.RemoveAt(i);
                }
            }

            while (_webglQueue.Count > 0 && TryGetFreeSourceWebgl(now, out int sourceIndex))
            {
                int pick = PickNextQueuedWebglSfxIndex();
                var req = _webglQueue[pick];
                _webglQueue.RemoveAt(pick);
                PlayWebglOnSource(sourceIndex, req);
            }
        }

        private bool TryGetFreeSourceWebgl(float now, out int sourceIndex)
        {
            sourceIndex = -1;
            if (_sources == null || _sources.Length == 0) return false;
            if (_sourceBusyUntil == null || _sourceBusyUntil.Length != _sources.Length) return false;

            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i] != null && _sourceBusyUntil[i] <= now)
                {
                    sourceIndex = i;
                    return true;
                }
            }

            return false;
        }

        private bool TryStealSourceWebgl(int priority, float now, out int sourceIndex)
        {
            sourceIndex = -1;
            if (_sources == null || _sources.Length == 0) return false;
            if (_sourceBusyUntil == null || _sourceBusyUntil.Length != _sources.Length) return false;
            if (_webglSourcePriority == null || _webglSourcePriority.Length != _sources.Length) return false;

            int bestIdx = -1;
            int bestPriority = int.MaxValue;
            float bestRemaining = -1f;

            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i] == null) continue;
                if (_sourceBusyUntil[i] <= now) continue; // already free

                int p = _webglSourcePriority[i];
                if (p >= priority) continue; // don't steal same/higher priority

                float remaining = _sourceBusyUntil[i] - now;
                if (p < bestPriority || (p == bestPriority && remaining > bestRemaining))
                {
                    bestIdx = i;
                    bestPriority = p;
                    bestRemaining = remaining;
                }
            }

            if (bestIdx < 0) return false;
            sourceIndex = bestIdx;
            return true;
        }

        private int PickNextQueuedWebglSfxIndex()
        {
            int best = 0;
            int bestPriority = _webglQueue[0].priority;
            float bestTime = _webglQueue[0].enqueuedAt;

            for (int i = 1; i < _webglQueue.Count; i++)
            {
                var r = _webglQueue[i];
                if (r.priority > bestPriority || (r.priority == bestPriority && r.enqueuedAt < bestTime))
                {
                    best = i;
                    bestPriority = r.priority;
                    bestTime = r.enqueuedAt;
                }
            }

            return best;
        }

        private static int GetWebglSfxPriority(SfxId id)
        {
            return id switch
            {
                // Gameplay-critical feedback
                SfxId.BlockEject => 100,
                SfxId.BlockInsert => 100,
                SfxId.BlockReject => 95,
                SfxId.BlockRejectLocked => 95,
                SfxId.BlockRejectBusy => 95,
                SfxId.BlockRejectFull => 95,
                SfxId.BlockRejectMismatch => 95,
                SfxId.BoxComplete => 90,
                SfxId.BoxUnlock => 90,

                // UI feedback
                SfxId.UiClick => 60,
                SfxId.UiConfirm => 60,
                SfxId.UiCancel => 60,
                SfxId.UiDenied => 60,
                SfxId.UiPopupOpen => 60,
                SfxId.UiPopupClose => 60,

                // Low-priority ambience ticks
                SfxId.ConveyorTick => 10,

                _ => 50
            };
        }

        private void EnqueueWebglSfx(SfxId id, AudioClip clip, float volume, float pitch)
        {
            float now = Time.realtimeSinceStartup;
            var req = new WebglQueuedSfx
            {
                id = id,
                clip = clip,
                volume = volume,
                pitch = pitch,
                enqueuedAt = now,
                priority = GetWebglSfxPriority(id),
            };

            if (_webglQueue.Count < WebglMaxQueuedSfx)
            {
                _webglQueue.Add(req);
                return;
            }

            int worst = 0;
            int worstPriority = _webglQueue[0].priority;
            float newestTime = _webglQueue[0].enqueuedAt;
            for (int i = 1; i < _webglQueue.Count; i++)
            {
                var r = _webglQueue[i];
                if (r.priority < worstPriority || (r.priority == worstPriority && r.enqueuedAt > newestTime))
                {
                    worst = i;
                    worstPriority = r.priority;
                    newestTime = r.enqueuedAt;
                }
            }

            if (req.priority > worstPriority)
            {
                _webglQueue[worst] = req;
            }
        }

        private void PlayWebglOnSource(int sourceIndex, WebglQueuedSfx req)
        {
            if (_sources == null || sourceIndex < 0 || sourceIndex >= _sources.Length) return;
            var src = _sources[sourceIndex];
            if (src == null) return;

            src.pitch = Mathf.Clamp(req.pitch, 0.25f, 3f);
            src.volume = req.volume;
            src.loop = false;
            src.clip = req.clip;
            src.Play();

            MarkSourceBusy(sourceIndex, req.clip, src.pitch);
            if (_webglSourcePriority != null && sourceIndex >= 0 && sourceIndex < _webglSourcePriority.Length)
            {
                _webglSourcePriority[sourceIndex] = req.priority;
            }

            if (debugLog && Debug.isDebugBuild)
            {
                Debug.Log($"SfxPlayer: play '{req.id}' clip='{req.clip.name}' src={sourceIndex} vol={req.volume:0.00} pitch={src.pitch:0.00} (webgl queued)");
            }
        }
#endif

        public void Play(SfxId id, float volumeMultiplier = 1f)
        {
            if (!Enabled)
            {
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!CanOperateAudioNow())
            {
                if (debugLog && Debug.isDebugBuild)
                {
                    Debug.Log($"SfxPlayer: skip '{id}' (WX hidden/unfocused).");
                }
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

            float pitch = profile.Pitch;
            if (profile.PitchRandom > 0f)
            {
                pitch += Random.Range(-profile.PitchRandom, profile.PitchRandom);
            }
            pitch = Mathf.Clamp(pitch, 0.25f, 3f);

            float vol = Mathf.Clamp01(masterVolume * profile.Volume * Mathf.Max(0f, volumeMultiplier));
            var clip = clips.Length == 1 ? clips[0] : clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                float nowRealtime = Time.realtimeSinceStartup;
                int priority = GetWebglSfxPriority(id);
                if (TryGetFreeSourceWebgl(nowRealtime, out int freeIndex))
                {
                    PlayWebglOnSource(freeIndex, new WebglQueuedSfx
                    {
                        id = id,
                        clip = clip,
                        volume = vol,
                        pitch = pitch,
                        enqueuedAt = nowRealtime,
                        priority = priority,
                    });
                }
                else if (TryStealSourceWebgl(priority, nowRealtime, out int stealIndex))
                {
                    PlayWebglOnSource(stealIndex, new WebglQueuedSfx
                    {
                        id = id,
                        clip = clip,
                        volume = vol,
                        pitch = pitch,
                        enqueuedAt = nowRealtime,
                        priority = priority,
                    });
                }
                else
                {
                    EnqueueWebglSfx(id, clip, vol, pitch);
                    if (debugLog && Debug.isDebugBuild)
                    {
                        Debug.Log($"SfxPlayer: queue '{id}' clip='{clip.name}' vol={vol:0.00} pitch={pitch:0.00}");
                    }
                }
#else
                var src = GetNextSource(out int sourceIndex);
                src.pitch = pitch;
                src.volume = 1f;
                src.PlayOneShot(clip, vol);
                MarkSourceBusy(sourceIndex, clip, src.pitch);

                if (debugLog && Debug.isDebugBuild)
                {
                    Debug.Log($"SfxPlayer: play '{id}' clip='{clip.name}' src={sourceIndex} vol={vol:0.00} pitch={src.pitch:0.00}");
                }
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
            // WeChat mini-games route larger clips through InnerAudioContext, which can spam `operateAudio` permission
            // errors on some devices/states. The conveyor loop is the only large SFX clip; keep gameplay SFX but suppress
            // this loop when running under WeChat.
            if (id == SfxId.ConveyorLoop)
            {
                EnsureWeChatVisibilityHooks();
                if (_wxVisibilityHooksRegistered)
                {
                    if (!_weChatConveyorLoopSuppressed)
                    {
                        StopLoop();
                        _weChatConveyorLoopSuppressed = true;
                        if (debugLog && Debug.isDebugBuild)
                        {
                            Debug.Log("SfxPlayer: suppressing ConveyorLoop on WeChat WebGL.");
                        }
                    }
                    return;
                }
            }

            if (_webglLoopRetryAt > 0f && Time.realtimeSinceStartup < _webglLoopRetryAt)
            {
                return;
            }
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

#if UNITY_WEBGL && !UNITY_EDITOR
            // If WeChat denies audio operations (e.g. runningState=background), Play() can silently fail and we end up
            // re-attempting every frame; back off to avoid log spam + perf spikes.
            if (!_loopSource.isPlaying)
            {
                _webglLoopRetryAt = Time.realtimeSinceStartup + 2f;
            }
            else
            {
                _webglLoopRetryAt = 0f;
            }
#endif
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
#if UNITY_WEBGL && !UNITY_EDITOR
            // Keep the pool intentionally small on WebGL (especially WeChat) to avoid backend channel limits.
            poolSize = Mathf.Clamp(poolSize, 1, WebglWeChatPoolSize);
#endif

            if (_sources != null &&
                _sources.Length == poolSize &&
                _sourceBusyUntil != null &&
                _sourceBusyUntil.Length == _sources.Length)
            {
                return;
            }

            var existing = GetComponents<AudioSource>();
            var list = new List<AudioSource>(existing.Length);
            for (int i = 0; i < existing.Length; i++)
            {
                var s = existing[i];
                if (s == null) continue;
                if (_loopSource != null && s == _loopSource) continue;
                list.Add(s);
            }

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

            // If we are reducing the pool size, destroy extras to keep WebGL channel count low.
            for (int i = list.Count - 1; i >= poolSize; i--)
            {
                var extra = list[i];
                list.RemoveAt(i);
                if (extra == null) continue;
                if (Application.isPlaying) Destroy(extra);
                else DestroyImmediate(extra);
            }

            _sources = list.ToArray();
            _sourceBusyUntil = new float[_sources.Length];
#if UNITY_WEBGL && !UNITY_EDITOR
            _webglSourcePriority = new int[_sources.Length];
#endif
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

        private AudioSource GetNextSource(out int index)
        {
            if (_sources == null || _sources.Length == 0)
            {
                EnsurePool();
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            // On some WebGL/WeChat builds, AudioSource.isPlaying can be unreliable; track a predicted end time instead.
            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < _sources.Length; i++)
            {
                int idx = (_nextSource + i) % _sources.Length;
                var s = _sources[idx];
                if (s != null && (_sourceBusyUntil == null || idx >= _sourceBusyUntil.Length || _sourceBusyUntil[idx] <= now))
                {
                    _nextSource = (idx + 1) % _sources.Length;
                    index = idx;
                    return s;
                }
            }

            // All sources are busy; pick the earliest-to-finish as a fallback.
            int bestIdx = _nextSource % _sources.Length;
            float bestUntil = _sourceBusyUntil != null && bestIdx < _sourceBusyUntil.Length ? _sourceBusyUntil[bestIdx] : 0f;
            for (int i = 0; i < _sources.Length; i++)
            {
                float until = _sourceBusyUntil != null && i < _sourceBusyUntil.Length ? _sourceBusyUntil[i] : 0f;
                if (until < bestUntil)
                {
                    bestUntil = until;
                    bestIdx = i;
                }
            }

            _nextSource = (bestIdx + 1) % _sources.Length;
            index = bestIdx;
            return _sources[bestIdx];
#else
            for (int i = 0; i < _sources.Length; i++)
            {
                int idx = (_nextSource + i) % _sources.Length;
                var s = _sources[idx];
                if (s != null && !s.isPlaying)
                {
                    _nextSource = (idx + 1) % _sources.Length;
                    index = idx;
                    return s;
                }
            }

            var fallback = _sources[_nextSource % _sources.Length];
            index = _nextSource % _sources.Length;
            _nextSource = (_nextSource + 1) % _sources.Length;
            return fallback;
#endif
        }

        private void MarkSourceBusy(int sourceIndex, AudioClip clip, float pitch)
        {
            if (_sourceBusyUntil == null) return;
            if (sourceIndex < 0 || sourceIndex >= _sourceBusyUntil.Length) return;
            if (clip == null) return;

            float duration = clip.length / Mathf.Max(0.01f, pitch);
            float until = Time.realtimeSinceStartup + Mathf.Max(0f, duration);
            _sourceBusyUntil[sourceIndex] = until;
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
