// ============================================================
// FUTA Rush — GameManager.cs
// Central state machine: Idle → Playing → GameOver
// Singleton — persists across scenes.
// ============================================================

using UnityEngine;

namespace FUTARush.Core
{
    public enum GameState { Idle, Playing, GameOver }

    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Speed Settings")]
        [SerializeField] private float _startSpeed     = 8f;
        [SerializeField] private float _maxSpeed       = 25f;
        [SerializeField] private float _speedIncrement = 0.5f;
        [SerializeField] private float _speedInterval  = 5f;   // seconds between speed bumps

        [Header("Score")]
        [SerializeField] private float _distanceMultiplier = 1f;

        // ── Public State ───────────────────────────────────────────────────
        public GameState State        { get; private set; } = GameState.Idle;
        public float     CurrentSpeed { get; private set; }
        public int       Score        { get; private set; }
        public int       Tokens       { get; private set; }

        // ── Private ────────────────────────────────────────────────────────
        private float _playTime;
        private float _nextSpeedBump;

        // ── Unity Lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            _playTime += Time.deltaTime;

            Score = Mathf.RoundToInt(_playTime * _distanceMultiplier * CurrentSpeed);
            EventBus.Publish(new ScoreChangedEvent { Score = Score });

            if (Time.time >= _nextSpeedBump)
            {
                BumpSpeed();
                _nextSpeedBump = Time.time + _speedInterval;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Call this to begin a new run.</summary>
        public void StartGame()
        {
            Score        = 0;
            Tokens       = 0;
            _playTime    = 0f;
            CurrentSpeed = _startSpeed;
            _nextSpeedBump = Time.time + _speedInterval;

            State = GameState.Playing;
            EventBus.Publish(new GameStartEvent());
        }

        /// <summary>Called when player hits an obstacle.</summary>
        public void TriggerGameOver()
        {
            if (State == GameState.GameOver) return;
            State = GameState.GameOver;
            EventBus.Publish(new GameOverEvent { Score = Score, Tokens = Tokens });
        }

        /// <summary>Restart without going back to home screen.</summary>
        public void RestartGame()
        {
            StartGame();
        }

        // ── Private ────────────────────────────────────────────────────────
        private void BumpSpeed()
        {
            CurrentSpeed = Mathf.Min(CurrentSpeed + _speedIncrement, _maxSpeed);
            EventBus.Publish(new SpeedChangedEvent { Speed = CurrentSpeed });
        }

        private void OnTokenCollected(TokenCollectedEvent e)
        {
            Tokens += e.Amount;
        }
    }
}
