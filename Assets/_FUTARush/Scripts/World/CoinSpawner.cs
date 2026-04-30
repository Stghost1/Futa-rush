// ============================================================
// FUTA Rush — CoinSpawner.cs
// Spawns FUTA School Tokens (₦ coins) in row patterns.
// Tokens are pooled; collected ones deactivate themselves.
// ============================================================

using UnityEngine;
using FUTARush.Systems;
using FUTARush.Core;

namespace FUTARush.World
{
    public class CoinSpawner : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Pool Tag")]
        [SerializeField] private string _tokenTag = "Token";

        [Header("Row Settings")]
        [Tooltip("Gap (world units) between token rows")]
        [SerializeField] private float _rowInterval    = 6f;

        [Tooltip("How far ahead to pre-spawn tokens")]
        [SerializeField] private float _lookAheadZ     = 35f;

        [Tooltip("Tokens per row")]
        [SerializeField] private int   _tokensPerRow   = 5;

        [Tooltip("Z spacing between tokens in a row")]
        [SerializeField] private float _tokenSpacing   = 1.4f;

        [Tooltip("Height above the ground")]
        [SerializeField] private float _tokenHeight    = 0.6f;

        [Header("Lanes")]
        [SerializeField] private float _laneWidth      = 3f;

        [Header("Start Zone")]
        [SerializeField] private float _minStartZ      = 15f;

        // ── Private ────────────────────────────────────────────────────────
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
                SpawnRow(_nextSpawnZ);
                _nextSpawnZ += _rowInterval;
            }
        }

        // ── Spawning ───────────────────────────────────────────────────────
        private void SpawnRow(float startZ)
        {
            int   lane = Random.Range(0, 3);
            float x    = (lane - 1) * _laneWidth;

            for (int i = 0; i < _tokensPerRow; i++)
            {
                float z = startZ + (i * _tokenSpacing);
                ObjectPooler.Instance.Spawn(_tokenTag,
                    new Vector3(x, _tokenHeight, z), Quaternion.identity);
            }
        }

        // ── Event Handlers ─────────────────────────────────────────────────
        private void OnGameStart(GameStartEvent _) => _nextSpawnZ = _minStartZ;

        // ── Public API ─────────────────────────────────────────────────────
        public void Init(Transform player)
        {
            _playerTransform = player;
            _nextSpawnZ      = _minStartZ;
        }
    }
}
