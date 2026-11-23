using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Desenha no chão o cone de visão do <see cref="VisionCone"/> (GDD §6.1): um leque recortado
    /// pelas paredes e prateleiras, para o jogador ver exatamente onde o segurança enxerga.
    /// Fica num objeto filho, no nível do chão; reaproveita os buffers (sem alocação por frame).
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VisionConeMesh : MonoBehaviour
    {
        private const int Segments = 24;
        private const float RayHeight = 0.5f; // altura dos raios que recortam o cone (acima do piso)

        [SerializeField] private VisionCone _visionCone;

        private Mesh _mesh;
        private readonly Vector3[] _vertices = new Vector3[Segments + 2];

        private void Awake()
        {
            if (_visionCone == null)
            {
                Debug.LogError("VisionConeMesh needs a Vision Cone assigned.", this);
                enabled = false;
                return;
            }

            _mesh = new Mesh { name = "VisionCone" };
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices;
            _mesh.triangles = BuildTriangles();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        private void LateUpdate()
        {
            VisionData data = _visionCone.Data;
            float halfAngle = data.AngleDegrees / 2f;
            Vector3 rayOrigin = transform.position + Vector3.up * RayHeight;

            _vertices[0] = Vector3.zero;
            for (int i = 0; i <= Segments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / Segments);
                Vector3 localDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                float distance = GetVisibleDistance(rayOrigin, transform.TransformDirection(localDirection), data.RangeMeters);
                _vertices[i + 1] = localDirection * distance;
            }

            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        private float GetVisibleDistance(Vector3 origin, Vector3 direction, float range)
        {
            bool blocked = Physics.Raycast(origin, direction, out RaycastHit hit, range, _visionCone.ObstacleMask,
                QueryTriggerInteraction.Ignore);
            return blocked ? hit.distance : range;
        }

        private static int[] BuildTriangles()
        {
            var triangles = new int[Segments * 3];
            for (int i = 0; i < Segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            return triangles;
        }
    }
}
