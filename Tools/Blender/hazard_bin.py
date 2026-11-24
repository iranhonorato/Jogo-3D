"""Gera a lixeira (Hazards/Hazard_Bin.fbx e textura) — GDD §9 e a referência de arte da pasta de próximos passos.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/hazard_bin.py -- [--preview <pasta>]

Lixeira quadrada de metal gasto, com frisos verticais, pés, borda e tampa basculante abaulada. Pivô na base,
frente em -Y, 0,9 m de altura (o collider do prefab é uma cápsula de 0,35 m de raio e 0,9 m de altura).
A tampa é o filho `Lid`, com o pivô na dobradiça do fundo (+Y), para o BinLidAnimator girar em X na Unity.
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

METAL = gc.srgb("#9d8ea6")
METAL_DARK = gc.srgb("#64586e")
RIM = gc.srgb("#7d7086")
RUST = gc.srgb("#8a5a3c")
FOOT = gc.srgb("#2f2a33")

WIDTH = 0.6
FOOT_HEIGHT = 0.04
BASE_TOP = 0.1
BODY_TOP = 0.72
RIM_TOP = 0.78
LID_HEIGHT = 0.12
TAPER = 0.9  # a base é 10% mais estreita que o topo
RIB_COUNT = 4


def wear(color, co):
    """Metal gasto: manchas de ferrugem e variação de tom."""
    rust = gc.smoothstep(0.55, 0.8, gc.turbulence(co, 7.0))
    return gc.lerp(gc.shade(color, 0.9 + 0.2 * gc.turbulence(co, 25.0)), RUST, 0.45 * rust)


def body():
    size = (WIDTH, WIDTH, BODY_TOP - BASE_TOP)
    center = (0.0, 0.0, (BODY_TOP + BASE_TOP) / 2)
    high = gc.grid_box("Body", center, size, spacing=0.015, skip=("-z", "+z"))
    low = gc.grid_box("Body_low", center, size, skip=("-z", "+z"))
    for obj in (high, low):
        for v in obj.data.vertices:
            t = (v.co.z - BASE_TOP) / (BODY_TOP - BASE_TOP)
            scale = TAPER + (1.0 - TAPER) * t
            v.co.x *= scale
            v.co.y *= scale

    def paint(co, n):
        along = co.x if abs(n.y) > abs(n.x) else co.y
        u = (along / (WIDTH * 0.5) + 1.0) / 2.0 * RIB_COUNT  # 0 a RIB_COUNT na largura da face
        groove = gc.band(u % 1.0, 0.5, 0.12, 0.04) * gc.band(co.z, (BODY_TOP + BASE_TOP) / 2, 0.24, 0.02)
        return wear(gc.lerp(METAL, METAL_DARK, groove), co)

    return gc.Part(high, paint, 40, low=low)


def base_and_rim():
    parts = []
    base = gc.box("BaseRim", (0.0, 0.0, (FOOT_HEIGHT + BASE_TOP) / 2), (WIDTH * TAPER + 0.04, WIDTH * TAPER + 0.04,
                                                                       BASE_TOP - FOOT_HEIGHT))
    top = gc.box("TopRim", (0.0, 0.0, (BODY_TOP + RIM_TOP) / 2), (WIDTH + 0.05, WIDTH + 0.05, RIM_TOP - BODY_TOP))
    parts += [gc.Part(base, lambda co, n: wear(RIM, co), 12), gc.Part(top, lambda co, n: wear(RIM, co), 12)]
    corner = WIDTH * TAPER / 2 - 0.04
    for sx in (-1, 1):
        for sy in (-1, 1):
            foot = gc.cylinder("Foot", (sx * corner, sy * corner, 0.0), (sx * corner, sy * corner, FOOT_HEIGHT),
                               0.035, segments=8)
            parts.append(gc.Part(foot, gc.flat(FOOT), 28))
    return parts


def lid():
    def dome(d, p):
        if d.z < 0.0:
            p.z = 0.0  # meia cúpula: a parte de baixo fica achatada na borda
        return p

    half = (WIDTH + 0.03) / 2
    shell = gc.ellipsoid("LidDome", (0.0, 0.0, RIM_TOP), (half, half, LID_HEIGHT * 1.3), segments=16, rings=10,
                         exponent=0.45, deform=dome)

    def paint(co, n):
        flap = gc.band(co.x, 0.0, 0.2, 0.01) * gc.band(co.y, -0.06, 0.16, 0.01) * gc.smoothstep(0.2, 0.5, n.z)
        edge = gc.band(co.x, 0.0, 0.21, 0.004) * gc.band(co.y, -0.06, 0.17, 0.004) * (1.0 - flap)
        return wear(gc.lerp(gc.lerp(METAL, METAL_DARK, edge), gc.shade(METAL, 1.1), flap), co)

    return gc.Part(shell, paint, 300)


def build():
    hinge = (0.0, (WIDTH + 0.03) / 2, RIM_TOP)
    return [
        gc.Piece("Hazard_Bin", [body()] + base_and_rim()),
        gc.Piece("Lid", [lid()], pivot=hinge, parent="Hazard_Bin"),
    ]


if __name__ == "__main__":
    gc.run_model("Hazard_Bin", "Hazards", build, texture_size=1024,
                 bake_cage=0.006, bake_ray=0.012, ao_distance=0.1,
                 blotch_scale=8.0, stroke_scale=60.0)
