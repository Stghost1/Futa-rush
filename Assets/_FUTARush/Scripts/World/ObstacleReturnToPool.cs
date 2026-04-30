// ============================================================
// FUTA Rush — ObstacleReturnToPool.cs
// Attach to every Obstacle prefab.
// Automatically returns the obstacle to the pool once the
// player has passed it far enough behind.
// ============================================================

using UnityEngine;
using FUTARush.Systems;

namespace FUTARush.World
{
    public class ObstacleReturnToPool : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Tooltip("How many units behind the player before despawn")]
        [SerializeField] private float _despawnBehind = 20f;

        // ── Private ────────────────────────────────────────────────────────
        private Transform _playerTransform;

        // ── Unity ──────────────────────────────────────────────────────────
        private void OnEnable()
        {
            // Cache player on activation (pool objects are re-used)
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerTransform = playerObj.transform;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            if (transform.position.z < _playerTransform.position.z - _despawnBehind)
                ObjectPooler.Instance.Despawn(gameObject);
        }
    }
}
