// ============================================================
// FUTA Rush — TileManager.cs
// Procedural infinite tile system.
// Keeps N tiles ahead of the player; recycles tiles behind.
// Uses ObjectPooler — zero runtime allocations.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using FUTARush.Systems;
using FUTARush.Core;

namespace FUTARush.World
{
    public class TileManager : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Tile Pool Tags")]
        [Tooltip("Tags registered in ObjectPooler — picked randomly")]
        [SerializeField] private string[] _tileTags = { "TileStraight", "TileCampus" };

        [Header("Tile Dimensions")]
        [Tooltip("Length of each tile in world units")]
        [SerializeField] private float _tileLength = 20f;

        [Header("Look-Ahead")]
        [Tooltip("How many tiles to keep spawned ahead of the player")]
        [SerializeField] private int _tilesAhead = 5;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;

        // ── Private ────────────────────────────────────────────────────────
        private readonly List<GameObject> _activeTiles = new();
        private float _nextSpawnZ;

        // ── Unity ──────────────────────────────────────────────────────────
        private void OnEnable()  => EventBus.Subscribe<GameStartEvent>(OnGameStart);
        private void OnDisable() => EventBus.Unsubscribe<GameStartEvent>(OnGameStart);

        private void Start()
        {
            _nextSpawnZ = 0f;
            // Pre-warm with enough tiles to fill the view
            for (int i = 0; i < _tilesAhead + 2; i++)
                SpawnNextTile();
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            float spawnThreshold = _playerTransform.position.z + (_tilesAhead * _tileLength);
            while (_nextSpawnZ < spawnThreshold)
                SpawnNextTile();

            RecycleStaleTiles();
        }

        // ── Tile Lifecycle ─────────────────────────────────────────────────
        private void SpawnNextTile()
        {
            if (_tileTags == null || _tileTags.Length == 0) return;

            string tag  = _tileTags[Random.Range(0, _tileTags.Length)];
            var    tile = ObjectPooler.Instance.Spawn(tag,
                              new Vector3(0f, 0f, _nextSpawnZ), Quaternion.identity);

            if (tile == null) return;

            _activeTiles.Add(tile);
            _nextSpawnZ += _tileLength;
        }

        private void RecycleStaleTiles()
        {
            float recycleZ = _playerTransform.position.z - _tileLength;

            for (int i = _activeTiles.Count - 1; i >= 0; i--)
            {
                var tile = _activeTiles[i];

                // Guard against null or already-pooled tiles
                if (tile == null || !tile.activeInHierarchy)
                {
                    _activeTiles.RemoveAt(i);
                    continue;
                }

                if (tile.transform.position.z < recycleZ)
                {
                    ObjectPooler.Instance.Despawn(tile);
                    _activeTiles.RemoveAt(i);
                }
            }
        }

        // ── Event Handlers ─────────────────────────────────────────────────
        private void OnGameStart(GameStartEvent _)
        {
            // Despawn all active tiles and reset
            foreach (var tile in _activeTiles)
                if (tile != null) ObjectPooler.Instance.Despawn(tile);

            _activeTiles.Clear();
            _nextSpawnZ = 0f;

            for (int i = 0; i < _tilesAhead + 2; i++)
                SpawnNextTile();
        }

        // ── Public API ─────────────────────────────────────────────────────
        public void SetPlayer(Transform player) => _playerTransform = player;
    }
}
