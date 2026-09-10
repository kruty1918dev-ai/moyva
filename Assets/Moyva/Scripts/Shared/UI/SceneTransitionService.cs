using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Shared.UI
{
    public interface ISceneTransitionService
    {
        Task CoverAsync(CancellationToken ct = default);
        Task RevealAsync(CancellationToken ct = default);
    }

    internal sealed class SceneTransitionService : ISceneTransitionService
    {
        private const int StripeCount = 7;
        private const float Duration = 0.32f;
        private CanvasGroup _group;
        private RectTransform[] _stripes;

        public Task CoverAsync(CancellationToken ct = default)
            => AnimateAsync(covered: true, ct);

        public Task RevealAsync(CancellationToken ct = default)
            => AnimateAsync(covered: false, ct);

        private async Task AnimateAsync(bool covered, CancellationToken ct)
        {
            EnsureOverlay();
            _group.gameObject.SetActive(true);
            _group.blocksRaycasts = true;
            _group.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < Duration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Duration);
                float eased = t * t * (3f - 2f * t);
                ApplyProgress(covered ? eased : 1f - eased);
                await Task.Yield();
            }

            ApplyProgress(covered ? 1f : 0f);
            _group.blocksRaycasts = covered;
            if (!covered)
                _group.gameObject.SetActive(false);
        }

        private void ApplyProgress(float progress)
        {
            if (_stripes == null)
                return;

            for (int i = 0; i < _stripes.Length; i++)
            {
                var stripe = _stripes[i];
                if (stripe == null)
                    continue;

                float delay = i * 0.045f;
                float local = Mathf.Clamp01((progress - delay) / (1f - delay));
                stripe.localScale = new Vector3(1f, local, 1f);
            }
        }

        private void EnsureOverlay()
        {
            if (_group != null)
                return;

            var root = new GameObject("MoyvaSceneTransition", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            Object.DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _group = root.GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _stripes = new RectTransform[StripeCount];

            for (int i = 0; i < StripeCount; i++)
            {
                var stripeGo = new GameObject($"Stripe_{i:00}", typeof(RectTransform), typeof(Image));
                stripeGo.transform.SetParent(root.transform, false);
                var rect = stripeGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(i / (float)StripeCount, 0f);
                rect.anchorMax = new Vector2((i + 1) / (float)StripeCount, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, i % 2 == 0 ? 0f : 1f);
                rect.localScale = new Vector3(1f, 0f, 1f);
                var image = stripeGo.GetComponent<Image>();
                image.color = i % 2 == 0
                    ? new Color(0.02f, 0.028f, 0.035f, 1f)
                    : new Color(0.055f, 0.07f, 0.08f, 1f);
                _stripes[i] = rect;
            }

            root.SetActive(false);
        }
    }
}
