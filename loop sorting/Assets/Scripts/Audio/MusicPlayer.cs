using UnityEngine;

namespace LoopSorting
{
    public sealed class MusicPlayer : MonoBehaviour
    {
        [Tooltip("Optional clip; if null, loads from Resources using resourcesPath.")]
        public AudioClip clip;

        [Tooltip("Resources path (no extension) to load if clip is null.")]
        public string resourcesPath = "Audio/BGM/bgm_loop";

        [Range(0f, 1f)]
        public float volume = 0.5f;

        public bool Enabled { get; private set; } = true;

        private AudioSource _source;

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            EnsureSource();

            if (!Enabled)
            {
                if (_source != null)
                {
                    _source.Stop();
                }
                return;
            }

            TryPlay();
        }

        private void Awake()
        {
            EnsureSource();
            if (Enabled)
            {
                TryPlay();
            }
        }

        private void OnEnable()
        {
            if (Enabled)
            {
                TryPlay();
            }
        }

        private void EnsureSource()
        {
            if (_source != null) return;

            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.dopplerLevel = 0f;
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.volume = Mathf.Clamp01(volume);
        }

        private void TryPlay()
        {
            if (_source == null) return;

            var resolvedClip = clip;
            if (resolvedClip == null && !string.IsNullOrWhiteSpace(resourcesPath))
            {
                resolvedClip = Resources.Load<AudioClip>(resourcesPath.Trim());
            }

            if (resolvedClip == null)
            {
                _source.clip = null;
                return;
            }

            _source.clip = resolvedClip;
            _source.volume = Mathf.Clamp01(volume);

            if (!_source.isPlaying)
            {
                _source.Play();
            }
        }
    }
}

