import argparse
import colorsys
import math
import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Dict, Iterable, List, Optional, Tuple

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont


RGBA = Tuple[int, int, int, int]
RGB = Tuple[int, int, int]


@dataclass(frozen=True)
class Palette:
    cream: RGB = (247, 235, 203)
    cream_hi: RGB = (255, 247, 231)
    brown: RGB = (106, 61, 23)
    navy: RGB = (11, 23, 48)

    mint: RGB = (72, 215, 180)
    purple: RGB = (142, 123, 255)
    orange: RGB = (243, 161, 43)
    pink: RGB = (255, 122, 165)
    red: RGB = (255, 90, 58)

    hud_dark: RGB = (38, 40, 56)


PAL = Palette()

_GRAD_CACHE: Dict[Tuple[int, int, bool], Image.Image] = {}
_LUT_SCALE_CACHE: Dict[int, List[int]] = {}
_LUT_THRESH_CACHE: Dict[int, List[int]] = {}


def clamp01(x: float) -> float:
    return max(0.0, min(1.0, x))


def mix(a: RGB, b: RGB, t: float) -> RGB:
    t = clamp01(t)
    return (
        int(round(a[0] + (b[0] - a[0]) * t)),
        int(round(a[1] + (b[1] - a[1]) * t)),
        int(round(a[2] + (b[2] - a[2]) * t)),
    )


def to_rgba(rgb: RGB, a: int) -> RGBA:
    return (rgb[0], rgb[1], rgb[2], int(a))


def darken(rgb: RGB, t: float) -> RGB:
    return mix(rgb, (0, 0, 0), clamp01(t))


def lighten(rgb: RGB, t: float) -> RGB:
    return mix(rgb, (255, 255, 255), clamp01(t))


def desaturate(rgb: RGB, t: float) -> RGB:
    r, g, b = [v / 255.0 for v in rgb]
    h, s, v = colorsys.rgb_to_hsv(r, g, b)
    s *= 1.0 - clamp01(t)
    r2, g2, b2 = colorsys.hsv_to_rgb(h, s, v)
    return (int(round(r2 * 255)), int(round(g2 * 255)), int(round(b2 * 255)))


def diagonal_gradient_l(size: Tuple[int, int], reverse: bool = False) -> Image.Image:
    # Fast diagonal gradient in pure Pillow:
    # diag(x,y) = (grad_x + grad_y) / 2.
    w, h = size
    if w <= 1 or h <= 1:
        return Image.new("L", size, 0 if not reverse else 255)

    key = (w, h, reverse)
    hit = _GRAD_CACHE.get(key)
    if hit is not None:
        return hit

    grad_y = Image.linear_gradient("L").resize((w, h), resample=Image.Resampling.BILINEAR)
    # Build a left-to-right gradient by rotating a vertical gradient.
    grad_x = (
        Image.linear_gradient("L")
        .resize((h, w), resample=Image.Resampling.BILINEAR)
        .transpose(Image.Transpose.ROTATE_270)
    )
    diag = ImageChops.add(grad_x, grad_y, scale=2.0)
    if reverse:
        diag = ImageChops.invert(diag)

    _GRAD_CACHE[key] = diag
    return diag


def mask_dilate(mask: Image.Image, r: int) -> Image.Image:
    r = max(0, int(r))
    if r == 0:
        return mask
    return mask.filter(ImageFilter.MaxFilter(r * 2 + 1))


def mask_erode(mask: Image.Image, r: int) -> Image.Image:
    r = max(0, int(r))
    if r == 0:
        return mask
    return mask.filter(ImageFilter.MinFilter(r * 2 + 1))


def _lut_scale(factor: float) -> List[int]:
    key = int(round(factor * 10000))
    hit = _LUT_SCALE_CACHE.get(key)
    if hit is not None:
        return hit
    lut = [max(0, min(255, int(round(i * factor)))) for i in range(256)]
    _LUT_SCALE_CACHE[key] = lut
    return lut


def safe_font_path() -> Optional[str]:
    candidates = [
        r"C:\Windows\Fonts\ARLRDBD.TTF",  # Arial Rounded MT Bold (often installed)
        r"C:\Windows\Fonts\COOPBL.TTF",  # Cooper Black (often installed)
        r"C:\Windows\Fonts\comicbd.ttf",
        r"C:\Windows\Fonts\trebucbd.ttf",
        r"C:\Windows\Fonts\arialbd.ttf",
    ]
    for p in candidates:
        if os.path.exists(p):
            return p
    return None


def fit_font(text: str, box: Tuple[int, int], font_path: Optional[str]) -> ImageFont.FreeTypeFont:
    w, h = box
    if not font_path:
        return ImageFont.load_default()

    # Binary-search font size.
    lo, hi = 8, max(10, int(h * 1.1))
    best = None
    while lo <= hi:
        mid = (lo + hi) // 2
        f = ImageFont.truetype(font_path, mid)
        bbox = f.getbbox(text)
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        if tw <= w and th <= h:
            best = f
            lo = mid + 1
        else:
            hi = mid - 1
    return best or ImageFont.truetype(font_path, max(8, int(h * 0.8)))


def rounded_rect_mask(size: Tuple[int, int], bbox: Tuple[int, int, int, int], radius: int) -> Image.Image:
    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    draw.rounded_rectangle(list(bbox), radius=radius, fill=255)
    return mask


def apply_noise(img: Image.Image, alpha: int, sigma: float, seed: int) -> Image.Image:
    if alpha <= 0:
        return img
    w, h = img.size
    noise = Image.effect_noise((w, h), sigma).convert("L")
    # Center around 128
    noise = ImageChops.add(noise, Image.new("L", (w, h), 128), scale=2.0)
    noise_rgba = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    noise_rgba.putalpha(noise.point(_lut_scale(alpha / 255.0)))
    return Image.alpha_composite(img, noise_rgba)


def paint_plastic_panel(
    size: Tuple[int, int],
    shape_bbox: Tuple[int, int, int, int],
    base: RGB,
    state: str,
    *,
    scale: int,
    force_radius: Optional[int] = None,
) -> Image.Image:
    w, h = size
    W, H = w * scale, h * scale
    x0, y0, x1, y1 = [v * scale for v in shape_bbox]
    sw, sh = max(1, x1 - x0), max(1, y1 - y0)
    radius = force_radius if force_radius is not None else int(round(min(sw, sh) * 0.22))

    # State tweaks.
    shadow_alpha = 110
    shadow_blur = int(round(min(sw, sh) * 0.035))
    shadow_dx = int(round(min(sw, sh) * 0.015))
    shadow_dy = int(round(min(sw, sh) * 0.022))

    highlight_alpha = 46
    inner_shadow_alpha = 70

    base_fill = base
    if state.lower() == "pressed":
        base_fill = darken(base, 0.10)
        shadow_alpha = 75
        shadow_blur = int(round(shadow_blur * 0.75))
        shadow_dx = int(round(shadow_dx * 0.55))
        shadow_dy = int(round(shadow_dy * 0.55))
        highlight_alpha = 26
        inner_shadow_alpha = 85
    elif state.lower() == "disabled":
        base_fill = lighten(desaturate(base, 0.55), 0.10)
        shadow_alpha = 45
        highlight_alpha = 18
        inner_shadow_alpha = 55

    outline = darken(base_fill, 0.62)
    rim = lighten(base_fill, 0.38)

    mask = rounded_rect_mask((W, H), (x0, y0, x1, y1), radius)

    # Canvas.
    canvas = Image.new("RGBA", (W, H), (0, 0, 0, 0))

    # Drop shadow.
    shadow = mask.filter(ImageFilter.GaussianBlur(max(1, shadow_blur)))
    shadow_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    shadow_layer.putalpha(shadow.point(_lut_scale(shadow_alpha / 255.0)))
    shadow_layer = ImageChops.offset(shadow_layer, shadow_dx, shadow_dy)
    canvas = Image.alpha_composite(canvas, shadow_layer)

    # Fill gradient.
    light = lighten(base_fill, 0.35)
    dark = darken(base_fill, 0.22)
    grad = diagonal_gradient_l((W, H), reverse=False)
    fill = Image.composite(Image.new("RGBA", (W, H), to_rgba(dark, 255)), Image.new("RGBA", (W, H), to_rgba(light, 255)), grad)
    fill.putalpha(mask)
    canvas = Image.alpha_composite(canvas, fill)

    # Stroke (thick).
    stroke_w = max(2, int(round(min(sw, sh) * 0.055)))
    stroke = Image.new("L", (W, H), 0)
    stroke_draw = ImageDraw.Draw(stroke)
    stroke_draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, outline=255, width=stroke_w)
    stroke_layer = Image.new("RGBA", (W, H), to_rgba(outline, 255))
    stroke_layer.putalpha(stroke)
    canvas = Image.alpha_composite(canvas, stroke_layer)

    # Inner rim.
    rim_w = max(1, int(round(stroke_w * 0.55)))
    rim_mask = Image.new("L", (W, H), 0)
    rim_draw = ImageDraw.Draw(rim_mask)
    inset = max(1, int(round(stroke_w * 0.60)))
    rx0, ry0, rx1, ry1 = x0 + inset, y0 + inset, x1 - inset, y1 - inset
    if rx1 <= rx0 + 2 or ry1 <= ry0 + 2:
        rx0, ry0, rx1, ry1 = x0, y0, x1, y1
    rim_radius = max(1, radius - inset)
    rim_draw.rounded_rectangle([rx0, ry0, rx1, ry1], radius=rim_radius, outline=255, width=rim_w)
    rim_grad = diagonal_gradient_l((W, H), reverse=True)
    rim_alpha = ImageChops.multiply(rim_mask, rim_grad).point(_lut_scale(0.55))
    rim_layer = Image.new("RGBA", (W, H), to_rgba(rim, 255))
    rim_layer.putalpha(rim_alpha)
    canvas = Image.alpha_composite(canvas, rim_layer)

    # Inner shadow.
    ao_w = max(2, int(round(min(sw, sh) * 0.040)))
    ao = Image.new("L", (W, H), 0)
    ao_draw = ImageDraw.Draw(ao)
    ao_inset = max(1, int(round(stroke_w * 0.90)))
    ax0, ay0, ax1, ay1 = x0 + ao_inset, y0 + ao_inset, x1 - ao_inset, y1 - ao_inset
    if ax1 <= ax0 + 2 or ay1 <= ay0 + 2:
        ax0, ay0, ax1, ay1 = x0, y0, x1, y1
    ao_radius = max(1, radius - ao_inset)
    ao_draw.rounded_rectangle([ax0, ay0, ax1, ay1], radius=ao_radius, outline=255, width=ao_w)
    ao = ao.filter(ImageFilter.GaussianBlur(max(1, int(round(ao_w * 0.55)))))
    ao_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    ao_layer.putalpha(ao.point(_lut_scale(inner_shadow_alpha / 255.0)))
    ao_layer = ImageChops.offset(ao_layer, int(round(ao_w * 0.12)), int(round(ao_w * 0.18)))
    ao_layer.putalpha(ImageChops.multiply(ao_layer.getchannel("A"), mask))
    canvas = Image.alpha_composite(canvas, ao_layer)

    # Specular highlight band.
    hl = Image.new("L", (W, H), 0)
    hl_draw = ImageDraw.Draw(hl)
    hl_w = int(round(sw * 1.05))
    hl_h = int(round(sh * 0.62))
    hl_x0 = x0 - int(round(sw * 0.10))
    hl_y0 = y0 - int(round(sh * 0.25))
    hl_draw.ellipse([hl_x0, hl_y0, hl_x0 + hl_w, hl_y0 + hl_h], fill=255)
    hl = hl.filter(ImageFilter.GaussianBlur(int(round(min(sw, sh) * 0.06))))
    hl = ImageChops.multiply(hl, mask).point(_lut_scale(highlight_alpha / 255.0))
    hl_layer = Image.new("RGBA", (W, H), to_rgba((255, 255, 255), 255))
    hl_layer.putalpha(hl)
    canvas = Image.alpha_composite(canvas, hl_layer)

    # Subtle grain.
    canvas = apply_noise(canvas, alpha=10 if state.lower() != "disabled" else 6, sigma=12.0, seed=0)

    # Downscale.
    out = canvas.resize((w, h), resample=Image.Resampling.LANCZOS)
    return out


def paint_digit(size: Tuple[int, int], digit: str) -> Image.Image:
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    fp = safe_font_path()
    pad_x = int(round(w * 0.12))
    pad_y = int(round(h * 0.10))
    font = fit_font(digit, (w - pad_x * 2, h - pad_y * 2), fp)

    # Measure.
    bbox = font.getbbox(digit)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    x = (w - tw) // 2 - bbox[0]
    y = (h - th) // 2 - bbox[1]

    outline_w = max(2, int(round(min(w, h) * 0.10)))
    shadow_off = max(1, int(round(min(w, h) * 0.05)))

    # Shadow.
    draw.text((x + shadow_off, y + shadow_off), digit, font=font, fill=(0, 0, 0, 90), stroke_width=outline_w, stroke_fill=(0, 0, 0, 90))
    # Main.
    draw.text((x, y), digit, font=font, fill=to_rgba(PAL.cream_hi, 255), stroke_width=outline_w, stroke_fill=to_rgba(PAL.brown, 255))

    # Tiny highlight.
    draw.text((x - 1, y - 1), digit, font=font, fill=(255, 255, 255, 55))

    return img


def _rot_rect_polygon(cx: float, cy: float, w: float, h: float, degrees: float) -> List[Tuple[float, float]]:
    ang = math.radians(degrees)
    ca, sa = math.cos(ang), math.sin(ang)
    dx, dy = w / 2.0, h / 2.0
    pts = [(-dx, -dy), (dx, -dy), (dx, dy), (-dx, dy)]
    out = []
    for px, py in pts:
        rx = px * ca - py * sa
        ry = px * sa + py * ca
        out.append((cx + rx, cy + ry))
    return out


def paint_icon(size: Tuple[int, int], subject: str, *, scale: int) -> Image.Image:
    w, h = size
    W, H = w * scale, h * scale
    subject = subject.strip().lower()

    mask = Image.new("L", (W, H), 0)
    draw = ImageDraw.Draw(mask)

    cx, cy = W * 0.5, H * 0.5
    s = min(W, H)

    def rr(b, r):
        draw.rounded_rectangle(b, radius=r, fill=255)

    def circ(x0, y0, x1, y1):
        draw.ellipse([x0, y0, x1, y1], fill=255)

    def poly(points):
        draw.polygon(points, fill=255)

    def line(p1, p2, width):
        draw.line([p1, p2], fill=255, width=width)
        r = width / 2.0
        circ(p1[0] - r, p1[1] - r, p1[0] + r, p1[1] + r)
        circ(p2[0] - r, p2[1] - r, p2[0] + r, p2[1] + r)

    # Common widths.
    thick = max(6, int(round(s * 0.10)))
    medium = max(4, int(round(s * 0.075)))

    # Shape dispatch.
    if subject == "plus":
        bw = thick
        bl = s * 0.72
        rr([cx - bw / 2, cy - bl / 2, cx + bw / 2, cy + bl / 2], bw / 2)
        rr([cx - bl / 2, cy - bw / 2, cx + bl / 2, cy + bw / 2], bw / 2)
    elif subject in ("close", "x"):
        bar_w = thick
        bar_l = s * 0.84
        poly(_rot_rect_polygon(cx, cy, bar_l, bar_w, 45))
        poly(_rot_rect_polygon(cx, cy, bar_l, bar_w, -45))
    elif subject == "pause":
        bw = thick
        gap = bw * 0.65
        bh = s * 0.78
        rr([cx - gap - bw, cy - bh / 2, cx - gap, cy + bh / 2], bw / 2)
        rr([cx + gap, cy - bh / 2, cx + gap + bw, cy + bh / 2], bw / 2)
    elif subject == "video":
        # play in rounded rect
        rw, rh = s * 0.86, s * 0.62
        rr([cx - rw / 2, cy - rh / 2, cx + rw / 2, cy + rh / 2], rh * 0.35)
        tri_w = s * 0.26
        tri_h = s * 0.28
        poly([(cx - tri_w * 0.35, cy - tri_h / 2), (cx - tri_w * 0.35, cy + tri_h / 2), (cx + tri_w * 0.65, cy)])
    elif subject == "next":
        tri_w = s * 0.32
        tri_h = s * 0.34
        poly([(cx - tri_w * 0.55, cy - tri_h / 2), (cx - tri_w * 0.55, cy + tri_h / 2), (cx + tri_w * 0.55, cy)])
        bw = thick * 0.65
        rr([cx + tri_w * 0.62, cy - tri_h / 2, cx + tri_w * 0.62 + bw, cy + tri_h / 2], bw / 2)
    elif subject == "retry":
        r = s * 0.36
        bbox = [cx - r, cy - r, cx + r, cy + r]
        draw.arc(bbox, start=35, end=320, fill=255, width=thick)
        # arrow head (top-right)
        ah = s * 0.18
        poly([(cx + r * 0.70, cy - r * 0.92), (cx + r * 1.02, cy - r * 0.64), (cx + r * 0.62, cy - r * 0.58)])
    elif subject == "loop":
        r = s * 0.34
        bbox = [cx - r, cy - r, cx + r, cy + r]
        draw.arc(bbox, start=40, end=320, fill=255, width=thick)
        ah = s * 0.18
        poly([(cx + r * 0.72, cy - r * 0.88), (cx + r * 1.03, cy - r * 0.60), (cx + r * 0.62, cy - r * 0.56)])
    elif subject == "clock":
        r = s * 0.40
        circ(cx - r, cy - r, cx + r, cy + r)
        # punch hole
        hole = Image.new("L", (W, H), 0)
        hole_draw = ImageDraw.Draw(hole)
        hole_r = r * 0.70
        hole_draw.ellipse([cx - hole_r, cy - hole_r, cx + hole_r, cy + hole_r], fill=255)
        mask = ImageChops.subtract(mask, hole)
        draw = ImageDraw.Draw(mask)
        # hands
        line((cx, cy), (cx, cy - r * 0.38), medium)
        line((cx, cy), (cx + r * 0.28, cy + r * 0.12), medium)
    elif subject == "coin":
        r = s * 0.40
        circ(cx - r, cy - r, cx + r, cy + r)
        inner_r = r * 0.62
        hole = Image.new("L", (W, H), 0)
        ImageDraw.Draw(hole).ellipse([cx - inner_r, cy - inner_r, cx + inner_r, cy + inner_r], fill=255)
        mask = ImageChops.subtract(mask, hole)
        draw = ImageDraw.Draw(mask)
        # center dot
        circ(cx - r * 0.12, cy - r * 0.12, cx + r * 0.12, cy + r * 0.12)
    elif subject == "coin stack":
        r = s * 0.32
        for i, dy in enumerate([r * 0.55, 0, -r * 0.55]):
            circ(cx - r, cy - r + dy, cx + r, cy + r + dy)
    elif subject == "heart":
        r = s * 0.22
        circ(cx - r * 1.15, cy - r * 0.65, cx + r * 0.05, cy + r * 0.55)
        circ(cx - r * 0.05, cy - r * 0.65, cx + r * 1.15, cy + r * 0.55)
        poly([(cx - r * 1.55, cy - r * 0.10), (cx + r * 1.55, cy - r * 0.10), (cx, cy + r * 1.85)])
    elif subject == "lock":
        bw, bh = s * 0.62, s * 0.52
        rr([cx - bw / 2, cy - bh / 2 + s * 0.12, cx + bw / 2, cy + bh / 2 + s * 0.12], bh * 0.20)
        # shackle
        sh_r = bw * 0.42
        sh = Image.new("L", (W, H), 0)
        shd = ImageDraw.Draw(sh)
        shd.ellipse([cx - sh_r, cy - sh_r - s * 0.05, cx + sh_r, cy + sh_r - s * 0.05], fill=255)
        inner = Image.new("L", (W, H), 0)
        inn = ImageDraw.Draw(inner)
        inn.ellipse([cx - sh_r * 0.64, cy - sh_r * 0.64 - s * 0.05, cx + sh_r * 0.64, cy + sh_r * 0.64 - s * 0.05], fill=255)
        sh = ImageChops.subtract(sh, inner)
        mask = ImageChops.add(mask, sh)
        draw = ImageDraw.Draw(mask)
    elif subject == "gear":
        r = s * 0.30
        circ(cx - r, cy - r, cx + r, cy + r)
        tooth_w, tooth_h = s * 0.14, s * 0.18
        for ang in range(0, 360, 45):
            poly(_rot_rect_polygon(cx + math.cos(math.radians(ang)) * r * 1.12, cy + math.sin(math.radians(ang)) * r * 1.12, tooth_w, tooth_h, ang))
        # hole
        hole = Image.new("L", (W, H), 0)
        ImageDraw.Draw(hole).ellipse([cx - r * 0.45, cy - r * 0.45, cx + r * 0.45, cy + r * 0.45], fill=255)
        mask = ImageChops.subtract(mask, hole)
        draw = ImageDraw.Draw(mask)
    elif subject == "shop":
        bw, bh = s * 0.74, s * 0.60
        rr([cx - bw / 2, cy - bh / 2 + s * 0.10, cx + bw / 2, cy + bh / 2 + s * 0.10], bh * 0.12)
        # roof
        roof_h = s * 0.20
        poly([(cx - bw / 2, cy - bh / 2 + s * 0.10), (cx + bw / 2, cy - bh / 2 + s * 0.10), (cx + bw * 0.40, cy - bh / 2 - roof_h), (cx - bw * 0.40, cy - bh / 2 - roof_h)])
    elif subject == "shuffle":
        # two crossing arrows
        y1 = cy - s * 0.12
        y2 = cy + s * 0.12
        xL = cx - s * 0.38
        xR = cx + s * 0.38
        line((xL, y1), (xR - s * 0.10, y2), medium)
        line((xL, y2), (xR - s * 0.10, y1), medium)
        # arrowheads
        ah = s * 0.14
        poly([(xR, y2), (xR - ah, y2 - ah * 0.55), (xR - ah, y2 + ah * 0.55)])
        poly([(xR, y1), (xR - ah, y1 - ah * 0.55), (xR - ah, y1 + ah * 0.55)])
    elif subject == "sort":
        # up/down arrows
        x = cx
        y_top = cy - s * 0.30
        y_bot = cy + s * 0.30
        line((x, y_top + s * 0.12), (x, y_bot - s * 0.12), medium)
        ah = s * 0.16
        poly([(x, y_top), (x - ah * 0.55, y_top + ah), (x + ah * 0.55, y_top + ah)])
        poly([(x, y_bot), (x - ah * 0.55, y_bot - ah), (x + ah * 0.55, y_bot - ah)])
    elif subject == "music":
        r = s * 0.16
        circ(cx - s * 0.18 - r, cy + s * 0.18 - r, cx - s * 0.18 + r, cy + s * 0.18 + r)
        circ(cx + s * 0.18 - r, cy + s * 0.10 - r, cx + s * 0.18 + r, cy + s * 0.10 + r)
        line((cx + s * 0.18, cy - s * 0.30), (cx + s * 0.18, cy + s * 0.10), medium)
        line((cx + s * 0.18, cy - s * 0.30), (cx - s * 0.18, cy - s * 0.20), medium)
    elif subject == "vibrate":
        bw, bh = s * 0.52, s * 0.76
        rr([cx - bw / 2, cy - bh / 2, cx + bw / 2, cy + bh / 2], bw * 0.18)
        wave_w = medium
        line((cx - bw / 2 - s * 0.10, cy - s * 0.18), (cx - bw / 2 - s * 0.16, cy - s * 0.30), wave_w)
        line((cx - bw / 2 - s * 0.10, cy + s * 0.18), (cx - bw / 2 - s * 0.16, cy + s * 0.30), wave_w)
        line((cx + bw / 2 + s * 0.10, cy - s * 0.18), (cx + bw / 2 + s * 0.16, cy - s * 0.30), wave_w)
        line((cx + bw / 2 + s * 0.10, cy + s * 0.18), (cx + bw / 2 + s * 0.16, cy + s * 0.30), wave_w)
    else:
        # Fallback: rounded square glyph.
        rr([W * 0.18, H * 0.18, W * 0.82, H * 0.82], s * 0.14)

    # Style: thick outline + warm fill + tiny shadow.
    stroke_w = max(2, int(round(s * 0.06)))
    outer = mask_dilate(mask, stroke_w)
    inner = mask_erode(mask, stroke_w)
    stroke = ImageChops.subtract(outer, inner)

    fill_rgb = PAL.cream_hi
    outline_rgb = PAL.brown

    canvas = Image.new("RGBA", (W, H), (0, 0, 0, 0))

    # Shadow (behind glyph).
    sh = mask.filter(ImageFilter.GaussianBlur(max(1, int(round(s * 0.03)))))
    sh_layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    sh_layer.putalpha(sh.point(_lut_scale(0.45)))
    sh_layer = ImageChops.offset(sh_layer, int(round(s * 0.01)), int(round(s * 0.02)))
    canvas = Image.alpha_composite(canvas, sh_layer)

    # Outline then fill.
    outline_layer = Image.new("RGBA", (W, H), to_rgba(outline_rgb, 255))
    outline_layer.putalpha(stroke)
    canvas = Image.alpha_composite(canvas, outline_layer)

    # Fill with subtle diagonal shading.
    grad = diagonal_gradient_l((W, H), reverse=False)
    fill_light = fill_rgb
    fill_dark = darken(fill_rgb, 0.16)
    fill = Image.composite(Image.new("RGBA", (W, H), to_rgba(fill_dark, 255)), Image.new("RGBA", (W, H), to_rgba(fill_light, 255)), grad)
    fill.putalpha(mask)
    canvas = Image.alpha_composite(canvas, fill)

    # Tiny specular highlight.
    hl = Image.new("L", (W, H), 0)
    hl_draw = ImageDraw.Draw(hl)
    hl_draw.ellipse([W * 0.08, H * 0.06, W * 0.72, H * 0.55], fill=255)
    hl = hl.filter(ImageFilter.GaussianBlur(int(round(s * 0.08))))
    hl = ImageChops.multiply(hl, mask).point(_lut_scale(0.10))
    hl_layer = Image.new("RGBA", (W, H), (255, 255, 255, 255))
    hl_layer.putalpha(hl)
    canvas = Image.alpha_composite(canvas, hl_layer)

    out = canvas.resize((w, h), resample=Image.Resampling.LANCZOS)
    return out


def parse_prompt_sheet_files(prompt_sheet_path: Path) -> List[str]:
    if prompt_sheet_path.suffix.lower() == ".json":
        try:
            from PromptDbLib import PromptDb
        except Exception as e:
            raise RuntimeError(f"Cannot read prompt db json: {e}") from e
        db = PromptDb.load(prompt_sheet_path)
        files = []
        for key, item in db.items.items():
            dir_name = str(item.get("dir") or "").strip() or key.split("/", 1)[0]
            filename = str(item.get("filename") or "").strip() or key.split("/", 1)[1]
            if dir_name == "UI_Sprites":
                files.append(filename)
        if not files:
            raise RuntimeError(f"No UI_Sprites entries found in prompt db: {prompt_sheet_path}")
        return files

    text = prompt_sheet_path.read_text(encoding="utf-8-sig")
    files = re.findall(r"^## UI_Sprites/([^ ]+)", text, flags=re.M)
    if not files:
        raise RuntimeError(f"No files found in prompt sheet: {prompt_sheet_path}")
    return files


def main() -> int:
    ap = argparse.ArgumentParser(description="Generate procedural v0.5 HUD PNGs (creamy plastic) as a starter pack.")
    ap.add_argument("--kit-root", default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti")
    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_db_all_v05.json")
    ap.add_argument("--out-dir", default="Tools/UiRestyleV05/_generated_v05")
    ap.add_argument("--scale", type=int, default=4)
    args = ap.parse_args()

    kit_root = Path(args.kit_root)
    ui_dir = kit_root / "UI_Sprites"
    if not ui_dir.exists():
        raise SystemExit(f"UI_Sprites not found: {ui_dir}")

    prompt_sheet = Path(args.prompt_sheet)
    if not prompt_sheet.exists():
        raise SystemExit(f"Prompt sheet not found: {prompt_sheet}")

    out_dir = Path(args.out_dir) / "UI_Sprites"
    out_dir.mkdir(parents=True, exist_ok=True)

    files = parse_prompt_sheet_files(prompt_sheet)

    # Do not derive bbox from existing images (avoid bbox-based fitting/cropping logic).
    # Use a simple padding rule instead.
    generated = 0
    for name in files:
        src = ui_dir / name
        if not src.exists():
            print(f"[skip] missing source: {src}")
            continue

        src_img = Image.open(src).convert("RGBA")
        w, h = src_img.size
        pad_x = int(round(w * 0.08))
        pad_y = int(round(h * 0.08))
        bbox = (pad_x, pad_y, max(pad_x + 1, w - pad_x), max(pad_y + 1, h - pad_y))

        out_path = out_dir / name

        # Classify.
        lower = name.lower()
        if lower.startswith("digit_") and lower.endswith(".png"):
            digit = lower.replace("digit_", "").replace(".png", "")
            img = paint_digit((w, h), digit)
        elif lower.startswith("icon_"):
            subj = lower.replace("icon_", "")
            while subj.endswith(".png"):
                subj = subj[: -len(".png")]
            subj = subj.replace("_128", "")
            subj = subj.replace("_", " ")
            img = paint_icon((w, h), subj, scale=args.scale)
        elif lower.startswith("tag_fast_"):
            base = PAL.mint if "info" in lower else PAL.red
            img = paint_plastic_panel((w, h), bbox, base, "normal", scale=args.scale, force_radius=int(round(((bbox[3] - bbox[1]) * args.scale) * 0.52)))
        elif lower.startswith("hud_pill_dark"):
            img = paint_plastic_panel((w, h), bbox, PAL.hud_dark, "normal", scale=args.scale, force_radius=int(round(((bbox[3] - bbox[1]) * args.scale) * 0.55)))
        elif lower == "hud_level_label_bg.png":
            img = paint_plastic_panel((w, h), bbox, PAL.cream, "normal", scale=args.scale, force_radius=int(round(((bbox[3] - bbox[1]) * args.scale) * 0.55)))
        elif "_square_" in lower:
            state = "normal"
            if "pressed" in lower:
                state = "pressed"
            elif "disabled" in lower:
                state = "disabled"

            role = lower.split("_square_")[0]
            role_color = getattr(PAL, role, PAL.mint)
            img = paint_plastic_panel((w, h), bbox, role_color, state, scale=args.scale)
        elif lower.startswith("badge_red_bg"):
            # Paint as a small round badge.
            base = PAL.red
            img = paint_plastic_panel((w, h), bbox, base, "normal", scale=args.scale, force_radius=int(round(min((bbox[2] - bbox[0]), (bbox[3] - bbox[1])) * args.scale * 0.50)))
        else:
            # Generic plastic panel.
            img = paint_plastic_panel((w, h), bbox, PAL.cream, "normal", scale=args.scale)

        img.save(out_path, format="PNG")
        generated += 1

    print(f"Generated {generated} files into: {out_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
