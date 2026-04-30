// ============================================================
// FUTA Rush — TokenRotate.cs
// Attach to the Token (School Token / ₦ coin) prefab.
// Rotates and bobs the token, then auto-despawns after lifetime.
// ============================================================

using UnityEngine;
using FUTARush.Systems;

namespace FUTARush.World
{
    public class TokenRotate : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Rotation")]
        [SerializeField] private float _rotateSpeed = 180f;   // degrees per second

        [Header("Bob")]
        [SerializeField] private float _bobAmplitude = 0.2f;  // world units
        [SerializeField] private float _bobFrequency = 2.5f;  // cycles per second

        [Header("Lifetime")]
        [Tooltip("Auto-despawn after this many seconds if not collected")]
        [SerializeField] private float _lifetime = 25f;

        // ── Private State ──────────────────────────────────────────────────
        private float _spawnTime;
        private float _baseY;

        // ── Unity ──────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _spawnTime = Time.time;
            _baseY     = transform.position.y;
        }

        private void Update()
        {
            // Spin
            transform.Rotate(0f, _rotateSpeed * Time.deltaTime, 0f, Space.World);

            // Bob
            float newY = _baseY + Mathf.Sin((Time.time - _spawnTime) * _bobFrequency * Mathf.PI * 2f)
                                  * _bobAmplitude;
            var pos = transform.position;
            pos.y   = newY;
            transform.position = pos;

            // Auto-despawn
            if (Time.time - _spawnTime >= _lifetime)
                ObjectPooler.Instance.Despawn(gameObject);
        }
    }
}
