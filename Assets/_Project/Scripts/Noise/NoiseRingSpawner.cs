using UnityEngine;
using UnityEngine.Pool;

namespace Guaxinim.Noise
{
    /// <summary>
    /// Mostra um <see cref="NoiseRingVFX"/> para cada barulho emitido. Os anéis vêm de um pool,
    /// porque correr emite vários por segundo e Instantiate/Destroy pesa no WebGL.
    /// </summary>
    public class NoiseRingSpawner : MonoBehaviour
    {
        private const int DefaultCapacity = 8;
        private const int MaxPoolSize = 32;

        [SerializeField] private NoiseRingVFX _ringPrefab;

        private ObjectPool<NoiseRingVFX> _pool;

        private void Awake()
        {
            if (_ringPrefab == null)
            {
                Debug.LogError("NoiseRingSpawner needs a Ring Prefab.", this);
                enabled = false;
                return;
            }

            _pool = new ObjectPool<NoiseRingVFX>(CreateRing, GetRing, ReleaseRing, DestroyRing,
                collectionCheck: false, DefaultCapacity, MaxPoolSize);
        }

        private void OnEnable() => NoiseSystem.OnNoiseEmitted += HandleNoiseEmitted;

        private void OnDisable() => NoiseSystem.OnNoiseEmitted -= HandleNoiseEmitted;

        private void HandleNoiseEmitted(NoiseEvent noise)
        {
            if (_pool == null)
            {
                return;
            }

            _pool.Get().Play(noise);
        }

        private NoiseRingVFX CreateRing()
        {
            NoiseRingVFX ring = Instantiate(_ringPrefab, transform);
            ring.OnFinished += HandleRingFinished;
            return ring;
        }

        private void HandleRingFinished(NoiseRingVFX ring) => _pool.Release(ring);

        private static void GetRing(NoiseRingVFX ring) => ring.gameObject.SetActive(true);

        private static void ReleaseRing(NoiseRingVFX ring) => ring.gameObject.SetActive(false);

        private static void DestroyRing(NoiseRingVFX ring) => Destroy(ring.gameObject);
    }
}
