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

# Background and title colors shared with the author's Archipelago mod art, so both Workshop items look related.
TOP = (18, 14, 44)
BOTTOM = (58, 28, 86)
TITLE = (255, 236, 250)
TITLE_GLOW = (190, 140, 255)
SLOT = (36, 38, 52)
SLOT_RIM = (97, 112, 135)
GREEN = (60, 255, 110)  # #3CFF6E, the mod's duplicate color
TEXT = (236, 232, 250)

# Crystal palettes (light, mid, dark facet).
PURPLE = ((214, 196, 255), (150, 104, 240), (92, 48, 170))
CYAN = ((200, 248, 255), (90, 200, 235), (34, 110, 160))
RED = ((255, 205, 200), (235, 92, 86), (140, 36, 46))
GOLD = ((255, 244, 200), (240, 196, 80), (160, 110, 30))
SILVER = ((246, 246, 252), (190, 196, 212), (112, 118, 138))


def gradient(w, h):
    img = Image.new("RGB", (w, h))
    px = img.load()
    for y in range(h):
        t = y / (h - 1)
        c = tuple(int(TOP[i] + (BOTTOM[i] - TOP[i]) * t) for i in range(3))
        for x in range(w):
            px[x, y] = c
    return img


def stars(img, n, seed, keep_clear=()):
    """Archipelago-style starfield; keep_clear: boxes (x0, y0, x1, y1) in final pixels left free for text."""
    rng = random.Random(seed)
    d = ImageDraw.Draw(img)
    w, h = img.size
    for _ in range(n):
        x, y = rng.uniform(0, w), rng.uniform(0, h * 0.9)
        if any(b[0] * SS <= x <= b[2] * SS and b[1] * SS <= y <= b[3] * SS for b in keep_clear):
            continue
        r = rng.choice([0.6, 0.8, 1.0, 1.4]) * SS
        d.ellipse((x - r, y - r, x + r, y + r), fill=(255, 245, 255, rng.randint(90, 230)))


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
    stars(img, 40, 3)
    r = 25 * SS
    slot(img, 37 * SS, 44 * SS, r, PURPLE, ring=True)
    slot(img, 91 * SS, 84 * SS, r, PURPLE, ring=True)
    img.convert("RGB").resize((128, 128), Image.LANCZOS).save(f"{OUT}/icon.png", optimize=True)


def preview():
    """Laid out like the Archipelago preview: glowing serif title and subtitle on the left, the picture on the right."""
    w, h = 1280, 720
    W, H = w * SS, h * SS
    img = gradient(W, H).convert("RGBA")
    stars(img, 260, 9, keep_clear=[(60, 110, 640, 430), (700, 150, 1250, 570)])

    lines, x, y0 = ["Who Has", "This Essence?"], 80 * SS, 130 * SS
    size = 118
    while True:
        title = ImageFont.truetype("C:/Windows/Fonts/constanb.ttf", size * SS)
        if max(title.getlength(t) for t in lines) <= 560 * SS:
            break
        size -= 2
    sub = ImageFont.truetype("C:/Windows/Fonts/segoeuil.ttf", 40 * SS)
    small = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 32 * SS)
    line_h = int(size * 1.08) * SS
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    g = ImageDraw.Draw(glow)
    for i, t in enumerate(lines):
        g.text((x, y0 + i * line_h), t, font=title, fill=TITLE_GLOW + (180,))
    img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(14 * SS)))
    d = ImageDraw.Draw(img)
    for i, t in enumerate(lines):
        d.text((x, y0 + i * line_h), t, font=title, fill=TITLE + (255,))
    d.text((x + 6 * SS, y0 + len(lines) * line_h + 40 * SS), "Shape of Dreams", font=sub, fill=(222, 206, 255, 255))

    # Scoreboard-style rows: the essence you share with a teammate is ringed on both rows; nothing else is.
    rows = [("You", [PURPLE, CYAN, None]), ("Player 2", [RED, PURPLE, GOLD]), ("Player 3", [SILVER, None, None])]
    r = 40 * SS
    x0, y0 = 720 * SS, 230 * SS
    for i, (name, gems) in enumerate(rows):
        y = y0 + i * 130 * SS
        d.text((x0, y), name, font=small, fill=TEXT, anchor="lm")
        for j, pal in enumerate(gems):
            slot(img, x0 + (210 + j * 110) * SS, y, r, pal, ring=pal is PURPLE)
        d = ImageDraw.Draw(img)

    img.convert("RGB").resize((w, h), Image.LANCZOS).save(f"{OUT}/preview.png", optimize=True)


if __name__ == "__main__":
    icon()
    preview()
