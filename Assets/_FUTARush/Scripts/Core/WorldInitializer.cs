// ============================================================
// FUTA Rush — WorldInitializer.cs
// Bootstrap script for the GamePlay scene.
// Finds the Player and wires it to world systems,
// then triggers GameManager.StartGame().
// ============================================================

using UnityEngine;
using FUTARush.Core;
using FUTARush.World;
using FUTARush.Player;

namespace FUTARush.Core
{
    public class WorldInitializer : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Scene References")]
        [SerializeField] private PlayerController  _player;
        [SerializeField] private TileManager       _tileManager;
        [SerializeField] private ObstacleSpawner   _obstacleSpawner;
        [SerializeField] private CoinSpawner       _coinSpawner;
        [SerializeField] private CameraFollow      _cameraFollow;

        [Header("Auto-Start")]
        [Tooltip("If true, game starts immediately when scene loads")]
        [SerializeField] private bool _autoStart = true;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Start()
        {
            if (_player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    _player = playerObj.GetComponent<PlayerController>();
            }

            if (_player == null)
            {
                Debug.LogError("[WorldInitializer] Player not found! Assign it in the Inspector.");
                return;
            }

            // Wire player transform to world systems
            _tileManager?.SetPlayer(_player.transform);
            _obstacleSpawner?.Init(_player.transform);
            _coinSpawner?.Init(_player.transform);
            _cameraFollow?.SetTarget(_player.transform);

            if (_autoStart)
                GameManager.Instance?.StartGame();
        }
    }
}
