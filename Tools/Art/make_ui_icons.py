"""Gera os ícones da HUD em Assets/_Project/Art/UI/ (desenhados aqui, sem assets de terceiros).

Arrow_Den.png: seta âmbar com contorno escuro, apontando para a direita (ângulo 0 na Unity).
Dot.png: círculo branco com borda suave (pontos e paradas da estrada do mapa, coloridos na Unity).
Icon_Raccoon.png: cabeça do guaxinim com a máscara, para o marcador do mapa.
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


def make_dot():
    big = Image.new("RGBA", (64 * SUPERSAMPLE, 64 * SUPERSAMPLE), (0, 0, 0, 0))
    ImageDraw.Draw(big).ellipse([4 * SUPERSAMPLE, 4 * SUPERSAMPLE, 60 * SUPERSAMPLE, 60 * SUPERSAMPLE],
                                fill=(255, 255, 255, 255))
    big.resize((64, 64), Image.LANCZOS).save(OUT / "Dot.png")


def make_raccoon_icon():
    k = SUPERSAMPLE
    big = Image.new("RGBA", (SIZE * k, SIZE * k), (0, 0, 0, 0))
    d = ImageDraw.Draw(big)

    def box(x0, y0, x1, y1):
        return [x0 * k, y0 * k, x1 * k, y1 * k]

    def poly(points):
        return [(x * k, y * k) for x, y in points]

    grey, dark, light, black = (150, 150, 165, 255), (70, 66, 82, 255), (235, 232, 225, 255), (28, 22, 34, 255)
    # Orelhas (contorno, pelo e miolo claro).
    for side in (-1, 1):
        cx = 128 + side * 66
        d.polygon(poly([(cx - 40, 92), (cx + side * 14, 14), (cx + 40, 92)]), fill=OUTLINE)
        d.polygon(poly([(cx - 28, 88), (cx + side * 12, 32), (cx + 28, 88)]), fill=dark)
        d.polygon(poly([(cx - 14, 84), (cx + side * 8, 52), (cx + 14, 84)]), fill=light)
    # Cabeça.
    d.ellipse(box(28, 52, 228, 236), fill=OUTLINE)
    d.ellipse(box(38, 62, 218, 226), fill=grey)
    # Focinho claro.
    d.ellipse(box(78, 128, 178, 222), fill=light)
    # Máscara preta com os olhos.
    d.polygon(poly([(46, 118), (98, 104), (128, 130), (158, 104), (210, 118), (196, 160), (150, 156),
                    (128, 142), (106, 156), (60, 160)]), fill=black)
    for cx in (96, 160):
        d.ellipse(box(cx - 13, 118, cx + 13, 144), fill=light)
        d.ellipse(box(cx - 6, 124, cx + 6, 138), fill=black)
    # Nariz.
    d.ellipse(box(114, 160, 142, 182), fill=black)
    big.resize((SIZE, SIZE), Image.LANCZOS).save(OUT / "Icon_Raccoon.png")


if __name__ == "__main__":
    make_arrow()
    make_dot()
    make_raccoon_icon()
