using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    public static class GameplayTurnHudProjectStyler
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string HudRootName = "GameplayTurnHud";
        private const string PanelSpritePath = "Assets/Moyva/Art/UI/Pop-Up/PopUp-Base.png";
        private const string ButtonIdlePath = "Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Idle.png";
        private const string ButtonHoverPath = "Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Hover.png";
        private const string ButtonPressedPath = "Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Clicked.png";
        private const string NextIconPath = "Assets/Moyva/Art/UI/Icons/Next-Icon/Next-Icon-Idle.png";

        private static readonly Color32 PrimaryText = new(255, 241, 210, 255);
        private static readonly Color32 SecondaryText = new(223, 201, 158, 255);
        private static readonly Color32 ViewportColor = new(37, 25, 17, 214);
        private static readonly Color32 QueueColor = new(236, 220, 190, 255);
        private static readonly Color32 Gold = new(205, 148, 73, 255);
        private static readonly Color32 DeepShadow = new(22, 12, 7, 220);

        public static string Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"No gameplay Canvas found in '{ScenePath}'.");

            Transform root = FindDirectChild(canvas.transform, HudRootName);
            if (root == null)
            {
                GameplayTurnHudSceneAuthoring.Apply();
                scene = SceneManager.GetActiveScene();
                canvas = FindPrimaryCanvas(scene);
                root = canvas == null ? null : FindDirectChild(canvas.transform, HudRootName);
            }

            if (canvas == null || root == null)
                throw new InvalidOperationException("GameplayTurnHud could not be authored before styling.");

            UiSkin skin = CaptureSkin(canvas, root);
            ApplyToOpenScene(scene, canvas, root, skin);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            return $"P17_STYLE_OK panel={AssetName(skin.PanelSprite)} button={AssetName(skin.ButtonIdle)} " +
                   $"font={(skin.Font != null ? skin.Font.name : "fallback")} source={skin.SourceSummary}";
        }

        public static void ApplyToOpenScene(Scene scene, Canvas canvas, Transform root)
        {
            ApplyToOpenScene(scene, canvas, root, CaptureSkin(canvas, root));
        }

        private static void ApplyToOpenScene(Scene scene, Canvas canvas, Transform root, UiSkin skin)
        {
            RectTransform turnPanel = RequireRect(root, "TurnPanel");
            SetTopLeft(turnPanel, 20f, 18f, 500f, 104f);
            ApplyPanel(turnPanel.GetComponent<Image>(), skin.PanelSprite);

            EnsureAccent(turnPanel, "TopAccent", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -2f), new Vector2(0f, 2f), Gold);

            TMP_Text turnText = Require<TMP_Text>(root, "TurnPanel/TurnSummary");
            ApplyText(turnText, skin, 15.5f, PrimaryText, TextAlignmentOptions.MidlineLeft);
            RectTransform turnTextRect = turnText.rectTransform;
            turnTextRect.anchorMin = new Vector2(0f, 0f);
            turnTextRect.anchorMax = new Vector2(1f, 1f);
            turnTextRect.pivot = new Vector2(0f, 0.5f);
            turnTextRect.offsetMin = new Vector2(18f, 34f);
            turnTextRect.offsetMax = new Vector2(-166f, -8f);
            turnText.textWrappingMode = TextWrappingModes.NoWrap;
            turnText.overflowMode = TextOverflowModes.Overflow;

            Button endTurnButton = Require<Button>(root, "TurnPanel/EndTurnButton");
            StyleButton(endTurnButton, skin, true);
            RectTransform endTurnRect = (RectTransform)endTurnButton.transform;
            endTurnRect.anchorMin = new Vector2(1f, 1f);
            endTurnRect.anchorMax = new Vector2(1f, 1f);
            endTurnRect.pivot = new Vector2(1f, 1f);
            endTurnRect.anchoredPosition = new Vector2(-12f, -10f);
            endTurnRect.sizeDelta = new Vector2(150f, 50f);

            TMP_Text endTurnLabel = endTurnButton.GetComponentInChildren<TMP_Text>(true);
            if (endTurnLabel != null)
            {
                ApplyText(endTurnLabel, skin, 13.5f, PrimaryText, TextAlignmentOptions.Center);
                SetStretch(endTurnLabel.rectTransform, 12f, 34f, 4f, 4f);
                EnsureTextShadow(endTurnLabel);
            }
            EnsureNextIcon(endTurnButton.transform, skin.NextIcon);

            TMP_Text status = Require<TMP_Text>(root, "TurnPanel/TurnStatus");
            ApplyText(status, skin, 12.5f, SecondaryText, TextAlignmentOptions.MidlineLeft);
            RectTransform statusRect = status.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0f);
            statusRect.pivot = new Vector2(0f, 0f);
            statusRect.anchoredPosition = new Vector2(18f, 9f);
            statusRect.sizeDelta = new Vector2(-36f, 23f);

            RectTransform unitPanel = RequireRect(root, "UnitStaminaPanel");
            unitPanel.anchorMin = Vector2.zero;
            unitPanel.anchorMax = Vector2.zero;
            unitPanel.pivot = Vector2.zero;
            unitPanel.anchoredPosition = new Vector2(20f, 18f);
            unitPanel.sizeDelta = new Vector2(350f, 46f);
            ApplyPanel(unitPanel.GetComponent<Image>(), skin.PanelSprite);

            TMP_Text unitText = Require<TMP_Text>(root, "UnitStaminaPanel/UnitStamina");
            ApplyText(unitText, skin, 13.5f, PrimaryText, TextAlignmentOptions.MidlineLeft);
            SetStretch(unitText.rectTransform, 14f, 14f, 4f, 4f);
            unitPanel.gameObject.SetActive(!string.IsNullOrWhiteSpace(unitText.text));

            RectTransform recruitment = RequireRect(root, "RecruitmentPanel");
            recruitment.anchorMin = new Vector2(1f, 0.5f);
            recruitment.anchorMax = new Vector2(1f, 0.5f);
            recruitment.pivot = new Vector2(1f, 0.5f);
            recruitment.anchoredPosition = new Vector2(-20f, 0f);
            recruitment.sizeDelta = new Vector2(352f, 474f);
            ApplyPanel(recruitment.GetComponent<Image>(), skin.PanelSprite);

            TMP_Text title = Require<TMP_Text>(root, "RecruitmentPanel/Title");
            ApplyText(title, skin, 18f, PrimaryText, TextAlignmentOptions.Center);
            SetTopStretch(title.rectTransform, 20f, 20f, 12f, 34f);
            EnsureTextShadow(title);

            RectTransform viewport = RequireRect(root, "RecruitmentPanel/RecipeViewport");
            viewport.anchorMin = new Vector2(0f, 0f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.offsetMin = new Vector2(16f, 132f);
            viewport.offsetMax = new Vector2(-16f, -58f);
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.sprite = null;
                viewportImage.type = Image.Type.Simple;
                viewportImage.color = ViewportColor;
                viewportImage.raycastTarget = true;
                EnsureOutline(viewportImage, new Color32(104, 70, 37, 190), new Vector2(1f, -1f));
            }

            RectTransform slots = RequireRect(root, "RecruitmentPanel/RecipeViewport/RecipeSlots");
            VerticalLayoutGroup layout = slots.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(7, 7, 7, 7);
                layout.spacing = 7f;
            }

            for (int index = 1; index <= 12; index++)
            {
                string path = $"RecruitmentPanel/RecipeViewport/RecipeSlots/RecipeSlot_{index:00}";
                Button recipeButton = Require<Button>(root, path);
                StyleButton(recipeButton, skin, false);
                LayoutElement element = recipeButton.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.minHeight = 39f;
                    element.preferredHeight = 39f;
                }

                TMP_Text label = recipeButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    ApplyText(label, skin, 12.5f, PrimaryText, TextAlignmentOptions.Center);
                    SetStretch(label.rectTransform, 10f, 10f, 3f, 3f);
                    EnsureTextShadow(label);
                }
            }

            TMP_Text queue = Require<TMP_Text>(root, "RecruitmentPanel/Queue");
            ApplyText(queue, skin, 12.5f, QueueColor, TextAlignmentOptions.TopLeft);
            RectTransform queueRect = queue.rectTransform;
            queueRect.anchorMin = new Vector2(0f, 0f);
            queueRect.anchorMax = new Vector2(1f, 0f);
            queueRect.pivot = new Vector2(0.5f, 0f);
            queueRect.anchoredPosition = new Vector2(0f, 12f);
            queueRect.sizeDelta = new Vector2(-34f, 104f);
            queue.textWrappingMode = TextWrappingModes.Normal;

            root.SetAsLastSibling();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static UiSkin CaptureSkin(Canvas canvas, Transform hudRoot)
        {
            TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text != null && !IsUnder(text.transform, hudRoot))
                .ToArray();

            TMP_Text buildLabel = texts.FirstOrDefault(text => ContainsAny(text.text, "Будувати", "Build"));
            TMP_Text resourceLabel = texts.FirstOrDefault(text => ContainsAny(text.text, "Materials", "Матеріали", "Food", "Їжа", "Money", "Гроші"));
            Button buildButton = FindButtonAncestor(buildLabel != null ? buildLabel.transform : null);
            Image resourcePanel = FindWideImageAncestor(resourceLabel != null ? resourceLabel.transform : null, canvas.transform);

            TMP_Text styleText = buildLabel != null ? buildLabel : resourceLabel;
            TMP_FontAsset font = styleText != null ? styleText.font : null;
            Material fontMaterial = styleText != null ? styleText.fontSharedMaterial : null;
            FontStyles fontStyle = styleText != null ? styleText.fontStyle : FontStyles.Normal;

            Sprite panelSprite = LoadSprite(PanelSpritePath);
            Sprite buttonIdle = LoadSprite(ButtonIdlePath);
            Sprite buttonHover = LoadSprite(ButtonHoverPath);
            Sprite buttonPressed = LoadSprite(ButtonPressedPath);
            Sprite nextIcon = LoadSprite(NextIconPath);

            if (buttonIdle == null && buildButton != null && buildButton.targetGraphic is Image buildImage)
                buttonIdle = buildImage.sprite;

            ColorBlock buttonColors = buildButton != null ? buildButton.colors : ColorBlock.defaultColorBlock;
            buttonColors.normalColor = Color.white;
            buttonColors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            buttonColors.pressedColor = new Color(0.88f, 0.82f, 0.72f, 1f);
            buttonColors.selectedColor = buttonColors.highlightedColor;
            buttonColors.disabledColor = new Color(0.56f, 0.54f, 0.50f, 0.72f);
            buttonColors.colorMultiplier = 1f;
            buttonColors.fadeDuration = 0.08f;

            string source = $"resource={(resourcePanel != null ? HierarchyPath(resourcePanel.transform) : "fallback")}," +
                            $"build={(buildButton != null ? HierarchyPath(buildButton.transform) : "fallback")}";

            return new UiSkin(
                panelSprite,
                buttonIdle,
                buttonHover,
                buttonPressed,
                nextIcon,
                font,
                fontMaterial,
                fontStyle,
                buttonColors,
                source);
        }

        private static void ApplyPanel(Image image, Sprite panelSprite)
        {
            if (image == null)
                return;

            image.sprite = panelSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = panelSprite != null ? Color.white : new Color32(61, 43, 34, 242);
            image.raycastTarget = false;
            EnsureShadow(image, DeepShadow, new Vector2(3f, -3f));
        }

        private static void StyleButton(Button button, UiSkin skin, bool isPrimary)
        {
            if (button == null)
                return;

            Image image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image == null)
                return;

            image.sprite = skin.ButtonIdle;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = Color.white;
            image.raycastTarget = true;

            button.targetGraphic = image;
            button.colors = skin.ButtonColors;

            if (skin.ButtonHover != null || skin.ButtonPressed != null)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                SpriteState states = button.spriteState;
                states.highlightedSprite = skin.ButtonHover ?? skin.ButtonIdle;
                states.selectedSprite = skin.ButtonHover ?? skin.ButtonIdle;
                states.pressedSprite = skin.ButtonPressed ?? skin.ButtonIdle;
                states.disabledSprite = skin.ButtonIdle;
                button.spriteState = states;
            }
            else
            {
                button.transition = Selectable.Transition.ColorTint;
            }

            EnsureShadow(image, new Color32(16, 8, 4, 215), new Vector2(2f, -2f));
        }

        private static void ApplyText(
            TMP_Text text,
            UiSkin skin,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment)
        {
            if (text == null)
                return;

            if (skin.Font != null)
                text.font = skin.Font;
            if (skin.FontMaterial != null && skin.Font != null)
                text.fontSharedMaterial = skin.FontMaterial;

            text.fontStyle = skin.FontStyle;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
        }

        private static void EnsureNextIcon(Transform button, Sprite sprite)
        {
            Transform existing = button.Find("TurnArrowIcon");
            GameObject go;
            if (existing == null)
            {
                go = new GameObject("TurnArrowIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = button.gameObject.layer;
                go.transform.SetParent(button, false);
            }
            else
            {
                go = existing.gameObject;
            }

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-10f, 0f);
            rect.sizeDelta = new Vector2(24f, 24f);
        }

        private static void EnsureAccent(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = parent.gameObject.layer;
                go.transform.SetParent(parent, false);
                go.transform.SetAsFirstSibling();
            }
            else
            {
                go = existing.gameObject;
            }

            Image image = go.GetComponent<Image>();
            image.sprite = null;
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void EnsureTextShadow(TMP_Text text)
        {
            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null)
                shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color32(23, 12, 7, 210);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;
        }

        private static void EnsureShadow(Graphic graphic, Color color, Vector2 distance)
        {
            Shadow shadow = graphic.GetComponent<Shadow>();
            if (shadow == null)
                shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void EnsureOutline(Graphic graphic, Color color, Vector2 distance)
        {
            Outline outline = graphic.GetComponent<Outline>();
            if (outline == null)
                outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static Sprite LoadSprite(string path)
            => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();

        private static string AssetName(Sprite sprite)
            => sprite != null ? sprite.name : "fallback";

        private static Button FindButtonAncestor(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                Button button = current.GetComponent<Button>();
                if (button != null)
                    return button;
                current = current.parent;
            }
            return null;
        }

        private static Image FindWideImageAncestor(Transform transform, Transform canvas)
        {
            Image fallback = null;
            Transform current = transform;
            while (current != null && current != canvas)
            {
                Image image = current.GetComponent<Image>();
                if (image != null)
                {
                    fallback = image;
                    RectTransform rect = current as RectTransform;
                    if (rect != null && rect.rect.width >= 240f)
                        return image;
                }
                current = current.parent;
            }
            return fallback;
        }

        private static bool IsUnder(Transform transform, Transform possibleAncestor)
        {
            if (transform == null || possibleAncestor == null)
                return false;
            Transform current = transform;
            while (current != null)
            {
                if (current == possibleAncestor)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            foreach (string needle in needles)
            {
                if (value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static Canvas FindPrimaryCanvas(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas != null && canvas.name == "Canvas")
                        return canvas;
                }
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    return canvas;
            }
            return null;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == name)
                    return child;
            }
            return null;
        }

        private static RectTransform RequireRect(Transform root, string path)
        {
            Transform child = root.Find(path);
            if (child is not RectTransform rect)
                throw new InvalidOperationException($"HUD RectTransform missing: {path}");
            return rect;
        }

        private static T Require<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path);
            if (child == null)
                throw new InvalidOperationException($"HUD object missing: {path}");
            T component = child.GetComponent<T>();
            if (component == null)
                throw new InvalidOperationException($"HUD component {typeof(T).Name} missing: {path}");
            return component;
        }

        private static void SetTopLeft(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopStretch(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }

        private static void SetStretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static string HierarchyPath(Transform transform)
        {
            if (transform == null)
                return "null";
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private readonly struct UiSkin
        {
            public readonly Sprite PanelSprite;
            public readonly Sprite ButtonIdle;
            public readonly Sprite ButtonHover;
            public readonly Sprite ButtonPressed;
            public readonly Sprite NextIcon;
            public readonly TMP_FontAsset Font;
            public readonly Material FontMaterial;
            public readonly FontStyles FontStyle;
            public readonly ColorBlock ButtonColors;
            public readonly string SourceSummary;

            public UiSkin(
                Sprite panelSprite,
                Sprite buttonIdle,
                Sprite buttonHover,
                Sprite buttonPressed,
                Sprite nextIcon,
                TMP_FontAsset font,
                Material fontMaterial,
                FontStyles fontStyle,
                ColorBlock buttonColors,
                string sourceSummary)
            {
                PanelSprite = panelSprite;
                ButtonIdle = buttonIdle;
                ButtonHover = buttonHover;
                ButtonPressed = buttonPressed;
                NextIcon = nextIcon;
                Font = font;
                FontMaterial = fontMaterial;
                FontStyle = fontStyle;
                ButtonColors = buttonColors;
                SourceSummary = sourceSummary;
            }
        }
    }
}
