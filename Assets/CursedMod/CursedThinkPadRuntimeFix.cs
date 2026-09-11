using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Finalizes the cursed You Can Think Pad after the normal horror installer has
/// created it. The background asset itself is the user's original Think Pad
/// image with only an ImageMagick color inversion applied.
/// </summary>
[DefaultExecutionOrder(10000)]
public sealed class CursedThinkPadRuntimeFix : MonoBehaviour
{
    private static Texture2D cursedTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        GameObject root = new GameObject("Cursed Think Pad Runtime Fix");
        DontDestroyOnLoad(root);
        root.AddComponent<CursedThinkPadRuntimeFix>();
    }

    private void LateUpdate()
    {
        if (!CursedHorrorBootstrap.HorrorActive) return;

        if (cursedTexture == null)
        {
            cursedTexture = Resources.Load<Texture2D>("CursedMod/CursedThinkPad");
            if (cursedTexture == null) return;
        }

        MathGameScript[] games = Resources.FindObjectsOfTypeAll<MathGameScript>();
        for (int i = 0; i < games.Length; i++)
        {
            MathGameScript math = games[i];
            if (math == null || !math.gameObject.scene.IsValid()) continue;
            FinalizeThinkPad(math);
        }
    }

    private static void FinalizeThinkPad(MathGameScript math)
    {
        GameObject root = math.mathGame != null ? math.mathGame : math.gameObject;
        if (root == null || root.transform.Find("Cursed Think Pad Finalized") != null) return;

        Transform skinTransform = root.transform.Find("Cursed Think Pad Skin");
        if (skinTransform == null) return; // The normal horror installer has not run yet.

        RawImage skin = skinTransform.GetComponent<RawImage>();
        if (skin != null)
        {
            skin.texture = cursedTexture;
            skin.color = Color.white;
            skin.raycastTarget = false;
        }

        // The supplied Think Pad artwork already paints these white panels.
        // Disable only the stock panel graphics; keep the live question text
        // and correct/incorrect result marks active.
        DisableStockGraphic(root.transform, "ResultBG");
        DisableStockGraphic(root.transform, "TextBG");

        if (math.playerAnswer != null)
        {
            if (math.playerAnswer.placeholder != null)
                math.playerAnswer.placeholder.gameObject.SetActive(false);

            Image answerBackground = math.playerAnswer.GetComponent<Image>();
            if (answerBackground != null)
            {
                answerBackground.enabled = false;
                answerBackground.raycastTarget = false;
            }
            SetReadable(math.playerAnswer.textComponent);
        }

        // The inverted LCD areas are black, so the live text must be white.
        SetReadable(math.questionText);
        SetReadable(math.questionText2);
        SetReadable(math.questionText3);

        AlignResultMarks(math, root.transform);

        Transform oldControls = root.transform.Find("Cursed Think Pad Controls");
        if (oldControls != null)
        {
            oldControls.gameObject.SetActive(false);
            Destroy(oldControls.gameObject);
        }

        BuildControls(root.transform, math);

        GameObject marker = new GameObject("Cursed Think Pad Finalized");
        marker.transform.SetParent(root.transform, false);
    }

    private static void SetReadable(TMP_Text text)
    {
        if (text != null) text.color = Color.white;
    }

    private static void DisableStockGraphic(Transform root, string objectName)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate.name != objectName) continue;
            Graphic graphic = candidate.GetComponent<Graphic>();
            if (graphic == null) continue;
            graphic.enabled = false;
            graphic.raycastTarget = false;
        }
    }

    private static void AlignResultMarks(MathGameScript math, Transform root)
    {
        if (math.results == null || math.results.Length == 0) return;

        Vector2[] anchors =
        {
            new Vector2(0.249779f, 0.664734f),
            new Vector2(0.242471f, 0.570842f),
            new Vector2(0.239235f, 0.472848f)
        };

        GameObject layer = new GameObject("Cursed Result Marks Final", typeof(RectTransform));
        layer.transform.SetParent(root, false);
        RectTransform layerRect = layer.GetComponent<RectTransform>();
        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;
        layer.transform.SetAsLastSibling();

        int count = Mathf.Min(math.results.Length, anchors.Length);
        for (int i = 0; i < count; i++)
        {
            RawImage result = math.results[i];
            if (result == null) continue;
            RectTransform rect = result.rectTransform;
            rect.SetParent(layerRect, false);
            rect.anchorMin = anchors[i];
            rect.anchorMax = anchors[i];
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(53f, 53f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            result.raycastTarget = false;
        }
    }

    private static void BuildControls(Transform root, MathGameScript math)
    {
        GameObject controls = new GameObject("Cursed Think Pad Controls", typeof(RectTransform));
        controls.transform.SetParent(root, false);
        RectTransform rect = controls.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        controls.transform.SetAsLastSibling();

        // Normalized directly from the 1536x1152 supplied Think Pad artwork.
        CreateKey(controls.transform, "7", new Vector2(0.755859f, 0.655382f), new Vector2(0.804688f, 0.722222f), math, 7, false);
        CreateKey(controls.transform, "8", new Vector2(0.818359f, 0.653646f), new Vector2(0.867839f, 0.720486f), math, 8, false);
        CreateKey(controls.transform, "9", new Vector2(0.881510f, 0.652778f), new Vector2(0.930990f, 0.718750f), math, 9, false);
        CreateKey(controls.transform, "4", new Vector2(0.757161f, 0.570312f), new Vector2(0.805990f, 0.636285f), math, 4, false);
        CreateKey(controls.transform, "5", new Vector2(0.819661f, 0.570312f), new Vector2(0.868490f, 0.635417f), math, 5, false);
        CreateKey(controls.transform, "6", new Vector2(0.882812f, 0.570312f), new Vector2(0.932292f, 0.636285f), math, 6, false);
        CreateKey(controls.transform, "1", new Vector2(0.756510f, 0.486111f), new Vector2(0.805990f, 0.551215f), math, 1, false);
        CreateKey(controls.transform, "2", new Vector2(0.819661f, 0.484375f), new Vector2(0.868490f, 0.550347f), math, 2, false);
        CreateKey(controls.transform, "3", new Vector2(0.882812f, 0.484375f), new Vector2(0.932292f, 0.550347f), math, 3, false);
        CreateKey(controls.transform, "C", new Vector2(0.757812f, 0.397569f), new Vector2(0.807292f, 0.463542f), math, -2, false);
        CreateKey(controls.transform, "0", new Vector2(0.819661f, 0.395833f), new Vector2(0.868490f, 0.461806f), math, 0, false);
        CreateKey(controls.transform, "Minus", new Vector2(0.880859f, 0.394965f), new Vector2(0.929688f, 0.460938f), math, -1, false);
        CreateKey(controls.transform, "OK", new Vector2(0.769531f, 0.187500f), new Vector2(0.916667f, 0.383681f), math, 0, true);
    }

    private static void CreateKey(Transform parent, string keyName, Vector2 anchorMin, Vector2 anchorMax,
        MathGameScript math, int value, bool submit)
    {
        GameObject key = new GameObject("Cursed Key " + keyName,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        key.transform.SetParent(parent, false);

        RectTransform rect = key.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image hitArea = key.GetComponent<Image>();
        hitArea.color = new Color(1f, 1f, 1f, 0f);
        hitArea.raycastTarget = true;

        Button button = key.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        if (submit)
            button.onClick.AddListener(delegate { math.OKButton(); });
        else
            button.onClick.AddListener(delegate { math.ButtonPress(value); });
    }
}
