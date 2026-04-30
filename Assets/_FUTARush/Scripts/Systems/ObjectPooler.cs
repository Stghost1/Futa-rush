// ============================================================
// FUTA Rush — ObjectPooler.cs
// Generic object pool — eliminates runtime Instantiate/Destroy.
// Configure pools in the Inspector then call Spawn() / Despawn().
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace FUTARush.Systems
{
    // ── Pool Definition (serialisable) ────────────────────────────────────
    [System.Serializable]
    public class Pool
    {
        [Tooltip("Unique identifier used when calling Spawn()")]
        public string     Tag;

        [Tooltip("The prefab to pre-instantiate")]
        public GameObject Prefab;

        [Tooltip("How many instances to create at startup")]
        public int        Size = 10;
    }

    // ── Object Pooler ─────────────────────────────────────────────────────
    public class ObjectPooler : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static ObjectPooler Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────
        [SerializeField] private List<Pool> _pools = new();

        // ── Internal Storage ───────────────────────────────────────────────
        private readonly Dictionary<string, Queue<GameObject>> _poolDict = new();

        // ── Unity ──────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitialisePools();
        }

        // ── Initialisation ─────────────────────────────────────────────────
        private void InitialisePools()
        {
            foreach (var pool in _pools)
            {
                if (pool.Prefab == null)
                {
                    Debug.LogWarning($"[ObjectPooler] Pool '{pool.Tag}' has no prefab assigned.");
                    continue;
                }

                var queue = new Queue<GameObject>();

                for (int i = 0; i < pool.Size; i++)
                {
                    var obj = Instantiate(pool.Prefab, transform);
                    obj.name = $"{pool.Tag}_{i:00}";
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }

                _poolDict[pool.Tag] = queue;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Retrieve a pooled object, activate it, and place it in the world.
        /// If all instances are in use, the oldest one is recycled.
        /// </summary>
        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.TryGetValue(tag, out var queue))
            {
                Debug.LogWarning($"[ObjectPooler] No pool found for tag: '{tag}'");
                return null;
            }

            // Dequeue from front, requeue at back (oldest gets recycled when pool is exhausted)
            GameObject obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            queue.Enqueue(obj);
            return obj;
        }

        /// <summary>Return an object to the pool by deactivating it.</summary>
        public void Despawn(GameObject obj)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}

// ── INSPECTOR SETUP GUIDE ─────────────────────────────────────────────────
//
//  Tag                 | Prefab               | Size
//  --------------------|----------------------|------
//  TileStraight        | TileStraight.prefab  |  8
//  TileCampus          | TileCampus.prefab    |  6
//  ObstacleBarrier     | ObstacleBarrier      | 10
//  ObstacleBus         | ObstacleBus          |  6
//  ObstacleGuard       | ObstacleGuard        |  6
//  Token               | Token.prefab         | 80
//
// ─────────────────────────────────────────────────────────────────────────
