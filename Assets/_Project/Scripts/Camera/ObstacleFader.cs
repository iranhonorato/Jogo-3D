using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.CameraSystem
{
    /// <summary>
    /// Deixa translúcido o que fica entre a câmera e o guaxinim (GDD §3.1), para ele nunca sumir atrás de
    /// uma gôndola ou parede. Troca o material por uma versão translúcida já pronta (asset do projeto, para
    /// o build incluir a variante do shader) e devolve o original quando o caminho fica livre.
    /// Barato para o WebGL: um SphereCast por frame e sem alocação depois que cada objeto foi visto uma vez.
    /// </summary>
    public class ObstacleFader : MonoBehaviour
    {
        private const int MaxHits = 16;

        [Serializable]
        private struct FadePair
        {
            [Tooltip("Nome do material opaco (ex.: M_Env_Kit)")]
            [SerializeField] private string _sourceMaterialName;

            [Tooltip("Versão translúcida do mesmo material")]
            [SerializeField] private Material _fadedMaterial;

            public string SourceMaterialName => _sourceMaterialName;
            public Material FadedMaterial => _fadedMaterial;
        }

        [Tooltip("O guaxinim (ligado na montagem da fase)")]
        [SerializeField] private Transform _target;

        [Tooltip("Layers que podem ficar translúcidas (Obstacle)")]
        [SerializeField] private LayerMask _obstacleMask;

        [SerializeField] private FadePair[] _fadePairs = new FadePair[0];

        [Tooltip("Metros: raio do cilindro testado entre a câmera e o guaxinim")]
        [SerializeField, Min(0.01f)] private float _castRadius = 0.5f;

        [Tooltip("Metros acima dos pés do guaxinim: ponto mirado")]
        [SerializeField, Min(0f)] private float _targetHeight = 0.3f;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
        private readonly Dictionary<string, Material> _fadedByName = new Dictionary<string, Material>();
        private readonly Dictionary<Collider, Renderer[]> _renderersByCollider = new Dictionary<Collider, Renderer[]>();
        private readonly Dictionary<Renderer, Material> _originalByFaded = new Dictionary<Renderer, Material>();
        private readonly HashSet<Renderer> _hitThisFrame = new HashSet<Renderer>();
        private readonly List<Renderer> _toRestore = new List<Renderer>();

        private void Awake()
        {
            if (_target == null)
            {
                Debug.LogError("ObstacleFader needs a Target assigned.", this);
                enabled = false;
                return;
            }

            foreach (FadePair pair in _fadePairs)
            {
                _fadedByName[pair.SourceMaterialName] = pair.FadedMaterial;
            }
        }

        private void LateUpdate()
        {
            CollectRenderersInTheWay();
            RestoreRenderersNoLongerInTheWay();
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Renderer, Material> faded in _originalByFaded)
            {
                if (faded.Key != null)
                {
                    faded.Key.sharedMaterial = faded.Value;
                }
            }

            _originalByFaded.Clear();
        }

        private void CollectRenderersInTheWay()
        {
            _hitThisFrame.Clear();
            Vector3 origin = transform.position;
            Vector3 toTarget = _target.position + Vector3.up * _targetHeight - origin;
            float distance = toTarget.magnitude - _castRadius; // não conta o chão sob o guaxinim
            if (distance <= 0f)
            {
                return;
            }

            int count = Physics.SphereCastNonAlloc(origin, _castRadius, toTarget.normalized, _hits, distance,
                _obstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                foreach (Renderer hitRenderer in GetRenderers(_hits[i].collider))
                {
                    _hitThisFrame.Add(hitRenderer);
                    Fade(hitRenderer);
                }
            }
        }

        private void RestoreRenderersNoLongerInTheWay()
        {
            _toRestore.Clear();
            foreach (KeyValuePair<Renderer, Material> faded in _originalByFaded)
            {
                if (!_hitThisFrame.Contains(faded.Key))
                {
                    _toRestore.Add(faded.Key);
                }
            }

            foreach (Renderer restored in _toRestore)
            {
                if (restored != null)
                {
                    restored.sharedMaterial = _originalByFaded[restored];
                }

                _originalByFaded.Remove(restored);
            }
        }

        private void Fade(Renderer target)
        {
            if (_originalByFaded.ContainsKey(target))
            {
                return;
            }

            Material original = target.sharedMaterial;
            if (original == null || !_fadedByName.TryGetValue(original.name, out Material faded))
            {
                return;
            }

            _originalByFaded[target] = original;
            target.sharedMaterial = faded;
        }

        /// <summary>Renderers do objeto do collider, guardados na primeira vez (única alocação).</summary>
        private Renderer[] GetRenderers(Collider hitCollider)
        {
            if (!_renderersByCollider.TryGetValue(hitCollider, out Renderer[] renderers))
            {
                renderers = hitCollider.GetComponentsInChildren<Renderer>();
                _renderersByCollider[hitCollider] = renderers;
            }

            return renderers;
        }
    }
}
