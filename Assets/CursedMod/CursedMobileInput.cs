using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Runtime-created touch controls. No prefab or paid input package is required.
/// The controls are also available in the editor so the Android layout can be tested.
/// </summary>
public class CursedMobileInput : MonoBehaviour
{
    private static CursedMobileInput instance;
    private static readonly bool[] actionStates = new bool[(int)InputAction.Count];
    private static Vector2 moveVector;
    private static float lookDeltaX;

    private Canvas canvas;
    private bool sceneWantsVisible;

    public static bool IsActive
    {
        get { return instance != null && instance.canvas != null && instance.canvas.enabled; }
    }

    public static bool GetAction(InputAction action)
    {
        if ((int)action < 0 || (int)action >= actionStates.Length)
        {
            return false;
        }

        switch (action)
        {
            case InputAction.MoveForward: return moveVector.y > 0.22f;
            case InputAction.MoveBackward: return moveVector.y < -0.22f;
            case InputAction.MoveLeft: return moveVector.x < -0.22f;
            case InputAction.MoveRight: return moveVector.x > 0.22f;
            default: return actionStates[(int)action];
        }
    }

    public static float ConsumeLookDeltaX()
    {
        float value = lookDeltaX;
        lookDeltaX = 0f;
        return value;
    }

    public static void EnsureForGameplayScene()
    {
        if (instance != null)
        {
            instance.SetVisible(true);
            return;
        }

        GameObject root = new GameObject("Cursed Mobile Controls");
        instance = root.AddComponent<CursedMobileInput>();
        DontDestroyOnLoad(root);
        instance.BuildUI();
    }

    public static void Hide()
    {
        if (instance != null)
        {
            instance.SetVisible(false);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            moveVector = Vector2.zero;
            lookDeltaX = 0f;
            for (int i = 0; i < actionStates.Length; i++) actionStates[i] = false;
        }
    }

    private void SetVisible(bool visible)
    {
        sceneWantsVisible = visible;
        if (canvas != null) canvas.enabled = visible;
        if (!visible)
        {
            moveVector = Vector2.zero;
            lookDeltaX = 0f;
            for (int i = 0; i < actionStates.Length; i++) actionStates[i] = false;
        }
    }

    private void Update()
    {
        if (canvas == null) return;
        // The Think Pad has its own touch buttons. Hiding the movement layer here
        // prevents the right-side look zone from stealing math-keypad touches.
        bool thinkPadIsOpen = FindObjectOfType<MathGameScript>() != null;
        canvas.enabled = sceneWantsVisible && !thinkPadIsOpen;
        if (thinkPadIsOpen)
        {
            moveVector = Vector2.zero;
            lookDeltaX = 0f;
        }
    }

    private void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        gameObject.AddComponent<GraphicRaycaster>();
        sceneWantsVisible = true;

        GameObject lookZone = MakePanel("Look Zone", transform, new Color(0f, 0f, 0f, 0.001f));
        SetRect(lookZone.GetComponent<RectTransform>(), new Vector2(0.42f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        CursedLookPad lookPad = lookZone.AddComponent<CursedLookPad>();
        lookPad.sensitivity = 0.23f;

        GameObject joystickBase = MakePanel("Movement", transform, new Color(0.08f, 0f, 0f, 0.46f));
        RectTransform baseRect = joystickBase.GetComponent<RectTransform>();
        SetRect(baseRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(220f, 220f), new Vector2(185f, 185f));
        CursedJoystick joystick = joystickBase.AddComponent<CursedJoystick>();
        joystick.radius = 110f;

        GameObject knob = MakePanel("Knob", joystickBase.transform, new Color(0.68f, 0.04f, 0.04f, 0.78f));
        RectTransform knobRect = knob.GetComponent<RectTransform>();
        SetRect(knobRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(92f, 92f), Vector2.zero);
        knob.GetComponent<Image>().raycastTarget = false;
        joystick.knob = knobRect;

        MakeActionButton("RUN", InputAction.Run, new Vector2(-150f, 185f), new Vector2(0.88f, 0f), new Vector2(210f, 116f), new Color(0.48f, 0.02f, 0.02f, 0.86f));
        MakeActionButton("GRAB", InputAction.Interact, new Vector2(-390f, 185f), new Vector2(0.88f, 0f), new Vector2(210f, 116f), new Color(0.18f, 0.02f, 0.02f, 0.82f));
        MakeActionButton("USE", InputAction.UseItem, new Vector2(-150f, 330f), new Vector2(0.88f, 0f), new Vector2(210f, 116f), new Color(0.18f, 0.02f, 0.02f, 0.82f));
        MakeActionButton("II", InputAction.PauseOrCancel, new Vector2(-82f, -74f), new Vector2(1f, 1f), new Vector2(92f, 92f), new Color(0.12f, 0f, 0f, 0.72f));
    }

    private void MakeActionButton(string label, InputAction action, Vector2 position, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject buttonObject = MakePanel(label, transform, color);
        SetRect(buttonObject.GetComponent<RectTransform>(), anchor, anchor, size, position);
        CursedHoldButton button = buttonObject.AddComponent<CursedHoldButton>();
        button.action = action;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        SetRect(textRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 34;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(1f, 0.86f, 0.76f, 0.95f);
        text.raycastTarget = false;
    }

    private static GameObject MakePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private class CursedJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform knob;
        public float radius;

        public void OnPointerDown(PointerEventData eventData) { UpdatePosition(eventData); }
        public void OnDrag(PointerEventData eventData) { UpdatePosition(eventData); }
        public void OnPointerUp(PointerEventData eventData)
        {
            moveVector = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }

        private void UpdatePosition(PointerEventData eventData)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out local);
            local = Vector2.ClampMagnitude(local, radius);
            moveVector = local / radius;
            if (knob != null) knob.anchoredPosition = local;
        }
    }

    private class CursedLookPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float sensitivity;
        private Vector2 previous;

        public void OnPointerDown(PointerEventData eventData) { previous = eventData.position; }
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - previous;
            previous = eventData.position;
            lookDeltaX += delta.x * sensitivity;
        }
        public void OnPointerUp(PointerEventData eventData) { }
    }

    private class CursedHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public InputAction action;
        public void OnPointerDown(PointerEventData eventData) { actionStates[(int)action] = true; }
        public void OnPointerUp(PointerEventData eventData) { actionStates[(int)action] = false; }
        public void OnPointerExit(PointerEventData eventData) { actionStates[(int)action] = false; }
    }
}
