"""Gera o Seu Zé, o dono e faxineiro do Mercadinho (Char_SeuZe.fbx e textura).

GDD §11 (segurança da fase 1), §15.1 e referência em docs/.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/char_seu_ze.py -- [--preview <pasta>]

Senhor magro e um pouco curvado, com cerca de 1,60 m: careca com tufos brancos, óculos,
nariz grande, bigode, camisa xadrez com as mangas dobradas, avental manchado, calça oliva,
meias e chinelos. Lanterna na mão direita (Socket_Flashlight) e vassoura na esquerda.
Frente em -Y.
"""

import math
import os
import sys

from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

SHIRT = gc.srgb("#8c3a2b")  # cor base do xadrez
PANTS = gc.srgb("#6b6646")
SOCK = gc.srgb("#e2ddd2")
SLIPPER = gc.srgb("#6a4a33")
SKIN = gc.srgb("#e2b094")
SKIN_ROSY = gc.srgb("#d48a73")
HAIR = gc.srgb("#dcd8d0")
EYE = gc.srgb("#1a1410")
GLASSES = gc.srgb("#2a2420")
APRON = gc.srgb("#ddd3bd")
STAIN = gc.srgb("#a68c68")
WOOD = gc.srgb("#a07848")
STRAW = gc.srgb("#c8a35e")
BAND = gc.srgb("#8a3b2b")
BLACK = gc.srgb("#1c1c1e")
LENS = gc.srgb("#fff0b0")

HEAD_CENTER = Vector((0.0, -0.06, 1.455))
HEAD_RADII = (0.082, 0.09, 0.1)
WAIST_Z = 0.88
PANTS_BOTTOM_Z = 0.30

RIGHT_ARM = (  # segura a lanterna para a frente
    ((-0.15, 0.03, 1.23), 0.052),
    ((-0.19, 0.06, 1.0), 0.042),
    ((-0.165, -0.12, 0.93), 0.034),
    ((-0.16, -0.18, 0.925), 0.038),  # mão
    ((-0.158, -0.215, 0.925), 0.028),
)
LEFT_ARM = (  # segura a vassoura
    ((0.15, 0.03, 1.23), 0.052),
    ((0.2, 0.05, 1.0), 0.042),
    ((0.215, -0.07, 0.90), 0.034),
    ((0.215, -0.11, 0.89), 0.038),  # mão
    ((0.214, -0.135, 0.89), 0.028),
)
FLASHLIGHT_AXIS = (Vector((-0.158, -0.16, 0.925)), Vector((-0.158, -0.30, 0.925)))
BROOM_HANDLE = (Vector((0.255, -0.15, 0.24)), Vector((0.2, -0.1, 1.38)))


def build_body():
    skeleton = gc.Skeleton()
    pelvis = skeleton.joint((0.0, 0.02, 0.84), 0.12)
    belly = skeleton.joint((0.0, -0.025, 0.98), 0.135, pelvis)
    chest = skeleton.joint((0.0, 0.005, 1.14), 0.125, belly)
    upper = skeleton.joint((0.0, 0.035, 1.25), 0.11, chest)  # costas curvadas
    skeleton.joint((0.0, -0.03, 1.37), 0.058, upper)  # pescoço inclinado para a frente
    skeleton.mirrored_chain(pelvis, (
        ((0.075, 0.02, 0.79), 0.072),  # quadril
        ((0.085, 0.0, 0.45), 0.055),   # joelho
        ((0.085, 0.02, 0.10), 0.04),   # tornozelo
        ((0.085, -0.01, 0.05), 0.037),
    ))
    return gc.skin("Body", skeleton, subsurf=3)


def paint_body(co, n):
    cuff = gc.band(co.z, PANTS_BOTTOM_Z + 0.02, 0.014, 0.004)
    color = gc.lerp(SOCK, gc.shade(PANTS, 1.0 - 0.2 * cuff), gc.above(co.z, PANTS_BOTTOM_Z))
    shirt = gc.above(co.z, WAIST_Z)
    neck = gc.above(co.z, 1.33)
    color = gc.lerp(gc.lerp(color, SHIRT, shirt), SKIN, neck)
    return color, shirt * (1.0 - neck)  # o xadrez só vale na camisa


def build_arm(points, name):
    skeleton = gc.Skeleton()
    skeleton.chain(None, points)
    arm = gc.skin(name, skeleton)
    line = [Vector(p) for p, _ in points]
    elbow_t = (line[1] - line[0]).length / sum((line[i + 1] - line[i]).length for i in range(len(line) - 1))

    def paint_arm(co, n):
        t, _ = gc.closest_on_polyline(Vector(co), line)
        if t < elbow_t - 0.02:
            return SHIRT, 1.0
        if t < elbow_t + 0.05:
            return gc.shade(SHIRT, 0.85), 1.0  # manga dobrada
        return SKIN

    return gc.Part(arm, paint_arm, 160)


def head_deform(d, p):
    jowl = gc.smoothstep(-0.2, -0.7, d.z)
    p.x *= 1.0 + 0.06 * jowl
    p.y -= 0.008 * jowl * max(0.0, -d.y)
    return p


def paint_head(co, n):
    q = co - HEAD_CENTER
    d = Vector((q.x / HEAD_RADII[0], q.y / HEAD_RADII[1], q.z / HEAD_RADII[2])).normalized()
    cheeks = gc.band(abs(d.x), 0.55, 0.18, 0.1) * gc.band(d.z, -0.1, 0.15, 0.1) * gc.smoothstep(0.0, 0.4, -d.y)
    return gc.lerp(SKIN, SKIN_ROSY, 0.45 * cheeks)


def frizz(d, p):
    return p * (1.0 + 0.3 * gc.turbulence(d, 3.0))


def build_head_details(head):
    c = HEAD_CENTER
    parts = []

    hit, normal = gc.front_hit(head, 0.0, c.z - 0.005)
    nose = gc.ellipsoid("Nose", hit + normal * 0.012, (0.024, 0.028, 0.027), segments=12, rings=8)
    parts.append(gc.Part(nose, gc.flat(SKIN_ROSY), 60))

    def droop(d, p):
        p.z -= 0.022 * abs(d.x)
        return p

    hit, normal = gc.front_hit(head, 0.0, c.z - 0.04)
    mustache = gc.ellipsoid("Mustache", hit + normal * 0.01, (0.052, 0.02, 0.018),
                            segments=14, rings=8, deform=droop)
    parts.append(gc.Part(mustache, gc.flat(HAIR), 80))

    ring_centers = []
    for side in (1, -1):
        suffix = "_L" if side > 0 else "_R"
        hit, normal = gc.front_hit(head, 0.032 * side, c.z + 0.02)
        eye = gc.ellipsoid("Eye" + suffix, hit - normal * 0.004, (0.009,) * 3, segments=8, rings=6)
        ring_centers.append(hit + gc.FRONT * 0.014)

        def frown(d, p, side=side):
            p.z += 0.25 * p.x * side
            return p

        hit, normal = gc.front_hit(head, 0.035 * side, c.z + 0.048)
        brow = gc.ellipsoid("Brow" + suffix, hit + normal * 0.006, (0.026, 0.011, 0.01),
                            segments=8, rings=6, deform=frown)
        hit, normal = gc.surface_hit(head, (side, c.y + 0.005, c.z), (-side, 0.0, 0.0))
        ear = gc.ellipsoid("Ear" + suffix, hit + normal * 0.003, (0.012, 0.024, 0.032), segments=10, rings=8)
        tuft = gc.ellipsoid("Hair" + suffix, c + Vector((0.074 * side, 0.03, 0.005)), (0.026, 0.045, 0.038),
                            segments=12, rings=8, deform=frizz)
        parts += [gc.Part(eye, gc.flat(EYE), 30), gc.Part(brow, gc.flat(HAIR), 40),
                  gc.Part(ear, gc.flat(SKIN_ROSY), 50), gc.Part(tuft, gc.flat(HAIR), 90)]

    back_hair = gc.ellipsoid("HairBack", c + Vector((0.0, 0.078, -0.01)), (0.055, 0.025, 0.035),
                             segments=12, rings=8, deform=frizz)
    parts.append(gc.Part(back_hair, gc.flat(HAIR), 90))

    for center in ring_centers:
        parts.append(gc.Part(gc.torus("GlassesRing", center, gc.FRONT, 0.019, 0.0028, 14, 4),
                             gc.flat(GLASSES), 112))
        side = 1.0 if center.x > 0 else -1.0
        temple_start = center + Vector((0.019 * side, 0.0, 0.0))
        temple_end = Vector((0.08 * side, c.y + 0.01, c.z + 0.02))
        parts.append(gc.Part(gc.cylinder("GlassesTemple", temple_start, temple_end, 0.0022, segments=4),
                             gc.flat(GLASSES), 16))
    left, right = ring_centers
    bridge = gc.cylinder("GlassesBridge", left - Vector((0.019, 0.0, 0.0)), right + Vector((0.019, 0.0, 0.0)),
                         0.0025, segments=4)
    parts.append(gc.Part(bridge, gc.flat(GLASSES), 16))
    return parts


def build_apron(body):
    zs = [0.50 + i * (1.20 - 0.50) / 13 for i in range(14)]  # da barra ao peito
    fronts = []
    for z in zs:
        ys = []
        for x in (-0.1, -0.05, 0.0, 0.05, 0.1):
            hit, location, _, _ = body.ray_cast(Vector((x, -5.0, z)), Vector((0.0, 1.0, 0.0)))
            if hit:
                ys.append(location.y)
        fronts.append(min(ys))

    belly = min(range(len(zs)), key=lambda i: fronts[i] if zs[i] <= 1.05 else float("inf"))
    for i in range(belly - 1, -1, -1):  # abaixo da barriga o tecido cai reto, com leve abertura
        fronts[i] = min(fronts[i], fronts[i + 1])
    for i in range(belly):
        fronts[i] -= 0.03 * (zs[belly] - zs[i]) / (zs[belly] - zs[0])

    rows = []
    for z, front in zip(zs, fronts):
        bib = gc.smoothstep(0.9, 1.0, z)
        half_width = 0.19 + (0.09 - 0.19) * bib
        wrap = 0.6 + (0.35 - 0.6) * bib
        row = []
        for k in range(9):
            u = -1.0 + 2.0 * k / 8
            row.append((u * half_width, front - 0.012 + wrap * half_width * u * u, z))
        rows.append(row)
    apron = gc.sheet("Apron", rows, 0.006)

    def paint_apron(co, n):
        stain = 0.6 * gc.smoothstep(0.25, 0.6, gc.turbulence(co, 9.0))
        dirt = 0.5 * (1.0 - gc.smoothstep(0.5, 0.62, co.z))
        return gc.lerp(APRON, STAIN, max(stain, dirt))

    parts = [gc.Part(apron, paint_apron, 220)]
    top_row = rows[-1]
    for corner in (top_row[1], top_row[-2]):
        side = 1.0 if corner[0] > 0 else -1.0
        neck, _ = gc.surface_hit(body, (0.055 * side, -0.02, 2.0), (0.0, 0.0, -1.0))
        strap = gc.cylinder("ApronStrap", corner, neck, 0.005, segments=4)
        parts.append(gc.Part(strap, gc.flat(APRON), 24))
    return parts


def build_props():
    parts = []
    bottom, top = BROOM_HANDLE
    parts.append(gc.Part(gc.cylinder("BroomHandle", bottom, top, 0.012, segments=8), gc.flat(WOOD), 32))

    bristles = gc.cylinder("BroomBristles", bottom + Vector((0.007, -0.008, -0.24)),
                           bottom + Vector((0.001, -0.002, 0.02)), 0.1, 0.032, segments=14)
    center_y = bottom.y - 0.005
    for v in bristles.data.vertices:
        v.co.y = center_y + (v.co.y - center_y) * 0.45

    def paint_bristles(co, n):
        streak = 0.85 + 0.25 * gc.turbulence((co.x * 80.0, co.y * 80.0, co.z * 4.0), 1.0)
        dirty = 1.0 - gc.smoothstep(0.0, 0.06, co.z - (bottom.z - 0.24))
        return gc.shade(STRAW, streak * (1.0 - 0.35 * dirty))

    band = gc.cylinder("BroomBand", bottom + Vector((0.0, 0.0, -0.005)), bottom + Vector((0.0, 0.0, 0.03)),
                       0.036, segments=12)
    parts += [gc.Part(bristles, paint_bristles, 120), gc.Part(band, gc.flat(BAND), 48)]

    start, end = FLASHLIGHT_AXIS
    head_end = end + Vector((0.0, -0.045, 0.0))
    lens_end = head_end + Vector((0.0, -0.006, 0.0))
    parts += [
        gc.Part(gc.cylinder("FlashlightBody", start, end, 0.019, segments=12), gc.flat(BLACK), 50),
        gc.Part(gc.cylinder("FlashlightHead", end, head_end, 0.021, 0.031, segments=12), gc.flat(BLACK), 50),
        gc.Part(gc.cylinder("FlashlightLens", head_end, lens_end, 0.028, segments=12), gc.flat(LENS), 50),
    ]

    for side in (1, -1):
        slipper = gc.ellipsoid("Slipper_L" if side > 0 else "Slipper_R", (0.085 * side, -0.035, 0.028),
                               (0.045, 0.1, 0.03), segments=14, rings=8, exponent=0.8)

        def paint_slipper(co, n):
            return gc.lerp(SLIPPER, gc.shade(SLIPPER, 0.55), 1.0 - gc.smoothstep(0.008, 0.016, co.z))

        parts.append(gc.Part(slipper, paint_slipper, 70))
    return parts, [("Socket_Flashlight", lens_end + Vector((0.0, -0.008, 0.0)))]


def build():
    body = build_body()
    head = gc.ellipsoid("Head", HEAD_CENTER, HEAD_RADII, segments=40, rings=28, deform=head_deform)
    parts = [
        gc.Part(body, paint_body, 700),
        build_arm(RIGHT_ARM, "Arm_R"),
        build_arm(LEFT_ARM, "Arm_L"),
        gc.Part(head, paint_head, 380),
    ]
    parts += build_head_details(head)
    parts += build_apron(body)
    props, sockets = build_props()
    return parts + props, sockets


if __name__ == "__main__":
    gc.run_character("Char_SeuZe", build, budget_tris=3000, texture_size=1024,
                     bake_cage=0.012, bake_ray=0.025, ao_distance=0.25,
                     blotch_scale=4.0, stroke_scale=50.0, plaid_scale=5.0)
