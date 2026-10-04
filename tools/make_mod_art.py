"""Renders the mod's about/icon.png and about/preview.png (original art: no game assets).

Needs Pillow and the Windows fonts Constantia and Segoe UI. Usage:
    python tools/make_mod_art.py mod/SoDWhoHasThisEssence/about
"""
import math
import random
import sys
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = sys.argv[1]
SS = 4  # supersampling factor

TOP = (16, 16, 34)
BOTTOM = (34, 26, 58)
SLOT = (36, 38, 52)
SLOT_RIM = (97, 112, 135)
GREEN = (60, 255, 110)  # #3CFF6E, the mod's duplicate color
TEXT = (236, 232, 250)
SUBTEXT = (180, 172, 210)

# Crystal palettes (light, mid, dark facet).
PURPLE = ((214, 196, 255), (150, 104, 240), (92, 48, 170))
CYAN = ((200, 248, 255), (90, 200, 235), (34, 110, 160))
RED = ((255, 205, 200), (235, 92, 86), (140, 36, 46))
GOLD = ((255, 244, 200), (240, 196, 80), (160, 110, 30))


def gradient(w, h):
    img = Image.new("RGB", (w, h))
    px = img.load()
    for y in range(h):
        t = y / (h - 1)
        c = tuple(int(TOP[i] + (BOTTOM[i] - TOP[i]) * t) for i in range(3))
        for x in range(w):
            px[x, y] = c
    return img


def specks(img, n, seed):
    rng = random.Random(seed)
    d = ImageDraw.Draw(img)
    w, h = img.size
    for _ in range(n):
        x, y, r = rng.uniform(0, w), rng.uniform(0, h), rng.uniform(0.6, 1.8) * SS
        a = rng.randint(40, 120)
        d.ellipse((x - r, y - r, x + r, y + r), fill=(200, 190, 255, a))


def crystal(d, cx, cy, s, palette):
    """A tilted faceted shard centered at (cx, cy), about 2*s tall."""
    light, mid, dark = palette
    pts = [(-0.30, -1.00), (0.42, -0.55), (0.55, 0.30), (0.10, 1.00), (-0.50, 0.55), (-0.62, -0.25)]
    a = math.radians(-28)
    P = [(cx + s * (x * math.cos(a) - y * math.sin(a)), cy + s * (x * math.sin(a) + y * math.cos(a))) for x, y in pts]
    core = (cx + s * 0.02, cy - s * 0.05)
    d.polygon(P, fill=mid)
    d.polygon([P[0], P[1], core], fill=light)
    d.polygon([P[5], P[0], core], fill=tuple((l + m) // 2 for l, m in zip(light, mid)))
    d.polygon([P[2], P[3], core], fill=dark)
    d.polygon([P[3], P[4], core], fill=tuple((m + k) // 2 for m, k in zip(mid, dark)))


def slot(img, cx, cy, r, palette=None, ring=False):
    """A round essence slot like the scoreboard's, optionally with the mod's green ring around it."""
    if ring:
        glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
        g = ImageDraw.Draw(glow)
        R = r * 1.22
        g.ellipse((cx - R, cy - R, cx + R, cy + R), outline=GREEN + (200,), width=int(r * 0.22))
        img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(r * 0.18)))
    d = ImageDraw.Draw(img)
    d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=SLOT, outline=SLOT_RIM, width=max(1, int(r * 0.06)))
    if ring:
        R = r * 1.17
        d.ellipse((cx - R, cy - R, cx + R, cy + R), outline=GREEN, width=int(r * 0.14))
    if palette:
        crystal(d, cx, cy, r * 0.78, palette)


def icon():
    n = 128 * SS
    img = gradient(n, n).convert("RGBA")
    specks(img, 30, 3)
    r = 25 * SS
    slot(img, 37 * SS, 44 * SS, r, PURPLE, ring=True)
    slot(img, 91 * SS, 84 * SS, r, PURPLE, ring=True)
    img.convert("RGB").resize((128, 128), Image.LANCZOS).save(f"{OUT}/icon.png", optimize=True)


def preview():
    w, h = 1280, 720
    W, H = w * SS, h * SS
    img = gradient(W, H).convert("RGBA")
    specks(img, 160, 7)
    d = ImageDraw.Draw(img)

    title = ImageFont.truetype("C:/Windows/Fonts/constanb.ttf", 96 * SS)
    sub = ImageFont.truetype("C:/Windows/Fonts/segoeuil.ttf", 36 * SS)
    small = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 30 * SS)
    d.text((W / 2, 110 * SS), "Who Has This Essence?", font=title, fill=TEXT, anchor="mm")
    d.text((W / 2, 190 * SS), "See which teammates can combine the essence at your feet", font=sub, fill=SUBTEXT,
           anchor="mm")

    # Left: three scoreboard rows; the shared purple essence is ringed on two of them.
    rows = [("You", [PURPLE, CYAN, None]), ("Bo", [RED, PURPLE, GOLD]), ("Cy", [CYAN, None, GOLD])]
    r = 34 * SS
    x0, y0 = 110 * SS, 300 * SS
    for i, (name, gems) in enumerate(rows):
        y = y0 + i * 125 * SS
        d.text((x0, y), name, font=small, fill=TEXT, anchor="lm")
        for j, pal in enumerate(gems):
            slot(img, x0 + (150 + j * 95) * SS, y, r, pal, ring=pal is PURPLE)
        d = ImageDraw.Draw(img)

    # Right: the pickup prompt over a ground essence you don't have, with the mod's line under the name.
    px, py = 900 * SS, 330 * SS
    d.rounded_rectangle((px - 260 * SS, py - 50 * SS, px + 260 * SS, py + 30 * SS), radius=12 * SS,
                        fill=(24, 22, 40, 230), outline=(120, 104, 180), width=2 * SS)
    d.text((px, py - 10 * SS), "(100%) Essence of Embers", font=small, fill=(255, 196, 90), anchor="mm")
    d.text((px, py + 62 * SS), "(Bo, Cy)", font=small, fill=GREEN, anchor="mm")
    d.text((px, py + 120 * SS), "[F] Equip      [Hold F] Dismantle", font=small, fill=SUBTEXT, anchor="mm")
    slot(img, px, py + 250 * SS, 48 * SS, GOLD)

    img.convert("RGB").resize((w, h), Image.LANCZOS).save(f"{OUT}/preview.png", optimize=True)


if __name__ == "__main__":
    icon()
    preview()
