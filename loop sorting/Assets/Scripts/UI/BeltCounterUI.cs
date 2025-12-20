using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoopSorting
{
    /// <summary>
    /// Simple UI counter to display remaining empty slots on the conveyor.
    /// </summary>
    // Supports both `Text` and `TMP_Text` (UI kit uses TMP by default).
    public class BeltCounterUI : MonoBehaviour
    {
        private enum CounterState
        {
            Normal,
            Warning,
            Danger
        }

        [Header("Thresholds")]
        [Min(0)]
        [Tooltip("When empty slots <= this value, the counter enters Warning visuals.")]
        [SerializeField] private int warningThreshold = 20;
        [Min(0)]
        [Tooltip("When empty slots <= this value, the counter enters Danger visuals. Typically 0.")]
        [SerializeField] private int dangerThreshold = 10;

        [Header("Normal")]
        [Tooltip("When enabled, Normal state uses this explicit color instead of capturing the initial TMP/Text color.")]
        [SerializeField] private bool useExplicitNormalColor = true;
        [SerializeField] private Color normalColor = Color.white;

        [Header("Colors")]
        [Tooltip("Text color in Warning state.")]
        [SerializeField] private Color warningColor = new Color(1f, 0.78f, 0.25f, 1f);
        [Tooltip("Text color in Danger state.")]
        [SerializeField] private Color dangerColor = new Color(1f, 0.28f, 0.28f, 1f);

        [Header("Pulse (Loop)")]
        [Min(0f)]
        [Tooltip("Max additional scale applied during Warning pulse (0.06 -> up to +6%).")]
        [SerializeField] private float warningPulseScale = 0.06f;
        [Min(0.05f)]
        [Tooltip("Seconds per pulse cycle in Warning state.")]
        [SerializeField] private float warningPulsePeriod = 0.90f;
        [Min(0f)]
        [Tooltip("Max additional scale applied during Danger pulse (0.10 -> up to +10%).")]
        [SerializeField] private float dangerPulseScale = 0.10f;
        [Min(0.05f)]
        [Tooltip("Seconds per pulse cycle in Danger state.")]
        [SerializeField] private float dangerPulsePeriod = 0.55f;

        [Header("State Enter (One-shot)")]
        [Min(0f)]
        [Tooltip("Scale punch when entering Warning state.")]
        [SerializeField] private float enterWarningPunchScale = 0.10f;
        [Min(0.05f)]
        [Tooltip("Duration of the enter-Warning punch animation.")]
        [SerializeField] private float enterWarningSeconds = 0.12f;
        [Min(0f)]
        [Tooltip("Scale punch when entering Danger state.")]
        [SerializeField] private float enterDangerPunchScale = 0.18f;
        [Min(0.05f)]
        [Tooltip("Duration of the enter-Danger punch/shake animation.")]
        [SerializeField] private float enterDangerSeconds = 0.18f;
        [Min(0f)]
        [Tooltip("Horizontal shake amplitude (local units) when entering Danger state.")]
        [SerializeField] private float enterDangerShakeAmplitude = 10f;
        [Min(2)]
        [Tooltip("Shake frequency multiplier when entering Danger state.")]
        [SerializeField] private int enterDangerShakes = 12;
        [Min(0f)]
        [Tooltip("Minimum seconds between one-shot animations (prevents noisy spam).")]
        [SerializeField] private float oneShotMinIntervalSeconds = 0.25f;

        private Text _text;
        private TMP_Text _tmp;

        private Vector3 _baseScale = Vector3.one;
        private Vector3 _baseLocalPos = Vector3.zero;
        private Color _baseColor = Color.white;
        private bool _baseColorCaptured;

        private int _lastEmpty = int.MaxValue;
        private CounterState _state = CounterState.Normal;
        private bool _oneShotOverrideActive;
        private float _lastOneShotTime = -999f;
        private Coroutine _pulseRoutine;
        private Coroutine _oneShotRoutine;

        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
            _text = GetComponent<Text>();
            CaptureBasePoseAndColor(captureColor: true);
        }

        private void OnEnable()
        {
            CaptureBasePoseAndColor(captureColor: false);
            ApplyStateVisuals(_state, immediate: true);
        }

        private void OnDisable()
        {
            StopAnimations(resetTransform: true);
        }

        public void SetValue(int empty, int total)
        {
            empty = Mathf.Max(0, empty);
            total = Mathf.Max(0, total);
            string value = empty.ToString();
            if (_tmp != null) _tmp.text = value;
            if (_text != null) _text.text = value;

            ComputeEffectiveThresholds(total, out int effectiveWarningThreshold, out int effectiveDangerThreshold);
            var nextState = ComputeState(empty, effectiveWarningThreshold, effectiveDangerThreshold);
            bool stateChanged = nextState != _state;
            var prevState = _state;
            int prevEmpty = _lastEmpty;

            _state = nextState;
            _lastEmpty = empty;

            if (_oneShotOverrideActive)
            {
                return;
            }

            if (stateChanged)
            {
                HandleStateTransition(prevState, nextState, prevEmpty, empty, effectiveWarningThreshold, effectiveDangerThreshold);
            }
        }

        private static CounterState ComputeState(int empty, int effectiveWarningThreshold, int effectiveDangerThreshold)
        {
            if (empty <= effectiveDangerThreshold) return CounterState.Danger;
            if (empty <= effectiveWarningThreshold) return CounterState.Warning;
            return CounterState.Normal;
        }

        private void HandleStateTransition(
            CounterState prev,
            CounterState next,
            int prevEmpty,
            int nextEmpty,
            int effectiveWarningThreshold,
            int effectiveDangerThreshold)
        {
            if (next == CounterState.Normal)
            {
                ApplyStateVisuals(next, immediate: true);
                return;
            }

            bool enteringWarning = prev != CounterState.Warning && next == CounterState.Warning;
            bool enteringDanger = prev != CounterState.Danger && next == CounterState.Danger;

            if (enteringDanger && prevEmpty > effectiveDangerThreshold && nextEmpty <= effectiveDangerThreshold)
            {
                PlayEnterStateOneShot(next);
                return;
            }

            if (enteringWarning && prevEmpty > effectiveWarningThreshold && nextEmpty <= effectiveWarningThreshold)
            {
                PlayEnterStateOneShot(next);
                return;
            }

            ApplyStateVisuals(next, immediate: true);
        }

        private void ComputeEffectiveThresholds(int total, out int effectiveWarningThreshold, out int effectiveDangerThreshold)
        {
            // Avoid showing Warning/Danger when the belt is completely empty on very small conveyors.
            int maxThreshold = Mathf.Max(0, total - 1);
            effectiveDangerThreshold = Mathf.Clamp(dangerThreshold, 0, maxThreshold);
            effectiveWarningThreshold = Mathf.Clamp(warningThreshold, 0, maxThreshold);
            effectiveWarningThreshold = Mathf.Max(effectiveWarningThreshold, effectiveDangerThreshold);
        }

        private void PlayEnterStateOneShot(CounterState state)
        {
            float now = Time.unscaledTime;
            if (now - _lastOneShotTime < oneShotMinIntervalSeconds)
            {
                ApplyStateVisuals(state, immediate: true);
                return;
            }
            _lastOneShotTime = now;

            StopAnimations(resetTransform: true);

            _oneShotOverrideActive = true;
            _oneShotRoutine = StartCoroutine(EnterStateOneShotRoutine(state));
        }

        private System.Collections.IEnumerator EnterStateOneShotRoutine(CounterState state)
        {
            ApplyColor(GetStateColor(state));

            float seconds = state == CounterState.Danger ? enterDangerSeconds : enterWarningSeconds;
            float punch = state == CounterState.Danger ? enterDangerPunchScale : enterWarningPunchScale;

            Coroutine shake = null;
            if (state == CounterState.Danger && enterDangerShakeAmplitude > 0.001f && enterDangerShakes >= 2)
            {
                shake = StartCoroutine(MotionUtil.ShakeLocalPosition(transform, _baseLocalPos, enterDangerShakeAmplitude, seconds, enterDangerShakes));
            }

            yield return StartCoroutine(MotionUtil.ScalePunch(transform, _baseScale, punch, seconds));

            if (shake != null)
            {
                transform.localPosition = _baseLocalPos;
            }

            _oneShotRoutine = null;
            _oneShotOverrideActive = false;
            ApplyStateVisuals(_state, immediate: true);
        }

        private void ApplyStateVisuals(CounterState state, bool immediate)
        {
            StopPulseLoop(resetTransform: true);

            ApplyColor(GetStateColor(state));

            if (state == CounterState.Normal)
            {
                if (immediate)
                {
                    transform.localScale = _baseScale;
                    transform.localPosition = _baseLocalPos;
                }
                return;
            }

            float pulseScale = state == CounterState.Danger ? dangerPulseScale : warningPulseScale;
            float period = state == CounterState.Danger ? dangerPulsePeriod : warningPulsePeriod;
            _pulseRoutine = StartCoroutine(PulseLoop(pulseScale, period));
        }

        private System.Collections.IEnumerator PulseLoop(float pulseScale, float periodSeconds)
        {
            pulseScale = Mathf.Max(0f, pulseScale);
            periodSeconds = Mathf.Max(0.05f, periodSeconds);

            float t = 0f;
            float pi2 = Mathf.PI * 2f;
            float startPhase = -Mathf.PI * 0.5f;

            while (true)
            {
                t += Time.unscaledDeltaTime;
                float phase = (t / periodSeconds) * pi2 + startPhase;
                float wave = Mathf.Sin(phase);
                float pulse01 = (wave + 1f) * 0.5f; // 0..1
                float s = 1f + pulseScale * pulse01;
                transform.localScale = _baseScale * s;
                yield return null;
            }
        }

        private void StopAnimations(bool resetTransform)
        {
            StopPulseLoop(resetTransform);

            if (_oneShotRoutine != null)
            {
                StopCoroutine(_oneShotRoutine);
                _oneShotRoutine = null;
            }
            _oneShotOverrideActive = false;

            if (resetTransform)
            {
                transform.localScale = _baseScale;
                transform.localPosition = _baseLocalPos;
                ApplyColor(GetStateColor(_state));
            }
        }

        private void StopPulseLoop(bool resetTransform)
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }

            if (resetTransform)
            {
                transform.localScale = _baseScale;
                transform.localPosition = _baseLocalPos;
            }
        }

        private void CaptureBasePoseAndColor(bool captureColor)
        {
            _baseScale = transform.localScale;
            _baseLocalPos = transform.localPosition;

            if (captureColor && !_baseColorCaptured)
            {
                if (_tmp != null) _baseColor = _tmp.color;
                else if (_text != null) _baseColor = _text.color;
                else _baseColor = Color.white;
                _baseColorCaptured = true;
            }
        }

        private Color GetStateColor(CounterState state)
        {
            return state switch
            {
                CounterState.Warning => warningColor,
                CounterState.Danger => dangerColor,
                _ => useExplicitNormalColor ? normalColor : _baseColor
            };
        }

        private void ApplyColor(Color color)
        {
            if (_tmp != null) _tmp.color = color;
            if (_text != null) _text.color = color;
        }
    }
}
