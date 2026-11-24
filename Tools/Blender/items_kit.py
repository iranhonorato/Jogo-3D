"""Gera os modelos dos itens da tabela do GDD §7.8 (Item_<Nome>.fbx) com uma textura compartilhada.

GDD §7.8, §15.2 (itens com cores vivas, que se destacam do cenário) e §15.4.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/items_kit.py -- [--preview <pasta>]

Todos usam o material M_Items_Kit (atlas Items_Kit_BaseColor.png), para a Unity agrupar os draw calls.
Pivô na base, no centro do item; frente em -Y. Os tamanhos são um pouco maiores que os reais, para ler bem
de longe com a câmera isométrica (cerca de 0,25 m para volume 1 e 0,5 m para volume 5, como o greybox).
O ItemFactory usa o modelo quando o FBX existe e cai no cubo greybox quando não.
"""

import math
import os
import sys

import bmesh
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

WHITE = gc.srgb("#f2efe6")
CREAM = gc.srgb("#ead9b0")
CRUST = gc.srgb("#b8742e")
CRUST_DARK = gc.srgb("#8a5320")
BANANA = gc.srgb("#f2cf3a")
BANANA_TIP = gc.srgb("#5a4a2a")
CHEESE = gc.srgb("#f4c23c")
CHEESE_HOLE = gc.srgb("#d39a22")
COOKIE_PACK = gc.srgb("#2f6fd0")
MILK_BLUE = gc.srgb("#3c8de0")
MELON_DARK = gc.srgb("#2f6b2a")
MELON_LIGHT = gc.srgb("#7fb04a")
JUG_BLUE = gc.srgb("#7cc6ee")
CAP_BLUE = gc.srgb("#1f5fb0")
SACK = gc.srgb("#efe8d8")
RED = gc.srgb("#d83a2f")
PUMPKIN = gc.srgb("#ee7d22")
PUMPKIN_RIB = gc.srgb("#c45a14")
STEM = gc.srgb("#5c7a2a")
CHIPS_RED = gc.srgb("#e0342c")
CHIPS_YELLOW = gc.srgb("#f6c933")
PLASTIC = gc.srgb("#dfe8ef")
CEREAL = gc.srgb("#f08c2a")
CEREAL_BAND = gc.srgb("#3fa04a")
POPCORN_RED = gc.srgb("#d8302a")
JUICE = gc.srgb("#f39a1e")
GLASS_CAP = gc.srgb("#2a8a3a")
EGG_BOX = gc.srgb("#c9b48a")
EGG = gc.srgb("#f4e9d2")
JAM = gc.srgb("#8a2a6a")
LID_GOLD = gc.srgb("#d8b04e")
OIL = gc.srgb("#7a9a2a")
OIL_DARK = gc.srgb("#4a6618")
CAN_MAGENTA = gc.srgb("#e0309a")
CAN_LIME = gc.srgb("#8fe03a")
BAR_PURPLE = gc.srgb("#7a3fc0")
BAR_GOLD = gc.srgb("#f2c23c")


def label(color, label_color, bottom, height, center=0.5, half=0.18):
    """Pintura com uma faixa de rótulo na altura relativa center (0 = base, 1 = topo)."""
    def paint(co, n):
        t = (co.z - bottom) / height
        return gc.lerp(color, label_color, gc.band(t, center, half, 0.03) * (1.0 - gc.above(abs(n.z), 0.8)))
    return paint


def stripes_z(dark, light, count):
    """Listras em volta do eixo vertical (melancia, abóbora)."""
    def paint(co, n):
        angle = math.atan2(co.y, co.x)
        return gc.lerp(light, dark, gc.smoothstep(-0.3, 0.3, math.sin(angle * count)))
    return paint


def piece(name, parts):
    return gc.Piece("Item_" + name, parts)


def dense_box(name, center, size, paint, spacing=0.01):
    """Caixa com malha densa para a pintura (rótulos, listras) e versão leve para o modelo final."""
    high = gc.grid_box(name, center, size, spacing=spacing)
    low = gc.grid_box(name + "_low", center, size)
    return gc.Part(high, paint, 12, low=low)


def dense_cylinder(name, bottom_z, top_z, radius, paint, segments=12, top_radius=None):
    """Cilindro com anéis no meio (para pintar rótulos) e versão leve sem eles."""
    low = gc.cylinder(name + "_low", (0.0, 0.0, bottom_z), (0.0, 0.0, top_z), radius, top_radius, segments=segments)
    bm = bmesh.new()
    bm.from_mesh(low.data)
    vertical = [e for e in bm.edges if abs(e.verts[0].co.z - e.verts[1].co.z) > 1e-4]
    bmesh.ops.subdivide_edges(bm, edges=vertical, cuts=24, use_grid_fill=False)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    high = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(high)
    tris = 2 * segments + 2 * (segments - 2)
    return gc.Part(high, paint, tris, low=low)


# ---------------------------------------------------------------------------
# Volume 1
# ---------------------------------------------------------------------------

def bread():
    def loaf(d, p):
        if d.z > 0.0:
            p.z *= 1.0 + 0.25 * d.z  # topo abaulado
        return p
    body = gc.ellipsoid("Loaf", (0.0, 0.0, 0.08), (0.15, 0.085, 0.08), segments=14, rings=10, exponent=0.45,
                        deform=loaf)

    def paint(co, n):
        crust = gc.lerp(CRUST, CRUST_DARK, gc.smoothstep(0.6, 1.0, n.z))
        return gc.lerp(crust, CREAM, gc.smoothstep(0.7, 0.9, abs(n.x)))  # fatia aparecendo nas pontas
    return piece("Bread", [gc.Part(body, paint, 120)])


def banana():
    parts = []
    for i, offset in enumerate((-0.035, 0.0, 0.035)):
        skeleton = gc.Skeleton()
        points = []
        for k in range(6):
            t = k / 5
            x = -0.13 + 0.26 * t
            z = 0.035 + 0.09 * (2 * t - 1) ** 2
            points.append(((x, offset, z + 0.01 * i), 0.012 if k in (0, 5) else 0.026))
        skeleton.chain(None, points)
        finger = gc.skin("Banana", skeleton, subsurf=1)

        def paint(co, n):
            return gc.lerp(BANANA, BANANA_TIP, gc.smoothstep(0.115, 0.13, abs(co.x)))
        parts.append(gc.Part(finger, paint, 60))
    return piece("Banana", parts)


def cheese():
    # Cunha: prisma triangular.
    bottom = gc.box("CheeseBase", (0.0, 0.0, 0.055), (0.24, 0.16, 0.11))
    for v in bottom.data.vertices:
        if v.co.z > 0.06 and v.co.y > 0.0:
            v.co.z = 0.11
        elif v.co.z > 0.06:
            v.co.z = 0.03  # o lado da frente desce: vira cunha

    def paint(co, n):
        holes = gc.smoothstep(0.55, 0.7, gc.turbulence(co, 18.0))
        return gc.lerp(CHEESE, CHEESE_HOLE, holes)
    high = bottom.copy()
    high.data = bottom.data.copy()
    bpy.context.scene.collection.objects.link(high)
    bm = bmesh.new()
    bm.from_mesh(high.data)
    bmesh.ops.subdivide_edges(bm, edges=bm.edges, cuts=14, use_grid_fill=True)
    bm.to_mesh(high.data)
    bm.free()
    return piece("Cheese", [gc.Part(high, paint, 12, low=bottom)])


def cookies():
    pack = gc.ellipsoid("Pack", (0.0, 0.0, 0.06), (0.13, 0.07, 0.06), segments=12, rings=8, exponent=0.35)
    return piece("Cookies", [gc.Part(pack, label(COOKIE_PACK, CREAM, 0.0, 0.12, 0.5, 0.2), 60)])


def milk():
    roof = gc.box("Roof", (0.0, 0.0, 0.245), (0.11, 0.11, 0.05))
    for v in roof.data.vertices:
        if v.co.z > 0.25:
            v.co.y *= 0.1  # telhadinho da caixa de leite
    return piece("Milk", [dense_box("Carton", (0.0, 0.0, 0.11), (0.11, 0.11, 0.22),
                                    label(WHITE, MILK_BLUE, 0.0, 0.22, 0.45, 0.2)),
                          gc.Part(roof, gc.flat(MILK_BLUE), 12)])


def jam():
    lid = gc.cylinder("Lid", (0.0, 0.0, 0.14), (0.0, 0.0, 0.17), 0.06, segments=12)
    return piece("Jam", [dense_cylinder("Jar", 0.0, 0.14, 0.065, label(JAM, CREAM, 0.0, 0.14, 0.5, 0.2)),
                         gc.Part(lid, gc.flat(LID_GOLD), 40)])


def energy_drink():
    return piece("EnergyDrink", [dense_cylinder("Can", 0.0, 0.2, 0.055, label(CAN_MAGENTA, CAN_LIME, 0.0, 0.2, 0.55, 0.12))])


def protein_bar():
    bar = gc.ellipsoid("Bar", (0.0, 0.0, 0.03), (0.13, 0.05, 0.03), segments=12, rings=8, exponent=0.3)

    def paint(co, n):
        return gc.lerp(BAR_PURPLE, BAR_GOLD, gc.band(co.x, 0.0, 0.04, 0.01))
    return piece("ProteinBar", [gc.Part(bar, paint, 60)])


# ---------------------------------------------------------------------------
# Volume 2 e 3
# ---------------------------------------------------------------------------

def watermelon():
    melon = gc.ellipsoid("Melon", (0.0, 0.0, 0.13), (0.19, 0.14, 0.13), segments=48, rings=24)

    def paint(co, n):
        angle = math.atan2(co.z - 0.13, co.y)
        return gc.lerp(MELON_LIGHT, MELON_DARK, gc.smoothstep(-0.3, 0.3, math.sin(angle * 7.0)))
    return piece("Watermelon", [gc.Part(melon, paint, 160)])


def water_jug():
    jug = gc.ellipsoid("Jug", (0.0, 0.0, 0.15), (0.12, 0.12, 0.15), segments=14, rings=10, exponent=0.5)
    neck = gc.cylinder("Neck", (0.0, 0.0, 0.29), (0.0, 0.0, 0.34), 0.04, segments=10)
    return piece("WaterJug", [gc.Part(jug, label(JUG_BLUE, WHITE, 0.0, 0.3, 0.5, 0.12), 120),
                              gc.Part(neck, gc.flat(CAP_BLUE), 30)])


def rice():
    sack = gc.ellipsoid("Sack", (0.0, 0.0, 0.17), (0.13, 0.08, 0.17), segments=12, rings=10, exponent=0.4)
    return piece("Rice", [gc.Part(sack, label(SACK, RED, 0.0, 0.34, 0.55, 0.15), 90)])


def pumpkin():
    def ribs(d, p):
        angle = math.atan2(d.y, d.x)
        bulge = 1.0 + 0.07 * math.cos(angle * 8.0)
        return Vector((p.x * bulge, p.y * bulge, p.z))
    body = gc.ellipsoid("Pumpkin", (0.0, 0.0, 0.11), (0.16, 0.16, 0.11), segments=24, rings=12, deform=ribs)
    stem = gc.cylinder("Stem", (0.0, 0.0, 0.2), (0.01, 0.0, 0.27), 0.022, 0.014, segments=6)
    return piece("Pumpkin", [gc.Part(body, stripes_z(PUMPKIN_RIB, PUMPKIN, 8.0), 180),
                             gc.Part(stem, gc.flat(STEM), 16)])


def eggs():
    box = gc.box("EggBox", (0.0, 0.0, 0.04), (0.3, 0.14, 0.08))
    parts = [gc.Part(box, gc.flat(EGG_BOX), 12)]
    for i in range(3):
        for j in (-1, 1):
            egg = gc.ellipsoid("Egg", (-0.1 + 0.1 * i, j * 0.035, 0.09), (0.03, 0.03, 0.035), segments=8, rings=6)
            parts.append(gc.Part(egg, gc.flat(EGG), 24))
    return piece("Eggs", parts)


def cereal():
    return piece("Cereal", [dense_box("CerealBox", (0.0, 0.0, 0.19), (0.26, 0.1, 0.38),
                                      label(CEREAL, CEREAL_BAND, 0.0, 0.38, 0.6, 0.15))])


def popcorn():
    def paint(co, n):
        return gc.lerp(WHITE, POPCORN_RED, gc.smoothstep(-0.3, 0.3, math.sin((co.x + co.y) * 60.0)))
    puff = gc.ellipsoid("Puff", (0.0, 0.0, 0.33), (0.12, 0.05, 0.04), segments=10, rings=6)
    return piece("Popcorn", [dense_box("PopcornBox", (0.0, 0.0, 0.16), (0.26, 0.12, 0.32), paint),
                             gc.Part(puff, gc.flat(CREAM), 40)])


def juice():
    neck = gc.cylinder("Neck", (0.0, 0.0, 0.22), (0.0, 0.0, 0.29), 0.05, 0.025, segments=12)
    cap = gc.cylinder("Cap", (0.0, 0.0, 0.29), (0.0, 0.0, 0.32), 0.027, segments=8)
    return piece("Juice", [dense_cylinder("Bottle", 0.0, 0.22, 0.06, label(JUICE, WHITE, 0.0, 0.22, 0.5, 0.18)),
                           gc.Part(neck, gc.flat(JUICE), 40), gc.Part(cap, gc.flat(GLASS_CAP), 20)])


def olive_oil():
    neck = gc.cylinder("Neck", (0.0, 0.0, 0.24), (0.0, 0.0, 0.33), 0.045, 0.018, segments=12)
    cap = gc.cylinder("Cap", (0.0, 0.0, 0.33), (0.0, 0.0, 0.36), 0.02, segments=8)
    return piece("OliveOil", [dense_cylinder("Bottle", 0.0, 0.24, 0.05, label(OIL, CREAM, 0.0, 0.24, 0.45, 0.16)),
                              gc.Part(neck, gc.flat(OIL_DARK), 40), gc.Part(cap, gc.flat(LID_GOLD), 20)])


# ---------------------------------------------------------------------------
# Volume 4 e 5
# ---------------------------------------------------------------------------

def giant_chips():
    def crimp(d, p):
        if abs(d.z) > 0.85:
            p.y *= 0.3  # as pontas do pacote são fechadas e achatadas
        return p
    bag = gc.ellipsoid("ChipsBag", (0.0, 0.0, 0.25), (0.2, 0.1, 0.25), segments=14, rings=12, exponent=0.5,
                       deform=crimp)

    def paint(co, n):
        return gc.lerp(CHIPS_RED, CHIPS_YELLOW, gc.band(co.z / 0.5, 0.5, 0.14, 0.04))
    return piece("GiantChips", [gc.Part(bag, paint, 140)])


def toilet_paper():
    parts = []
    for i in range(3):
        for j in range(2):
            roll = gc.cylinder("Roll", (-0.15 + 0.15 * i, -0.075 + 0.15 * j, 0.0),
                               (-0.15 + 0.15 * i, -0.075 + 0.15 * j, 0.28), 0.07, segments=10)
            parts.append(gc.Part(roll, gc.flat(WHITE), 32))
    wrap = gc.box("Wrap", (0.0, 0.0, 0.14), (0.45, 0.3, 0.1))

    def paint(co, n):
        return PLASTIC if abs(n.z) > 0.5 else gc.lerp(PLASTIC, MILK_BLUE, 0.8)
    parts.append(gc.Part(wrap, paint, 12))
    return piece("ToiletPaper", parts)


def build():
    return [
        bread(), banana(), cheese(), cookies(), milk(), jam(), energy_drink(), protein_bar(),
        watermelon(), water_jug(), rice(), pumpkin(), eggs(), cereal(), popcorn(), juice(), olive_oil(),
        giant_chips(), toilet_paper(),
    ]


if __name__ == "__main__":
    gc.run_model("Items_Kit", "Items", build, texture_size=1024,
                 bake_cage=0.005, bake_ray=0.01, ao_distance=0.08,
                 blotch_scale=12.0, stroke_scale=60.0)
