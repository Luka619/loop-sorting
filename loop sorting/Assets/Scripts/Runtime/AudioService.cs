using UnityEngine;

namespace LoopSorting
{
    public sealed class AudioService
    {
        private readonly MonoBehaviour _owner;
        private SfxPlayer _sfx;
        private BgmPlayer _bgm;

        public AudioService(MonoBehaviour owner)
        {
            _owner = owner;
        }

        public SfxPlayer Sfx => _sfx;
        public BgmPlayer Bgm => _bgm;

        public void EnsureSfx(bool enabled)
        {
            if (_sfx == null)
            {
                _sfx = _owner.GetComponent<SfxPlayer>();
                if (_sfx == null)
                {
                    _sfx = _owner.gameObject.AddComponent<SfxPlayer>();
                }
            }

            _sfx.SetEnabled(enabled);
        }

        public void EnsureBgm(bool enabled)
        {
            if (_bgm == null)
            {
                _bgm = _owner.GetComponentInChildren<BgmPlayer>(includeInactive: true);
                if (_bgm == null)
                {
                    var bgmGO = new GameObject("BGM");
                    bgmGO.transform.SetParent(_owner.transform, false);
                    _bgm = bgmGO.AddComponent<BgmPlayer>();
                }
            }

            _bgm.SetEnabled(enabled);
        }

        public void PlaySfx(SfxId id, float volumeMultiplier)
        {
            if (_sfx == null) return;
            _sfx.Play(id, volumeMultiplier);
        }

        public void StartSfxLoop(SfxId id, float volumeMultiplier, float pitch)
        {
            if (_sfx == null) return;
            _sfx.StartLoop(id, volumeMultiplier, pitch);
        }

        public void StopSfxLoop()
        {
            if (_sfx == null) return;
            _sfx.StopLoop();
        }

        public void PlayBgmLoop(BgmLoopId id, float fadeSeconds)
        {
            if (_bgm == null) return;
            _bgm.PlayLoop(id, fadeSeconds);
        }

        public void PlayBgmStinger(BgmStingerId id)
        {
            if (_bgm == null) return;
            _bgm.PlayStinger(id);
        }
    }
}
