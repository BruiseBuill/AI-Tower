using Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>
    /// BattleSceneBuilder 的 UI 部分：战斗画布、能量条、倒计时、部署按钮、暂停与结算面板。
    /// 参考分辨率 1920x1080（横屏），CanvasScaler 按屏幕尺寸缩放（match 0.5）。
    /// </summary>
    public static partial class BattleSceneBuilder
    {
        static readonly Color HudPanelColor = new Color32(0x20, 0x20, 0x28, 0xE6);
        static readonly Color EnergyColor = new Color32(0x3F, 0xA7, 0xFF, 0xFF);
        static readonly Color ButtonColor = new Color32(0x2A, 0x35, 0x50, 0xFF);
        static readonly Color AccentColor = new Color32(0xFF, 0xC8, 0x4A, 0xFF);
        static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        const string RoundedRectSpritePath = "Assets/Art/PolySprite/RoundedRectangle.png";

        static BattleHUD CreateHud()
        {
            Sprite roundedRectSprite = LoadSprite(RoundedRectSpritePath);

            var canvasObject = new GameObject("BattleCanvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            // ---- 顶部能量条 ----
            GameObject energyPanel = CreateUiObject("EnergyPanel", canvasRect);
            Anchor(energyPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1000f, 60f));

            GameObject energyBackground = CreateUiObject("Background", energyPanel.transform);
            AddImage(energyBackground, null, HudPanelColor);
            FillParent(energyBackground.GetComponent<RectTransform>());

            GameObject energyFillObject = CreateUiObject("Fill", energyPanel.transform);
            Image energyFill = AddImage(energyFillObject, null, EnergyColor);
            energyFill.type = Image.Type.Filled;
            energyFill.fillMethod = Image.FillMethod.Horizontal;
            energyFill.fillAmount = 0.5f;
            RectTransform fillRect = energyFillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(6f, 6f);
            fillRect.offsetMax = new Vector2(-6f, -6f);

            GameObject energyLabelObject = CreateUiObject("Label", energyPanel.transform);
            Text energyLabel = AddText(energyLabelObject, "能量 0/0", 28, TextAnchor.MiddleCenter, Color.white);
            FillParent(energyLabelObject.GetComponent<RectTransform>());

            // ---- 右上角倒计时 ----
            GameObject timerObject = CreateUiObject("TimerText", canvasRect);
            Text timerText = AddText(timerObject, "--:--", 42, TextAnchor.MiddleRight, Color.white);
            Anchor(timerObject.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-30f, -20f), new Vector2(320f, 60f));

            GameObject badgeObject = CreateUiObject("ClearedBadge", canvasRect);
            Text clearedBadge = AddText(badgeObject, "本关已攻克", 22, TextAnchor.MiddleRight, AccentColor);
            Anchor(badgeObject.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-30f, -84f), new Vector2(320f, 32f));
            badgeObject.SetActive(false);

            // ---- 左上角暂停按钮 ----
            GameObject pauseButtonObject = CreateUiObject("PauseButton", canvasRect);
            AddImage(pauseButtonObject, roundedRectSprite, ButtonColor);
            Anchor(pauseButtonObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(26f, -20f), new Vector2(120f, 60f));
            Button pauseButton = AddButton(pauseButtonObject);
            GameObject pauseLabel = CreateUiObject("Label", pauseButtonObject.transform);
            AddText(pauseLabel, "暂停", 30, TextAnchor.MiddleCenter, Color.white);
            FillParent(pauseLabel.GetComponent<RectTransform>());

            // ---- 底部部署按钮容器（运行时克隆模板生成） ----
            GameObject deployBar = CreateUiObject("DeployBar", canvasRect);
            Anchor(deployBar.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 24f), Vector2.zero);
            HorizontalLayoutGroup layout = deployBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = deployBar.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject templateObject = CreateUiObject("DeployButtonTemplate", deployBar.transform);
            AddImage(templateObject, roundedRectSprite, ButtonColor);
            templateObject.GetComponent<RectTransform>().sizeDelta = new Vector2(230f, 96f);
            AddButton(templateObject);
            GameObject templateLabel = CreateUiObject("Label", templateObject.transform);
            AddText(templateLabel, "模板", 30, TextAnchor.MiddleCenter, Color.white);
            FillParent(templateLabel.GetComponent<RectTransform>());
            templateObject.SetActive(false);

            // ---- 暂停面板 ----
            GameObject pausePanel = CreateUiObject("PausePanel", canvasRect);
            FillParent(pausePanel.GetComponent<RectTransform>());
            AddImage(pausePanel, null, DimColor);

            GameObject pauseTitle = CreateUiObject("Title", pausePanel.transform);
            AddText(pauseTitle, "已暂停", 64, TextAnchor.MiddleCenter, Color.white);
            Anchor(pauseTitle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(600f, 90f));

            GameObject resumeButtonObject = CreateUiObject("ResumeButton", pausePanel.transform);
            AddImage(resumeButtonObject, roundedRectSprite, ButtonColor);
            Anchor(resumeButtonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(320f, 96f));
            Button resumeButton = AddButton(resumeButtonObject);
            GameObject resumeLabel = CreateUiObject("Label", resumeButtonObject.transform);
            AddText(resumeLabel, "继续", 34, TextAnchor.MiddleCenter, Color.white);
            FillParent(resumeLabel.GetComponent<RectTransform>());

            GameObject pauseRetryObject = CreateUiObject("PauseRetryButton", pausePanel.transform);
            AddImage(pauseRetryObject, roundedRectSprite, ButtonColor);
            Anchor(pauseRetryObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(320f, 96f));
            Button pauseRetryButton = AddButton(pauseRetryObject);
            GameObject pauseRetryLabel = CreateUiObject("Label", pauseRetryObject.transform);
            AddText(pauseRetryLabel, "重新开始", 34, TextAnchor.MiddleCenter, Color.white);
            FillParent(pauseRetryLabel.GetComponent<RectTransform>());

            pausePanel.SetActive(false);

            // ---- 结算面板 ----
            GameObject resultPanel = CreateUiObject("ResultPanel", canvasRect);
            FillParent(resultPanel.GetComponent<RectTransform>());
            AddImage(resultPanel, null, DimColor);

            GameObject resultTitleObject = CreateUiObject("ResultTitle", resultPanel.transform);
            Text resultTitle = AddText(resultTitleObject, "胜利", 72, TextAnchor.MiddleCenter, AccentColor);
            Anchor(resultTitleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(800f, 110f));

            GameObject resultNoteObject = CreateUiObject("ResultNote", resultPanel.transform);
            Text resultNote = AddText(resultNoteObject, "", 32, TextAnchor.MiddleCenter, Color.white);
            Anchor(resultNoteObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 60f));

            GameObject resultRetryObject = CreateUiObject("ResultRetryButton", resultPanel.transform);
            AddImage(resultRetryObject, roundedRectSprite, ButtonColor);
            Anchor(resultRetryObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(340f, 96f));
            Button resultRetryButton = AddButton(resultRetryObject);
            GameObject resultRetryLabel = CreateUiObject("Label", resultRetryObject.transform);
            AddText(resultRetryLabel, "重新开始", 34, TextAnchor.MiddleCenter, Color.white);
            FillParent(resultRetryLabel.GetComponent<RectTransform>());

            resultPanel.SetActive(false);

            // ---- HUD 控制器 ----
            GameObject hudObject = CreateUiObject("BattleHUD", canvasRect);
            BattleHUD hud = hudObject.AddComponent<BattleHUD>();
            SetSerialized(hud, so =>
            {
                so.FindProperty("energyFill").objectReferenceValue = energyFill;
                so.FindProperty("energyLabel").objectReferenceValue = energyLabel;
                so.FindProperty("timerText").objectReferenceValue = timerText;
                so.FindProperty("clearedBadge").objectReferenceValue = clearedBadge;
                so.FindProperty("deployContainer").objectReferenceValue = deployBar.transform;
                so.FindProperty("deployButtonTemplate").objectReferenceValue = templateObject;
                so.FindProperty("pauseButton").objectReferenceValue = pauseButton;
                so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
                so.FindProperty("resumeButton").objectReferenceValue = resumeButton;
                so.FindProperty("pauseRetryButton").objectReferenceValue = pauseRetryButton;
                so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
                so.FindProperty("resultTitle").objectReferenceValue = resultTitle;
                so.FindProperty("resultNote").objectReferenceValue = resultNote;
                so.FindProperty("resultRetryButton").objectReferenceValue = resultRetryButton;
            });

            return hud;
        }

        // ---------- UI 工具 ----------

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var uiObject = new GameObject(name, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        static Image AddImage(GameObject uiObject, Sprite sprite, Color color)
        {
            Image image = uiObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        static Text AddText(GameObject uiObject, string content, int fontSize, TextAnchor alignment, Color color)
        {
            Text text = uiObject.AddComponent<Text>();
            text.text = content;
            // Unity 2022.2 起 Arial.ttf 已废弃，改用 LegacyRuntime.ttf
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static Button AddButton(GameObject uiObject)
        {
            Button button = uiObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.7f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
            button.colors = colors;
            button.targetGraphic = uiObject.GetComponent<Image>();
            return button;
        }

        static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        static void FillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
