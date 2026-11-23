"""Gera o kit modular de cenário (gôndolas, parede, piso e toca) com uma textura compartilhada.

GDD §11 (kit modular), §14.4 (layers), §15.1, §15.4 e §15.5.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/env_kit.py -- [--preview <pasta>]

Todas as peças usam o material M_Env_Kit (atlas Env_Kit_BaseColor.png), para a Unity agrupar os
draw calls. Pivô na base, no centro da peça. Medidas modulares de 2 m:
- Shelf_Aisle_2m e Shelf_Aisle_4m: gôndola dupla (produtos dos dois lados), 1,8 m de altura e
  0,9 m de profundidade, comprida no eixo X.
- Wall_2m: parede de 2 m x 2,5 m, 0,2 m de espessura, comprida no eixo X.
- Floor_2m: placa de piso de 2 m x 2 m com o topo em z = 0; os azulejos casam entre placas.
- Den_Manhole: a toca, um bueiro aberto com a grade encostada ao lado.
"""

import math
import os
import random
import sys

from mathutils import Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

# Gôndola
SHELF_METAL = gc.srgb("#cfcdc5")
SHELF_BASE = gc.srgb("#55565e")
SHELF_BACK = gc.srgb("#a9aaa6")
PRICE_STRIP = gc.srgb("#e9e4d6")
PRICE_TAGS = (gc.srgb("#f2d24b"), gc.srgb("#ffffff"), gc.srgb("#e2553f"))
PRODUCT_COLORS = tuple(gc.srgb(h) for h in (
    "#a8553f", "#c08a3e", "#5d7f63", "#4f6d8a", "#8a5a7a",
    "#b8a06a", "#7b8f9c", "#9c4a4a", "#d0c4a8", "#6a7d4a"))
LABEL_COLORS = tuple(gc.srgb(h) for h in ("#ece3cb", "#f0d58a", "#d9dfe2", "#5a4a3e"))

# Parede e piso
WALL_PAINT = gc.srgb("#c8bea8")
WALL_TILE = gc.srgb("#86a097")
BASEBOARD = gc.srgb("#3d3b3e")
FLOOR_LIGHT = gc.srgb("#d6d0c1")
FLOOR_DARK = gc.srgb("#b3ac9b")
GROUT = gc.srgb("#8e887b")
DIRT = gc.srgb("#7b7262")

# Toca
CONCRETE = gc.srgb("#8b8780")
HOLE = gc.srgb("#0f0d13")
IRON = gc.srgb("#3d3a38")
RUST = gc.srgb("#7a4a2c")

SHELF_HEIGHT = 1.8
SHELF_DEPTH = 0.9
BASE_HEIGHT = 0.12
SHELF_LEVELS = (0.5, 0.9, 1.3)  # altura das prateleiras acima da base
SHELF_BOARD_DEPTH = 0.4
PRODUCT_DEPTH = 0.3
WALL_HEIGHT = 2.5
WALL_THICKNESS = 0.2
TILE_SIZE = 0.5
DEN_RADIUS = 0.36

LABEL_SPACING = 0.02  # tamanho da grade usada para pintar rótulos e azulejos


# ---------------------------------------------------------------------------
# Gôndola
# ---------------------------------------------------------------------------

def _box_part(name, center, size, color, skip=()):
    obj = gc.grid_box(name, center, size, skip=skip)
    return gc.Part(obj, gc.flat(color), 12)


def _price_strip(name, center, size, side):
    """Faixa de preços na borda da prateleira, com etiquetas coloridas a cada ~25 cm."""
    high = gc.grid_box(name, center, size, spacing=0.01)
    low = gc.grid_box(name + "_low", center, size)
    tag_count = max(1, round(size[0] / 0.25))

    def paint_strip(co, n):
        if n.y * side < 0.5:
            return PRICE_STRIP
        u = (co.x - (center[0] - size[0] / 2)) / size[0] * tag_count
        index = int(u) % len(PRICE_TAGS)
        inside = gc.band(u - math.floor(u), 0.5, 0.18, 0.04) * gc.band(co.z, center[2], 0.016, 0.004)
        return gc.lerp(PRICE_STRIP, PRICE_TAGS[index], inside)

    return gc.Part(high, paint_strip, 12, low=low)


def _product(name, center, size, color, label, side):
    """Produto sem a face de baixo e a de trás (encostada no fundo), com uma faixa de rótulo."""
    skip = ("-z", "-y") if side > 0 else ("-z", "+y")
    high = gc.grid_box(name, center, size, spacing=LABEL_SPACING, skip=skip)
    low = gc.grid_box(name + "_low", center, size, skip=skip)
    bottom = center[2] - size[2] / 2

    def paint_product(co, n):
        t = (co.z - bottom) / size[2]
        stripe = gc.band(t, 0.55, 0.14, 0.03) * (1.0 - gc.above(n.z, 0.5))
        top = gc.shade(color, 0.85)
        return gc.lerp(top if n.z > 0.5 else color, label, stripe)

    return gc.Part(high, paint_product, 12, low=low)


def _fill_shelf(rng, parts, length, side, floor_z, clearance):
    """Enfileira produtos de larguras e alturas variadas sobre uma prateleira."""
    x = -length / 2 + 0.05
    y = side * (0.04 + PRODUCT_DEPTH / 2 + 0.03)
    while True:
        width = rng.uniform(0.14, 0.34)
        if x + width > length / 2 - 0.05:
            break
        height = rng.uniform(0.45, 0.85) * clearance
        color = rng.choice(PRODUCT_COLORS)
        label = rng.choice(LABEL_COLORS)
        center = (x + width / 2, y, floor_z + height / 2)
        parts.append(_product("Product", center, (width - 0.015, PRODUCT_DEPTH, height), color, label, side))
        x += width + rng.uniform(0.0, 0.03)


def build_shelf(name, length, seed):
    rng = random.Random(seed)
    parts = [
        _box_part("Base", (0.0, 0.0, BASE_HEIGHT / 2), (length - 0.06, SHELF_DEPTH - 0.04, BASE_HEIGHT), SHELF_BASE,
                  skip=("-z",)),
        _box_part("Back", (0.0, 0.0, (BASE_HEIGHT + SHELF_HEIGHT) / 2),
                  (length - 0.04, 0.04, SHELF_HEIGHT - BASE_HEIGHT), SHELF_BACK),
    ]
    for side in (1, -1):
        parts.append(_box_part("EndPanel", (side * (length / 2 - 0.015), 0.0, SHELF_HEIGHT / 2),
                               (0.03, SHELF_DEPTH, SHELF_HEIGHT), SHELF_METAL, skip=("-z",)))

    floors = (BASE_HEIGHT,) + SHELF_LEVELS
    for side in (1, -1):
        front_y = side * (0.02 + SHELF_BOARD_DEPTH)
        for index, floor_z in enumerate(floors):
            top = floors[index + 1] if index + 1 < len(floors) else SHELF_HEIGHT + 0.1
            if index > 0:
                parts.append(_box_part("Board", (0.0, side * (0.02 + SHELF_BOARD_DEPTH / 2), floor_z - 0.0125),
                                       (length - 0.06, SHELF_BOARD_DEPTH, 0.025), SHELF_METAL))
            parts.append(_price_strip("PriceStrip", (0.0, front_y + side * 0.006, floor_z - 0.02),
                                      (length - 0.06, 0.012, 0.05), side))
            _fill_shelf(rng, parts, length - 0.06, side, floor_z, top - floor_z - 0.06)
    return gc.Piece(name, parts)


# ---------------------------------------------------------------------------
# Parede, piso e toca
# ---------------------------------------------------------------------------

def build_wall():
    size = (2.0, WALL_THICKNESS, WALL_HEIGHT)
    center = (0.0, 0.0, WALL_HEIGHT / 2)
    high = gc.grid_box("Wall", center, size, spacing=0.02, skip=("-z",))
    low = gc.grid_box("Wall_low", center, size, skip=("-z",))

    def paint_wall(co, n):
        tiles = 1.0 - gc.above(co.z, 1.2)
        row = co.z % 0.15
        grout = gc.band(min(row, 0.15 - row), 0.0, 0.006, 0.003) * tiles
        color = gc.lerp(WALL_PAINT, gc.lerp(WALL_TILE, GROUT, grout), tiles)
        trim = gc.band(co.z, 1.2, 0.02, 0.004)
        color = gc.lerp(color, gc.shade(WALL_TILE, 0.7), trim)
        grime = 0.25 * gc.smoothstep(0.2, 0.7, gc.turbulence(co, 2.5)) * (1.0 - gc.smoothstep(0.0, 1.0, co.z))
        color = gc.lerp(color, DIRT, grime)
        return gc.lerp(color, BASEBOARD, 1.0 - gc.above(co.z, 0.1, 0.003))

    return gc.Piece("Wall_2m", [gc.Part(high, paint_wall, 12, low=low)])


def build_floor():
    only_top = ("+x", "-x", "+y", "-y", "-z")
    high = gc.grid_box("Floor", (0.0, 0.0, -0.01), (2.0, 2.0, 0.02), spacing=0.02, skip=only_top)
    low = gc.grid_box("Floor_low", (0.0, 0.0, -0.01), (2.0, 2.0, 0.02), skip=only_top)

    def paint_floor(co, n):
        u, v = (co.x + 1.0) / TILE_SIZE, (co.y + 1.0) / TILE_SIZE
        checker = (math.floor(u) + math.floor(v)) % 2
        color = FLOOR_DARK if checker else FLOOR_LIGHT
        grout = max(gc.band(u - round(u), 0.0, 0.012, 0.01), gc.band(v - round(v), 0.0, 0.012, 0.01))
        color = gc.lerp(color, GROUT, grout)
        scuff = 0.18 * gc.smoothstep(0.3, 0.8, gc.turbulence(co, 1.7))
        return gc.lerp(color, DIRT, scuff)

    return gc.Piece("Floor_2m", [gc.Part(high, paint_floor, 2, low=low)])


def build_den():
    parts = []
    rim = gc.torus("Rim", (0.0, 0.0, 0.02), (0.0, 0.0, 1.0), DEN_RADIUS + 0.05, 0.06, segments=20, sides=4)
    for v in rim.data.vertices:
        v.co.z = 0.02 + (v.co.z - 0.02) * 0.4

    def paint_rim(co, n):
        crack = gc.smoothstep(0.35, 0.6, gc.turbulence(co, 12.0))
        return gc.lerp(CONCRETE, gc.shade(CONCRETE, 0.6), 0.6 * crack)

    hole = gc.cylinder("Hole", (0.0, 0.0, 0.0), (0.0, 0.0, 0.006), DEN_RADIUS + 0.01, segments=20, smooth=False)
    parts += [gc.Part(rim, paint_rim, 200), gc.Part(hole, gc.flat(HOLE), 80)]  # fica acima do piso

    lid_tilt = Matrix.Translation((DEN_RADIUS + 0.42, 0.06, 0.1)) @ Matrix.Rotation(math.radians(-18.0), 4, 'Y')
    lid_parts = [gc.Part(gc.torus("LidRing", (0.0, 0.0, 0.03), (0.0, 0.0, 1.0), DEN_RADIUS - 0.02, 0.025,
                                  segments=20, sides=4), _paint_iron, 160)]
    for i in range(-2, 3):
        bar_length = 2.0 * math.sqrt(max(0.0, (DEN_RADIUS - 0.03) ** 2 - (i * 0.11) ** 2))
        bar = gc.box("LidBar", (i * 0.11, 0.0, 0.03), (0.025, bar_length, 0.03))
        lid_parts.append(gc.Part(bar, _paint_iron, 12))
    for part in lid_parts:
        part.obj.data.transform(lid_tilt)
    parts += lid_parts

    rng = random.Random(7)
    for _ in range(4):
        angle = rng.uniform(0.0, 2.0 * math.pi)
        distance = DEN_RADIUS + rng.uniform(0.12, 0.25)
        center = (distance * math.cos(angle), distance * math.sin(angle), 0.02)
        chunk = gc.ellipsoid("Debris", center, (rng.uniform(0.02, 0.04),) * 2 + (0.02,), segments=6, rings=4)
        parts.append(gc.Part(chunk, gc.flat(gc.shade(CONCRETE, 0.85)), 24))
    return gc.Piece("Den_Manhole", parts)


def _paint_iron(co, n):
    rust = gc.smoothstep(0.2, 0.6, gc.turbulence(co, 9.0))
    return gc.lerp(IRON, RUST, 0.55 * rust)


def build():
    return [
        build_shelf("Shelf_Aisle_2m", 2.0, seed=11),
        build_shelf("Shelf_Aisle_4m", 4.0, seed=23),
        build_wall(),
        build_floor(),
        build_den(),
    ]


if __name__ == "__main__":
    gc.run_model("Env_Kit", "Environment", build, texture_size=2048,
                 bake_cage=0.01, bake_ray=0.02, ao_distance=0.3,
                 blotch_scale=3.0, stroke_scale=30.0)
