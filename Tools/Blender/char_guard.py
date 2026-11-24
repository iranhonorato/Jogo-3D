"""Gera o segurança (Char_Guard.fbx e textura) — GDD §6, §15.1, §15.2 e referência em docs/.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/char_guard.py -- [--preview <pasta>]

Homem corpulento de cerca de 1,80 m, uniforme azul-marinho (tons frios, GDD §15.2), quepe,
bigode, rádio no peito, cinto com bolsas e chaves, lanterna na mão direita apontando para a
frente. O empty Socket_Flashlight marca a lente: é onde a luz spot da visão fica na Unity.
Frente em -Y.
"""

import math
import os
import sys

from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

SHIRT = gc.srgb("#384875")
PANTS = gc.srgb("#2b365b")
CAP = gc.srgb("#2a355a")
VISOR = gc.srgb("#14182a")
BLACK = gc.srgb("#19191b")
BOOT = gc.srgb("#4a3022")  # botas marrons da referência
GOLD = gc.srgb("#c2a253")
STEEL = gc.srgb("#8e9298")
SKIN = gc.srgb("#dca886")
SKIN_RED = gc.srgb("#c98068")
STUBBLE = gc.srgb("#a98370")
HAIR = gc.srgb("#2a211c")
EYE = gc.srgb("#1a1410")
LENS = gc.srgb("#fff0b0")
TEE = gc.srgb("#ecebe6")
SLEEVE_LENGTH = 0.2  # metros do ombro até a dobra da manga curta

HEAD_CENTER = Vector((0.0, -0.02, 1.665))
HEAD_RADII = (0.095, 0.105, 0.12)
BELT_Z = (0.985, 1.04)

LEFT_ARM = (
    ((0.20, 0.0, 1.43), 0.082),
    ((0.265, 0.015, 1.17), 0.064),
    ((0.275, -0.01, 0.95), 0.05),
    ((0.27, -0.02, 0.87), 0.052),   # mão
    ((0.268, -0.03, 0.81), 0.036),
)
RIGHT_ARM = (  # segura a lanterna para a frente
    ((-0.20, 0.0, 1.43), 0.082),
    ((-0.265, 0.04, 1.18), 0.064),
    ((-0.24, -0.17, 1.08), 0.05),
    ((-0.23, -0.24, 1.07), 0.052),  # mão
    ((-0.225, -0.28, 1.07), 0.038),
)
FLASHLIGHT_AXIS = (Vector((-0.228, -0.20, 1.07)), Vector((-0.228, -0.40, 1.07)))


def build_body():
    skeleton = gc.Skeleton()
    pelvis = skeleton.joint((0.0, 0.0, 0.96), 0.16)
    belly = skeleton.joint((0.0, -0.05, 1.13), 0.205, pelvis)
    chest = skeleton.joint((0.0, -0.02, 1.32), 0.19, belly)
    shoulders = skeleton.joint((0.0, 0.0, 1.45), 0.16, chest)
    skeleton.joint((0.0, -0.005, 1.57), 0.075, shoulders)  # pescoço
    skeleton.mirrored_chain(pelvis, (
        ((0.115, 0.0, 0.90), 0.118),    # quadril
        ((0.12, -0.015, 0.52), 0.092),  # joelho
        ((0.12, 0.01, 0.15), 0.066),    # tornozelo
        ((0.12, -0.06, 0.075), 0.066),  # bota
        ((0.12, -0.15, 0.06), 0.052),   # bico da bota
    ))
    return gc.skin("Body", skeleton, subsurf=3)


def paint_body(co, n):
    front = gc.smoothstep(0.2, 0.5, -n.y)
    color = gc.lerp(gc.shade(BOOT, 0.6), BOOT, gc.above(co.z, 0.022))
    color = gc.lerp(color, PANTS, gc.above(co.z, 0.17))

    buckle = gc.band(co.x, 0.0, 0.03, 0.006) * gc.smoothstep(0.3, 0.6, -n.y)
    color = gc.lerp(color, gc.lerp(BLACK, GOLD, buckle), gc.above(co.z, BELT_Z[0]))

    pocket = gc.band(abs(co.x), 0.085, 0.04, 0.008) * gc.band(co.z, 1.33, 0.04, 0.008) * front
    badge = gc.band(co.x, 0.085, 0.02, 0.006) * gc.band(co.z, 1.395, 0.022, 0.006) * front
    shirt = gc.lerp(gc.shade(SHIRT, 1.0 - 0.18 * pocket), GOLD, badge)
    color = gc.lerp(color, shirt, gc.above(co.z, BELT_Z[1]))
    tee = gc.band(co.x, 0.0, 0.05, 0.01) * gc.band(co.z, 1.51, 0.025, 0.006) * front  # camiseta na gola
    color = gc.lerp(color, TEE, tee)
    return gc.lerp(color, SKIN, gc.above(co.z, 1.53))  # pescoço


def build_arm(points, name, hand):
    shoulder = Vector(points[0][0])
    skeleton = gc.Skeleton()
    skeleton.chain(None, points)
    arm = gc.skin(name, skeleton)

    def paint_arm(co, n):
        # Manga curta dobrada (referência de arte): a dobra mais escura e o antebraço à mostra.
        from_shoulder = (co - shoulder).length
        cuff = gc.band(from_shoulder, SLEEVE_LENGTH - 0.02, 0.022, 0.006)
        sleeve = gc.lerp(SHIRT, gc.shade(SHIRT, 0.7), cuff)
        return gc.lerp(sleeve, SKIN, gc.smoothstep(SLEEVE_LENGTH, SLEEVE_LENGTH + 0.008, from_shoulder))

    return gc.Part(arm, paint_arm, 200)


def head_deform(d, p):
    jaw = gc.smoothstep(0.0, -0.7, d.z)
    p.x *= 1.0 + 0.1 * jaw
    p.y -= 0.012 * jaw * max(0.0, -d.y)
    return p


def paint_head(co, n):
    q = co - HEAD_CENTER
    d = Vector((q.x / HEAD_RADII[0], q.y / HEAD_RADII[1], q.z / HEAD_RADII[2])).normalized()
    color = gc.lerp(SKIN, STUBBLE, 0.6 * gc.smoothstep(-0.25, -0.55, d.z) * gc.smoothstep(0.0, 0.4, -d.y))
    hair = gc.smoothstep(-0.25, 0.05, d.z) * gc.smoothstep(-0.35, 0.0, d.y)
    sideburn = gc.band(abs(d.x), 0.9, 0.12, 0.05) * gc.band(d.z, 0.05, 0.25, 0.08)
    return gc.lerp(color, HAIR, max(hair, sideburn))


def build_head_details(head):
    c = HEAD_CENTER
    parts = []

    hit, normal = gc.front_hit(head, 0.0, c.z - 0.01)
    nose = gc.ellipsoid("Nose", hit + normal * 0.01, (0.02, 0.022, 0.028), segments=12, rings=8)
    parts.append(gc.Part(nose, gc.flat(SKIN_RED), 70))

    def droop(d, p):
        p.z -= 0.012 * abs(d.x)
        return p

    hit, normal = gc.front_hit(head, 0.0, c.z - 0.045)
    mustache = gc.ellipsoid("Mustache", hit + normal * 0.008, (0.048, 0.016, 0.015),
                            segments=14, rings=8, deform=droop)
    parts.append(gc.Part(mustache, gc.flat(HAIR), 80))

    for side in (1, -1):
        suffix = "_L" if side > 0 else "_R"
        hit, normal = gc.front_hit(head, 0.036 * side, c.z + 0.028)
        eye = gc.ellipsoid("Eye" + suffix, hit - normal * 0.004, (0.011,) * 3, segments=8, rings=6)
        hit, normal = gc.front_hit(head, 0.04 * side, c.z + 0.056)
        brow = gc.ellipsoid("Brow" + suffix, hit + normal * 0.004, (0.025, 0.008, 0.007), segments=8, rings=6)
        hit, normal = gc.surface_hit(head, (side, c.y + 0.005, c.z), (-side, 0.0, 0.0))
        ear = gc.ellipsoid("Ear" + suffix, hit + normal * 0.004, (0.014, 0.028, 0.034), segments=10, rings=8)
        parts += [gc.Part(eye, gc.flat(EYE), 40), gc.Part(brow, gc.flat(HAIR), 30),
                  gc.Part(ear, gc.flat(SKIN), 60)]

    crown_bottom, crown_top = c.z + 0.045, c.z + 0.13
    crown = gc.cylinder("CapCrown", (0.0, c.y + 0.005, crown_bottom), (0.0, c.y + 0.005, crown_top),
                        0.104, 0.112, segments=20)

    def paint_crown(co, n):
        badge = gc.band(co.x, 0.0, 0.022, 0.006) * gc.band(co.z, crown_bottom + 0.05, 0.02, 0.006) \
            * gc.smoothstep(0.5, 0.8, -n.y)
        color = gc.lerp(CAP, BLACK, 1.0 - gc.smoothstep(crown_bottom + 0.018, crown_bottom + 0.024, co.z))
        return gc.lerp(color, GOLD, badge)

    def slope(d, p):
        p.z += 0.25 * p.y
        return p

    visor = gc.ellipsoid("CapVisor", (0.0, c.y - 0.085, crown_bottom + 0.008), (0.09, 0.065, 0.011),
                         segments=16, rings=8, deform=slope)
    parts += [gc.Part(crown, paint_crown, 130), gc.Part(visor, gc.flat(VISOR), 110)]
    return parts


def build_gear(body):
    parts = []

    hit, normal = gc.front_hit(body, -0.1, 1.42)
    radio_center = hit + normal * 0.016
    parts.append(gc.Part(gc.box("Radio", radio_center, (0.045, 0.028, 0.075)), gc.flat(BLACK), 12))
    antenna = gc.cylinder("Antenna", radio_center + Vector((0.012, 0.0, 0.037)),
                          radio_center + Vector((0.012, 0.0, 0.1)), 0.005, segments=6)
    parts.append(gc.Part(antenna, gc.flat(BLACK), 24))

    for side in (1, -1):
        hit, normal = gc.front_hit(body, 0.14 * side, 1.01)
        pouch = gc.box("Pouch_L" if side > 0 else "Pouch_R", hit + normal * 0.02, (0.055, 0.04, 0.06))
        parts.append(gc.Part(pouch, gc.flat(BLACK), 12))

    hit, normal = gc.surface_hit(body, (1.0, 0.03, 0.975), (-1.0, 0.0, 0.0))
    ring_center = hit + normal * 0.012
    parts.append(gc.Part(gc.torus("KeyRing", ring_center, (1.0, 0.0, 0.0), 0.018, 0.003, 12, 4),
                         gc.flat(STEEL), 96))
    for offset in (-0.01, 0.012):
        key = gc.box("Key", ring_center + Vector((0.003, offset, -0.032)), (0.004, 0.012, 0.035))
        parts.append(gc.Part(key, gc.flat(GOLD if offset > 0 else STEEL), 12))

    start, end = FLASHLIGHT_AXIS
    head_end = end + Vector((0.0, -0.055, 0.0))
    lens_end = head_end + Vector((0.0, -0.007, 0.0))
    parts += [
        gc.Part(gc.cylinder("FlashlightBody", start, end, 0.024, segments=12), gc.flat(BLACK), 50),
        gc.Part(gc.cylinder("FlashlightHead", end, head_end, 0.026, 0.04, segments=12), gc.flat(BLACK), 50),
        gc.Part(gc.cylinder("FlashlightLens", head_end, lens_end, 0.036, segments=12), gc.flat(LENS), 50),
    ]
    return parts, [("Socket_Flashlight", lens_end + Vector((0.0, -0.008, 0.0)))]


def build():
    body = build_body()
    head = gc.ellipsoid("Head", HEAD_CENTER, HEAD_RADII, segments=40, rings=28, deform=head_deform)
    parts = [
        gc.Part(body, paint_body, 880),
        build_arm(LEFT_ARM, "Arm_L", Vector(LEFT_ARM[3][0])),
        build_arm(RIGHT_ARM, "Arm_R", Vector(RIGHT_ARM[3][0])),
        gc.Part(head, paint_head, 420),
    ]
    parts += build_head_details(head)
    gear, sockets = build_gear(body)
    return parts + gear, sockets


if __name__ == "__main__":
    gc.run_character("Char_Guard", build, budget_tris=3000, texture_size=1024,
                     bake_cage=0.015, bake_ray=0.03, ao_distance=0.3,
                     blotch_scale=4.0, stroke_scale=50.0)
