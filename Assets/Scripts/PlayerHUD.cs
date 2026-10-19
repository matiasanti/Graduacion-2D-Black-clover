using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class PlayerHUD
{
    private readonly Canvas canvas;
    private Image healthFill;
    private Text healthText;

    private PlayerHUD(Canvas canvas)
    {
        this.canvas = canvas;
        CreateHealthBar();
    }

    public static PlayerHUD Create(Canvas canvas)
    {
        if (canvas == null)
        {
            GameObject canvasObject = CreateUiElement(
                "Canvas",
                null,
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        canvas.transform.localScale = Vector3.one;
        return new PlayerHUD(canvas);
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float healthPercent = maxHealth > 0f
            ? Mathf.Clamp01(currentHealth / maxHealth)
            : 0f;
        healthFill.rectTransform.sizeDelta = new Vector2(288f * healthPercent, -4f);
        healthFill.color = healthPercent > 0.5f
            ? Color.Lerp(new Color(0.9f, 0.65f, 0.16f), new Color(0.35f, 0.78f, 0.3f), (healthPercent - 0.5f) * 2f)
            : Color.Lerp(new Color(0.78f, 0.12f, 0.1f), new Color(0.9f, 0.65f, 0.16f), healthPercent * 2f);
        healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    public void ShowGameOver(UnityAction restartAction, UnityAction menuAction)
    {
        GameObject overlay = CreateUiElement("GameOverScreen", canvas.transform, typeof(Image));
        SetStretch(overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        overlay.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.025f, 0.82f);

        GameObject panel = CreateUiElement("GameOverPanel", overlay.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(470f, 330f);
        panel.GetComponent<Image>().color = new Color(0.075f, 0.06f, 0.055f, 1f);

        Image border = CreateImage("GoldBorder", panel.transform, new Color(0.72f, 0.49f, 0.18f, 1f));
        SetStretch(border.rectTransform, new Vector2(2f, 2f), new Vector2(-2f, -2f));
        Image innerPanel = CreateImage("InnerPanel", panel.transform, new Color(0.075f, 0.06f, 0.055f, 1f));
        SetStretch(innerPanel.rectTransform, new Vector2(5f, 5f), new Vector2(-5f, -5f));

        Text title = CreateText(
            "GAME OVER",
            panel.transform,
            40,
            TextAnchor.MiddleCenter,
            new Color(0.9f, 0.19f, 0.15f, 1f));
        SetRect(title.rectTransform, new Vector2(0f, -40f), new Vector2(390f, 62f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        Text message = CreateText(
            "Tu aventura termina aquí",
            panel.transform,
            18,
            TextAnchor.MiddleCenter,
            new Color(0.85f, 0.8f, 0.72f, 1f));
        SetRect(message.rectTransform, new Vector2(0f, -108f), new Vector2(390f, 36f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        CreateGameOverButton(panel.transform, "RESTART", new Vector2(0f, -195f), restartAction);
        CreateGameOverButton(panel.transform, "GO TO MENU", new Vector2(0f, -260f), menuAction);
    }

    private void CreateHealthBar()
    {
        GameObject panel = CreateUiElement("PlayerHealthBar", canvas.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetRect(panelRect, new Vector2(34f, -34f), new Vector2(320f, 88f), Vector2.up, Vector2.up);
        panelRect.localScale = Vector3.one * 0.6f;
        panel.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.04f, 0.94f);

        Image goldBorder = CreateImage("GoldBorder", panel.transform, new Color(0.72f, 0.49f, 0.18f, 1f));
        SetStretch(goldBorder.rectTransform, new Vector2(2f, 2f), new Vector2(-2f, -2f));

        Image innerPanel = CreateImage("InnerPanel", panel.transform, new Color(0.09f, 0.07f, 0.06f, 1f));
        SetStretch(innerPanel.rectTransform, new Vector2(4f, 4f), new Vector2(-4f, -4f));

        Image accent = CreateImage("TopAccent", panel.transform, new Color(0.92f, 0.68f, 0.25f, 1f));
        SetRect(accent.rectTransform, new Vector2(14f, -12f), new Vector2(292f, 2f), Vector2.up, Vector2.up);

        Text title = CreateText(
            "HEALTH",
            panel.transform,
            16,
            TextAnchor.MiddleLeft,
            new Color(0.95f, 0.83f, 0.59f, 1f));
        SetRect(title.rectTransform, new Vector2(14f, -31f), new Vector2(165f, 23f), Vector2.up, Vector2.up);

        healthText = CreateText(string.Empty, panel.transform, 14, TextAnchor.MiddleRight, Color.white);
        SetRect(healthText.rectTransform, new Vector2(-14f, -31f), new Vector2(105f, 23f), Vector2.one, Vector2.one);

        Image healthBackground = CreateImage("HealthBackground", panel.transform, new Color(0.22f, 0.08f, 0.07f, 1f));
        SetRect(healthBackground.rectTransform, new Vector2(14f, -58f), new Vector2(292f, 18f), Vector2.up, Vector2.up);

        healthFill = CreateImage("HealthFill", healthBackground.transform, new Color(0.35f, 0.78f, 0.3f, 1f));
        RectTransform healthFillRect = healthFill.rectTransform;
        healthFillRect.anchorMin = new Vector2(0f, 0f);
        healthFillRect.anchorMax = new Vector2(0f, 1f);
        healthFillRect.pivot = new Vector2(0f, 0.5f);
        healthFillRect.anchoredPosition = new Vector2(2f, 0f);
        healthFillRect.sizeDelta = new Vector2(288f, -4f);

        foreach (Graphic graphic in panel.GetComponentsInChildren<Graphic>())
        {
            graphic.raycastTarget = false;
        }
    }

    private static void CreateGameOverButton(Transform parent, string label, Vector2 position, UnityAction action)
    {
        GameObject buttonObject = CreateUiElement(
            "Button_" + label.Replace(" ", string.Empty),
            parent,
            typeof(Image),
            typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, position, new Vector2(260f, 48f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.35f, 0.12f, 0.09f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        Text buttonLabel = CreateText(label, buttonObject.transform, 18, TextAnchor.MiddleCenter, Color.white);
        SetStretch(buttonLabel.rectTransform, Vector2.zero, Vector2.zero);
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = CreateUiElement(objectName, parent, typeof(Image));
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string value, Transform parent, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = CreateUiElement("Label", parent, typeof(Text));
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        text.text = value;
        return text;
    }

    private static GameObject CreateUiElement(string objectName, Transform parent, params Type[] components)
    {
        Type[] uiComponents = new Type[components.Length + 2];
        uiComponents[0] = typeof(RectTransform);
        uiComponents[1] = typeof(CanvasRenderer);
        Array.Copy(components, 0, uiComponents, 2, components.Length);

        GameObject element = new GameObject(objectName, uiComponents);
        if (parent != null)
        {
            element.layer = parent.gameObject.layer;
            element.transform.SetParent(parent, false);
        }

        return element;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetStretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
