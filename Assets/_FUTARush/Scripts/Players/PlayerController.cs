// ============================================================
// FUTA Rush — PlayerController.cs
// Handles: lane switching, jumping, sliding, death.
// Requires: CharacterController, SwipeInput, PlayerAnimator
// ============================================================

using UnityEngine;
using FUTARush.Core;

namespace FUTARush.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SwipeInput))]
    public class PlayerController : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Lanes")]
        [Tooltip("World-unit distance between lane centres")]
        [SerializeField] private float _laneWidth       = 3f;
        [SerializeField] private float _laneSwitchSpeed = 10f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce  = 12f;
        [SerializeField] private float _gravity    = -28f;

        [Header("Slide")]
        [SerializeField] private float _slideDuration  = 0.75f;
        [SerializeField] private float _slideHeight     = 0.6f;
        [SerializeField] private float _normalHeight    = 1.8f;

        // ── Components ─────────────────────────────────────────────────────
        private CharacterController _cc;
        private SwipeInput          _swipeInput;
        private PlayerAnimator      _anim;

        // ── State ──────────────────────────────────────────────────────────
        private int   _currentLane     = 1;   // 0=Left | 1=Center | 2=Right
        private float _targetX;
        private float _verticalVelocity;
        private bool  _isSliding;
        private float _slideTimer;
        private bool  _isDead;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Awake()
        {
            _cc        = GetComponent<CharacterController>();
            _swipeInput = GetComponent<SwipeInput>();
            _anim       = GetComponent<PlayerAnimator>();

            _cc.height = _normalHeight;
            RefreshTargetX();
        }

        private void OnEnable()
        {
            _swipeInput.OnSwipe += HandleSwipe;
            EventBus.Subscribe<GameStartEvent>(OnGameStart);
        }

        private void OnDisable()
        {
            _swipeInput.OnSwipe -= HandleSwipe;
            EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
        }

        private void Update()
        {
            if (_isDead) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State != GameState.Playing) return;

            ApplyForwardAndLateral();
            ApplyGravity();
            TickSlide();
        }

        // ── Movement ───────────────────────────────────────────────────────
        private void ApplyForwardAndLateral()
        {
            float speed    = GameManager.Instance.CurrentSpeed;
            float currentX = transform.position.x;
            float newX     = Mathf.MoveTowards(currentX, _targetX, _laneSwitchSpeed * Time.deltaTime);

            Vector3 motion = new Vector3(
                newX - currentX,
                _verticalVelocity * Time.deltaTime,
                speed * Time.deltaTime
            );

            _cc.Move(motion);
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;         // small push to stay grounded
            else
                _verticalVelocity += _gravity * Time.deltaTime;
        }

        private void TickSlide()
        {
            if (!_isSliding) return;
            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0f) EndSlide();
        }

        // ── Swipe Handling ─────────────────────────────────────────────────
        private void HandleSwipe(SwipeDirection dir)
        {
            if (_isDead) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            switch (dir)
            {
                case SwipeDirection.Left:
                    if (_currentLane > 0) { _currentLane--; RefreshTargetX(); }
                    break;

                case SwipeDirection.Right:
                    if (_currentLane < 2) { _currentLane++; RefreshTargetX(); }
                    break;

                case SwipeDirection.Up:
                    TryJump();
                    break;

                case SwipeDirection.Down:
                    TrySlide();
                    break;
            }
        }

        private void TryJump()
        {
            if (!_cc.isGrounded) return;
            _verticalVelocity = _jumpForce;
            _anim?.PlayJump();
        }

        private void TrySlide()
        {
            if (_isSliding) return;
            _isSliding  = true;
            _slideTimer = _slideDuration;
            _cc.height  = _slideHeight;
            _anim?.PlaySlide();
        }

        private void EndSlide()
        {
            _isSliding  = false;
            _cc.height  = _normalHeight;
            _anim?.PlayRun();
        }

        private void RefreshTargetX() => _targetX = (_currentLane - 1) * _laneWidth;

        // ── Collision & Death ──────────────────────────────────────────────
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_isDead && hit.gameObject.CompareTag("Obstacle"))
                Die();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Token"))
            {
                EventBus.Publish(new TokenCollectedEvent { Amount = 1 });
                other.gameObject.SetActive(false);   // return to pool
            }
        }

        private void Die()
        {
            _isDead = true;
            _anim?.PlayDeath();
            GameManager.Instance.TriggerGameOver();
        }

        // ── Event Callbacks ────────────────────────────────────────────────
        private void OnGameStart(GameStartEvent _)
        {
            _isDead           = false;
            _currentLane      = 1;
            _verticalVelocity = 0f;
            _isSliding        = false;
            _cc.height        = _normalHeight;
            RefreshTargetX();
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _anim?.PlayRun();
        }
    }
}
