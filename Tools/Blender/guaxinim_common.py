"""Funções compartilhadas pelos scripts de personagem do Operação Guaxinim.

Rodar pelos scripts char_*.py com o Blender 5.3 (ver README desta pasta).

Convenções, verificadas importando um FBX de teste na Unity 6.3:
- 1 unidade = 1 metro, pivô na base (z = 0), Z para cima.
- A frente do modelo fica em -Y no Blender e vira +Z (forward) na Unity.
  O +X do Blender vira -X na Unity, sem espelhar o modelo.

Fluxo de cada personagem (run_character):
1. O script do personagem monta as partes em alta resolução e diz como pintar cada uma.
2. As partes são pintadas (atributo de cor por vértice), postas no chão e unidas.
3. Uma cópia de cada parte é simplificada até o orçamento de triângulos (versão low poly).
4. A cor da versão alta é assada (bake) numa textura da versão low, com variação de
   pincel e oclusão ambiente, para o visual de textura pintada (GDD §15.1 e §15.4).
5. O FBX e a textura vão para Assets/_Project/Art/, e uma prévia opcional é renderizada.
"""

import math
import os
import sys

import bmesh
import bpy
import numpy as np
from mathutils import Matrix, Vector, noise

PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
MODELS_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Art", "Models", "Characters")
TEXTURES_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Art", "Textures", "Characters")

FRONT = Vector((0.0, -1.0, 0.0))


# ---------------------------------------------------------------------------
# Cores e funções de pintura
# ---------------------------------------------------------------------------

def srgb(hex_code):
    """Converte '#rrggbb' (sRGB, como nas ferramentas de pintura) para RGB linear."""
    h = hex_code.lstrip("#")
    return tuple(_to_linear(int(h[i:i + 2], 16) / 255.0) for i in (0, 2, 4))


def _to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def lerp(a, b, t):
    return tuple(x + (y - x) * t for x, y in zip(a, b))


def shade(color, factor):
    return tuple(c * factor for c in color)


def clamp01(x):
    return max(0.0, min(1.0, x))


def smoothstep(edge0, edge1, x):
    t = clamp01((x - edge0) / (edge1 - edge0))
    return t * t * (3.0 - 2.0 * t)


def above(x, edge, softness=0.005):
    """0 abaixo de edge e 1 acima, com transição suave (evita bordas serrilhadas)."""
    return smoothstep(edge - softness, edge + softness, x)


def band(x, center, half_width, softness):
    """1 dentro da faixa [center - half_width, center + half_width], com bordas suaves."""
    return 1.0 - smoothstep(half_width, half_width + softness, abs(x - center))


def turbulence(co, scale):
    """Ruído de -1 a 1, usado para manchas e variação de cor."""
    return noise.noise(Vector(co) * scale)


def closest_on_polyline(point, polyline):
    """Devolve (t, distância): t vai de 0 a 1 ao longo da linha, pelo comprimento."""
    lengths = [(polyline[i + 1] - polyline[i]).length for i in range(len(polyline) - 1)]
    total = sum(lengths)
    best_t, best_dist, walked = 0.0, float("inf"), 0.0
    for i, seg_len in enumerate(lengths):
        a, b = polyline[i], polyline[i + 1]
        ab = b - a
        u = clamp01((point - a).dot(ab) / max(ab.length_squared, 1e-12))
        dist = (point - (a + ab * u)).length
        if dist < best_dist:
            best_dist, best_t = dist, (walked + seg_len * u) / total
        walked += seg_len
    return best_t, best_dist


class Part:
    """Uma parte do personagem.

    paint(co, normal) devolve a cor linear (r, g, b), ou ((r, g, b), mask) quando a parte
    usa o padrão xadrez (mask = 1). low_tris é o orçamento da parte na versão low poly.
    """

    def __init__(self, obj, paint, low_tris, low=None):
        self.obj = obj
        self.paint = paint
        self.low_tris = low_tris
        self.low = low  # versão low poly pronta; se None, é uma cópia simplificada de obj


class Piece:
    """Um objeto exportado pelo run_model: várias partes unidas numa malha só.

    pivot é a origem do objeto, nas coordenadas em que as partes foram montadas.
    parent é o nome da peça mãe (None para a raiz de um FBX). sockets: [(nome, posição)].
    """

    def __init__(self, name, parts, pivot=(0.0, 0.0, 0.0), parent=None, sockets=()):
        self.name = name
        self.parts = parts
        self.pivot = Vector(pivot)
        self.parent = parent
        self.sockets = list(sockets)


def flat(color):
    return lambda co, n: color


# ---------------------------------------------------------------------------
# Geometria
# ---------------------------------------------------------------------------

def _link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj


def _object_from_bmesh(name, bm, smooth=True):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    mesh.polygons.foreach_set("use_smooth", [smooth] * len(mesh.polygons))
    return _link(bpy.data.objects.new(name, mesh))


def ellipsoid(name, center, radii, segments=24, rings=16, exponent=1.0, deform=None):
    """Esfera esticada. exponent < 1 deixa a forma mais quadrada; deform(d, p) esculpe.

    d é a direção unitária do vértice na esfera original e p a posição relativa ao centro.
    """
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=rings, radius=1.0)
    c = Vector(center)
    for v in bm.verts:
        d = v.co.normalized()
        q = Vector([math.copysign(abs(x) ** exponent, x) for x in d]) if exponent != 1.0 else d.copy()
        p = Vector((q.x * radii[0], q.y * radii[1], q.z * radii[2]))
        if deform is not None:
            p = deform(d, p)
        v.co = c + p
    return _object_from_bmesh(name, bm)


def cylinder(name, start, end, radius_start, radius_end=None, segments=12, smooth=True):
    """Cilindro (ou tronco de cone) com tampas, de start até end."""
    radius_end = radius_start if radius_end is None else radius_end
    a, b = Vector(start), Vector(end)
    axis = b - a
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments,
                          radius1=radius_start, radius2=radius_end, depth=axis.length)
    rotation = Vector((0.0, 0.0, 1.0)).rotation_difference(axis.normalized()).to_matrix().to_4x4()
    bmesh.ops.transform(bm, matrix=Matrix.Translation((a + b) / 2.0) @ rotation, verts=bm.verts)
    return _object_from_bmesh(name, bm, smooth)


def box(name, center, size, rotation=None):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    matrix = Matrix.Translation(Vector(center))
    if rotation is not None:
        matrix = matrix @ rotation.to_matrix().to_4x4()
    bmesh.ops.transform(bm, matrix=matrix, verts=bm.verts)
    return _object_from_bmesh(name, bm, smooth=False)


_BOX_FACES = {  # nome: (canto de origem, vetor u, vetor v), em unidades da caixa (-0,5 a 0,5)
    "+x": ((0.5, -0.5, -0.5), (0, 1, 0), (0, 0, 1)),
    "-x": ((-0.5, 0.5, -0.5), (0, -1, 0), (0, 0, 1)),
    "+y": ((0.5, 0.5, -0.5), (-1, 0, 0), (0, 0, 1)),
    "-y": ((-0.5, -0.5, -0.5), (1, 0, 0), (0, 0, 1)),
    "+z": ((-0.5, -0.5, 0.5), (1, 0, 0), (0, 1, 0)),
    "-z": ((-0.5, 0.5, -0.5), (1, 0, 0), (0, -1, 0)),
}


def grid_box(name, center, size, spacing=None, skip=()):
    """Caixa cujas faces são grades com quadrados de cerca de spacing metros.

    A grade dá vértices para a pintura por vértice (azulejos, etiquetas). spacing=None faz
    uma face por lado (versão low poly). skip lista as faces omitidas ('-z', '+y'...).
    """
    c, s = Vector(center), Vector(size)
    bm = bmesh.new()
    for face_name, (corner, u, v) in _BOX_FACES.items():
        if face_name in skip:
            continue
        origin = Vector((c.x + corner[0] * s.x, c.y + corner[1] * s.y, c.z + corner[2] * s.z))
        u_vec = Vector((u[0] * s.x, u[1] * s.y, u[2] * s.z))
        v_vec = Vector((v[0] * s.x, v[1] * s.y, v[2] * s.z))
        nu = max(1, math.ceil(u_vec.length / spacing)) if spacing else 1
        nv = max(1, math.ceil(v_vec.length / spacing)) if spacing else 1
        grid = [[bm.verts.new(origin + u_vec * (i / nu) + v_vec * (j / nv)) for j in range(nv + 1)]
                for i in range(nu + 1)]
        for i in range(nu):
            for j in range(nv):
                bm.faces.new((grid[i][j], grid[i + 1][j], grid[i + 1][j + 1], grid[i][j + 1]))
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)  # sem recalcular normais: a ordem dos vértices já aponta para fora
    bm.free()
    return _link(bpy.data.objects.new(name, mesh))


def torus(name, center, axis, major, minor, segments=16, sides=6):
    bm = bmesh.new()
    rings = []
    for i in range(segments):
        a = 2.0 * math.pi * i / segments
        ring = []
        for j in range(sides):
            b = 2.0 * math.pi * j / sides
            r = major + minor * math.cos(b)
            ring.append(bm.verts.new((r * math.cos(a), r * math.sin(a), minor * math.sin(b))))
        rings.append(ring)
    for i in range(segments):
        nxt = (i + 1) % segments
        for j in range(sides):
            k = (j + 1) % sides
            bm.faces.new((rings[i][j], rings[nxt][j], rings[nxt][k], rings[i][k]))
    rotation = Vector((0.0, 0.0, 1.0)).rotation_difference(Vector(axis).normalized()).to_matrix().to_4x4()
    bmesh.ops.transform(bm, matrix=Matrix.Translation(Vector(center)) @ rotation, verts=bm.verts)
    return _object_from_bmesh(name, bm)


def sheet(name, rows, thickness):
    """Superfície a partir de linhas de pontos (todas com o mesmo número), com espessura."""
    bm = bmesh.new()
    grid = [[bm.verts.new(p) for p in row] for row in rows]
    for r in range(len(grid) - 1):
        for c in range(len(grid[r]) - 1):
            bm.faces.new((grid[r][c], grid[r][c + 1], grid[r + 1][c + 1], grid[r + 1][c]))
    obj = _object_from_bmesh(name, bm)
    mod = obj.modifiers.new("Solidify", 'SOLIDIFY')
    mod.thickness = thickness
    mod.offset = 0.0
    apply_modifiers(obj)
    return obj


class Skeleton:
    """Esqueleto de pontos e raios que o modificador Skin transforma em corpo orgânico."""

    def __init__(self):
        self.verts, self.radii, self.edges = [], [], []

    def joint(self, co, radius, parent=None):
        self.verts.append(tuple(co))
        self.radii.append(radius if isinstance(radius, tuple) else (radius, radius))
        index = len(self.verts) - 1
        if parent is not None:
            self.edges.append((parent, index))
        return index

    def chain(self, parent, points):
        indices = []
        for co, radius in points:
            parent = self.joint(co, radius, parent)
            indices.append(parent)
        return indices

    def mirrored_chain(self, parent, points):
        """Cria a cadeia no lado +X (esquerda do personagem) e a espelhada em -X."""
        left = self.chain(parent, points)
        right = self.chain(parent, [((-co[0], co[1], co[2]), r) for co, r in points])
        return left, right


def skin(name, skeleton, subsurf=2):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(skeleton.verts, skeleton.edges, [])
    obj = _link(bpy.data.objects.new(name, mesh))
    mod = obj.modifiers.new("Skin", 'SKIN')
    mod.use_smooth_shade = True
    for skin_vertex, radius in zip(mesh.skin_vertices[0].data, skeleton.radii):
        skin_vertex.radius = radius
    mesh.skin_vertices[0].data[0].use_root = True
    if subsurf:
        sub = obj.modifiers.new("Subsurf", 'SUBSURF')
        sub.levels = subsurf
        sub.render_levels = subsurf
    apply_modifiers(obj)
    return obj


def apply_modifiers(obj):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    mesh = bpy.data.meshes.new_from_object(obj.evaluated_get(depsgraph),
                                           preserve_all_data_layers=True, depsgraph=depsgraph)
    old = obj.data
    obj.modifiers.clear()
    obj.data = mesh
    mesh.name = obj.name
    bpy.data.meshes.remove(old)


def surface_hit(target, origin, direction):
    """Lança um raio contra target (objeto sem transformação) e devolve (ponto, normal)."""
    hit, location, normal, _ = target.ray_cast(Vector(origin), Vector(direction).normalized())
    if not hit:
        raise RuntimeError(f"Raio a partir de {tuple(origin)} não acertou {target.name}")
    return location, normal


def front_hit(target, x, z):
    """Ponto da superfície de target visto de frente, na altura z e na posição lateral x."""
    return surface_hit(target, (x, -5.0, z), (0.0, 1.0, 0.0))


def tri_count(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)


def decimate_to(obj, target_tris):
    tris = tri_count(obj)
    if tris <= target_tris:
        return
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.decimate_type = 'COLLAPSE'
    mod.ratio = target_tris / tris
    apply_modifiers(obj)


def _select_only(objects, active):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = active


def join(objects, name):
    _select_only(objects, objects[0])
    bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    obj.name = name
    obj.data.name = name
    return obj


# ---------------------------------------------------------------------------
# Pintura, materiais e bake
# ---------------------------------------------------------------------------

def paint(part):
    mesh = part.obj.data
    count = len(mesh.vertices)
    colors = [0.0] * (count * 4)
    masks = [0.0] * (count * 4)
    for i, v in enumerate(mesh.vertices):
        result = part.paint(v.co, v.normal)
        if isinstance(result[0], tuple):
            rgb, mask = result
        else:
            rgb, mask = result, 0.0
        colors[i * 4:i * 4 + 4] = (rgb[0], rgb[1], rgb[2], 1.0)
        masks[i * 4:i * 4 + 4] = (mask, mask, mask, 1.0)
    mesh.color_attributes.new("Col", 'FLOAT_COLOR', 'POINT').data.foreach_set("color", colors)
    mesh.color_attributes.new("Mask", 'FLOAT_COLOR', 'POINT').data.foreach_set("color", masks)


def _new_material(name):
    mat = bpy.data.materials.new(name)
    if not mat.node_tree:
        mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    mat.node_tree.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Roughness"].default_value = 0.9
    return mat, bsdf


def _socket(sockets, identifier):
    return next(s for s in sockets if s.identifier == identifier)


def _map_range(tree, value_socket, from_min, from_max, to_min, to_max):
    node = tree.nodes.new("ShaderNodeMapRange")
    node.clamp = True
    tree.links.new(value_socket, node.inputs[0])
    node.inputs[1].default_value = from_min
    node.inputs[2].default_value = from_max
    node.inputs[3].default_value = to_min
    node.inputs[4].default_value = to_max
    return node.outputs[0]


def _math(tree, operation, a, b=None, c=None):
    node = tree.nodes.new("ShaderNodeMath")
    node.operation = operation
    for index, value in enumerate((a, b, c)):
        if value is None:
            continue
        if isinstance(value, (int, float)):
            node.inputs[index].default_value = value
        else:
            tree.links.new(value, node.inputs[index])
    return node.outputs[0]


def _multiply_color(tree, color_socket, factor_socket):
    node = tree.nodes.new("ShaderNodeVectorMath")
    node.operation = 'MULTIPLY'
    tree.links.new(color_socket, node.inputs[0])
    tree.links.new(factor_socket, node.inputs[1])
    return node.outputs[0]


def _plaid(tree, coords, base_socket, mask_socket, scale):
    """Xadrez: listras escuras cruzadas sobre a cor base, só onde mask = 1."""
    stripes = []
    for direction in ('X', 'Z', 'Y'):
        wave = tree.nodes.new("ShaderNodeTexWave")
        wave.wave_type = 'BANDS'
        wave.bands_direction = direction
        wave.inputs["Scale"].default_value = scale
        tree.links.new(coords, wave.inputs["Vector"])
        stripes.append(_math(tree, 'GREATER_THAN', wave.outputs["Fac"], 0.7))
    crossings = _math(tree, 'ADD', _math(tree, 'ADD', stripes[0], stripes[1]), stripes[2])
    darken = _math(tree, 'MULTIPLY_ADD', crossings, -0.28, 1.0)
    plaid = _multiply_color(tree, base_socket, darken)
    mix = tree.nodes.new("ShaderNodeMix")
    mix.data_type = 'RGBA'
    tree.links.new(mask_socket, _socket(mix.inputs, "Factor_Float"))
    tree.links.new(base_socket, _socket(mix.inputs, "A_Color"))
    tree.links.new(plaid, _socket(mix.inputs, "B_Color"))
    return _socket(mix.outputs, "Result_Color")


def make_paint_material(name, blotch_scale, stroke_scale, plaid_scale=None):
    """Material da versão alta: cor por vértice com variação de pincel (e xadrez opcional)."""
    mat, bsdf = _new_material(name)
    tree = mat.node_tree
    nodes, links = tree.nodes, tree.links
    coords = nodes.new("ShaderNodeTexCoord").outputs["Object"]

    color = nodes.new("ShaderNodeVertexColor")
    color.layer_name = "Col"
    base = color.outputs["Color"]
    if plaid_scale:
        mask = nodes.new("ShaderNodeVertexColor")
        mask.layer_name = "Mask"
        separate = nodes.new("ShaderNodeSeparateColor")
        links.new(mask.outputs["Color"], separate.inputs[0])
        base = _plaid(tree, coords, base, separate.outputs["Red"], plaid_scale)

    blotch = nodes.new("ShaderNodeTexNoise")
    blotch.inputs["Scale"].default_value = blotch_scale
    blotch.inputs["Detail"].default_value = 3.0
    links.new(coords, blotch.inputs["Vector"])

    stretch = nodes.new("ShaderNodeMapping")
    stretch.inputs["Scale"].default_value = (stroke_scale, stroke_scale, stroke_scale * 0.2)
    links.new(coords, stretch.inputs["Vector"])
    stroke = nodes.new("ShaderNodeTexNoise")
    stroke.inputs["Scale"].default_value = 1.0
    stroke.inputs["Detail"].default_value = 1.0
    links.new(stretch.outputs["Vector"], stroke.inputs["Vector"])

    factor = _math(tree, 'MULTIPLY',
                   _map_range(tree, blotch.outputs["Fac"], 0.3, 0.7, 0.88, 1.08),
                   _map_range(tree, stroke.outputs["Fac"], 0.3, 0.7, 0.93, 1.05))
    links.new(_multiply_color(tree, base, factor), bsdf.inputs["Base Color"])
    return mat


def make_final_material(name):
    mat, bsdf = _new_material(name)
    image_node = mat.node_tree.nodes.new("ShaderNodeTexImage")
    image_node.name = "BaseColorTexture"
    mat.node_tree.nodes.active = image_node
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.15
    return mat


def unwrap(obj):
    _select_only([obj], obj)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(60.0), island_margin=0.006)
    bpy.ops.object.mode_set(mode='OBJECT')


def smooth_by_angle(obj, degrees):
    _select_only([obj], obj)
    try:
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(degrees))
    except (AttributeError, RuntimeError):
        bpy.ops.object.shade_smooth()


def bake_texture(high, low, size, cage, ray, ao_distance, ao_strength, out_png):
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.device = 'CPU'
    if scene.world is None:
        scene.world = bpy.data.worlds.new("BakeWorld")

    mat = low.data.materials[0]
    image_node = mat.node_tree.nodes["BaseColorTexture"]
    color_image = bpy.data.images.new(low.name + "_BaseColor", size, size, alpha=False)
    ao_image = bpy.data.images.new(low.name + "_AO", size, size, alpha=False)
    ao_image.colorspace_settings.name = 'Non-Color'

    image_node.image = color_image
    mat.node_tree.nodes.active = image_node
    _select_only([high, low], low)
    scene.cycles.samples = 1
    bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, use_selected_to_active=True,
                        cage_extrusion=cage, max_ray_distance=ray, margin=16, use_clear=True)

    image_node.image = ao_image
    high.hide_render = True
    _select_only([low], low)
    scene.world.light_settings.distance = ao_distance
    scene.cycles.samples = 64
    bpy.ops.object.bake(type='AO', margin=16, use_clear=True)

    pixels = np.empty(size * size * 4, dtype=np.float32)
    ao = np.empty_like(pixels)
    color_image.pixels.foreach_get(pixels)
    ao_image.pixels.foreach_get(ao)
    pixels = pixels.reshape(-1, 4)
    pixels[:, :3] *= (1.0 - ao_strength + ao_strength * ao.reshape(-1, 4)[:, :1])
    color_image.pixels.foreach_set(pixels.ravel())
    color_image.update()

    os.makedirs(os.path.dirname(out_png), exist_ok=True)
    color_image.filepath_raw = out_png
    color_image.file_format = 'PNG'
    color_image.save()
    bpy.data.images.remove(ao_image)

    image_node.image = color_image
    bsdf = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    mat.node_tree.links.new(image_node.outputs["Color"], bsdf.inputs["Base Color"])


# ---------------------------------------------------------------------------
# Exportação e prévia
# ---------------------------------------------------------------------------

def export_fbx(objects, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    _select_only(objects, objects[0])
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={'MESH', 'EMPTY'},
        apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
        bake_space_transform=True, mesh_smooth_type='FACE', use_mesh_modifiers=True,
        add_leaf_bones=False, path_mode='RELATIVE', embed_textures=False)


def _world_bounds(objects):
    corners = [obj.matrix_world @ Vector(c) for obj in objects if obj.type == 'MESH' for c in obj.bound_box]
    low = Vector([min(c[i] for c in corners) for i in range(3)])
    high = Vector([max(c[i] for c in corners) for i in range(3)])
    return low, high


def render_preview(obj, out_png, objects=None):
    """Três vistas lado a lado: isométrica (como no jogo), 3/4 de frente e perfil.

    objects (opcional) são as malhas enquadradas; as outras malhas da cena ficam escondidas.
    """
    objects = objects or [obj]
    low_corner, high_corner = _world_bounds(objects)
    size = high_corner - low_corner
    extent = max(size)
    hidden = [o for o in bpy.context.scene.objects if o.type == 'MESH' and o not in objects]
    for other in hidden:
        other.hide_render = True
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'
    shading = scene.display.shading
    shading.light = 'STUDIO'
    shading.color_type = 'TEXTURE'
    shading.show_shadows = True
    scene.view_settings.view_transform = 'Standard'
    scene.world.color = (0.16, 0.14, 0.2)
    scene.render.resolution_x, scene.render.resolution_y = 600, 750
    scene.render.resolution_percentage = 100

    camera_data = bpy.data.cameras.new("PreviewCamera")
    camera = _link(bpy.data.objects.new("PreviewCamera", camera_data))
    scene.camera = camera
    target = (low_corner + high_corner) * 0.5
    views = (("iso", 45.0, 35.0, 'ORTHO'), ("front", 8.0, -30.0, 'PERSP'), ("side", 4.0, 90.0, 'PERSP'))

    tiles = []
    tmp = os.path.join(os.path.dirname(out_png), "_preview_tile.png")
    for _, elevation, azimuth, kind in views:
        el, az = math.radians(elevation), math.radians(azimuth)
        direction = Vector((math.sin(az) * math.cos(el), -math.cos(az) * math.cos(el), math.sin(el)))
        camera_data.type = kind
        camera_data.ortho_scale = extent * 1.3
        camera_data.lens = 50.0
        camera.location = target + direction * (extent * (4.0 if kind == 'ORTHO' else 2.1))
        camera.rotation_euler = (target - camera.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = tmp
        bpy.ops.render.render(write_still=True)
        image = bpy.data.images.load(tmp, check_existing=False)
        tile = np.empty(image.size[0] * image.size[1] * 4, dtype=np.float32)
        image.pixels.foreach_get(tile)
        tiles.append(tile.reshape(image.size[1], image.size[0], 4))
        bpy.data.images.remove(image)
    os.remove(tmp)

    strip = np.concatenate(tiles, axis=1)
    out = bpy.data.images.new("Preview", strip.shape[1], strip.shape[0], alpha=False)
    out.pixels.foreach_set(strip.ravel())
    out.filepath_raw = out_png
    out.file_format = 'PNG'
    out.save()
    bpy.data.objects.remove(camera)
    for other in hidden:
        other.hide_render = False


# ---------------------------------------------------------------------------
# Fluxo completo
# ---------------------------------------------------------------------------

def _cli_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = {"preview": None}
    if "--preview" in argv:
        args["preview"] = argv[argv.index("--preview") + 1]
    return args


def run_character(name, build, budget_tris, texture_size, bake_cage, bake_ray, ao_distance,
                  blotch_scale, stroke_scale, plaid_scale=None, target_height=None, ao_strength=0.35):
    """build() devolve (parts, sockets); sockets é uma lista de (nome, posição)."""
    args = _cli_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)

    parts, sockets = build()
    for part in parts:
        paint(part)

    min_z = min(min(v.co.z for v in p.obj.data.vertices) for p in parts)
    max_z = max(max(v.co.z for v in p.obj.data.vertices) for p in parts)
    factor = target_height / (max_z - min_z) if target_height else 1.0
    placement = Matrix.Scale(factor, 4) @ Matrix.Translation((0.0, 0.0, -min_z))
    for part in parts:
        part.obj.data.transform(placement)
    sockets = [(socket_name, placement @ Vector(co)) for socket_name, co in sockets]

    low_parts = []
    for part in parts:
        low = part.obj.copy()
        low.data = part.obj.data.copy()
        low.name = part.obj.name + "_low"
        _link(low)
        for attribute in list(low.data.color_attributes):
            low.data.color_attributes.remove(attribute)
        decimate_to(low, part.low_tris)
        low_parts.append(low)

    high = join([p.obj for p in parts], name + "_High")
    high.data.materials.append(make_paint_material("M_" + name + "_Paint", blotch_scale, stroke_scale, plaid_scale))

    low = join(low_parts, name)
    decimate_to(low, budget_tris)
    smooth_by_angle(low, 60.0)
    unwrap(low)
    low.data.materials.append(make_final_material("M_" + name))

    texture_path = os.path.join(TEXTURES_DIR, name + "_BaseColor.png")
    bake_texture(high, low, texture_size, bake_cage, bake_ray, ao_distance, ao_strength, texture_path)
    bpy.data.objects.remove(high)

    exported = [low]
    for socket_name, co in sockets:
        empty = _link(bpy.data.objects.new(socket_name, None))
        empty.empty_display_type = 'ARROWS'
        empty.empty_display_size = 0.05
        empty.location = co
        empty.parent = low
        exported.append(empty)

    fbx_path = os.path.join(MODELS_DIR, name + ".fbx")
    export_fbx(exported, fbx_path)

    dims = low.dimensions
    print(f"REPORT {name}: tris={tri_count(low)} size=({dims.x:.3f}, {dims.y:.3f}, {dims.z:.3f}) m")
    print(f"REPORT {name}: fbx={fbx_path}")
    print(f"REPORT {name}: texture={texture_path}")

    if args["preview"]:
        preview_path = os.path.join(args["preview"], name + "_preview.png")
        render_preview(low, preview_path)
        print(f"REPORT {name}: preview={preview_path}")


# ---------------------------------------------------------------------------
# Objetos de cenário e perigos (várias peças, uma textura)
# ---------------------------------------------------------------------------

BAKE_SPACING = 8.0  # distância entre as peças durante o bake, para um raio não acertar a vizinha


def _model_dirs(folder):
    art = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Art")
    return os.path.join(art, "Models", folder), os.path.join(art, "Textures", folder)


def _make_low(part):
    if part.low is not None:
        return part.low
    low = part.obj.copy()
    low.data = part.obj.data.copy()
    low.name = part.obj.name + "_low"
    _link(low)
    for attribute in list(low.data.color_attributes):
        low.data.color_attributes.remove(attribute)
    decimate_to(low, part.low_tris)
    return low


def _separate_pieces(low, pieces):
    """Separa a malha unida em um objeto por peça, pelo grupo de vértices com o nome da peça."""
    objects = {}
    for piece in pieces[:-1]:
        _select_only([low], low)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_mode(type='VERT')
        bpy.ops.mesh.select_all(action='DESELECT')
        low.vertex_groups.active_index = low.vertex_groups[piece.name].index
        bpy.ops.object.vertex_group_select()
        bpy.ops.mesh.separate(type='SELECTED')
        bpy.ops.object.mode_set(mode='OBJECT')
        objects[piece.name] = next(o for o in bpy.context.selected_objects if o != low)
    objects[pieces[-1].name] = low
    for piece in pieces:
        obj = objects[piece.name]
        obj.name = piece.name
        obj.data.name = piece.name
        obj.vertex_groups.clear()
    return objects


def _root_of(piece, by_name):
    while piece.parent is not None:
        piece = by_name[piece.parent]
    return piece


def run_model(texture_name, folder, build, texture_size, bake_cage, bake_ray, ao_distance,
              blotch_scale, stroke_scale, ao_strength=0.35, smooth_angle=30.0):
    """Gera um ou mais FBX que dividem a textura <texture_name>_BaseColor.png (atlas).

    build() devolve a lista de Piece. Cada peça raiz vira um FBX com o nome dela, levando as
    peças filhas (com pivô próprio, para girar na Unity) e os sockets.
    """
    args = _cli_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    models_dir, textures_dir = _model_dirs(folder)

    pieces = build()
    by_name = {p.name: p for p in pieces}
    roots = [p for p in pieces if p.parent is None]
    offsets = {}
    for piece in pieces:
        offsets[piece.name] = Vector((BAKE_SPACING * roots.index(_root_of(piece, by_name)), 0.0, 0.0))

    highs, lows = [], []
    for piece in pieces:
        shift = Matrix.Translation(offsets[piece.name])
        for part in piece.parts:
            paint(part)
            low = _make_low(part)
            part.obj.data.transform(shift)
            low.data.transform(shift)
            low.vertex_groups.new(name=piece.name).add(range(len(low.data.vertices)), 1.0, 'REPLACE')
            highs.append(part.obj)
            lows.append(low)

    high = join(highs, texture_name + "_High")
    high.data.materials.append(make_paint_material("M_" + texture_name + "_Paint", blotch_scale, stroke_scale))
    low = join(lows, texture_name)
    smooth_by_angle(low, smooth_angle)
    unwrap(low)
    low.data.materials.append(make_final_material("M_" + texture_name))

    texture_path = os.path.join(textures_dir, texture_name + "_BaseColor.png")
    bake_texture(high, low, texture_size, bake_cage, bake_ray, ao_distance, ao_strength, texture_path)
    bpy.data.objects.remove(high)

    objects = _separate_pieces(low, pieces)
    for piece in pieces:
        objects[piece.name].data.transform(Matrix.Translation(-(offsets[piece.name] + piece.pivot)))
    for piece in pieces:
        obj = objects[piece.name]
        if piece.parent is not None:
            obj.parent = objects[piece.parent]
            obj.location = piece.pivot - by_name[piece.parent].pivot
        for socket_name, co in piece.sockets:
            empty = _link(bpy.data.objects.new(socket_name, None))
            empty.empty_display_type = 'ARROWS'
            empty.empty_display_size = 0.05
            empty.parent = obj
            empty.location = Vector(co) - piece.pivot

    print(f"REPORT {texture_name}: texture={texture_path}")
    for root in roots:
        root_obj = objects[root.name]
        family = [root_obj] + list(root_obj.children_recursive)
        fbx_path = os.path.join(models_dir, root.name + ".fbx")
        export_fbx(family, fbx_path)
        meshes = [o for o in family if o.type == 'MESH']
        low_corner, high_corner = _world_bounds(meshes)
        size = high_corner - low_corner
        tris = sum(tri_count(o) for o in meshes)
        print(f"REPORT {root.name}: tris={tris} size=({size.x:.3f}, {size.y:.3f}, {size.z:.3f}) m fbx={fbx_path}")
        if args["preview"]:
            preview_path = os.path.join(args["preview"], root.name + "_preview.png")
            render_preview(root_obj, preview_path, meshes)
            print(f"REPORT {root.name}: preview={preview_path}")
