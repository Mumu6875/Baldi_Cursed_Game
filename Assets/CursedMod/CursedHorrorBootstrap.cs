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
    private Sprite cursedBaldiSprite;
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
        if (cursedBaldiTexture != null)
        {
            cursedBaldiSprite = Sprite.Create(cursedBaldiTexture, new Rect(0f, 0f, cursedBaldiTexture.width, cursedBaldiTexture.height), new Vector2(0.5f, 0.02f), 256f);
            cursedBaldiSprite.name = "Cursed Baldi Runtime Sprite";
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

        bool gameplay = FindObjectOfType<PlayerScript>() != null || FindObjectOfType<PlayerMovement>() != null;
        if (gameplay)
        {
            CursedMobileInput.EnsureForGameplayScene();
            CursedFinalExitSequence.EnsureInstalled();
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

        if (scene.name == "GameOver" && horrorActive)
        {
            InstallGameOverImage();
        }
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

        GameControllerScript controller = FindObjectOfType<GameControllerScript>();
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
        BaldiScript baldi = FindObjectOfType<BaldiScript>();
        PlayerScript player = FindObjectOfType<PlayerScript>();
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
        if (!CursedHorrorBootstrap.HorrorActive) return;
        Texture2D texture = Resources.Load<Texture2D>("CursedMod/CursedThinkPad");
        if (texture == null) return;
        GameObject root = math.mathGame != null ? math.mathGame : math.gameObject;
        if (root.transform.Find("Cursed Think Pad Skin") != null) return;

        GameObject skin = new GameObject("Cursed Think Pad Skin", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        skin.transform.SetParent(root.transform, false);
        skin.transform.SetAsFirstSibling();
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
            float ratio = oldHeight / newHeight;
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
