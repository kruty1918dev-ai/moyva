using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Набір спрайтів boot-екрана. Процедурно згенеровані спрайти реєструються
    /// через <see cref="Track"/> і звільняються в <see cref="Dispose"/>;
    /// імпортовані override-спрайти не відстежуються й не знищуються.
    /// </summary>
    internal sealed class BootScreenArtSet : IDisposable
    {
        private readonly List<Sprite> _ownedSprites = new();

        public Sprite Logo;
        public Sprite[] Slides = Array.Empty<Sprite>();
        public Sprite Background;
        public Sprite Veil;
        public Sprite Vignette;
        public Sprite BarTrack;
        public Sprite BarFill;
        public Sprite BarFrame;

        /// <summary>Зареєструвати згенерований спрайт як власність цього набору.</summary>
        public void Track(Sprite sprite)
        {
            if (sprite != null)
                _ownedSprites.Add(sprite);
        }

        public void Dispose()
        {
            for (int i = 0; i < _ownedSprites.Count; i++)
                DestroySprite(_ownedSprites[i]);
            _ownedSprites.Clear();
        }

        /// <summary>Знищити спрайт разом із його текстурою, якщо вони існують.</summary>
        public static void DestroySprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (sprite.texture != null)
                UnityEngine.Object.Destroy(sprite.texture);

            UnityEngine.Object.Destroy(sprite);
        }
    }

    /// <summary>
    /// Генерує runtime-графіку для екрана завантаження: емблему Moyva,
    /// тематичні слайди та елементи прогрес-бару. Працює без імпортованих asset-ів.
    /// </summary>
    internal static class BootScreenArt
    {
        /// <summary>Створити набір спрайтів boot-екрана з опційними override-замінами.</summary>
        /// <param name="logoOverride">Імпортований логотип; якщо null — генерується процедурний.</param>
        /// <param name="slideOverrides">Імпортовані слайди; якщо порожньо — генеруються процедурні.</param>
        public static BootScreenArtSet Create(Sprite logoOverride, Sprite[] slideOverrides)
        {
            var set = new BootScreenArtSet();

            if (logoOverride != null)
                set.Logo = logoOverride;
            else
            {
                set.Logo = CreateLogoSprite();
                set.Track(set.Logo);
            }

            if (slideOverrides != null && slideOverrides.Length > 0)
                set.Slides = slideOverrides;
            else
            {
                set.Slides = CreateSlideSprites();
                for (int i = 0; i < set.Slides.Length; i++)
                    set.Track(set.Slides[i]);
            }

            set.Background = CreateBackgroundSprite();
            set.Veil = CreateVeilSprite();
            set.Vignette = CreateVignetteSprite();
            set.BarTrack = CreateBarTrackSprite();
            set.BarFill = CreateBarFillSprite();
            set.BarFrame = CreateBarFrameSprite();
            set.Track(set.Background);
            set.Track(set.Veil);
            set.Track(set.Vignette);
            set.Track(set.BarTrack);
            set.Track(set.BarFill);
            set.Track(set.BarFrame);
            return set;
        }

        // ------------------------------------------------------------------
        // Логотип
        // ------------------------------------------------------------------

        /// <summary>Намалювати емблему Moyva: гексагональний щит із золотим монограмним «M».</summary>
        private static Sprite CreateLogoSprite()
        {
            const int size = 512;
            var tex = NewTexture(size, size);
            var pixels = new Color32[size * size];

            // 1: Контрольні точки монограми «M» у просторі -1..1.
            var mA = new Vector2(-0.40f, -0.46f);
            var mB = new Vector2(-0.40f, 0.46f);
            var mC = new Vector2(0f, -0.08f);
            var mD = new Vector2(0.40f, 0.46f);
            var mE = new Vector2(0.40f, -0.46f);
            const float strokeRadius = 0.085f;
            const float hexRadius = 0.74f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(
                        (x + 0.5f - size * 0.5f) / (size * 0.5f),
                        (y + 0.5f - size * 0.5f) / (size * 0.5f));

                    var color = new Color(0f, 0f, 0f, 0f);

                    // 2: М'яка тінь під щитом.
                    float shadowSd = SdHexagon(p - new Vector2(0f, -0.05f), hexRadius + 0.02f);
                    float shadow = Mathf.Clamp01(-shadowSd * 6f) * 0.45f;
                    color = BlendOver(color, new Color(0f, 0f, 0f, shadow));

                    // 3: Зовнішнє золоте світіння навколо щита.
                    float hexSd = SdHexagon(p, hexRadius);
                    float glow = Mathf.Clamp01(1f - Mathf.Abs(hexSd) * 8f) * 0.35f;
                    if (hexSd > 0f)
                        glow = Mathf.Clamp01(1f - hexSd * 5f) * 0.30f;
                    color = BlendOver(color, new Color(0.95f, 0.75f, 0.35f, glow * 0.5f));

                    // 4: Заливка щита глибоким синьо-графітовим градієнтом.
                    float fill = Mathf.Clamp01(-hexSd * 40f);
                    float fillT = Mathf.Clamp01(p.y * 0.5f + 0.5f);
                    var fillColor = Color.Lerp(
                        new Color(0.035f, 0.05f, 0.085f),
                        new Color(0.10f, 0.14f, 0.20f),
                        fillT);
                    color = BlendOver(color, new Color(fillColor.r, fillColor.g, fillColor.b, fill));

                    // 5: Золота облямівка щита з вертикальним градієнтом металу.
                    float border = Mathf.Clamp01(1f - Mathf.Abs(hexSd) * 34f);
                    float goldT = Mathf.Clamp01(p.y * 0.45f + 0.55f);
                    var gold = Color.Lerp(
                        new Color(0.62f, 0.45f, 0.16f),
                        new Color(1f, 0.86f, 0.5f),
                        goldT);
                    color = BlendOver(color, new Color(gold.r, gold.g, gold.b, border));

                    // 6: Тонка внутрішня лінія для глибини облямівки.
                    float inner = Mathf.Clamp01(1f - Mathf.Abs(hexSd + 0.075f) * 90f) * 0.4f;
                    color = BlendOver(color, new Color(0.85f, 0.68f, 0.35f, inner));

                    // 7: Тінь монограми.
                    float mShadow = MonogramSd(p - new Vector2(0f, -0.035f), mA, mB, mC, mD, mE);
                    float mShadowCover = Mathf.Clamp01(1f - (mShadow - strokeRadius) * 30f);
                    color = BlendOver(color, new Color(0f, 0f, 0f, mShadowCover * 0.55f));

                    // 8: Золота монограма «M» з вертикальним металевим градієнтом.
                    float mSd = MonogramSd(p, mA, mB, mC, mD, mE);
                    float mCover = Mathf.Clamp01(1f - (mSd - strokeRadius) * 30f);
                    float mT = Mathf.Clamp01(p.y * 0.6f + 0.5f);
                    var mGold = Color.Lerp(
                        new Color(0.68f, 0.5f, 0.18f),
                        new Color(1f, 0.9f, 0.58f),
                        mT);
                    color = BlendOver(color, new Color(mGold.r, mGold.g, mGold.b, mCover));

                    // 9: Верхній відблиск на монограмі.
                    float sheen = Mathf.Clamp01(1f - (mSd - strokeRadius * 0.55f) * 40f)
                        * Mathf.Clamp01(p.y * 2f - 0.2f) * 0.35f;
                    color = BlendOver(color, new Color(1f, 0.98f, 0.85f, sheen * mCover));

                    pixels[y * size + x] = color;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        /// <summary>SDF монограми «M» як мінімум відстаней до чотирьох сегментів.</summary>
        private static float MonogramSd(
            Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 e)
        {
            float sd = SdSegment(p, a, b);
            sd = Mathf.Min(sd, SdSegment(p, b, c));
            sd = Mathf.Min(sd, SdSegment(p, c, d));
            sd = Mathf.Min(sd, SdSegment(p, d, e));
            return sd;
        }

        // ------------------------------------------------------------------
        // Слайди
        // ------------------------------------------------------------------

        /// <summary>Створити три тематичні слайди у стилі процедурного світу гри.</summary>
        private static Sprite[] CreateSlideSprites()
        {
            return new[]
            {
                CreateSlideTexture(PaintValleySlide),
                CreateSlideTexture(PaintHighlandsSlide),
                CreateSlideTexture(PaintNightSlide),
            };
        }

        private static Sprite CreateSlideTexture(Action<Texture2D> painter)
        {
            var tex = NewTexture(960, 540);
            painter(tex);
            tex.Apply();
            return ToSprite(tex);
        }

        /// <summary>Слайд денної долини: небо, сонце й клітинний ландшафт.</summary>
        private static void PaintValleySlide(Texture2D tex)
        {
            int w = tex.width, h = tex.height;
            var skyTop = new Color(0.36f, 0.55f, 0.74f);
            var skyHorizon = new Color(0.78f, 0.85f, 0.85f);
            const float terrainTop = 0.48f;
            const int cellPx = 40;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float ny = y / (float)(h - 1);
                    var color = Color.Lerp(skyHorizon, skyTop, Mathf.Clamp01((ny - terrainTop) / (1f - terrainTop)));

                    // 1: Світіння сонця у верхньому правому куті.
                    float sun = Radial(x / (float)w, ny, 0.8f, 0.82f, 0.28f);
                    color = Color.Lerp(color, new Color(1f, 0.92f, 0.7f), sun * 0.55f);

                    if (ny < terrainTop)
                    {
                        // 2: Клітинний біом під небом.
                        int cx = x / cellPx;
                        int cy = y / cellPx;
                        color = BiomeColor(cx, cy, seed: 5, night: false);
                        color = ApplyGridLine(color, x, y, cellPx);
                        color = ApplyForestDetail(color, cx, cy, x, y, cellPx, seed: 5);
                    }

                    // 3: Легке затемнення низу під прогрес-бар.
                    color = ApplyBottomShade(color, ny);
                    tex.SetPixel(x, y, color);
                }
            }
        }

        /// <summary>Слайд верховин на світанку: шаруваті силуети хребтів і замок.</summary>
        private static void PaintHighlandsSlide(Texture2D tex)
        {
            int w = tex.width, h = tex.height;
            var skyTop = new Color(0.28f, 0.2f, 0.36f);
            var skyHorizon = new Color(0.93f, 0.58f, 0.34f);
            var ridgeFar = new Color(0.45f, 0.31f, 0.38f);
            var ridgeMid = new Color(0.31f, 0.21f, 0.3f);
            var ridgeNear = new Color(0.18f, 0.13f, 0.2f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = x / (float)(w - 1);
                    float ny = y / (float)(h - 1);

                    // 1: Світанкове небо з теплим світінням над обрієм.
                    var color = Color.Lerp(skyHorizon, skyTop, Mathf.Clamp01((ny - 0.3f) / 0.7f));
                    float sun = Radial(nx, ny, 0.52f, 0.34f, 0.3f);
                    color = Color.Lerp(color, new Color(1f, 0.8f, 0.5f), sun * 0.5f);

                    // 2: Три хребти від дальнього до ближнього.
                    float ridge1 = 0.52f + 0.12f * Fbm(nx * 3.1f, 0f, 21);
                    float ridge2 = 0.38f + 0.10f * Fbm(nx * 4.4f, 7f, 34);
                    float ridge3 = 0.22f + 0.09f * Fbm(nx * 6.2f, 3f, 55);
                    if (ny < ridge1) color = ridgeFar;
                    if (ny < ridge2) color = ridgeMid;
                    if (ny < ridge3) color = ridgeNear;

                    tex.SetPixel(x, y, ApplyBottomShade(color, ny));
                }
            }

            // 3: Силует замка на середньому хребті.
            PaintCastleSilhouette(tex, 0.68f, 0.40f, new Color(0.12f, 0.09f, 0.14f));
        }

        /// <summary>Слайд ночі: зорі, місяць і темний ландшафт із вогнями осель.</summary>
        private static void PaintNightSlide(Texture2D tex)
        {
            int w = tex.width, h = tex.height;
            var skyTop = new Color(0.03f, 0.045f, 0.1f);
            var skyHorizon = new Color(0.09f, 0.13f, 0.22f);
            const float terrainTop = 0.45f;
            const int cellPx = 40;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = x / (float)(w - 1);
                    float ny = y / (float)(h - 1);
                    var color = Color.Lerp(skyHorizon, skyTop, Mathf.Clamp01((ny - terrainTop) / (1f - terrainTop)));

                    // 1: Місячне світіння у верхньому лівому куті.
                    float moon = Radial(nx, ny, 0.18f, 0.8f, 0.16f);
                    color = Color.Lerp(color, new Color(0.75f, 0.82f, 0.95f), moon * 0.5f);

                    if (ny >= terrainTop)
                    {
                        // 2: Рідкісні зорі різної яскравості.
                        float star = Hash(x, y, 97);
                        if (star > 0.9975f)
                            color = Color.Lerp(color, Color.white, (star - 0.9975f) * 220f);
                    }
                    else
                    {
                        // 3: Нічний ландшафт і теплі вогні поселень.
                        int cx = x / cellPx;
                        int cy = y / cellPx;
                        color = BiomeColor(cx, cy, seed: 23, night: true);
                        color = ApplyGridLine(color, x, y, cellPx);
                        color = ApplySettlementLights(color, cx, cy, x, y, cellPx, seed: 23);
                    }

                    tex.SetPixel(x, y, ApplyBottomShade(color, ny));
                }
            }
        }

        /// <summary>Колір біому клітини за процедурною висотою/вологістю.</summary>
        private static Color BiomeColor(int cx, int cy, int seed, bool night)
        {
            float e = Fbm(cx * 0.13f, cy * 0.13f, seed);
            float m = Fbm(cx * 0.17f + 37f, cy * 0.17f, seed + 11);
            float jitter = (Hash(cx, cy, seed + 3) - 0.5f) * 0.05f;
            Color color;

            if (night)
            {
                if (e < 0.40f) color = new Color(0.05f, 0.09f, 0.16f);
                else if (e < 0.46f) color = new Color(0.1f, 0.12f, 0.12f);
                else if (e < 0.80f) color = m > 0.6f ? new Color(0.04f, 0.1f, 0.07f) : new Color(0.08f, 0.15f, 0.1f);
                else color = new Color(0.1f, 0.1f, 0.13f);
            }
            else
            {
                if (e < 0.32f) color = new Color(0.13f, 0.32f, 0.47f);
                else if (e < 0.40f) color = new Color(0.22f, 0.47f, 0.6f);
                else if (e < 0.46f) color = new Color(0.83f, 0.75f, 0.55f);
                else if (e < 0.78f) color = m > 0.6f ? new Color(0.2f, 0.42f, 0.25f) : new Color(0.36f, 0.56f, 0.32f);
                else if (e < 0.88f) color = new Color(0.5f, 0.5f, 0.38f);
                else if (e > 0.94f) color = new Color(0.88f, 0.9f, 0.92f);
                else color = new Color(0.48f, 0.45f, 0.42f);
            }

            return new Color(
                Mathf.Clamp01(color.r + jitter),
                Mathf.Clamp01(color.g + jitter),
                Mathf.Clamp01(color.b + jitter));
        }

        /// <summary>Тонка сітка поверх ландшафту.</summary>
        private static Color ApplyGridLine(Color color, int x, int y, int cellPx)
        {
            if (x % cellPx == 0 || y % cellPx == 0)
                return Color.Lerp(color, Color.black, 0.12f);
            return color;
        }

        /// <summary>Трикутні крони дерев на лісових клітинах денного слайда.</summary>
        private static Color ApplyForestDetail(
            Color color, int cx, int cy, int x, int y, int cellPx, int seed)
        {
            float m = Fbm(cx * 0.17f + 37f, cy * 0.17f, seed + 11);
            float e = Fbm(cx * 0.13f, cy * 0.13f, seed);
            if (e < 0.46f || e >= 0.78f || m <= 0.6f)
                return color;

            // 1: Локальні координати всередині клітини.
            int lx = x % cellPx;
            int ly = y % cellPx;
            float tree = Hash(cx, cy, seed + 29);
            if (tree < 0.35f)
                return color;

            // 2: Маленький трикутник крони у центрі клітини.
            float cxp = cellPx * 0.5f;
            float tri = Mathf.Abs(lx - cxp) / (cellPx * 0.28f) + (ly - cellPx * 0.3f) / (cellPx * 0.4f);
            if (tri < 1f && ly > cellPx * 0.25f)
                return Color.Lerp(color, new Color(0.1f, 0.25f, 0.14f), 0.85f);
            return color;
        }

        /// <summary>Теплі вогні вікон/багать на поселенських клітинах нічного слайда.</summary>
        private static Color ApplySettlementLights(
            Color color, int cx, int cy, int x, int y, int cellPx, int seed)
        {
            float settled = Hash(cx, cy, seed + 41);
            if (settled < 0.12f)
                return color;

            // 1: Кілька теплих пікселів усередині клітини за детермінованим хешем.
            int lx = x % cellPx;
            int ly = y % cellPx;
            float spark = Hash(lx + cx * 31, ly + cy * 57, seed + 43);
            if (spark > 0.94f)
                return Color.Lerp(color, new Color(1f, 0.72f, 0.3f), 0.9f);
            return color;
        }

        /// <summary>Силует замка: база, дві вежі й зубці.</summary>
        private static void PaintCastleSilhouette(Texture2D tex, float centerNx, float baseNy, Color color)
        {
            int w = tex.width, h = tex.height;
            int cx = Mathf.RoundToInt(centerNx * w);
            int baseY = Mathf.RoundToInt(baseNy * h);
            int bodyW = Mathf.RoundToInt(w * 0.055f);
            int bodyH = Mathf.RoundToInt(h * 0.085f);
            int towerW = Mathf.RoundToInt(w * 0.016f);
            int towerH = Mathf.RoundToInt(h * 0.05f);
            int merlon = Mathf.Max(2, Mathf.RoundToInt(w * 0.004f));

            // 1: Центральна зала.
            FillRect(tex, cx - bodyW / 2, baseY, bodyW, bodyH, color);
            // 2: Бічні вежі.
            FillRect(tex, cx - bodyW / 2 - towerW, baseY, towerW, bodyH + towerH, color);
            FillRect(tex, cx + bodyW / 2, baseY, towerW, bodyH + towerH, color);
            // 3: Зубці над залою та вежами.
            for (int mx = cx - bodyW / 2; mx < cx + bodyW / 2; mx += merlon * 2)
                FillRect(tex, mx, baseY + bodyH, merlon, merlon, color);
            FillRect(tex, cx - bodyW / 2 - towerW, baseY + bodyH + towerH, towerW, merlon, color);
            FillRect(tex, cx + bodyW / 2, baseY + bodyH + towerH, towerW, merlon, color);
            // 4: Стрічковий прапор над лівою вежею.
            FillRect(tex, cx - bodyW / 2 - towerW / 2, baseY + bodyH + towerH + merlon, 1, merlon * 2, color);
            FillRect(tex, cx - bodyW / 2 - towerW / 2, baseY + bodyH + towerH + merlon * 2, merlon * 2, merlon, color);
        }

        private static void FillRect(Texture2D tex, int x0, int y0, int w, int h, Color color)
        {
            for (int y = y0; y < y0 + h && y < tex.height; y++)
            {
                if (y < 0)
                    continue;
                for (int x = x0; x < x0 + w && x < tex.width; x++)
                {
                    if (x < 0)
                        continue;
                    tex.SetPixel(x, y, color);
                }
            }
        }

        /// <summary>Поступове затемнення нижньої смуги слайда.</summary>
        private static Color ApplyBottomShade(Color color, float ny)
        {
            float shade = Mathf.Clamp01((0.22f - ny) / 0.22f) * 0.45f;
            return Color.Lerp(color, Color.black, shade);
        }

        /// <summary>Радіальне світіння з м'якими краями.</summary>
        private static float Radial(float nx, float ny, float cx, float cy, float radius)
        {
            float dx = (nx - cx) * 1.6f;
            float dy = ny - cy;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(1f - d / radius);
        }

        // ------------------------------------------------------------------
        // UI-спрайти
        // ------------------------------------------------------------------

        /// <summary>Темний вертикальний градієнт фону під слайдами.</summary>
        private static Sprite CreateBackgroundSprite()
        {
            var tex = NewTexture(8, 512);
            var top = new Color(0.03f, 0.04f, 0.06f);
            var bottom = new Color(0.012f, 0.016f, 0.026f);
            for (int y = 0; y < tex.height; y++)
            {
                var color = Color.Lerp(bottom, top, y / (float)(tex.height - 1));
                for (int x = 0; x < tex.width; x++)
                    tex.SetPixel(x, y, color);
            }
            tex.Apply();
            return ToSprite(tex);
        }

        /// <summary>Напівпрозора завіса, що затемнює слайди для читабельності логотипа й бару.</summary>
        private static Sprite CreateVeilSprite()
        {
            var tex = NewTexture(8, 512);
            for (int y = 0; y < tex.height; y++)
            {
                float t = y / (float)(tex.height - 1);
                float alpha = Mathf.Lerp(0.78f, 0.34f, Mathf.Pow(t, 1.3f));
                for (int x = 0; x < tex.width; x++)
                    tex.SetPixel(x, y, new Color(0.01f, 0.012f, 0.02f, alpha));
            }
            tex.Apply();
            return ToSprite(tex);
        }

        /// <summary>Радіальна віньєтка, що затемнює краї екрана.</summary>
        private static Sprite CreateVignetteSprite()
        {
            const int size = 256;
            var tex = NewTexture(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                    float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((d - 0.55f) / 0.75f);
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha * 0.6f));
                }
            }
            tex.Apply();
            return ToSprite(tex);
        }

        /// <summary>Темна заокруглена основа прогрес-бару.</summary>
        private static Sprite CreateBarTrackSprite()
        {
            return CreateRoundedRectSprite(256, 20, 10f, (p, half, r, sd) =>
            {
                float fill = Mathf.Clamp01(-sd * 2f);
                var c = Color.Lerp(new Color(0.03f, 0.04f, 0.055f), new Color(0.07f, 0.08f, 0.11f), p.y * 0.5f + 0.5f);
                float topEdge = Mathf.Clamp01((p.y - 0.55f) * 4f) * 0.25f;
                return new Color(c.r + topEdge, c.g + topEdge, c.b + topEdge, fill * 0.95f);
            });
        }

        /// <summary>Золота заливка прогрес-бару з вертикальним металевим градієнтом.</summary>
        private static Sprite CreateBarFillSprite()
        {
            return CreateRoundedRectSprite(256, 20, 10f, (p, half, r, sd) =>
            {
                float fill = Mathf.Clamp01(-sd * 2f);
                float t = Mathf.Clamp01(p.y * 0.5f + 0.5f);
                var c = Color.Lerp(new Color(0.68f, 0.48f, 0.16f), new Color(1f, 0.84f, 0.45f), t);
                float gloss = Mathf.Clamp01((p.y - 0.35f) * 3f) * 0.35f;
                return new Color(c.r + gloss * 0.4f, c.g + gloss * 0.35f, c.b + gloss * 0.25f, fill);
            });
        }

        /// <summary>Тонка золота рамка навколо прогрес-бару.</summary>
        private static Sprite CreateBarFrameSprite()
        {
            return CreateRoundedRectSprite(256, 20, 10f, (p, half, r, sd) =>
            {
                float ring = Mathf.Clamp01(1f - Mathf.Abs(sd) * 2.2f);
                return new Color(0.9f, 0.74f, 0.4f, ring * 0.5f);
            });
        }

        /// <summary>Намалювати спрайт заокругленого прямокутника через SDF-шейдер у пікселях.</summary>
        private static Sprite CreateRoundedRectSprite(
            int w, int h, float radius, Func<Vector2, Vector2, float, float, Color> shade)
        {
            var tex = NewTexture(w, h);
            var half = new Vector2(w * 0.5f, h * 0.5f);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = new Vector2(x + 0.5f - half.x, y + 0.5f - half.y);
                    float sd = SdRoundRect(p, half, radius);
                    tex.SetPixel(x, y, shade(p, half, radius, sd));
                }
            }
            tex.Apply();
            return ToSprite(tex);
        }

        // ------------------------------------------------------------------
        // Математика
        // ------------------------------------------------------------------

        /// <summary>Відстань від точки до відрізка.</summary>
        private static float SdSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * t).magnitude;
        }

        /// <summary>SDF гексагона з гострою вершиною вгорі (iq).</summary>
        private static float SdHexagon(Vector2 p, float r)
        {
            const float kx = -0.866025404f;
            const float ky = 0.5f;
            const float kz = 0.577350269f;
            p = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y));
            float dot = Mathf.Min(kx * p.x + ky * p.y, 0f);
            p -= new Vector2(kx, ky) * (2f * dot);
            p -= new Vector2(Mathf.Clamp(p.x, -kz * r, kz * r), r);
            return p.magnitude * Mathf.Sign(p.y);
        }

        /// <summary>SDF заокругленого прямокутника, центр у нулі.</summary>
        private static float SdRoundRect(Vector2 p, Vector2 half, float r)
        {
            var q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - (half - new Vector2(r, r));
            var outer = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
            return Mathf.Min(Mathf.Max(q.x, q.y), 0f) + outer.magnitude - r;
        }

        /// <summary>Детермінований хеш у 0..1 для процедурного шуму.</summary>
        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }

        /// <summary>Згладжений value-шум у 0..1.</summary>
        private static float ValueNoise(float x, float y, int seed)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;
            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);
            float a = Hash(ix, iy, seed);
            float b = Hash(ix + 1, iy, seed);
            float c = Hash(ix, iy + 1, seed);
            float d = Hash(ix + 1, iy + 1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, ux), Mathf.Lerp(c, d, ux), uy);
        }

        /// <summary>Фрактальний шум (кілька октав value-шуму).</summary>
        private static float Fbm(float x, float y, int seed, int octaves = 4)
        {
            float value = 0f;
            float amplitude = 0.5f;
            float frequency = 1f;
            for (int i = 0; i < octaves; i++)
            {
                value += amplitude * ValueNoise(x * frequency, y * frequency, seed + i * 131);
                frequency *= 2f;
                amplitude *= 0.5f;
            }
            return value;
        }

        /// <summary>Накласти src поверх dst за звичайним alpha-правилом.</summary>
        private static Color BlendOver(Color dst, Color src)
        {
            float a = src.a + dst.a * (1f - src.a);
            if (a <= 0.0001f)
                return new Color(0f, 0f, 0f, 0f);
            return new Color(
                (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a,
                (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a,
                (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a,
                a);
        }

        private static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        private static Sprite ToSprite(Texture2D tex)
        {
            return Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
