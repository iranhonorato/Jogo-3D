"""Gera os ícones da HUD em Assets/_Project/Art/UI/ (desenhados aqui, sem assets de terceiros).

Arrow_Den.png: seta âmbar com contorno escuro, apontando para a direita (ângulo 0 na Unity).
Uso: python Tools/Art/make_ui_icons.py
"""
from pathlib import Path

from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Art" / "UI"
SIZE = 256
SUPERSAMPLE = 4
AMBER = (255, 200, 77, 255)
OUTLINE = (34, 18, 46, 255)


def arrow_points(scale: float, inset: float):
    s = SIZE * scale
    return [
        (s * 0.12 + inset, s * 0.36 + inset),
        (s * 0.52, s * 0.36 + inset),
        (s * 0.52, s * 0.14 + inset * 1.6),
        (s * 0.90 - inset * 1.6, s * 0.50),
        (s * 0.52, s * 0.86 - inset * 1.6),
        (s * 0.52, s * 0.64 - inset),
        (s * 0.12 + inset, s * 0.64 - inset),
    ]


def make_arrow():
    big = Image.new("RGBA", (SIZE * SUPERSAMPLE, SIZE * SUPERSAMPLE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(big)
    draw.polygon(arrow_points(SUPERSAMPLE, 0), fill=OUTLINE)
    draw.polygon(arrow_points(SUPERSAMPLE, 12 * SUPERSAMPLE), fill=AMBER)
    big.resize((SIZE, SIZE), Image.LANCZOS).save(OUT / "Arrow_Den.png")


if __name__ == "__main__":
    make_arrow()
