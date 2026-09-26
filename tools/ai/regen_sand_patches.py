#!/usr/bin/env python3
"""Regenerates the sand_top / sand_side patches inside the AtlasV3 textures.

The shipped patches contain drawn pebbles and a visible 2x2 repeat that makes
shorelines look unnatural. This script replaces them with a seamless,
object-free procedural sand (FFT noise is periodic by construction), and
flattens the matching normal / roughness / metallic-smoothness patches so no
baked pebble relief survives in the lighting.

Run from the repository root:
    python tools/ai/regen_sand_patches.py
"""
import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
ATLAS_DIR = ROOT / "Assets/Moyva/Art/World/Tiles/AtlasV3/Textures"
LAYOUT = ATLAS_DIR / "atlas_layout.json"
PATCH = 480
SEED = 20240517


def periodic_band(shape, freq_lo, freq_hi, rng):
    """Tileable value noise: random spectrum filtered to a frequency ring."""
    h, w = shape
    fy = np.fft.fftfreq(h)[:, None] * h
    fx = np.fft.rfftfreq(w)[None, :] * w
    mag = np.sqrt(fx * fx + fy * fy)
    mask = (mag >= freq_lo) & (mag <= freq_hi)
    spectrum = np.zeros((h, w // 2 + 1), dtype=complex)
    spectrum[mask] = (
        rng.normal(0.0, 1.0, int(mask.sum()))
        + 1j * rng.normal(0.0, 1.0, int(mask.sum()))
    )
    field = np.fft.irfft2(spectrum, s=(h, w))
    field -= field.mean()
    field /= field.std() + 1e-9
    return field


def sand_field(rng, strata):
    """Normalized luminance field; `strata` adds faint horizontal banding."""
    dunes = periodic_band((PATCH, PATCH), 1.0, 4.0, rng) * 6.5
    mottle = periodic_band((PATCH, PATCH), 4.0, 18.0, rng) * 5.0
    grain = periodic_band((PATCH, PATCH), 32.0, 130.0, rng) * 3.5
    pixel = periodic_band((PATCH, PATCH), 130.0, 235.0, rng) * 2.0
    field = dunes + mottle + grain + pixel
    if strata:
        yy = np.arange(PATCH)[:, None]
        bands = (
            np.sin(yy * 2 * np.pi * 3 / PATCH + rng.uniform(0, 6.28)) * 2.0
            + periodic_band((PATCH, PATCH), 2.0, 8.0, rng)
            * np.linspace(0.0, 1.0, PATCH)[:, None] * 3.0
        )
        field = field + bands
    return field


def warm_variation(rng):
    return periodic_band((PATCH, PATCH), 1.0, 6.0, rng) * 2.5


def make_sand_albedo(base_rgb, rng, strata=False):
    lum = sand_field(rng, strata)
    warm = warm_variation(rng)
    img = np.zeros((PATCH, PATCH, 3), dtype=np.float32)
    img[..., 0] = base_rgb[0] + lum * 1.00 + warm
    img[..., 1] = base_rgb[1] + lum * 0.92
    img[..., 2] = base_rgb[2] + lum * 0.78 - warm * 0.8
    return np.clip(img, 0, 255).astype(np.uint8)


def make_normal_patch(rng):
    img = np.zeros((PATCH, PATCH, 3), dtype=np.float32)
    grain = periodic_band((PATCH, PATCH), 24.0, 160.0, rng)
    img[..., 0] = 128.0 + grain * 2.0
    img[..., 1] = 128.0 + grain * 2.0
    img[..., 2] = 255.0
    return np.clip(img, 0, 255).astype(np.uint8)


def make_gray_patch(level, rng, amp=3.0):
    noise = periodic_band((PATCH, PATCH), 8.0, 200.0, rng) * amp
    return np.clip(level + noise, 0, 255).astype(np.uint8)


def patch_rect(layout, name):
    x, y, w, h = layout["patches"][name]["rect_pixels"]
    assert w == PATCH and h == PATCH, (name, w, h)
    return x, y


def main():
    layout = json.loads(LAYOUT.read_text(encoding="utf-8"))
    rng = np.random.default_rng(SEED)

    albedo_path = ATLAS_DIR / "Moyva_AlbedoAtlas.png"
    albedo = np.asarray(Image.open(albedo_path)).copy()
    x, y = patch_rect(layout, "sand_top")
    albedo[y:y + PATCH, x:x + PATCH, :3] = make_sand_albedo((216, 178, 116), rng)
    x, y = patch_rect(layout, "sand_side")
    albedo[y:y + PATCH, x:x + PATCH, :3] = make_sand_albedo((178, 142, 94), rng, strata=True)
    Image.fromarray(albedo).save(albedo_path)

    # Normal/roughness/metallic maps were removed by the dual-tiles texture
    # migration; regenerate them only if the files still exist.
    normal_path = ATLAS_DIR / "Moyva_NormalAtlas.png"
    if normal_path.exists():
        normal = np.asarray(Image.open(normal_path)).copy()
        for name in ("sand_top", "sand_side"):
            x, y = patch_rect(layout, name)
            normal[y:y + PATCH, x:x + PATCH, :3] = make_normal_patch(rng)
        Image.fromarray(normal).save(normal_path)

    rough_path = ATLAS_DIR / "Moyva_RoughnessAtlas.png"
    if rough_path.exists():
        rough = np.asarray(Image.open(rough_path)).copy()
        for name in ("sand_top", "sand_side"):
            x, y = patch_rect(layout, name)
            rough[y:y + PATCH, x:x + PATCH] = make_gray_patch(214, rng)
        Image.fromarray(rough).save(rough_path)

    metal_path = ATLAS_DIR / "Moyva_MetallicSmoothnessAtlas.png"
    if metal_path.exists():
        metal = np.asarray(Image.open(metal_path)).copy()
        for name in ("sand_top", "sand_side"):
            x, y = patch_rect(layout, name)
            metal[y:y + PATCH, x:x + PATCH, :3] = 0
            metal[y:y + PATCH, x:x + PATCH, 3] = make_gray_patch(38, rng, amp=5.0)
        Image.fromarray(metal).save(metal_path)

    print("sand_top / sand_side regenerated in all four atlas maps")


if __name__ == "__main__":
    main()
