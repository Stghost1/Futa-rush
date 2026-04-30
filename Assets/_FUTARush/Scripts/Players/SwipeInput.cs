// ============================================================
// FUTA Rush — SwipeInput.cs
// Detects swipe gestures on mobile touch screens.
// Falls back to arrow keys in the Editor.
// ============================================================

using UnityEngine;
using System;

namespace FUTARush.Player
{
    public enum SwipeDirection { None, Left, Right, Up, Down }

    public class SwipeInput : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Swipe Sensitivity")]
        [Tooltip("Minimum pixel distance to register as a swipe")]
        [SerializeField] private float _minSwipeDistance = 50f;

        // ── Event ──────────────────────────────────────────────────────────
        /// <summary>Fired once per swipe gesture with the detected direction.</summary>
        public event Action<SwipeDirection> OnSwipe;

        // ── Private State ──────────────────────────────────────────────────
        private Vector2 _touchStartPos;
        private bool    _isSwiping;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Update()
        {
            HandleTouchInput();

#if UNITY_EDITOR
            HandleKeyboardFallback();
#endif
        }

        // ── Touch ──────────────────────────────────────────────────────────
        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStartPos = touch.position;
                    _isSwiping     = true;
                    break;

                case TouchPhase.Ended:
                    if (_isSwiping)
                    {
                        Vector2 delta = touch.position - _touchStartPos;
                        if (delta.magnitude >= _minSwipeDistance)
                            DispatchFromDelta(delta);
                        _isSwiping = false;
                    }
                    break;

                case TouchPhase.Canceled:
                    _isSwiping = false;
                    break;
            }
        }

        // ── Editor Fallback ────────────────────────────────────────────────
        private void HandleKeyboardFallback()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A))
                Fire(SwipeDirection.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                Fire(SwipeDirection.Right);
            if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W))
                Fire(SwipeDirection.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S))
                Fire(SwipeDirection.Down);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private void DispatchFromDelta(Vector2 delta)
        {
            bool horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
            Fire(horizontal
                ? (delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
                : (delta.y > 0 ? SwipeDirection.Up    : SwipeDirection.Down));
        }

        private void Fire(SwipeDirection dir) => OnSwipe?.Invoke(dir);
    }
}
