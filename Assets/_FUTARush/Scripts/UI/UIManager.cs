// ============================================================
// FUTA Rush — UIManager.cs
// Controls all UI panels: Home, HUD, Pause, Game Over.
// Listens to EventBus — zero polling required.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FUTARush.Core;

namespace FUTARush.UI
{
    public class UIManager : MonoBehaviour
    {
        // ── Inspector — Panels ─────────────────────────────────────────────
        [Header("Panels")]
        [SerializeField] private GameObject _homePanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _gameOverPanel;

        // ── Inspector — HUD ────────────────────────────────────────────────
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _tokenText;
        [SerializeField] private TextMeshProUGUI _multiplierText;   // "x8 ⭐"
        [SerializeField] private Button          _pauseButton;

        // ── Inspector — Game Over ──────────────────────────────────────────
        [Header("Game Over Elements")]
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _finalTokensText;
        [SerializeField] private Button          _restartButton;
        [SerializeField] private Button          _menuButton;

        // ── Inspector — Home ───────────────────────────────────────────────
        [Header("Home Screen Elements")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;

        // ── Inspector — Pause ──────────────────────────────────────────────
        [Header("Pause Elements")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _pauseMenuButton;

        // ── Inspector — Score Animation ────────────────────────────────────
        [Header("Score Counter")]
        [Tooltip("How fast the displayed score catches up to the real score")]
        [SerializeField] private float _scoreCountSpeed = 800f;

        // ── Private State ──────────────────────────────────────────────────
        private float _displayedScore;
        private int   _targetScore;
        private bool  _isPaused;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Awake()
        {
            WireButtons();
            ShowHome();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStartEvent>(OnGameStart);
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void Update()
        {
            AnimateScore();
        }

        // ── Button Wiring ──────────────────────────────────────────────────
        private void WireButtons()
        {
            _playButton?.onClick.AddListener(OnPlayPressed);
            _pauseButton?.onClick.AddListener(OnPausePressed);
            _resumeButton?.onClick.AddListener(OnResumePressed);
            _restartButton?.onClick.AddListener(OnRestartPressed);
            _menuButton?.onClick.AddListener(OnMenuPressed);
            _pauseMenuButton?.onClick.AddListener(OnMenuPressed);
        }

        // ── Button Callbacks ───────────────────────────────────────────────
        private void OnPlayPressed()
        {
            SceneLoader.Instance?.LoadScene("GamePlay");
        }

        private void OnPausePressed()
        {
            _isPaused      = true;
            Time.timeScale = 0f;
            _hudPanel?.SetActive(false);
            _pausePanel?.SetActive(true);
        }

        private void OnResumePressed()
        {
            _isPaused      = false;
            Time.timeScale = 1f;
            _pausePanel?.SetActive(false);
            _hudPanel?.SetActive(true);
        }

        private void OnRestartPressed()
        {
            Time.timeScale = 1f;
            _isPaused      = false;
            GameManager.Instance?.RestartGame();
        }

        private void OnMenuPressed()
        {
            Time.timeScale = 1f;
            _isPaused      = false;
            SceneLoader.Instance?.LoadScene("HomeScreen");
        }

        // ── Event Handlers ─────────────────────────────────────────────────
        private void OnGameStart(GameStartEvent _)
        {
            _displayedScore = 0f;
            _targetScore    = 0;
            ShowHUD();
        }

        private void OnGameOver(GameOverEvent e)
        {
            _hudPanel?.SetActive(false);
            _gameOverPanel?.SetActive(true);

            if (_finalScoreText  != null) _finalScoreText.text  = $"Score\n{e.Score:N0}";
            if (_finalTokensText != null) _finalTokensText.text = $"Tokens\n{e.Tokens}";
        }

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            _targetScore = e.Score;
        }

        private void OnTokenCollected(TokenCollectedEvent _)
        {
            if (_tokenText != null)
                _tokenText.text = GameManager.Instance.Tokens.ToString();

            StartCoroutine(PunchScale(_tokenText?.transform));
        }

        // ── Score Animation ────────────────────────────────────────────────
        private void AnimateScore()
        {
            if (_scoreText == null) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            _displayedScore = Mathf.MoveTowards(
                _displayedScore,
                _targetScore,
                _scoreCountSpeed * Time.deltaTime
            );

            _scoreText.text = Mathf.RoundToInt(_displayedScore).ToString("N0");
        }

        // ── Panel Management ───────────────────────────────────────────────
        private void ShowHome()
        {
            _homePanel?.SetActive(true);
            _hudPanel?.SetActive(false);
            _pausePanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
        }

        private void ShowHUD()
        {
            _homePanel?.SetActive(false);
            _hudPanel?.SetActive(true);
            _pausePanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private IEnumerator PunchScale(Transform t)
        {
            if (t == null) yield break;

            float duration = 0.15f;
            float elapsed  = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s  = Mathf.Lerp(1.3f, 1f, elapsed / duration);
                t.localScale = Vector3.one * s;
                yield return null;
            }

            t.localScale = Vector3.one;
        }
    }
}
