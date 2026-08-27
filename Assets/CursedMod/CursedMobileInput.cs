using System.Collections.Generic;
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
    private static float pausePulseUntil;
    private static float jumpPulseUntil;
    private static float interactPulseUntil;
    private static bool itemUseQueued;
    private static int slotSelectionQueued = -1;

    private Canvas canvas;
    private bool sceneWantsVisible;
    private int lookFingerId = -1;
    private Vector2 previousLookPosition;
    private Vector2 lookTouchStartPosition;
    private float lookTouchStartTime;
    private bool lookTouchExceededTapMovement;
    private readonly List<RectTransform> lookBlockedRects = new List<RectTransform>();
    private readonly RectTransform[] slotTouchRects = new RectTransform[3];
    private readonly RectTransform[] trackedSlotRects = new RectTransform[3];
    private readonly Vector3[] slotWorldCorners = new Vector3[4];

    private const float LookSensitivity = 0.12f;
    private const float MaxLookPerFrame = 6.5f;
    private const float DefaultMouseSensitivity = 2f;
    private const float TapMaxDuration = 0.30f;
    private const float TapMaxMovementScreenFraction = 0.025f;
    private const float MinTapMovementPixels = 18f;
    private const float ActionPulseDuration = 0.14f;
    private static Sprite circleSprite;

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
            case InputAction.MoveForward: return moveVector.y > 0.075f;
            case InputAction.MoveBackward: return moveVector.y < -0.075f;
            case InputAction.MoveLeft: return moveVector.x < -0.075f;
            case InputAction.MoveRight: return moveVector.x > 0.075f;
            case InputAction.Interact:
                return actionStates[(int)action] || Time.unscaledTime < interactPulseUntil;
            case InputAction.PauseOrCancel:
                return actionStates[(int)action] || Time.unscaledTime < pausePulseUntil;
            case InputAction.Jump:
                return actionStates[(int)action] || Time.unscaledTime < jumpPulseUntil;
            default: return actionStates[(int)action];
        }
    }

    public static Vector2 GetMoveVector()
    {
        return IsActive ? Vector2.ClampMagnitude(moveVector, 1f) : Vector2.zero;
    }

    public static float ConsumeLookDeltaX()
    {
        float value = lookDeltaX;
        lookDeltaX = 0f;
        return value;
    }

    public static void SetTouchAction(InputAction action, bool pressed)
    {
        int index = (int)action;
        if (index < 0 || index >= actionStates.Length) return;

        actionStates[index] = pressed;
        if (pressed && action == InputAction.PauseOrCancel)
        {
            // Keep a short unscaled pulse so InputManager cannot miss a quick tap.
            pausePulseUntil = Time.unscaledTime + ActionPulseDuration;
        }
    }

    public static bool ConsumeItemUsePress()
    {
        bool pressed = itemUseQueued;
        itemUseQueued = false;
        return pressed;
    }

    public static int ConsumeSlotSelection()
    {
        int slot = slotSelectionQueued;
        slotSelectionQueued = -1;
        return slot;
    }

    public static void EnsureForGameplayScene()
    {
        if (instance != null)
        {
            instance.SetVisible(true);
            instance.BindInventorySlots();
            return;
        }

        GameObject root = new GameObject("Cursed Mobile Controls");
        instance = root.AddComponent<CursedMobileInput>();
        DontDestroyOnLoad(root);
        Input.simulateMouseWithTouches = false;
        instance.BuildUI();
        instance.BindInventorySlots();
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
            ResetLookTouch();
            instance = null;
            moveVector = Vector2.zero;
            lookDeltaX = 0f;
            pausePulseUntil = 0f;
            jumpPulseUntil = 0f;
            interactPulseUntil = 0f;
            itemUseQueued = false;
            slotSelectionQueued = -1;
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
            jumpPulseUntil = 0f;
            interactPulseUntil = 0f;
            itemUseQueued = false;
            slotSelectionQueued = -1;
            ResetLookTouch();
            for (int i = 0; i < actionStates.Length; i++) actionStates[i] = false;
        }
    }

    private void Update()
    {
        if (canvas == null) return;
        // The Think Pad has its own touch buttons. Hide gameplay controls and
        // suspend raw camera-touch tracking while the math keypad is open.
        bool thinkPadIsOpen = FindFirstObjectByType<MathGameScript>() != null;
        canvas.enabled = sceneWantsVisible && !thinkPadIsOpen;
        UpdateInventorySlotTargets();
        if (thinkPadIsOpen)
        {
            moveVector = Vector2.zero;
            lookDeltaX = 0f;
            jumpPulseUntil = 0f;
            interactPulseUntil = 0f;
            ResetLookTouch();
        }
        else if (canvas.enabled && Time.timeScale > 0f)
        {
            PlayerScript player = FindFirstObjectByType<PlayerScript>();
            if (player != null && player.jumpRope)
            {
                ResetLookTouch();
                UpdateJumpRopeTouch();
            }
            else
            {
                jumpPulseUntil = 0f;
                UpdateRawTouchLook();
            }
        }
        else
        {
            jumpPulseUntil = 0f;
            interactPulseUntil = 0f;
            ResetLookTouch();
        }
    }

    private void UpdateRawTouchLook()
    {
        bool trackedFingerFound = false;

        // Unity's documented legacy mobile path: read every Android finger
        // directly from Input.touchCount/Input.GetTouch in Update.
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (lookFingerId == -1 && touch.phase == TouchPhase.Began && IsLookTouchStart(touch.position))
            {
                lookFingerId = touch.fingerId;
                previousLookPosition = touch.position;
                lookTouchStartPosition = touch.position;
                lookTouchStartTime = Time.unscaledTime;
                lookTouchExceededTapMovement = false;
            }

            if (touch.fingerId != lookFingerId) continue;
            trackedFingerFound = true;

            if (touch.phase == TouchPhase.Moved)
            {
                float tapMovementThreshold = GetTapMovementThreshold();
                if ((touch.position - lookTouchStartPosition).sqrMagnitude > tapMovementThreshold * tapMovementThreshold)
                {
                    lookTouchExceededTapMovement = true;
                }

                float screenDeltaX = touch.position.x - previousLookPosition.x;
                previousLookPosition = touch.position;
                // Keep the original camera feel at the default value (2), while
                // allowing the existing 0.1-10 options slider to affect touch look.
                float sensitivityScale = Mathf.Clamp(PlayerPrefs.GetFloat("MouseSensitivity", DefaultMouseSensitivity) / DefaultMouseSensitivity, 0.05f, 5f);
                float scaledDelta = screenDeltaX * LookSensitivity * sensitivityScale * (1920f / Mathf.Max(Screen.width, 1));
                float limitedDelta = Mathf.Clamp(scaledDelta, -MaxLookPerFrame, MaxLookPerFrame);
                lookDeltaX = Mathf.Clamp(lookDeltaX + limitedDelta, -18f, 18f);
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
                previousLookPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.phase == TouchPhase.Ended && IsShortWorldTap(touch.position))
                {
                    interactPulseUntil = Time.unscaledTime + ActionPulseDuration;
                }
                ResetLookTouch();
            }
        }

        if (lookFingerId != -1 && !trackedFingerFound)
        {
            ResetLookTouch();
        }
    }

    private void UpdateJumpRopeTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase != TouchPhase.Began || IsControlTouch(touch.position)) continue;

            // A short unscaled pulse is long enough for InputManager and
            // CameraScript to observe even a very quick Android screen tap.
            jumpPulseUntil = Time.unscaledTime + 0.14f;
            break;
        }
    }

    private bool IsControlTouch(Vector2 screenPosition)
    {
        for (int i = 0; i < lookBlockedRects.Count; i++)
        {
            RectTransform blocked = lookBlockedRects[i];
            if (blocked != null && RectTransformUtility.RectangleContainsScreenPoint(blocked, screenPosition, null))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsLookTouchStart(Vector2 screenPosition)
    {
        // The top inventory HUD is reserved. Every other non-control area can
        // rotate the camera when dragged or interact when briefly tapped.
        if (screenPosition.y > Screen.height * 0.80f)
        {
            return false;
        }

        // RUN/USE/PAUSE, the joystick and inventory slots remain normal UI
        // touches and must never rotate the camera or interact with the world.
        for (int i = 0; i < lookBlockedRects.Count; i++)
        {
            RectTransform blocked = lookBlockedRects[i];
            if (blocked != null && RectTransformUtility.RectangleContainsScreenPoint(blocked, screenPosition, null))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsShortWorldTap(Vector2 endPosition)
    {
        if (!IsLookTouchStart(endPosition))
        {
            return false;
        }

        float tapMovementThreshold = GetTapMovementThreshold();
        bool endedWithinMovementThreshold = (endPosition - lookTouchStartPosition).sqrMagnitude <= tapMovementThreshold * tapMovementThreshold;
        return !lookTouchExceededTapMovement && endedWithinMovementThreshold && Time.unscaledTime - lookTouchStartTime <= TapMaxDuration;
    }

    private static float GetTapMovementThreshold()
    {
        return Mathf.Max(MinTapMovementPixels, Mathf.Min(Screen.width, Screen.height) * TapMaxMovementScreenFraction);
    }

    private void ResetLookTouch()
    {
        lookFingerId = -1;
        previousLookPosition = Vector2.zero;
        lookTouchStartPosition = Vector2.zero;
        lookTouchStartTime = 0f;
        lookTouchExceededTapMovement = false;
    }

    private void BuildUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
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

        GameObject joystickBase = MakePanel("Movement", transform, new Color(0.08f, 0f, 0f, 0.56f));
        RectTransform baseRect = joystickBase.GetComponent<RectTransform>();
        SetRect(baseRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 320f), new Vector2(205f, 205f));
        ConfigureCircularImage(joystickBase.GetComponent<Image>(), new Color(0.08f, 0f, 0f, 0.56f));
        Outline baseOutline = joystickBase.AddComponent<Outline>();
        baseOutline.effectColor = new Color(0.9f, 0.08f, 0.08f, 0.72f);
        baseOutline.effectDistance = new Vector2(4f, -4f);
        CursedJoystick joystick = joystickBase.AddComponent<CursedJoystick>();
        joystick.radius = 112f;
        joystick.deadZone = 0.055f;
        lookBlockedRects.Add(baseRect);

        GameObject knob = MakePanel("Knob", joystickBase.transform, new Color(0.68f, 0.04f, 0.04f, 0.78f));
        RectTransform knobRect = knob.GetComponent<RectTransform>();
        SetRect(knobRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(118f, 118f), Vector2.zero);
        ConfigureCircularImage(knob.GetComponent<Image>(), new Color(0.72f, 0.03f, 0.03f, 0.88f));
        Outline knobOutline = knob.AddComponent<Outline>();
        knobOutline.effectColor = new Color(1f, 0.34f, 0.2f, 0.82f);
        knobOutline.effectDistance = new Vector2(3f, -3f);
        knob.GetComponent<Image>().raycastTarget = false;
        joystick.knob = knobRect;

        MakeTextureActionButton("LookBackIcon", "CursedMod/MobileLookBackButton", InputAction.LookBehind, new Vector2(-135f, 335f), new Vector2(0.88f, 0f));
        MakeTextureActionButton("RunIcon", "CursedMod/MobileRunButton", InputAction.Run, new Vector2(-135f, 165f), new Vector2(0.88f, 0f));
        MakeTextureItemButton("ItemIcon", "CursedMod/MobileUseItemButton", new Vector2(-430f, 330f), new Vector2(0.88f, 0f));
        // Top-center placement avoids covering the third inventory slot.
        MakeActionButton("II", InputAction.PauseOrCancel, new Vector2(0f, -68f), new Vector2(0.5f, 1f), new Vector2(104f, 88f), new Color(0.12f, 0f, 0f, 0.72f));

        MakeSlotSelectButton(0);
        MakeSlotSelectButton(1);
        MakeSlotSelectButton(2);

    }

    private void MakeActionButton(string label, InputAction action, Vector2 position, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject buttonObject = MakePanel(label, transform, color);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetRect(buttonRect, anchor, anchor, size, position);
        lookBlockedRects.Add(buttonRect);
        CursedMobileHoldButton button = buttonObject.AddComponent<CursedMobileHoldButton>();
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

    private void MakeTextureActionButton(string objectName, string resourcePath, InputAction action, Vector2 position, Vector2 anchor)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogError("Mobile button texture could not be loaded: " + resourcePath);
            MakeActionButton(string.Empty, action, position, anchor, new Vector2(150f, 150f), new Color(0.18f, 0.02f, 0.02f, 0.82f));
            return;
        }

        GameObject buttonObject = MakePanel(objectName, transform, new Color(0.12f, 0f, 0f, 0.58f));
        buttonObject.transform.SetParent(transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetRect(buttonRect, anchor, anchor, new Vector2(150f, 150f), position);
        ConfigureCircularImage(buttonObject.GetComponent<Image>(), new Color(0.12f, 0f, 0f, 0.58f));
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.1f, 0.08f, 0.68f);
        outline.effectDistance = new Vector2(3f, -3f);
        lookBlockedRects.Add(buttonRect);

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        iconObject.transform.SetParent(buttonObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        SetRect(iconRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        iconRect.offsetMin = new Vector2(12f, 12f);
        iconRect.offsetMax = new Vector2(-12f, -12f);
        RawImage image = iconObject.GetComponent<RawImage>();
        image.texture = texture;
        image.color = Color.white;
        image.raycastTarget = false;

        CursedMobileHoldButton button = buttonObject.AddComponent<CursedMobileHoldButton>();
        button.action = action;
    }

    private void MakeTextureItemButton(string objectName, string resourcePath, Vector2 position, Vector2 anchor)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogError("Item button texture could not be loaded: " + resourcePath);
            return;
        }

        GameObject buttonObject = MakePanel(objectName, transform, Color.clear);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetRect(buttonRect, anchor, anchor, new Vector2(150f, 150f), position);
        lookBlockedRects.Add(buttonRect);

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        iconObject.transform.SetParent(buttonObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        SetRect(iconRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RawImage image = iconObject.GetComponent<RawImage>();
        image.texture = texture;
        image.color = Color.white;
        image.raycastTarget = false;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(QueueItemUse);
    }

    private void BindInventorySlots()
    {
        GameControllerScript controller = FindFirstObjectByType<GameControllerScript>();
        if (controller == null || controller.itemSlot == null) return;

        for (int slot = 0; slot < trackedSlotRects.Length; slot++)
        {
            trackedSlotRects[slot] = slot < controller.itemSlot.Length && controller.itemSlot[slot] != null
                ? controller.itemSlot[slot].rectTransform
                : null;
        }
    }

    private void MakeSlotSelectButton(int slot)
    {
        GameObject touchTarget = MakePanel("Select Item Slot " + (slot + 1), transform, Color.clear);
        RectTransform touchRect = touchTarget.GetComponent<RectTransform>();
        SetRect(touchRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(76f, 92f), Vector2.zero);
        slotTouchRects[slot] = touchRect;
        lookBlockedRects.Add(touchRect);

        CursedMobileSlotButton selector = touchTarget.AddComponent<CursedMobileSlotButton>();
        selector.slot = slot;
    }

    private void UpdateInventorySlotTargets()
    {
        RectTransform mobileCanvasRect = transform as RectTransform;
        if (mobileCanvasRect == null) return;

        for (int slot = 0; slot < slotTouchRects.Length; slot++)
        {
            RectTransform target = slotTouchRects[slot];
            RectTransform source = trackedSlotRects[slot];
            if (target == null) continue;
            target.gameObject.SetActive(source != null && canvas.enabled);
            if (source == null || !canvas.enabled) continue;

            source.GetWorldCorners(slotWorldCorners);
            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(null, slotWorldCorners[0]);
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(null, slotWorldCorners[2]);
            Vector2 screenCenter = (screenMin + screenMax) * 0.5f;
            Vector2 localCenter;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mobileCanvasRect, screenCenter, null, out localCenter))
            {
                target.anchoredPosition = localCenter;
            }
        }
    }

    private void QueueItemUse()
    {
        itemUseQueued = true;
    }

    public static void QueueSlotSelection(int slot)
    {
        slotSelectionQueued = Mathf.Clamp(slot, 0, 2);
    }

    private static void ConfigureCircularImage(Image image, Color color)
    {
        if (circleSprite == null)
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "CursedMobileCircle";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius + 1f - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            circleSprite.name = "CursedMobileCircleSprite";
        }

        image.sprite = circleSprite;
        image.type = Image.Type.Simple;
        image.color = color;
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

    private sealed class CursedMobileSlotButton : MonoBehaviour, IPointerDownHandler
    {
        public int slot;

        public void OnPointerDown(PointerEventData eventData)
        {
            // Select immediately on finger-down. A small drag or release outside
            // the slot must not cancel an inventory selection on Android.
            CursedMobileInput.QueueSlotSelection(slot);
        }
    }

    private class CursedJoystick : MonoBehaviour, IInitializePotentialDragHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform knob;
        public float radius;
        public float deadZone;
        private int activePointerId = int.MinValue;

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Do not wait for EventSystem's pixel drag threshold. Directional
            // movement must react on the first tiny finger movement.
            eventData.useDragThreshold = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue) return;
            activePointerId = eventData.pointerId;
            UpdatePosition(eventData.position, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId) UpdatePosition(eventData.position, eventData.pressEventCamera);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            activePointerId = int.MinValue;
            ResetJoystick();
        }

        private void OnDisable()
        {
            activePointerId = int.MinValue;
            ResetJoystick();
        }

        private void Update()
        {
            if (!CursedMobileInput.IsActive)
            {
                if (activePointerId >= 0) ResetPointer();
                return;
            }

            bool trackedTouchFound = false;
            RectTransform touchArea = (RectTransform)transform;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (activePointerId == int.MinValue && touch.phase == TouchPhase.Began &&
                    RectTransformUtility.RectangleContainsScreenPoint(touchArea, touch.position, null))
                {
                    activePointerId = touch.fingerId;
                }

                if (touch.fingerId != activePointerId) continue;
                trackedTouchFound = true;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ResetPointer();
                }
                else
                {
                    // Raw Android fallback: keeps tracking the captured finger
                    // even if StandaloneInputModule drops an OnDrag callback.
                    UpdatePosition(touch.position, null);
                }
            }

            if (activePointerId >= 0 && !trackedTouchFound)
            {
                ResetPointer();
            }
        }

        private void ResetJoystick()
        {
            moveVector = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }

        private void ResetPointer()
        {
            activePointerId = int.MinValue;
            ResetJoystick();
        }

        private void UpdatePosition(Vector2 screenPosition, Camera eventCamera)
        {
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPosition, eventCamera, out local))
            {
                return;
            }
            local = Vector2.ClampMagnitude(local, radius);
            float normalizedMagnitude = local.magnitude / Mathf.Max(radius, 1f);
            if (normalizedMagnitude <= deadZone)
            {
                moveVector = Vector2.zero;
            }
            else
            {
                float adjustedMagnitude = Mathf.InverseLerp(deadZone, 1f, normalizedMagnitude);
                moveVector = local.normalized * adjustedMagnitude;
            }
            if (knob != null) knob.anchoredPosition = local;
        }
    }

}
