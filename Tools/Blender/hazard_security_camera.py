"""Gera a câmera de segurança de teto (Hazard_SecurityCamera.fbx e textura).

GDD §6.6, §15.2 (câmera com luz vermelha), §15.5 e referência em docs/.

Uso:
    blender.exe -b --factory-startup -P Tools/Blender/hazard_security_camera.py -- [--preview <pasta>]

Suporte preso ao teto (o pivô da raiz fica no ponto de fixação, em z = 0, e o modelo pende
para baixo) e cabeça separada no objeto filho "Head", com o pivô no eixo vertical do suporte:
o SecurityCamera da Unity gira esse filho no eixo Y para a varredura. A lente aponta para a
frente (-Y no Blender, +Z na Unity), inclinada para o chão. Socket_Lens marca a ponta da lente,
de onde sai o cone de visão.
"""

import math
import os
import sys

from mathutils import Euler, Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import guaxinim_common as gc  # noqa: E402

HOUSING = gc.srgb("#6c6d74")
HOUSING_DARK = gc.srgb("#4a4b52")
MOUNT = gc.srgb("#8a8b90")
SCREW = gc.srgb("#2e2e33")
LENS_RING = gc.srgb("#1b1b20")
LENS_GLASS = gc.srgb("#5a0d10")
LED = gc.srgb("#ff2a20")

PIVOT = Vector((0.0, 0.0, -0.25))  # eixo da varredura, logo abaixo do braço
TILT_DEGREES = 25.0  # inclinação da lente para o chão
BODY_LENGTH = 0.26
BODY_SIZE = 0.105


def build_mount():
    plate = gc.cylinder("MountPlate", (0.0, 0.0, -0.025), (0.0, 0.0, 0.0), 0.075, segments=16)
    arm = gc.cylinder("MountArm", (0.0, 0.0, -0.025), (0.0, 0.0, PIVOT.z + 0.02), 0.017, segments=10)
    parts = [gc.Part(plate, gc.flat(MOUNT), 80), gc.Part(arm, gc.flat(MOUNT), 60)]
    for angle in (45.0, 135.0, 225.0, 315.0):
        a = math.radians(angle)
        x, y = 0.05 * math.cos(a), 0.05 * math.sin(a)
        screw = gc.cylinder("Screw", (x, y, -0.03), (x, y, -0.024), 0.008, segments=6)
        parts.append(gc.Part(screw, gc.flat(SCREW), 20))
    return parts


def build_head():
    """Monta a cabeça na horizontal, com o pivô na origem, e depois inclina para o chão."""
    parts = []
    joint = gc.cylinder("Joint", (0.0, 0.0, 0.025), (0.0, 0.0, -0.03), 0.03, segments=12)
    neck = gc.box("Neck", (0.0, 0.0, -0.05), (0.03, 0.05, 0.05))
    parts += [gc.Part(joint, gc.flat(HOUSING_DARK), 60), gc.Part(neck, gc.flat(HOUSING_DARK), 12)]

    body_center = Vector((0.0, -0.06, -0.1))
    body = gc.ellipsoid("Body", body_center, (BODY_SIZE / 2, BODY_LENGTH / 2, BODY_SIZE / 2),
                        segments=16, rings=10, exponent=0.35)

    def paint_body(co, n):
        back = gc.smoothstep(0.0, 0.08, co.y - body_center.y)  # traseira mais escura
        return gc.lerp(HOUSING, HOUSING_DARK, 0.6 * back)

    parts.append(gc.Part(body, paint_body, 240))

    front_y = body_center.y - BODY_LENGTH / 2
    hood = gc.box("Hood", (0.0, front_y + 0.09, body_center.z + BODY_SIZE / 2 + 0.008),
                  (BODY_SIZE + 0.03, 0.22, 0.012))
    parts.append(gc.Part(hood, gc.flat(HOUSING), 12))

    lens_axis = (Vector((0.0, front_y + 0.01, body_center.z)), Vector((0.0, front_y - 0.022, body_center.z)))
    ring = gc.cylinder("LensRing", lens_axis[0], lens_axis[1], 0.04, segments=16)
    glass_end = lens_axis[1] + Vector((0.0, -0.004, 0.0))
    glass = gc.cylinder("LensGlass", lens_axis[1], glass_end, 0.03, segments=16)

    def paint_glass(co, n):
        glow = 1.0 - gc.smoothstep(0.0, 0.016, (Vector((co.x, 0.0, co.z - body_center.z))).length)
        return gc.lerp(LENS_GLASS, LED, 0.85 * glow)

    led = gc.ellipsoid("Led", (0.035, front_y + 0.003, body_center.z + 0.034), (0.007, 0.007, 0.007),
                       segments=8, rings=6)
    parts += [gc.Part(ring, gc.flat(LENS_RING), 70), gc.Part(glass, paint_glass, 70),
              gc.Part(led, gc.flat(LED), 40)]

    tilt = Matrix.Translation(PIVOT) @ Euler((math.radians(TILT_DEGREES), 0.0, 0.0)).to_matrix().to_4x4()
    for part in parts:
        part.obj.data.transform(tilt)
    lens_tip = tilt @ (glass_end + Vector((0.0, -0.002, 0.0)))
    return parts, lens_tip


def build():
    head_parts, lens_tip = build_head()
    return [
        gc.Piece("Hazard_SecurityCamera", build_mount()),
        gc.Piece("Head", head_parts, pivot=PIVOT, parent="Hazard_SecurityCamera",
                 sockets=[("Socket_Lens", lens_tip)]),
    ]


if __name__ == "__main__":
    gc.run_model("Hazard_SecurityCamera", "Hazards", build, texture_size=512,
                 bake_cage=0.006, bake_ray=0.012, ao_distance=0.08,
                 blotch_scale=12.0, stroke_scale=80.0)
