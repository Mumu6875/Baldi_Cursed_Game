using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Persists the three-stage horror flow between Android launches.
/// Phase 1 unlocks Phase 2; finishing Phase 2 stores the password and unlocks Phase 3.
/// </summary>
public static class CursedPhaseManager
{
    // Versioned so updating from the earlier test build starts the revised
    // Phase 1 flow once instead of inheriting its already-unlocked Phase 2.
    private const string Phase2Key = "CursedHorrorPhase2Unlocked_v2";
    private const string Phase3Key = "CursedHorrorPhase3Unlocked_v1";
    private const string Phase3PasswordKey = "CursedHorrorPhase3Password_v1";
    private static bool warningVisible;

    public static bool IsPhase2
    {
        get { return PlayerPrefs.GetInt(Phase2Key, 0) == 1; }
    }

    public static bool IsPhase3
    {
        get { return PlayerPrefs.GetInt(Phase3Key, 0) == 1; }
    }

    public static string Phase3Password
    {
        get { return PlayerPrefs.GetString(Phase3PasswordKey, "0000"); }
    }

    public static void UnlockPhase3(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length != 4)
        {
            Debug.LogError("Phase 3 password must contain exactly four digits.");
            return;
        }
        PlayerPrefs.SetString(Phase3PasswordKey, password);
        PlayerPrefs.SetInt(Phase3Key, 1);
        PlayerPrefs.Save();
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

        return ShowPiracyWarning();
    }

    public static bool HandleFirstNotebookWrongAnswer()
    {
        if (IsPhase2)
        {
            CursedHorrorBootstrap.ActivateHorror();
            return false;
        }
        return ShowPiracyWarning();
    }

    private static bool ShowPiracyWarning()
    {
        if (warningVisible) return true;
        warningVisible = true;

        Texture2D warningTexture = Resources.Load<Texture2D>("CursedMod/PiracyWarningPhase1");
        if (warningTexture == null)
        {
            Debug.LogError("Phase 1 warning texture could not be loaded.");
            warningVisible = false;
            return false;
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
        button.targetGraphic = image;
        button.onClick.AddListener(UnlockPhase2AndQuit);

        CursedMobileInput.Hide();
        AudioListener.pause = true;
        Time.timeScale = 0f;
        return true;
    }

    private static void UnlockPhase2AndQuit()
    {
        PlayerPrefs.SetInt(Phase2Key, 1);
        PlayerPrefs.Save();
        Application.Quit();
    }
}
