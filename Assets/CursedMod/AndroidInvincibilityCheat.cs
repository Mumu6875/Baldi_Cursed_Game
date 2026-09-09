using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Temporary Android play-testing option. Remove this file and the PlayerScript guard
/// before producing a public build.
/// </summary>
public sealed class AndroidInvincibilityCheat : MonoBehaviour
{
    private const string PreferenceKey = "CursedTestInvincibility";
    private static AndroidInvincibilityCheat instance;

    public static bool Enabled
    {
        get { return PlayerPrefs.GetInt(PreferenceKey, 0) == 1; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (instance != null) return;
        GameObject root = new GameObject("Android Invincibility Test Cheat");
        instance = root.AddComponent<AndroidInvincibilityCheat>();
        DontDestroyOnLoad(root);
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") StartCoroutine(AddOptionsToggle(scene));
    }

    private IEnumerator AddOptionsToggle(Scene scene)
    {
        yield return null;

        Toggle template = null;
        Toggle[] toggles = Resources.FindObjectsOfTypeAll<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].gameObject.scene == scene && toggles[i].gameObject.name == "RumbleToggle")
            {
                template = toggles[i];
                break;
            }
        }

        if (template == null || template.transform.parent.Find("InvincibilityToggle") != null) yield break;

        GameObject copy = Instantiate(template.gameObject, template.transform.parent);
        copy.name = "InvincibilityToggle";
        RectTransform rect = copy.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(template.GetComponent<RectTransform>().anchoredPosition.x, -80f);

        Toggle toggle = copy.GetComponent<Toggle>();
        toggle.onValueChanged = new Toggle.ToggleEvent();
        toggle.SetIsOnWithoutNotify(Enabled);
        toggle.onValueChanged.AddListener(SetEnabled);

        Text[] legacyLabels = copy.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < legacyLabels.Length; i++) legacyLabels[i].text = "INVINCIBILITY";
        TextMeshProUGUI[] tmpLabels = copy.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tmpLabels.Length; i++) tmpLabels[i].text = "INVINCIBILITY";
    }

    private static void SetEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(PreferenceKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
