"""Gera as texturas de efeito em Assets/_Project/Art/Textures/Effects/ (desenhadas aqui, sem assets de terceiros).

LightPool_Gradient.png: círculo branco que some para as bordas, para as poças de luz no chão das fases.
Uso: python Tools/Art/make_effect_textures.py
"""
from pathlib import Path

from PIL import Image

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Art" / "Textures" / "Effects"
SIZE = 256


def make_light_pool():
    image = Image.new("RGBA", (SIZE, SIZE))
    pixels = image.load()
    center = (SIZE - 1) / 2
    for y in range(SIZE):
        for x in range(SIZE):
            distance = (((x - center) ** 2 + (y - center) ** 2) ** 0.5) / center
            falloff = max(0.0, 1.0 - distance)
            pixels[x, y] = (255, 255, 255, round(255 * falloff * falloff))
    OUT.mkdir(parents=True, exist_ok=True)
    image.save(OUT / "LightPool_Gradient.png")


if __name__ == "__main__":
    make_light_pool()
