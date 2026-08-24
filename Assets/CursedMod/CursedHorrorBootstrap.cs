using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Installs the horror skin and atmosphere without replacing the original Baldi gameplay code.
/// This keeps the mod based on the original Baldi character and school project.
/// </summary>
public class CursedHorrorBootstrap : MonoBehaviour
{
    private static CursedHorrorBootstrap instance;
    private Texture2D cursedBaldiTexture;
    private Texture2D cursedThinkPadTexture;
    private Texture2D helpMeExitTexture;
    private Sprite cursedBaldiSprite;
    private Sprite helpMeExitSprite;
    private Image dangerFlash;
    private float pulse;
    private bool horrorActive;

    public static bool HorrorActive
    {
        get { return instance != null && instance.horrorActive; }
    }

    public static void ActivateHorror()
    {
        if (instance != null) instance.ActivateHorrorInternal();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (instance != null) return;
        GameObject root = new GameObject("Cursed Horror Mod");
        instance = root.AddComponent<CursedHorrorBootstrap>();
        DontDestroyOnLoad(root);
    }

    private void Awake()
    {
        cursedBaldiTexture = Resources.Load<Texture2D>("CursedMod/CursedBaldi");
        cursedThinkPadTexture = Resources.Load<Texture2D>("CursedMod/CursedThinkPad");
        helpMeExitTexture = Resources.Load<Texture2D>("CursedMod/HelpMeExitSign");
        if (cursedBaldiTexture != null)
        {
            // Account for the different transparent bottom padding so the
            // cursed feet use the exact same ground line as original Baldi.
            cursedBaldiSprite = Sprite.Create(cursedBaldiTexture, new Rect(0f, 0f, cursedBaldiTexture.width, cursedBaldiTexture.height), new Vector2(0.5f, 0.5344603f), 256f);
            cursedBaldiSprite.name = "Cursed Baldi Runtime Sprite";
        }
        if (helpMeExitTexture != null)
        {
            // Match the original ExitSign.png import: centered pivot and 100 pixels per unit.
            helpMeExitSprite = Sprite.Create(helpMeExitTexture, new Rect(0f, 0f, helpMeExitTexture.width, helpMeExitTexture.height), new Vector2(0.5f, 0.5f), 100f);
            helpMeExitSprite.name = "Phase 2 Help Me Exit Sign";
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (CursedPhaseManager.IsPhase4)
        {
            horrorActive = false;
            RemoveDangerOverlay();
            CursedPhase4Screen.Show();
            return;
        }

        if (CursedPhaseManager.IsPhase3)
        {
            horrorActive = false;
            RemoveDangerOverlay();
            CursedPhase3Screen.Show();
            return;
        }

        if (scene.name == "MainMenu" || scene.name == "Warning")
        {
            horrorActive = false;
            RemoveDangerOverlay();
        }
        ApplyPhase2MusicSpeed(scene);
        StartCoroutine(PatchSceneAfterActivation(scene));
    }

    private IEnumerator PatchSceneAfterActivation(Scene scene)
    {
        yield return null;

        // Repeat after one frame as a safety net for objects instantiated by Start().
        ApplyPhase2MusicSpeed(scene);

        bool gameplay = FindFirstObjectByType<PlayerScript>() != null || FindFirstObjectByType<PlayerMovement>() != null;
        if (gameplay)
        {
            CursedMobileInput.EnsureForGameplayScene();
            CursedFinalExitSequence.EnsureInstalled();
            if (CursedPhaseManager.IsPhase2)
            {
                PatchExitSigns();
            }
            if (horrorActive)
            {
                PatchBaldiVisuals();
                PatchThinkPad();
                InstallAtmosphere();
                InstallDangerOverlay();
            }
        }
        else
        {
            CursedMobileInput.Hide();
        }

        if (scene.name == "GameOver" && CursedPhaseManager.IsPhase2)
        {
            InstallGameOverImage();
        }
    }

    private void PatchExitSigns()
    {
        if (helpMeExitSprite == null)
        {
            Debug.LogError("Phase 2 HELP ME exit sign texture could not be loaded.");
            return;
        }

        int patched = 0;
        SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (!renderer.gameObject.scene.IsValid()) continue;
            if (renderer.gameObject.name != "ExitSignSprite") continue;
            renderer.sprite = helpMeExitSprite;
            patched++;
        }
        Debug.Log("Phase 2 HELP ME exit signs applied: " + patched);
    }

    private static void ApplyPhase2MusicSpeed(Scene scene)
    {
        if (!CursedPhaseManager.IsPhase2) return;

        if (scene.name == "MainMenu")
        {
            AudioSource[] menuSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            for (int i = 0; i < menuSources.Length; i++)
            {
                AudioSource source = menuSources[i];
                if (!source.gameObject.scene.IsValid() || source.gameObject.scene != scene) continue;
                if (source.clip != null && source.clip.name == "mus_Intro")
                {
                    source.pitch = 0.5f;
                }
            }
        }

        GameControllerScript controller = FindFirstObjectByType<GameControllerScript>();
        if (controller != null)
        {
            // schoolMusic is heard when gameplay begins; learnMusic is the
            // You Can Think Pad background track.
            if (controller.schoolMusic != null) controller.schoolMusic.pitch = 0.5f;
            if (controller.learnMusic != null) controller.learnMusic.pitch = 0.5f;
        }
    }

    private void ActivateHorrorInternal()
    {
        if (horrorActive) return;
        horrorActive = true;
        PatchBaldiVisuals();
        PatchThinkPad();
        InstallAtmosphere();
        InstallDangerOverlay();
    }

    private void PatchBaldiVisuals()
    {
        if (cursedBaldiSprite == null) return;
        SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (!renderer.gameObject.scene.IsValid()) continue;
            if (!ContainsBaldiName(renderer.transform)) continue;
            if (renderer.GetComponent<CursedBaldiVisual>() != null) continue;

            CursedBaldiVisual visual = renderer.gameObject.AddComponent<CursedBaldiVisual>();
            visual.Apply(renderer, cursedBaldiSprite);
        }

        BaldiScript[] baldis = Resources.FindObjectsOfTypeAll<BaldiScript>();
        for (int i = 0; i < baldis.Length; i++)
        {
            if (!baldis[i].gameObject.scene.IsValid()) continue;
            baldis[i].speed *= 1.12f;
            baldis[i].baldiSpeedScale *= 1.08f;
            baldis[i].baseTime = Mathf.Min(baldis[i].baseTime, 2.65f);
        }
    }

    private static bool ContainsBaldiName(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            if (current.name.ToLowerInvariant().Contains("baldi")) return true;
            current = current.parent;
        }
        return false;
    }

    private void PatchThinkPad()
    {
        MathGameScript[] mathGames = Resources.FindObjectsOfTypeAll<MathGameScript>();
        for (int i = 0; i < mathGames.Length; i++)
        {
            MathGameScript math = mathGames[i];
            if (!math.gameObject.scene.IsValid()) continue;
            CursedThinkPadInstaller.ApplyTo(math);
        }
    }

    private void InstallAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.010f;
        RenderSettings.fogColor = new Color(0.075f, 0.018f, 0.018f, 1f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.30f, 0.17f, 0.16f, 1f);

        Camera camera = Camera.main;
        if (camera != null && camera.GetComponent<CursedFlickerLight>() == null)
        {
            Light light = camera.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 18f;
            light.intensity = 1.05f;
            light.color = new Color(1f, 0.62f, 0.52f);
            camera.gameObject.AddComponent<CursedFlickerLight>().lightSource = light;
        }
    }

    private void InstallDangerOverlay()
    {
        if (dangerFlash != null) Destroy(dangerFlash.gameObject.transform.root.gameObject);
        GameObject canvasObject = new GameObject("Cursed Danger Canvas", typeof(Canvas), typeof(CanvasScaler));
        DontDestroyOnLoad(canvasObject);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 31000;

        GameObject flashObject = new GameObject("Danger Pulse", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = flashObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        dangerFlash = flashObject.GetComponent<Image>();
        dangerFlash.color = new Color(0.35f, 0f, 0f, 0f);
        dangerFlash.raycastTarget = false;
    }

    private void RemoveDangerOverlay()
    {
        if (dangerFlash == null) return;
        GameObject root = dangerFlash.transform.root.gameObject;
        dangerFlash = null;
        Destroy(root);
    }

    private void Update()
    {
        if (dangerFlash == null) return;
        BaldiScript baldi = FindFirstObjectByType<BaldiScript>();
        PlayerScript player = FindFirstObjectByType<PlayerScript>();
        float targetAlpha = 0f;
        if (baldi != null && player != null && baldi.gameObject.activeInHierarchy)
        {
            float distance = Vector3.Distance(baldi.transform.position, player.transform.position);
            float danger = 1f - Mathf.Clamp01((distance - 2f) / 24f);
            pulse += Time.unscaledDeltaTime * Mathf.Lerp(2f, 8f, danger);
            targetAlpha = danger * (0.055f + Mathf.Abs(Mathf.Sin(pulse)) * 0.12f);
        }
        Color color = dangerFlash.color;
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.unscaledDeltaTime * 4f);
        dangerFlash.color = color;
    }

    private void InstallGameOverImage()
    {
        if (cursedBaldiTexture == null) return;
        GameObject canvasObject = new GameObject("Cursed Baldi Jumpscare", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;

        GameObject background = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(canvasObject.transform, false);
        Stretch(background.GetComponent<RectTransform>());
        background.GetComponent<Image>().color = Color.black;

        GameObject face = new GameObject("Cursed Baldi", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        face.transform.SetParent(canvasObject.transform, false);
        RectTransform faceRect = face.GetComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0.16f, -0.1f);
        faceRect.anchorMax = new Vector2(0.84f, 1.1f);
        faceRect.offsetMin = Vector2.zero;
        faceRect.offsetMax = Vector2.zero;
        RawImage raw = face.GetComponent<RawImage>();
        raw.texture = cursedBaldiTexture;
        raw.raycastTarget = false;
        face.AddComponent<CursedJumpscarePulse>();
        StartCoroutine(QuitAfterPhase2Jumpscare());
    }

    private IEnumerator QuitAfterPhase2Jumpscare()
    {
        // Keep the jumpscare visible before closing the Phase 2 game session.
        yield return new WaitForSecondsRealtime(3f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

public static class CursedThinkPadInstaller
{
    public static void ApplyTo(MathGameScript math)
    {
        if (math == null) return;
        // Keep Phase 2 notebooks normal until runtime horror activation: either
        // a wrong first-notebook answer or the second notebook's final answer.
        if (!CursedHorrorBootstrap.HorrorActive) return;
        Texture2D texture = Resources.Load<Texture2D>("CursedMod/CursedThinkPad");
        if (texture == null) return;
        GameObject root = math.mathGame != null ? math.mathGame : math.gameObject;
        if (root.transform.Find("Cursed Think Pad Skin") != null) return;

        // The stock YCTP image is opaque around its transparent display cutouts.
        // Hide only that background graphic; its keypad children remain active.
        Transform stockThinkPad = root.transform.Find("YCTP");
        if (stockThinkPad != null)
        {
            RawImage stockBackground = stockThinkPad.GetComponent<RawImage>();
            if (stockBackground != null) stockBackground.enabled = false;

            // Keep the original Button components and callbacks, but align their
            // invisible hit areas with the keys baked into the cursed artwork.
            ConfigureKey(stockThinkPad, "Button (7)", new Vector2(224.6f, 203.3f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (8)", new Vector2(281.5f, 202.0f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (9)", new Vector2(336.4f, 202.6f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (4)", new Vector2(224.9f, 133.0f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (5)", new Vector2(281.2f, 132.3f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (6)", new Vector2(337.1f, 132.8f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (1)", new Vector2(225.5f, 62.8f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (2)", new Vector2(281.9f, 62.9f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (3)", new Vector2(336.4f, 62.5f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (0)", new Vector2(253.4f, -7.4f), new Vector2(106f, 64f));
            ConfigureKey(stockThinkPad, "Button (-)", new Vector2(337.0f, -7.9f), new Vector2(53f, 64f));
            ConfigureKey(stockThinkPad, "Button (OK)", new Vector2(276.4f, -149.8f), new Vector2(142f, 164f));

            // The cursed artwork has a wide zero key instead of a clear key.
            // Disable the old clear button so the left half of zero cannot erase.
            Transform clearKey = stockThinkPad.Find("Buttons/Button (C)");
            if (clearKey != null) clearKey.gameObject.SetActive(false);
        }

        GameObject skin = new GameObject("Cursed Think Pad Skin", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        skin.transform.SetParent(root.transform, false);
        // Insert the cursed background immediately before the live result layer.
        // Questions, result marks, answer text and buttons then render above it.
        int foregroundIndex = 1;
        if (math.results != null && math.results.Length > 0 && math.results[0] != null)
        {
            Transform resultLayer = math.results[0].transform.parent;
            if (resultLayer != null && resultLayer.parent == root.transform)
            {
                foregroundIndex = resultLayer.GetSiblingIndex();
            }
        }
        skin.transform.SetSiblingIndex(Mathf.Clamp(foregroundIndex, 0, root.transform.childCount - 1));
        RectTransform rect = skin.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        RawImage image = skin.GetComponent<RawImage>();
        image.texture = texture;
        image.color = new Color(0.82f, 0.82f, 0.82f, 1f);
        image.raycastTarget = false;
    }

    private static void ConfigureKey(Transform stockThinkPad, string keyName, Vector2 position, Vector2 size)
    {
        Transform key = stockThinkPad.Find("Buttons/" + keyName);
        if (key == null) return;

        RectTransform rect = key as RectTransform;
        if (rect != null)
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        Button button = key.GetComponent<Button>();
        if (button != null) button.transition = Selectable.Transition.None;

        // The generated skin already draws the keys. Preserve transparent
        // raycast graphics for input without drawing the stock keys twice.
        Graphic[] graphics = key.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = 0f;
            graphics[i].color = color;
            graphics[i].raycastTarget = true;
        }
    }
}

public class CursedBaldiVisual : MonoBehaviour
{
    private SpriteRenderer target;
    private Sprite cursedSprite;

    public void Apply(SpriteRenderer renderer, Sprite sprite)
    {
        target = renderer;
        cursedSprite = sprite;
        float oldHeight = target.sprite != null ? target.sprite.bounds.size.y : 2.56f;
        float newHeight = cursedSprite.bounds.size.y;
        if (newHeight > 0.01f)
        {
            // The original 256 px sprite has 232 visible character pixels; the
            // cursed 1536 px sprite has 1460. Ignore each texture's transparent
            // padding so both characters have exactly the same visible height.
            const float originalVisibleFraction = 232f / 256f;
            const float cursedVisibleFraction = 1460f / 1536f;
            float ratio = oldHeight * originalVisibleFraction / (newHeight * cursedVisibleFraction);
            transform.localScale = new Vector3(transform.localScale.x * ratio, transform.localScale.y * ratio, transform.localScale.z * ratio);
        }
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
        target.sprite = cursedSprite;
        target.color = Color.white;
    }

    private void LateUpdate()
    {
        if (target != null && cursedSprite != null) target.sprite = cursedSprite;
    }
}

public class CursedFlickerLight : MonoBehaviour
{
    public Light lightSource;
    private float baseIntensity;
    private float nextDrop;

    private void Start()
    {
        if (lightSource != null) baseIntensity = lightSource.intensity;
    }

    private void Update()
    {
        if (lightSource == null) return;
        if (Time.unscaledTime >= nextDrop)
        {
            nextDrop = Time.unscaledTime + Random.Range(0.035f, 0.22f);
            lightSource.intensity = Random.value < 0.12f ? baseIntensity * Random.Range(0.05f, 0.3f) : baseIntensity * Random.Range(0.82f, 1.12f);
        }
    }
}

public class CursedJumpscarePulse : MonoBehaviour
{
    private RectTransform rect;
    private float time;

    private void Start() { rect = GetComponent<RectTransform>(); }
    private void Update()
    {
        time += Time.unscaledDeltaTime;
        if (rect != null)
        {
            float scale = 1f + Mathf.Sin(time * 28f) * 0.025f + Mathf.Clamp01(time) * 0.28f;
            rect.localScale = new Vector3(scale, scale, 1f);
            rect.anchoredPosition = Random.insideUnitCircle * Mathf.Lerp(2f, 16f, Mathf.Clamp01(time));
        }
    }
}
