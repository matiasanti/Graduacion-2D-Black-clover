using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField, TextArea]
    private string gameInformation = "";

    [SerializeField, TextArea]
    private string gameUpdates = "";

    private const string VolumePreference = "MasterVolume";

    private readonly Dictionary<PlayerControl.Control, Text> bindingLabels =
        new Dictionary<PlayerControl.Control, Text>();
    private readonly List<Button> menuButtons = new List<Button>();

    private Transform contentPanel;
    private GameObject pageOverlay;
    private PlayerControl.Control? rebindingControl;
    private bool showingSubmenu;

    private void Start()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            throw new InvalidOperationException("MainMenu requires a Canvas in the scene.");
        }

        canvas.transform.localScale = Vector3.one;
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            UnityAction action = GetActionForButton(button);
            if (action == null)
            {
                continue;
            }

            menuButtons.Add(button);
            if (!HasPersistentListener(button, action.Method.Name))
            {
                button.onClick.AddListener(action);
            }
        }

        SetVolume(PlayerPrefs.GetFloat(VolumePreference, AudioListener.volume));
    }

    private void Update()
    {
        if (!showingSubmenu)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (rebindingControl.HasValue)
            {
                rebindingControl = null;
                UpdateBindingLabels();
            }
            else
            {
                ShowMainMenu();
            }

            return;
        }

        if (!rebindingControl.HasValue)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                BeginRebinding(PlayerControl.Control.Left);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                BeginRebinding(PlayerControl.Control.Right);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                BeginRebinding(PlayerControl.Control.Jump);
            }

            return;
        }

        if (!Input.anyKeyDown)
        {
            return;
        }

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (key == KeyCode.None || key >= KeyCode.Mouse0 || !Input.GetKeyDown(key))
            {
                continue;
            }

            PlayerControl.Control control = rebindingControl.Value;
            if (PlayerControl.IsKeyAssigned(key, control))
            {
                UpdateBindingLabels("Esa tecla ya está asignada. Elige otra.");
                return;
            }

            PlayerControl.SetKey(control, key);
            rebindingControl = null;
            UpdateBindingLabels();
            return;
        }
    }

    public void PlayGame()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }

    public void ShowMainMenu()
    {
        PlayerPrefs.Save();
        showingSubmenu = false;
        rebindingControl = null;
        bindingLabels.Clear();

        if (pageOverlay != null)
        {
            Destroy(pageOverlay);
            pageOverlay = null;
        }

    }

    public void ShowSettings()
    {
        OpenSubmenu();
        CreateCloseButton();
        CreateText("AJUSTES", 30, 145f, 0f);
        CreateText("Volumen", 20, 95f, 0f);

        Slider volumeSlider = CreateSlider(new Vector2(0f, 55f));
        volumeSlider.value = PlayerPrefs.GetFloat(VolumePreference, AudioListener.volume);
        volumeSlider.onValueChanged.AddListener(SetVolume);

        CreateText("Controles", 20, 5f, 0f);
        CreateBindingLabel("1 - Mover a la izquierda", PlayerControl.Control.Left, -35f);
        CreateBindingLabel("2 - Mover a la derecha", PlayerControl.Control.Right, -75f);
        CreateBindingLabel("3 - Saltar", PlayerControl.Control.Jump, -115f);
        CreateText("Pulsa 1, 2 o 3 y luego la tecla nueva.", 14, -160f, 0f, 380f, 36f);
    }

    public void ShowInfo()
    {
        OpenSubmenu();
        CreateCloseButton();
        CreateText("INFORMACION", 30, 150f, 0f);
        CreateText(gameInformation, 16, 35f, 0f, 380f, 170f);
        CreateText(gameUpdates, 16, -115f, 0f, 380f, 110f);
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }

    private void OpenSubmenu()
    {
        PlayerPrefs.Save();
        showingSubmenu = true;
        rebindingControl = null;
        bindingLabels.Clear();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            throw new InvalidOperationException("MainMenu requires a Canvas in the scene.");
        }

        pageOverlay = CreateUIObject("MenuSubmenu", canvas.transform, typeof(Image));
        RectTransform overlayRect = pageOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        pageOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

        GameObject panel = CreateUIObject("SubmenuContent", pageOverlay.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(460f, 400f);
        panel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.08f, 0.82f);
        contentPanel = panel.transform;
    }

    private UnityAction GetActionForButton(Button button)
    {
        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
        Text legacyText = button.GetComponentInChildren<Text>(true);
        string label = tmpText != null
            ? tmpText.text
            : legacyText != null
                ? legacyText.text
                : button.gameObject.name;
        string normalizedLabel = Normalize(label + " " + button.gameObject.name);

        if (normalizedLabel.Contains("play") || normalizedLabel.Contains("jugar"))
        {
            return PlayGame;
        }
        if (normalizedLabel.Contains("setting")
            || normalizedLabel.Contains("ajuste")
            || normalizedLabel.Contains("config")
            || normalizedLabel.Contains("option")
            || normalizedLabel.Contains("opcion"))
        {
            return ShowSettings;
        }
        if (normalizedLabel.Contains("info"))
        {
            return ShowInfo;
        }
        if (normalizedLabel.Contains("exit") || normalizedLabel.Contains("salir"))
        {
            return QuitGame;
        }

        return null;
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this
                && button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string value)
    {
        return value.ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("_", " ")
            .Replace("-", " ");
    }

    private void BeginRebinding(PlayerControl.Control control)
    {
        rebindingControl = control;
        UpdateBindingLabels();
    }

    private void CreateBindingLabel(string label, PlayerControl.Control control, float y)
    {
        Text text = CreateText(GetBindingLabel(label, control), 16, y, 0f, 390f, 40f);
        bindingLabels.Add(control, text);
    }

    private void CreateCloseButton()
    {
        GameObject buttonObject = CreateUIObject(
            "CloseSubmenuButton",
            contentPanel,
            typeof(Image),
            typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-12f, -12f);
        rect.sizeDelta = new Vector2(38f, 38f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.7f, 0.12f, 0.12f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ShowMainMenu);

        Text label = CreateText("X", 24, 0f, 0f, 38f, 38f);
        label.transform.SetParent(buttonObject.transform, false);
    }

    private void UpdateBindingLabels(string message = null)
    {
        foreach (KeyValuePair<PlayerControl.Control, Text> binding in bindingLabels)
        {
            string label = GetControlLabel(binding.Key);
            binding.Value.text = rebindingControl == binding.Key
                ? message ?? label + ": pulsa la nueva tecla (ESC para cancelar)"
                : GetBindingLabel(label, binding.Key);
        }
    }

    private static string GetBindingLabel(string label, PlayerControl.Control control)
    {
        return label + ": " + PlayerControl.GetKey(control);
    }

    private static string GetControlLabel(PlayerControl.Control control)
    {
        switch (control)
        {
            case PlayerControl.Control.Left:
                return "Mover a la izquierda";
            case PlayerControl.Control.Right:
                return "Mover a la derecha";
            default:
                return "Saltar";
        }
    }

    private void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePreference, volume);
    }

    private Slider CreateSlider(Vector2 position)
    {
        GameObject sliderObject = CreateUIObject("VolumeSlider", contentPanel, typeof(Slider));
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320f, 24f);

        GameObject background = CreateUIObject("Background", sliderObject.transform, typeof(Image));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);

        GameObject fillArea = CreateUIObject("FillArea", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5f, 0f);
        fillAreaRect.offsetMax = new Vector2(-5f, 0f);

        GameObject fill = CreateUIObject("Fill", fillArea.transform, typeof(Image));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.25f, 0.85f, 0.15f, 1f);

        GameObject handleArea = CreateUIObject("HandleSlideArea", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = CreateUIObject("Handle", handleArea.transform, typeof(Image));
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 30f);
        Image handleImage = handle.GetComponent<Image>();
        handleImage.color = Color.white;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private Text CreateText(string value, int fontSize, float y, float x, float width = 360f, float height = 48f)
    {
        GameObject textObject = CreateUIObject(
            "SubmenuText",
            contentPanel,
            typeof(CanvasRenderer),
            typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUIObject(string name, Transform parent, params Type[] components)
    {
        Type[] uiComponents = new Type[components.Length + 1];
        uiComponents[0] = typeof(RectTransform);
        Array.Copy(components, 0, uiComponents, 1, components.Length);
        GameObject gameObject = new GameObject(name, uiComponents);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }
}
