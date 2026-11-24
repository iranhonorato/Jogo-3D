"""Gera o guaxinim (Char_Raccoon.fbx e textura) — GDD §4, §15.1 e referência em docs/.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/char_raccoon.py -- [--preview <pasta>]

Guaxinim em pé, com cerca de 0,6 m, máscara escura, cauda anelada e bolsa de couro
transversal (sem o cajado da referência, GDD §15.1). Frente em -Y. Detalhes da referência de
arte (pasta de próximos passos): bochechas com pontas de pelo, cabeça grande, brilho nos olhos,
cauda cheia com a ponta clara e bolsa com aba e fivela.
"""

import math
import os
import sys

from mathutils import Euler, Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

FUR = gc.srgb("#8a8079")
FUR_DARK = gc.srgb("#575049")
CREAM = gc.srgb("#efe7da")
EAR_INNER = gc.srgb("#b5a794")
MASK = gc.srgb("#1a1715")
BRIDGE = gc.srgb("#4a4038")
NOSE = gc.srgb("#141212")
EYE = gc.srgb("#1a120c")
PAW = gc.srgb("#26211e")
TAIL_LIGHT = gc.srgb("#cbbda8")
TAIL_DARK = gc.srgb("#332d28")
LEATHER = gc.srgb("#7b4b2b")
LEATHER_DARK = gc.srgb("#5c3720")
BRASS = gc.srgb("#d8b04e")
EYE_SHINE = gc.srgb("#ffffff")

HEAD_CENTER = Vector((0.0, -0.035, 0.44))
HEAD_RADII = (0.071, 0.063, 0.057)  # cabeça grande: lê bem de cima
SNOUT_DIR = Vector((0.0, -1.0, -0.35)).normalized()
SNOUT_LENGTH = 0.035

ARM = (
    ((0.045, -0.01, 0.345), 0.032),   # ombro
    ((0.07, -0.02, 0.275), 0.025),    # cotovelo
    ((0.06, -0.06, 0.235), 0.019),    # pulso
    ((0.055, -0.078, 0.225), 0.021),  # mão
    ((0.052, -0.092, 0.218), 0.015),  # dedos
)
HANDS = (Vector((0.055, -0.082, 0.223)), Vector((-0.055, -0.082, 0.223)))

TAIL = [Vector(p) for p in (
    (0.0, 0.06, 0.15), (0.01, 0.12, 0.09), (0.03, 0.19, 0.055), (0.055, 0.26, 0.05),
    (0.075, 0.32, 0.065), (0.085, 0.36, 0.1), (0.08, 0.38, 0.14),
)]
TAIL_RADII = (0.034, 0.056, 0.066, 0.066, 0.06, 0.048, 0.024)  # cauda cheia


def place(obj, location, rotation_degrees=(0.0, 0.0, 0.0)):
    rotation = Euler([math.radians(a) for a in rotation_degrees]).to_matrix().to_4x4()
    obj.data.transform(Matrix.Translation(Vector(location)) @ rotation)


# ---------------------------------------------------------------------------
# Corpo
# ---------------------------------------------------------------------------

def build_body():
    skeleton = gc.Skeleton()
    pelvis = skeleton.joint((0.0, 0.02, 0.17), 0.08)
    belly = skeleton.joint((0.0, 0.0, 0.25), 0.082, pelvis)
    chest = skeleton.joint((0.0, -0.012, 0.325), 0.066, belly)
    skeleton.joint((0.0, -0.025, 0.395), 0.05, chest)  # pescoço, entra na cabeça
    skeleton.mirrored_chain(pelvis, (
        ((0.05, 0.025, 0.15), 0.055),    # quadril
        ((0.06, -0.01, 0.09), 0.042),    # joelho
        ((0.058, 0.025, 0.04), 0.028),   # tornozelo
        ((0.058, -0.02, 0.022), 0.03),   # pé
        ((0.058, -0.06, 0.018), 0.022),  # dedos
    ))
    return gc.skin("Body", skeleton)


def paint_body(co, n):
    color = gc.lerp(FUR, FUR_DARK, 0.5 * gc.smoothstep(0.0, 0.8, n.y))
    chest = (gc.smoothstep(0.1, 0.5, -n.y) * gc.band(co.x, 0.0, 0.035, 0.02)
             * gc.band(co.z, 0.3, 0.085, 0.03))
    color = gc.lerp(color, CREAM, chest)
    return gc.lerp(color, PAW, 1.0 - gc.smoothstep(0.03, 0.06, co.z))


def build_arm(side):
    skeleton = gc.Skeleton()
    skeleton.chain(None, [((x * side, y, z), r) for (x, y, z), r in ARM])
    return gc.skin("Arm_L" if side > 0 else "Arm_R", skeleton)


def paint_arm(co, n):
    distance = min((co - hand).length for hand in HANDS)
    color = gc.lerp(FUR, FUR_DARK, 0.4 * gc.smoothstep(0.0, 0.8, n.y))
    return gc.lerp(color, PAW, 1.0 - gc.smoothstep(0.02, 0.05, distance))


def build_tail():
    skeleton = gc.Skeleton()
    skeleton.chain(None, list(zip([tuple(p) for p in TAIL], TAIL_RADII)))
    return gc.skin("Tail", skeleton)


def paint_tail(co, n):
    t, _ = gc.closest_on_polyline(Vector(co), TAIL)
    ring = gc.smoothstep(-0.3, 0.3, math.sin(2.0 * math.pi * (t - 0.14) * 5.0)) if t > 0.14 else 0.0
    color = gc.lerp(TAIL_LIGHT, TAIL_DARK, ring if t < 0.88 else 0.0)
    color = gc.lerp(color, CREAM, gc.smoothstep(0.9, 0.97, t))  # ponta clara
    return gc.lerp(FUR, color, gc.smoothstep(0.04, 0.14, t))


# ---------------------------------------------------------------------------
# Cabeça
# ---------------------------------------------------------------------------

def snout_weight(d):
    """Mesmo peso usado para esculpir o focinho: 1 na ponta, cai rápido para os lados."""
    return max(0.0, d.dot(SNOUT_DIR)) ** 3


def head_deform(d, p):
    weight = snout_weight(d)
    along = p.dot(SNOUT_DIR)
    perpendicular = p - SNOUT_DIR * along
    p = SNOUT_DIR * along + perpendicular * (1.0 - 0.5 * weight) + SNOUT_DIR * (SNOUT_LENGTH * weight)
    cheek = gc.smoothstep(0.35, 0.9, abs(d.x)) * (1.0 - gc.smoothstep(0.1, 0.6, abs(d.z + 0.2)))
    p.x *= 1.0 + 0.4 * cheek
    p.y += 0.012 * cheek
    # Pontas de pelo nas bochechas (a referência tem a cara bem fofa dos lados).
    tufts = 0.5 + 0.5 * math.sin(9.0 * math.atan2(d.z, d.y))
    p.x += math.copysign(0.014 * cheek * tufts, d.x)
    if d.z > 0.0:
        p.z *= 1.0 - 0.12 * d.z
    return p


def head_direction(co):
    q = Vector(co) - HEAD_CENTER
    return Vector((q.x / HEAD_RADII[0], q.y / HEAD_RADII[1], q.z / HEAD_RADII[2])).normalized()


def paint_head(co, n):
    d = head_direction(co)
    snout = snout_weight(d)
    front = gc.smoothstep(0.1, 0.5, -d.y)
    away_from_bridge = gc.smoothstep(0.08, 0.18, abs(d.x))

    color = gc.lerp(FUR, FUR_DARK, 0.5 * gc.smoothstep(0.3, 0.9, d.z) + 0.4 * gc.smoothstep(0.2, 0.9, d.y))
    brow = gc.band(d.z, 0.45, 0.08, 0.06) * front * away_from_bridge
    muzzle = gc.smoothstep(0.5, 0.72, snout)
    chin = gc.smoothstep(0.35, 0.7, -d.z) * front
    color = gc.lerp(color, CREAM, max(brow, muzzle, chin))

    bridge = (1.0 - away_from_bridge) * front * gc.smoothstep(-0.1, 0.25, d.z) * (1.0 - muzzle)
    color = gc.lerp(color, BRIDGE, bridge)

    eye_band = (gc.band(d.z, 0.18, 0.15, 0.06) * front * away_from_bridge
                * (1.0 - gc.smoothstep(0.5, 0.7, snout)))
    cheek = gc.band(d.z, 0.0, 0.12, 0.08) * gc.smoothstep(0.45, 0.75, abs(d.x)) * gc.smoothstep(-0.3, 0.1, -d.y)
    return gc.lerp(color, MASK, max(eye_band, 0.9 * cheek))


def build_ear(head, side):
    def deform(d, p):
        p.x *= 1.0 - 0.45 * gc.smoothstep(-0.2, 1.0, d.z)
        p.z += 0.02
        return p

    ear = gc.ellipsoid("Ear_L" if side > 0 else "Ear_R", (0.0, 0.0, 0.0), (0.022, 0.009, 0.026),
                       segments=14, rings=10, deform=deform)
    hit, normal = gc.surface_hit(head, (0.036 * side, HEAD_CENTER.y + 0.012, 1.0), (0.0, 0.0, -1.0))
    place(ear, hit - normal * 0.008, (-10.0, 25.0 * side, 0.0))
    top = max(v.co.z for v in ear.data.vertices)

    def paint_ear(co, n):
        color = gc.lerp(FUR_DARK, EAR_INNER, gc.smoothstep(0.0, 0.5, -n.y))
        return gc.lerp(color, CREAM, gc.smoothstep(top - 0.014, top - 0.002, co.z))

    return ear, paint_ear


def build_face_details(head):
    parts = []
    for side in (1, -1):
        hit, normal = gc.front_hit(head, 0.028 * side, HEAD_CENTER.z + 0.014)
        eye = gc.ellipsoid("Eye_L" if side > 0 else "Eye_R", hit - normal * 0.0045, (0.0095,) * 3,
                           segments=10, rings=8)
        shine = gc.ellipsoid("EyeShine", hit + normal * 0.0035 + Vector((0.003 * side, 0.0, 0.004)),
                             (0.0028,) * 3, segments=6, rings=4)
        parts += [gc.Part(eye, gc.flat(EYE), 80), gc.Part(shine, gc.flat(EYE_SHINE), 16)]
    tip = max((v.co for v in head.data.vertices), key=lambda co: (co - HEAD_CENTER).dot(SNOUT_DIR))
    nose = gc.ellipsoid("Nose", tip + SNOUT_DIR * 0.001, (0.012, 0.01, 0.009), segments=10, rings=8)
    parts.append(gc.Part(nose, gc.flat(NOSE), 80))
    return parts


# ---------------------------------------------------------------------------
# Bolsa transversal
# ---------------------------------------------------------------------------

def build_bag(body):
    hit, _ = gc.surface_hit(body, (-1.0, -0.005, 0.215), (1.0, 0.0, 0.0))
    center = hit + Vector((-0.019, 0.0, 0.0))
    bag = gc.ellipsoid("Bag", center, (0.021, 0.048, 0.04), segments=16, rings=12, exponent=0.55)

    def paint_bag(co, n):
        flap = gc.smoothstep(center.z - 0.004, center.z + 0.004, co.z) * gc.smoothstep(0.2, 0.6, -n.x)
        color = gc.lerp(LEATHER, LEATHER_DARK, 0.7 * flap)
        return gc.shade(color, 1.0 + 0.12 * gc.turbulence(co, 60.0))

    flap = gc.box("Flap", center + Vector((-0.021, 0.0, 0.014)), (0.006, 0.09, 0.032))
    buckle = gc.box("Buckle", center + Vector((-0.025, 0.0, 0.002)), (0.004, 0.014, 0.012))

    path = []
    for u in (0.0, 0.25, 0.5, 0.75, 1.0):  # costas, do quadril direito ao ombro esquerdo
        x, z = -0.055 + 0.095 * u, 0.25 + 0.12 * u
        point, normal = gc.surface_hit(body, (x, 5.0, z), (0.0, -1.0, 0.0))
        path.append(point + normal * 0.004)
    top, normal = gc.surface_hit(body, (0.045, -0.005, 1.0), (0.0, 0.0, -1.0))
    path.append(top + normal * 0.004)
    for u in (1.0, 0.75, 0.5, 0.25, 0.0):  # frente, do ombro de volta ao quadril
        x, z = -0.055 + 0.095 * u, 0.25 + 0.12 * u
        point, normal = gc.front_hit(body, x, z)
        path.append(point + normal * 0.004)
    skeleton = gc.Skeleton()
    skeleton.chain(None, [(tuple(p), 0.005) for p in path])
    strap = gc.skin("Strap", skeleton, subsurf=1)

    return [gc.Part(bag, paint_bag, 160), gc.Part(flap, gc.flat(LEATHER_DARK), 12),
            gc.Part(buckle, gc.flat(BRASS), 12),
            gc.Part(strap, gc.flat(LEATHER_DARK), 160)]


def build():
    body = build_body()
    head = gc.ellipsoid("Head", HEAD_CENTER, HEAD_RADII, segments=48, rings=32, deform=head_deform)
    parts = [
        gc.Part(body, paint_body, 760),
        gc.Part(build_arm(1), paint_arm, 170),
        gc.Part(build_arm(-1), paint_arm, 170),
        gc.Part(build_tail(), paint_tail, 400),
        gc.Part(head, paint_head, 580),
    ]
    for side in (1, -1):
        ear, paint_ear = build_ear(head, side)
        parts.append(gc.Part(ear, paint_ear, 60))
    parts += build_face_details(head)
    parts += build_bag(body)
    return parts, []


if __name__ == "__main__":
    gc.run_character("Char_Raccoon", build, budget_tris=3300, texture_size=1024,
                     bake_cage=0.01, bake_ray=0.02, ao_distance=0.12,
                     blotch_scale=9.0, stroke_scale=120.0, target_height=0.6)
