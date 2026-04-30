// ============================================================
// FUTA Rush — ObstacleSpawner.cs
// Spawns campus obstacles procedurally in random lanes.
// Difficulty scales via GameManager.CurrentSpeed.
// ============================================================

using UnityEngine;
using FUTARush.Systems;
using FUTARush.Core;

namespace FUTARush.World
{
    public class ObstacleSpawner : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Obstacle Pool Tags")]
        [SerializeField] private string[] _obstacleTags =
        {
            "ObstacleBarrier",
            "ObstacleBus",
            "ObstacleGuard"
        };

        [Header("Spawn Settings")]
        [Tooltip("Don't spawn closer than this to the start")]
        [SerializeField] private float _minStartZ      = 25f;

        [Tooltip("World-unit gap between each obstacle attempt")]
        [SerializeField] private float _spawnInterval  = 8f;

        [Tooltip("Distance ahead of the player to pre-spawn")]
        [SerializeField] private float _lookAheadZ     = 30f;

        [Header("Lanes")]
        [SerializeField] private float _laneWidth      = 3f;

        [Header("Difficulty")]
        [Range(0f, 1f)]
        [Tooltip("Base probability of spawning an obstacle at each interval")]
        [SerializeField] private float _baseSpawnChance = 0.55f;

        // ── Private State ──────────────────────────────────────────────────
        private Transform _playerTransform;
        private float     _nextSpawnZ;

        // ── Unity ──────────────────────────────────────────────────────────
        private void OnEnable()  => EventBus.Subscribe<GameStartEvent>(OnGameStart);
        private void OnDisable() => EventBus.Unsubscribe<GameStartEvent>(OnGameStart);

        private void Update()
        {
            if (_playerTransform == null) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            float threshold = _playerTransform.position.z + _lookAheadZ;

            while (_nextSpawnZ < threshold)
            {
                TrySpawnAt(_nextSpawnZ);
                _nextSpawnZ += _spawnInterval;
            }
        }

        // ── Spawning ───────────────────────────────────────────────────────
        private void TrySpawnAt(float z)
        {
            // Scale spawn chance slightly with speed
            float speed       = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 8f;
            float spawnChance = Mathf.Min(_baseSpawnChance + (speed * 0.005f), 0.85f);

            if (Random.value > spawnChance) return;
            if (_obstacleTags == null || _obstacleTags.Length == 0) return;

            int   lane = Random.Range(0, 3);
            float x    = (lane - 1) * _laneWidth;

            string tag = _obstacleTags[Random.Range(0, _obstacleTags.Length)];
            ObjectPooler.Instance.Spawn(tag, new Vector3(x, 0f, z), Quaternion.identity);
        }

        // ── Event Handlers ─────────────────────────────────────────────────
        private void OnGameStart(GameStartEvent _)
        {
            _nextSpawnZ = _minStartZ;
        }

        // ── Public API ─────────────────────────────────────────────────────
        public void Init(Transform player)
        {
            _playerTransform = player;
            _nextSpawnZ      = _minStartZ;
        }
    }
}
