using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Persists the two-stage horror flow between Android launches.
/// Phase 1 ends with the warning screen; tapping it unlocks Phase 2 and quits.
/// </summary>
public static class CursedPhaseManager
{
    private const string Phase2Key = "CursedHorrorPhase2Unlocked";
    private static bool warningVisible;

    public static bool IsPhase2
    {
        get { return PlayerPrefs.GetInt(Phase2Key, 0) == 1; }
    }

    /// <summary>
    /// Returns true when normal answer processing must stop for the warning.
    /// </summary>
    public static bool HandleSecondNotebookFinalAnswer()
    {
        if (IsPhase2)
        {
            CursedHorrorBootstrap.ActivateHorror();
            return false;
        }

        ShowPiracyWarning();
        return true;
    }

    private static void ShowPiracyWarning()
    {
        if (warningVisible) return;
        warningVisible = true;

        Texture2D warningTexture = Resources.Load<Texture2D>("CursedMod/PiracyWarningPhase1");
        if (warningTexture == null)
        {
            Debug.LogError("Phase 1 warning texture could not be loaded.");
            warningVisible = false;
            return;
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasObject = new GameObject("Phase 1 Piracy Warning", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32750;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1672f, 941f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject screen = new GameObject("Tap To Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(Button));
        screen.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = screen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage image = screen.GetComponent<RawImage>();
        image.texture = warningTexture;
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = screen.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(UnlockPhase2AndQuit);

        CursedMobileInput.Hide();
        AudioListener.pause = true;
        Time.timeScale = 0f;
    }

    private static void UnlockPhase2AndQuit()
    {
        PlayerPrefs.SetInt(Phase2Key, 1);
        PlayerPrefs.Save();
        Application.Quit();
    }
}
