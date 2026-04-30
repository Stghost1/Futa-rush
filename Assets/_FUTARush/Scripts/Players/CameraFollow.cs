// ============================================================
// FUTA Rush — CameraFollow.cs
// Smooth trailing camera with separate lateral follow.
// Attach to Main Camera.
// ============================================================

using UnityEngine;

namespace FUTARush.Player
{
    public class CameraFollow : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Offset from target")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 4f, -7f);

        [Header("Smoothing")]
        [Tooltip("Smooth time for vertical / forward follow")]
        [SerializeField] private float _smoothTime    = 0.15f;

        [Tooltip("Smooth time for left-right lane following")]
        [SerializeField] private float _lateralSmooth = 0.08f;

        // ── Private State ──────────────────────────────────────────────────
        private Vector3 _smoothVelocity;
        private float   _xVelocity;

        // ── Unity ──────────────────────────────────────────────────────────
        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + _offset;
            Vector3 current = transform.position;

            // Lateral (X) — snappier for lane changes
            float smoothX = Mathf.SmoothDamp(current.x, desired.x, ref _xVelocity, _lateralSmooth);

            // Vertical (Y) + Forward (Z) — smoother trail
            Vector3 smoothYZ = Vector3.SmoothDamp(
                new Vector3(current.x, current.y, current.z),
                new Vector3(current.x, desired.y, desired.z),
                ref _smoothVelocity,
                _smoothTime
            );

            transform.position = new Vector3(smoothX, smoothYZ.y, smoothYZ.z);

            // Always look slightly ahead of player
            transform.LookAt(_target.position + Vector3.forward * 3f);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Assign the player transform at runtime if needed.</summary>
        public void SetTarget(Transform target) => _target = target;
    }
}
