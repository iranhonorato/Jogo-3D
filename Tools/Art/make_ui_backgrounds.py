"""Gera as imagens de fundo das telas que não têm arte pronta (GDD §13 e §15.1).

- Background_CityMap.jpg: mapa ilustrado da cidade à noite, visto de cima, desenhado por código (ruas, quarteirões,
  janelas acesas, árvores e postes de luz). Os cartões das fases ficam sobre a avenida do meio.
- Background_Ending.jpg: a ilustração do menu principal em versão comemorativa (desfocada, dourada e com brilhos).

Uso (na raiz do repositório):  python Tools/Art/make_ui_backgrounds.py
Precisa só do Pillow. A saída vai para Assets/_Project/Art/UI/ e entra no Unity como Sprite.
"""
import random
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageOps

ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "Assets" / "_Project" / "Art" / "UI"
WIDTH, HEIGHT = 1920, 1080
SCALE = 2  # desenha no dobro e reduz no fim, para as bordas ficarem suaves

# Paleta noturna do GDD §15.1: sombras em roxo e azul, pontos de luz quente.
SKY_TOP = (13, 11, 30)
SKY_BOTTOM = (27, 21, 53)
ASPHALT = (43, 39, 69)
SIDEWALK = (66, 58, 98)
LANE_MARK = (138, 127, 176)
CROSSWALK = (150, 142, 190)
BUILDING_COLORS = [(26, 22, 48), (33, 27, 59), (39, 32, 71), (47, 38, 82)]
ROOF_EDGE = (72, 61, 112)
TREE_COLOR = (28, 58, 60)
TREE_SHADOW = (16, 30, 36)
PARK_GROUND = (24, 36, 48)
WINDOW_LIT = (255, 212, 138)
WINDOW_DARK = (20, 17, 38)
LAMP_GLOW = (255, 190, 110)

# O mapa tem uma avenida horizontal no meio (atrás dos cartões das fases) e ruas verticais entre os cartões.
AVENUE_Y = (400, 680)
CROSS_STREET_X = [0.215, 0.405, 0.595, 0.785]
CROSS_WIDTH = 78
SIDEWALK_WIDTH = 16


def px(value):
    return int(value * SCALE)


def vertical_gradient(size, top, bottom):
    width, height = size
    column = Image.new("RGB", (1, height))
    for y in range(height):
        t = y / max(1, height - 1)
        column.putpixel((0, y), tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)))
    return column.resize(size)


def scaled_mask(mask, strength):
    return mask.point(lambda value: int(value * strength))


def falloff(value):
    """Do centro (1) até a borda do círculo (0), suave. O radial_gradient do Pillow só chega ao branco nos cantos."""
    distance = min(1.0, value * 1.4143 / 255)
    return int(255 * (1.0 - distance) ** 2)


def glow(size, center, radius, color, strength):
    """Brilho redondo e suave, para somar sobre a imagem (postes de luz e brilhos)."""
    diameter = int(radius * 2)
    mask = Image.radial_gradient("L").point(falloff).resize((diameter, diameter))
    mask = scaled_mask(mask, strength)
    layer = Image.new("RGB", size, (0, 0, 0))
    layer.paste(Image.new("RGB", mask.size, color), (int(center[0] - radius), int(center[1] - radius)), mask)
    return layer


def vignette(image, darkness):
    edges = scaled_mask(Image.radial_gradient("L").resize(image.size), darkness)  # preto no centro, claro nas bordas
    return Image.composite(Image.new("RGB", image.size, (0, 0, 0)), image, edges)


def road_rects():
    """Avenida e ruas (já na escala de desenho)."""
    avenue = (0, px(AVENUE_Y[0]), px(WIDTH), px(AVENUE_Y[1]))
    streets = []
    for fraction in CROSS_STREET_X:
        x = px(WIDTH * fraction)
        streets.append((x - px(CROSS_WIDTH) // 2, 0, x + px(CROSS_WIDTH) // 2, px(HEIGHT)))
    return avenue, streets


def grow(rect, margin):
    return (rect[0] - margin, rect[1] - margin, rect[2] + margin, rect[3] + margin)


def draw_roads(draw):
    avenue, streets = road_rects()
    for rect in [avenue] + streets:
        draw.rectangle(grow(rect, px(SIDEWALK_WIDTH)), fill=SIDEWALK)
    for rect in [avenue] + streets:
        draw.rectangle(rect, fill=ASPHALT)

    middle = (avenue[1] + avenue[3]) // 2
    for x in range(0, px(WIDTH), px(70)):
        draw.rectangle((x, middle - px(3), x + px(34), middle + px(3)), fill=LANE_MARK)
    for street in streets:
        center = (street[0] + street[2]) // 2
        for y in range(0, px(HEIGHT), px(60)):
            if avenue[1] - px(30) < y < avenue[3] + px(10):
                continue
            draw.rectangle((center - px(2), y, center + px(2), y + px(28)), fill=LANE_MARK)
        for stripe in range(-3, 4):  # faixas de pedestres nos cruzamentos
            x = center + stripe * px(18)
            draw.rectangle((x - px(5), avenue[1] - px(14), x + px(5), avenue[1] - px(3)), fill=CROSSWALK)
            draw.rectangle((x - px(5), avenue[3] + px(3), x + px(5), avenue[3] + px(14)), fill=CROSSWALK)


def free_blocks():
    """Áreas entre as ruas onde entram prédios e parques."""
    avenue, streets = road_rects()
    margin = px(SIDEWALK_WIDTH + 14)
    edges = [0] + [value for street in streets for value in (street[0], street[2])] + [px(WIDTH)]
    columns = [(edges[i], edges[i + 1]) for i in range(0, len(edges), 2)]
    blocks = []
    for left, right in columns:
        blocks.append((left + margin, margin, right - margin, avenue[1] - margin))
        blocks.append((left + margin, avenue[3] + margin, right - margin, px(HEIGHT) - margin))
    return [b for b in blocks if b[2] - b[0] > px(60) and b[3] - b[1] > px(60)]


def draw_building(draw, rect, rng):
    color = rng.choice(BUILDING_COLORS)
    draw.rectangle(rect, fill=color)
    draw.line((rect[0], rect[1], rect[2], rect[1]), fill=ROOF_EDGE, width=px(3))
    draw.line((rect[0], rect[1], rect[0], rect[3]), fill=ROOF_EDGE, width=px(3))
    lighter = tuple(min(255, c + 14) for c in color)
    for _ in range(rng.randint(1, 3)):  # caixas de ar-condicionado no telhado
        w, h = px(rng.randint(14, 28)), px(rng.randint(10, 20))
        x = rng.randint(rect[0] + px(8), max(rect[0] + px(9), rect[2] - w - px(8)))
        y = rng.randint(rect[1] + px(8), max(rect[1] + px(9), rect[3] - h - px(8)))
        draw.rectangle((x, y, x + w, y + h), fill=lighter)
    step = px(22)
    for y in range(rect[1] + px(26), rect[3] - px(14), step):  # janelas: poucas acesas, em tom quente
        for x in range(rect[0] + px(14), rect[2] - px(14), step):
            is_lit = rng.random() < 0.16
            draw.rectangle((x, y, x + px(8), y + px(10)), fill=WINDOW_LIT if is_lit else WINDOW_DARK)


def draw_park(draw, rect, rng):
    draw.rectangle(rect, fill=PARK_GROUND)
    tree_count = int((rect[2] - rect[0]) * (rect[3] - rect[1]) / px(60) ** 2 * 3)
    for _ in range(tree_count):
        radius = px(rng.randint(14, 26))
        x = rng.randint(rect[0] + radius, max(rect[0] + radius + 1, rect[2] - radius))
        y = rng.randint(rect[1] + radius, max(rect[1] + radius + 1, rect[3] - radius))
        shadow = (x - radius + px(4), y - radius + px(6), x + radius + px(4), y + radius + px(6))
        draw.ellipse(shadow, fill=TREE_SHADOW)
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=TREE_COLOR)


def draw_blocks(draw, rng):
    for block in free_blocks():
        if rng.random() < 0.22:
            draw_park(draw, block, rng)
            continue
        x = block[0]
        while x < block[2] - px(40):  # divide o quarteirão em prédios lado a lado
            right = min(block[2], x + px(rng.randint(70, 150)))
            if block[2] - right < px(40):
                right = block[2]
            draw_building(draw, (x, block[1], right - px(6), block[3]), rng)
            x = right


def street_lamps(size):
    """Postes de luz ao longo da avenida e das ruas, somados como brilho."""
    avenue, streets = road_rects()
    lamps = []
    for x in range(px(90), px(WIDTH), px(210)):
        lamps.append((x, avenue[1] - px(8)))
        lamps.append((x + px(105), avenue[3] + px(8)))
    for street in streets:
        for y in range(px(60), px(HEIGHT), px(240)):
            if avenue[1] - px(70) < y < avenue[3] + px(70):
                continue
            lamps.append((street[0] - px(6), y))
    layer = Image.new("RGB", size, (0, 0, 0))
    for center in lamps:
        layer = ImageChops.add(layer, glow(size, center, px(70), LAMP_GLOW, 0.55))
        layer = ImageChops.add(layer, glow(size, center, px(14), (255, 235, 200), 1.0))
    return layer


def make_city_map(path):
    rng = random.Random(24112026)
    size = (px(WIDTH), px(HEIGHT))
    image = vertical_gradient(size, SKY_TOP, SKY_BOTTOM)
    draw = ImageDraw.Draw(image)
    draw_blocks(draw, rng)
    draw_roads(draw)
    image = ImageChops.add(image, street_lamps(size))
    image = image.resize((WIDTH, HEIGHT), Image.LANCZOS)
    image = vignette(image, 0.75)
    image.save(path, "JPEG", quality=90, subsampling=0, optimize=True)


def make_ending(source, path):
    image = Image.open(source).convert("RGB")
    image = ImageOps.fit(image, (WIDTH, HEIGHT), Image.LANCZOS)
    image = image.filter(ImageFilter.GaussianBlur(3))
    warm = ImageChops.multiply(image, Image.new("RGB", image.size, (255, 205, 130)))
    image = Image.blend(image, warm, 0.55)  # esquenta o tom
    sun = glow(image.size, (int(WIDTH * 0.62), int(HEIGHT * 0.78)), 760, (255, 196, 90), 0.5)
    image = ImageChops.add(image, sun)

    rng = random.Random(25112026)
    sparkles = Image.new("RGB", image.size, (0, 0, 0))
    sparkle_draw = ImageDraw.Draw(sparkles)
    for _ in range(140):  # brilhos dourados, como moedas e confete
        x, y = rng.randint(0, WIDTH), rng.randint(0, HEIGHT)
        radius = rng.randint(2, 6)
        sparkle_draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=(255, 226, 150))
    image = ImageChops.add(image, sparkles.filter(ImageFilter.GaussianBlur(1.5)))
    image = vignette(image, 0.8)
    image.save(path, "JPEG", quality=90, subsampling=0, optimize=True)


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    make_city_map(OUT_DIR / "Background_CityMap.jpg")
    make_ending(OUT_DIR / "Background_MainMenu.jpg", OUT_DIR / "Background_Ending.jpg")
    print("ok:", OUT_DIR)


if __name__ == "__main__":
    main()
