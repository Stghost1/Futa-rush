// ============================================================
// FUTA Rush — SceneLoader.cs
// Handles fade-in / fade-out scene transitions.
// Attach to a persistent GameObject with a full-screen
// black Image child called "FadePanel".
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FUTARush.Core
{
    public class SceneLoader : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static SceneLoader Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Fade Panel")]
        [Tooltip("Full-screen black UI Image used for fade transitions")]
        [SerializeField] private Image _fadePanel;

        [SerializeField] private float _fadeDuration = 0.4f;

        // ── Unity ──────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Start fully transparent
            if (_fadePanel != null)
            {
                Color c = _fadePanel.color;
                c.a = 0f;
                _fadePanel.color = c;
                _fadePanel.raycastTarget = false;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Fade out → load scene → fade in.</summary>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }

        // ── Private ────────────────────────────────────────────────────────
        private IEnumerator FadeAndLoad(string sceneName)
        {
            _fadePanel.raycastTarget = true;

            // Fade to black
            yield return StartCoroutine(Fade(0f, 1f));

            yield return SceneManager.LoadSceneAsync(sceneName);

            // Fade to clear
            yield return StartCoroutine(Fade(1f, 0f));

            _fadePanel.raycastTarget = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            Color c = _fadePanel.color;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                _fadePanel.color = c;
                yield return null;
            }

            c.a = to;
            _fadePanel.color = c;
        }
    }
}
