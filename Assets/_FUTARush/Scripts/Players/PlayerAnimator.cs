// ============================================================
// FUTA Rush — PlayerAnimator.cs
// Bridges game logic to Unity Animator.
// Uses hashed parameter IDs for optimal performance.
// ============================================================

using UnityEngine;

namespace FUTARush.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        // Pre-hashed animator state names (faster than string lookup every frame)
        private static readonly int _hashRun   = Animator.StringToHash("Run");
        private static readonly int _hashJump  = Animator.StringToHash("Jump");
        private static readonly int _hashSlide = Animator.StringToHash("Slide");
        private static readonly int _hashDead  = Animator.StringToHash("Dead");

        private Animator _animator;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Awake() => _animator = GetComponent<Animator>();

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Cross-fade into the Run animation loop.</summary>
        public void PlayRun()   => _animator.CrossFadeInFixedTime(_hashRun,   0.1f);

        /// <summary>Cross-fade into the Jump animation (one-shot).</summary>
        public void PlayJump()  => _animator.CrossFadeInFixedTime(_hashJump,  0.1f);

        /// <summary>Cross-fade into the Slide animation (one-shot).</summary>
        public void PlaySlide() => _animator.CrossFadeInFixedTime(_hashSlide, 0.1f);

        /// <summary>Cross-fade into the Death animation (no exit).</summary>
        public void PlayDeath() => _animator.CrossFadeInFixedTime(_hashDead,  0.2f);
    }
}

// ── ANIMATOR SETUP GUIDE ──────────────────────────────────────────────────
//
// Create an Animator Controller with these states:
//
//   [Run]   → loop, default entry state
//   [Jump]  → one-shot, transition back to Run on finish
//   [Slide] → one-shot, transition back to Run on finish
//   [Dead]  → one-shot, no exit transition
//
// Parameters needed:
//   None — CrossFadeInFixedTime uses the state NAME hash directly.
//   The Animator hashes match the state names set above.
//
// ─────────────────────────────────────────────────────────────────────────
