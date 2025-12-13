import math
import os
import random
import struct
import uuid
import wave


SR = 44100


def _clamp01(x: float) -> float:
    if x < 0.0:
        return 0.0
    if x > 1.0:
        return 1.0
    return x


def _write_wav_mono_16(path: str, samples: list[float]) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(SR)
        frames = bytearray()
        for s in samples:
            s = max(-1.0, min(1.0, s))
            frames += struct.pack("<h", int(s * 32767.0))
        wf.writeframes(frames)


def _env_exp(t: float, tau: float) -> float:
    return math.exp(-t / max(1e-6, tau))


def _sine(t: float, f: float) -> float:
    return math.sin(2.0 * math.pi * f * t)


def _chirp(t: float, f0: float, f1: float, dur: float) -> float:
    u = _clamp01(t / max(1e-6, dur))
    f = f0 + (f1 - f0) * u
    return _sine(t, f)


def _noise() -> float:
    return random.uniform(-1.0, 1.0)


def _make(dur: float, fn) -> list[float]:
    n = int(dur * SR)
    out = [0.0] * n
    for i in range(n):
        t = i / SR
        out[i] = fn(t)
    return out


def _soft_clip(x: float) -> float:
    # Cheap tanh-ish.
    return x / (1.0 + abs(x))


def sfx_ui_click() -> list[float]:
    dur = 0.06

    def f(t: float) -> float:
        e = _env_exp(t, 0.018)
        tone = _sine(t, 1600.0) * 0.9 + _sine(t, 2400.0) * 0.4
        hiss = _noise() * 0.5
        return _soft_clip((tone * 0.7 + hiss * 0.3) * e) * 0.55

    return _make(dur, f)


def sfx_box_tap() -> list[float]:
    dur = 0.12

    def f(t: float) -> float:
        e = _env_exp(t, 0.06)
        body = _sine(t, 260.0) * 1.0 + _sine(t, 520.0) * 0.35
        click = _sine(t, 1800.0) * _env_exp(t, 0.01)
        return _soft_clip((body * 0.8 + click * 0.4) * e) * 0.65

    return _make(dur, f)


def sfx_block_release() -> list[float]:
    dur = 0.08

    def f(t: float) -> float:
        e = _env_exp(t, 0.03)
        pop = _sine(t, 900.0) * 0.9 + _sine(t, 1200.0) * 0.35
        return _soft_clip(pop * e) * 0.6

    return _make(dur, f)


def sfx_block_enter() -> list[float]:
    dur = 0.09

    def f(t: float) -> float:
        e = _env_exp(t, 0.035)
        thunk = _sine(t, 520.0) * 1.0 + _sine(t, 780.0) * 0.25
        snap = _sine(t, 2100.0) * _env_exp(t, 0.012)
        return _soft_clip((thunk * 0.75 + snap * 0.35) * e) * 0.7

    return _make(dur, f)


def sfx_box_complete() -> list[float]:
    dur = 0.35
    notes = [659.25, 783.99, 987.77]  # E5, G5, B5-ish

    def f(t: float) -> float:
        e = _env_exp(t, 0.22)
        idx = 0 if t < dur * 0.33 else (1 if t < dur * 0.66 else 2)
        tone = _sine(t, notes[idx]) + _sine(t, notes[idx] * 2.0) * 0.15
        sparkle = _noise() * 0.12 * _env_exp(t, 0.06)
        return _soft_clip((tone * 0.9 + sparkle) * e) * 0.75

    return _make(dur, f)


def sfx_unlock() -> list[float]:
    dur = 0.42

    def f(t: float) -> float:
        e = _env_exp(t, 0.35)
        chirp = _chirp(t, 420.0, 1400.0, dur) * 0.9
        bell = _sine(t, 1760.0) * _env_exp(t, 0.12) * 0.25
        return _soft_clip((chirp + bell) * e) * 0.75

    return _make(dur, f)


def sfx_fast_forward() -> list[float]:
    dur = 0.18

    def f(t: float) -> float:
        u = _clamp01(t / dur)
        e = math.sin(u * math.pi) ** 1.5
        sweep = _chirp(t, 1200.0, 300.0, dur) * 0.35
        whoosh = _noise() * 0.85
        return _soft_clip((whoosh * 0.7 + sweep) * e) * 0.45

    return _make(dur, f)


def sfx_win() -> list[float]:
    dur = 0.75
    chord = [523.25, 659.25, 783.99]  # C5 E5 G5

    def f(t: float) -> float:
        e = _env_exp(t, 0.45)
        tone = sum(_sine(t, f0) for f0 in chord) / len(chord)
        bright = sum(_sine(t, f0 * 2.0) for f0 in chord) / len(chord) * 0.12
        return _soft_clip((tone + bright) * e) * 0.75

    return _make(dur, f)


def sfx_fail() -> list[float]:
    dur = 0.75

    def f(t: float) -> float:
        e = _env_exp(t, 0.38)
        tone = _chirp(t, 420.0, 160.0, dur) * 0.9
        buzz = _sine(t, 70.0) * 0.25
        return _soft_clip((tone + buzz) * e) * 0.75

    return _make(dur, f)


def sfx_error() -> list[float]:
    dur = 0.22

    def f(t: float) -> float:
        e = _env_exp(t, 0.1)
        buzz = _sine(t, 110.0) * 0.85 + _sine(t, 220.0) * 0.25
        return _soft_clip(buzz * e) * 0.55

    return _make(dur, f)


def sfx_booster() -> list[float]:
    dur = 0.38

    def f(t: float) -> float:
        e = _env_exp(t, 0.28)
        sparkle = _chirp(t, 800.0, 1800.0, dur) * 0.55 + _chirp(t, 1200.0, 2400.0, dur) * 0.35
        shimmer = _noise() * 0.15 * _env_exp(t, 0.08)
        return _soft_clip((sparkle + shimmer) * e) * 0.7

    return _make(dur, f)


def _write_meta_audio(path_meta: str, guid_hex: str) -> None:
    # Minimal AudioImporter meta; Unity may rewrite on first import.
    txt = "\n".join(
        [
            "fileFormatVersion: 2",
            f"guid: {guid_hex}",
            "AudioImporter:",
            "  externalObjects: {}",
            "  serializedVersion: 2",
            "  defaultSettings:",
            "    loadType: 0",
            "    sampleRateSetting: 0",
            "    sampleRateOverride: 44100",
            "    compressionFormat: 1",
            "    quality: 1",
            "    conversionMode: 0",
            "  userData: ",
            "  assetBundleName: ",
            "  assetBundleVariant: ",
            "",
        ]
    )
    with open(path_meta, "w", encoding="utf-8", newline="\n") as f:
        f.write(txt)


def _write_meta_folder(path_meta: str, guid_hex: str) -> None:
    txt = "\n".join(
        [
            "fileFormatVersion: 2",
            f"guid: {guid_hex}",
            "folderAsset: yes",
            "DefaultImporter:",
            "  externalObjects: {}",
            "  userData: ",
            "  assetBundleName: ",
            "  assetBundleVariant: ",
            "",
        ]
    )
    with open(path_meta, "w", encoding="utf-8", newline="\n") as f:
        f.write(txt)


def _ensure_folder_with_meta(folder: str) -> None:
    os.makedirs(folder, exist_ok=True)
    meta_path = folder.rstrip("/\\") + ".meta"
    if not os.path.exists(meta_path):
        _write_meta_folder(meta_path, uuid.uuid4().hex)


def main() -> None:
    random.seed(0xC0FFEE)
    root = os.path.join("Assets", "Resources", "Audio")
    sfx_dir = os.path.join(root, "SFX")
    _ensure_folder_with_meta(os.path.join("Assets", "Resources", "Audio"))
    _ensure_folder_with_meta(sfx_dir)

    items = {
        "ui_click": sfx_ui_click,
        "box_tap": sfx_box_tap,
        "block_release": sfx_block_release,
        "block_enter": sfx_block_enter,
        "box_complete": sfx_box_complete,
        "unlock": sfx_unlock,
        "fast_forward": sfx_fast_forward,
        "win": sfx_win,
        "fail": sfx_fail,
        "error": sfx_error,
        "booster": sfx_booster,
    }

    for name, fn in items.items():
        wav_path = os.path.join(sfx_dir, f"{name}.wav")
        meta_path = wav_path + ".meta"
        guid_hex = uuid.uuid4().hex
        samples = fn()
        _write_wav_mono_16(wav_path, samples)
        _write_meta_audio(meta_path, guid_hex)
        print(f"Wrote {wav_path} (+.meta)")


if __name__ == "__main__":
    main()

