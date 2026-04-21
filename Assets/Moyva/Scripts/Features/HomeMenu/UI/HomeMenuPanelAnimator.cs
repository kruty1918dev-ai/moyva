using System;
using System.Collections;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Анімує переходи між панелями головного меню через CanvasGroup:
    /// плавний fade (alpha 0→1 / 1→0) без сторонніх бібліотек.
    ///
    /// Якщо компонент відсутній — <see cref="HomeMenuRootView"/> автоматично
    /// перемикається на миттєвий SetActive без анімації.
    /// </summary>
    public sealed class HomeMenuPanelAnimator : MonoBehaviour
    {
        [Serializable]
        public struct PanelBinding
        {
            public HomeMenuPanel Panel;
            public CanvasGroup   Group;
        }

        [SerializeField] private float fadeDuration = 0.22f;
        [SerializeField] private List<PanelBinding> bindings = new();

        private Coroutine _running;

        // ── Public API ───────────────────────────────────────────────────

        /// <summary>Анімований перехід від одного стану до іншого.</summary>
        public void TransitionTo(HomeMenuPanel from, HomeMenuPanel to)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Routine(from, to));
        }

        /// <summary>Миттєво встановлює стан панелей без анімації (використовується при ініціалізації).</summary>
        public void SetInstant(HomeMenuPanel active)
        {
            foreach (var b in bindings)
                ApplyGroup(b.Group, b.Panel == active);
        }

        // ── Internal ──────────────────────────────────────────────────────

        private IEnumerator Routine(HomeMenuPanel from, HomeMenuPanel to)
        {
            CanvasGroup fromGrp = Find(from);
            CanvasGroup toGrp   = Find(to);

            // Підготувати destination: видима структура, але прозора
            if (toGrp != null)
            {
                toGrp.gameObject.SetActive(true);
                toGrp.alpha          = 0f;
                toGrp.interactable   = false;
                toGrp.blocksRaycasts = false;
            }

            float elapsed    = 0f;
            float startAlpha = fromGrp != null ? fromGrp.alpha : 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));

                if (fromGrp != null) fromGrp.alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (toGrp   != null) toGrp.alpha   = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            if (fromGrp != null)
            {
                fromGrp.alpha          = 0f;
                fromGrp.interactable   = false;
                fromGrp.blocksRaycasts = false;
                fromGrp.gameObject.SetActive(false);
            }

            if (toGrp != null)
            {
                toGrp.alpha          = 1f;
                toGrp.interactable   = true;
                toGrp.blocksRaycasts = true;
            }

            _running = null;
        }

        private CanvasGroup Find(HomeMenuPanel p)
        {
            foreach (var b in bindings)
                if (b.Panel == p) return b.Group;
            return null;
        }

        private static void ApplyGroup(CanvasGroup g, bool active)
        {
            if (g == null) return;
            g.gameObject.SetActive(active);
            g.alpha          = active ? 1f : 0f;
            g.interactable   = active;
            g.blocksRaycasts = active;
        }
    }
}
