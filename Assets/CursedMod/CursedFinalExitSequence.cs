using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Replaces the normal fourth-exit victory with a sealed exit and turns the
/// existing Room 99 entrance into a foggy runtime-generated maze.
/// </summary>
public class CursedFinalExitSequence : MonoBehaviour
{
    private const int MazeWidth = 11;
    private const int MazeHeight = 11;
    private const float CellSize = 5.5f;
    private const float WallHeight = 4.2f;

    private static CursedFinalExitSequence instance;
    private bool sequenceActive;
    private Canvas overlayCanvas;
    private Image fadeImage;
    private Text messageText;
    private Material wallMaterial;
    private Material floorMaterial;
    private bool completionVisible;
    private string completionCode;

    public static void EnsureInstalled()
    {
        if (instance != null) return;
        GameObject host = new GameObject("Cursed Final Exit Sequence");
        instance = host.AddComponent<CursedFinalExitSequence>();
    }

    public static bool TryStart(ExitTriggerScript exit, Collider playerCollider, GameControllerScript gc)
    {
        EnsureInstalled();
        if (instance.sequenceActive || exit == null || playerCollider == null || gc == null) return false;
        instance.StartCoroutine(instance.Begin(exit, playerCollider, gc));
        return true;
    }

    private IEnumerator Begin(ExitTriggerScript exit, Collider playerCollider, GameControllerScript gc)
    {
        sequenceActive = true;
        DisableAllFinalExitTriggers();
        LockFinalExit(exit.transform);
        BuildOverlay();

        messageText.text = "FINAL EXIT LOCKED\nFIND ROOM 99";
        yield return Fade(0f, 1f, 0.75f);

        HideCharacters(gc);
        ApplyRoom99Lighting();
        Vector3 mazeOrigin = new Vector3(600f, gc.player.height - 0.45f, 600f);
        Vector3 spawnPosition;
        BuildMaze(mazeOrigin, out spawnPosition);
        InstallRoom99Portal(spawnPosition);

        yield return new WaitForSecondsRealtime(0.55f);
        messageText.text = "THE SCHOOL IS EMPTY";
        yield return Fade(1f, 0f, 1.2f);
        messageText.text = string.Empty;
    }

    private void LockFinalExit(Transform exitTransform)
    {
        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.name = "Final Exit Locked Barrier";
        barrier.transform.position = exitTransform.position;
        barrier.transform.rotation = exitTransform.rotation;
        barrier.transform.localScale = new Vector3(4.4f, 5.2f, 0.65f);
        Renderer renderer = barrier.GetComponent<Renderer>();
        renderer.material = CreateMaterial(new Color(0.025f, 0.025f, 0.025f, 1f));
        Collider barrierCollider = barrier.GetComponent<Collider>();
        if (barrierCollider != null) barrierCollider.enabled = false;
    }

    private static void DisableAllFinalExitTriggers()
    {
        ExitTriggerScript[] exits = Resources.FindObjectsOfTypeAll<ExitTriggerScript>();
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i].gameObject.scene.IsValid()) exits[i].enabled = false;
        }
    }

    private void HideCharacters(GameControllerScript gc)
    {
        SetInactive(gc.baldiTutor);
        SetInactive(gc.baldi);
        SetInactive(gc.principal);
        SetInactive(gc.crafters);
        SetInactive(gc.playtime);
        SetInactive(gc.gottaSweep);
        SetInactive(gc.bully);
        SetInactive(gc.firstPrize);
        SetInactive(gc.TestEnemy);

        if (gc.schoolMusic != null) gc.schoolMusic.Stop();
        if (gc.learnMusic != null) gc.learnMusic.Stop();
    }

    private static void SetInactive(GameObject target)
    {
        if (target != null) target.SetActive(false);
    }

    private void ApplyRoom99Lighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.028f, 0.028f, 0.035f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.88f, 0.9f, 0.92f, 1f);
        RenderSettings.fogDensity = 0.048f;

        Light[] lights = FindObjectsOfType<Light>();
        for (int i = 0; i < lights.Length; i++) lights[i].intensity *= 0.18f;

        CursedFlickerLight flicker = Camera.main != null ? Camera.main.GetComponent<CursedFlickerLight>() : null;
        if (flicker != null) flicker.enabled = false;

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = RenderSettings.fogColor;
            Camera.main.farClipPlane = 38f;
            Light dimLight = Camera.main.GetComponent<Light>();
            if (dimLight == null) dimLight = Camera.main.gameObject.AddComponent<Light>();
            dimLight.type = LightType.Point;
            dimLight.range = 10f;
            dimLight.intensity = 0.30f;
            dimLight.color = new Color(0.72f, 0.77f, 0.82f, 1f);
        }
    }

    private void BuildMaze(Vector3 origin, out Vector3 spawnPosition)
    {
        GameObject root = new GameObject("Room 99 Nightmare Maze");
        wallMaterial = CreateMaterial(new Color(0.055f, 0.055f, 0.065f, 1f));
        floorMaterial = CreateMaterial(new Color(0.012f, 0.012f, 0.016f, 1f));

        bool[,] verticalWalls = new bool[MazeWidth + 1, MazeHeight];
        bool[,] horizontalWalls = new bool[MazeWidth, MazeHeight + 1];
        for (int x = 0; x <= MazeWidth; x++)
            for (int z = 0; z < MazeHeight; z++) verticalWalls[x, z] = true;
        for (int x = 0; x < MazeWidth; x++)
            for (int z = 0; z <= MazeHeight; z++) horizontalWalls[x, z] = true;

        CarveMaze(verticalWalls, horizontalWalls);

        float totalWidth = MazeWidth * CellSize;
        float totalDepth = MazeHeight * CellSize;
        CreateCube("Room 99 Floor", root.transform,
            origin + new Vector3(totalWidth * 0.5f, -0.2f, totalDepth * 0.5f),
            new Vector3(totalWidth, 0.4f, totalDepth), floorMaterial);
        CreateCube("Room 99 Ceiling", root.transform,
            origin + new Vector3(totalWidth * 0.5f, WallHeight, totalDepth * 0.5f),
            new Vector3(totalWidth, 0.3f, totalDepth), floorMaterial);

        for (int x = 0; x <= MazeWidth; x++)
        {
            for (int z = 0; z < MazeHeight; z++)
            {
                if (!verticalWalls[x, z]) continue;
                CreateCube("Maze Wall V", root.transform,
                    origin + new Vector3(x * CellSize, WallHeight * 0.5f, (z + 0.5f) * CellSize),
                    new Vector3(0.24f, WallHeight, CellSize + 0.24f), wallMaterial);
            }
        }

        for (int x = 0; x < MazeWidth; x++)
        {
            for (int z = 0; z <= MazeHeight; z++)
            {
                if (!horizontalWalls[x, z]) continue;
                CreateCube("Maze Wall H", root.transform,
                    origin + new Vector3((x + 0.5f) * CellSize, WallHeight * 0.5f, z * CellSize),
                    new Vector3(CellSize + 0.24f, WallHeight, 0.24f), wallMaterial);
            }
        }

        spawnPosition = origin + new Vector3(CellSize * 0.5f, 0.45f, CellSize * 0.5f);
        Vector3 endPosition = origin + new Vector3((MazeWidth - 0.5f) * CellSize, 1.8f, (MazeHeight - 0.5f) * CellSize);
        GameObject end = new GameObject("Room 99 Deep Exit", typeof(BoxCollider), typeof(CursedMazeEndTrigger));
        end.transform.SetParent(root.transform, false);
        end.transform.position = endPosition;
        BoxCollider trigger = end.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(CellSize * 0.72f, 3.8f, CellSize * 0.72f);

        GameObject fogCore = CreateCube("White Fog Core", root.transform, endPosition,
            new Vector3(CellSize * 0.55f, 3.2f, CellSize * 0.55f),
            CreateMaterial(new Color(0.8f, 0.82f, 0.85f, 1f)));
        Collider fogCollider = fogCore.GetComponent<Collider>();
        if (fogCollider != null) fogCollider.enabled = false;
    }

    private void InstallRoom99Portal(Vector3 mazeSpawn)
    {
        Transform marker = FindRoom99Marker();
        Transform doorway = FindNearestDoor(marker);
        Vector3 portalPosition = doorway != null ? doorway.position + Vector3.up * 2.4f : marker.position;

        GameObject portal = new GameObject("Existing Room 99 Maze Portal", typeof(BoxCollider), typeof(CursedRoom99Portal));
        portal.transform.position = portalPosition;
        portal.transform.rotation = doorway != null ? doorway.rotation : marker.rotation;
        BoxCollider trigger = portal.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.8f, 5.5f, 2.4f);
        portal.GetComponent<CursedRoom99Portal>().mazeSpawn = mazeSpawn;

        GameObject fogBeacon = new GameObject("Room 99 White Fog Beacon", typeof(Light));
        fogBeacon.transform.position = portalPosition + Vector3.up * 0.8f;
        Light light = fogBeacon.GetComponent<Light>();
        light.type = LightType.Point;
        light.range = 5.5f;
        light.intensity = 0.22f;
        light.color = new Color(0.88f, 0.92f, 1f, 1f);
    }

    private static Transform FindRoom99Marker()
    {
        MeshRenderer[] renderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (!renderer.gameObject.scene.IsValid()) continue;
            Material[] materials = renderer.sharedMaterials;
            for (int m = 0; m < materials.Length; m++)
            {
                if (materials[m] != null && materials[m].name == "99") return renderer.transform;
            }
        }

        // The open-source School scene's Room 99 marker is at this wall.
        GameObject fallback = new GameObject("Room 99 Marker Fallback");
        fallback.transform.position = new Vector3(-45f, 5f, 20f);
        fallback.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        return fallback.transform;
    }

    private static Transform FindNearestDoor(Transform marker)
    {
        DoorScript[] doors = Resources.FindObjectsOfTypeAll<DoorScript>();
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < doors.Length; i++)
        {
            if (!doors[i].gameObject.scene.IsValid()) continue;
            float distance = (doors[i].transform.position - marker.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = doors[i].transform;
            }
        }
        return nearest;
    }

    private static void CarveMaze(bool[,] verticalWalls, bool[,] horizontalWalls)
    {
        bool[,] visited = new bool[MazeWidth, MazeHeight];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        System.Random random = new System.Random(99);
        Vector2Int current = new Vector2Int(0, 0);
        visited[0, 0] = true;
        int visitedCount = 1;

        while (visitedCount < MazeWidth * MazeHeight)
        {
            List<Vector2Int> choices = new List<Vector2Int>();
            if (current.x > 0 && !visited[current.x - 1, current.y]) choices.Add(Vector2Int.left);
            if (current.x < MazeWidth - 1 && !visited[current.x + 1, current.y]) choices.Add(Vector2Int.right);
            if (current.y > 0 && !visited[current.x, current.y - 1]) choices.Add(Vector2Int.down);
            if (current.y < MazeHeight - 1 && !visited[current.x, current.y + 1]) choices.Add(Vector2Int.up);

            if (choices.Count == 0)
            {
                current = stack.Pop();
                continue;
            }

            Vector2Int direction = choices[random.Next(choices.Count)];
            Vector2Int next = current + direction;
            if (direction == Vector2Int.left) verticalWalls[current.x, current.y] = false;
            else if (direction == Vector2Int.right) verticalWalls[current.x + 1, current.y] = false;
            else if (direction == Vector2Int.down) horizontalWalls[current.x, current.y] = false;
            else horizontalWalls[current.x, current.y + 1] = false;

            stack.Push(current);
            current = next;
            visited[current.x, current.y] = true;
            visitedCount++;
        }
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = material;
        return cube;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("Room 99 Overlay", typeof(Canvas), typeof(CanvasScaler));
        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 32765;

        GameObject fade = new GameObject("Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fade.transform.SetParent(canvasObject.transform, false);
        RectTransform fadeRect = fade.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        fadeImage = fade.GetComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        GameObject message = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        message.transform.SetParent(canvasObject.transform, false);
        RectTransform textRect = message.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.35f);
        textRect.anchorMax = new Vector2(0.9f, 0.65f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        messageText = message.GetComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        messageText.fontSize = 42;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = Color.white;
        messageText.raycastTarget = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
    }

    public void QuitFromMaze()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        messageText.text = string.Empty;
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        yield return Fade(0f, 1f, 0.65f);
        yield return new WaitForSecondsRealtime(0.35f);

        if (CursedPhaseManager.IsPhase2)
        {
            ShowPhase2CompletionScreen();
            yield break;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowPhase2CompletionScreen()
    {
        if (completionVisible) return;
        completionVisible = true;
        completionCode = GenerateFourDigitCode();
        CursedPhaseManager.UnlockPhase3(completionCode);

        Texture2D completionTexture = Resources.Load<Texture2D>("CursedMod/Phase2Completion");
        if (completionTexture == null)
        {
            Debug.LogError("Phase 2 completion image could not be loaded.");
            QuitFromCompletionScreen();
            return;
        }

        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
            overlayCanvas = null;
            fadeImage = null;
            messageText = null;
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        CursedMobileInput.Hide();
        AudioListener.pause = true;
        Time.timeScale = 0f;

        GameObject canvasObject = new GameObject("Phase 2 Completion Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32766;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1672f, 941f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject screen = new GameObject("Tap To Quit", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(Button));
        screen.transform.SetParent(canvasObject.transform, false);
        RectTransform screenRect = screen.GetComponent<RectTransform>();
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.offsetMin = Vector2.zero;
        screenRect.offsetMax = Vector2.zero;

        RawImage background = screen.GetComponent<RawImage>();
        background.texture = completionTexture;
        background.color = Color.white;
        background.raycastTarget = true;

        Button quitButton = screen.GetComponent<Button>();
        quitButton.transition = Selectable.Transition.None;
        quitButton.targetGraphic = background;
        quitButton.onClick.AddListener(QuitFromCompletionScreen);

        GameObject codeObject = new GameObject("Random Four Digit Code", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
        codeObject.transform.SetParent(screen.transform, false);
        RectTransform codeRect = codeObject.GetComponent<RectTransform>();
        // Matches the empty green display in the generated 1672x941 background.
        codeRect.anchorMin = new Vector2(0.50f, 0.395f);
        codeRect.anchorMax = new Vector2(0.812f, 0.751f);
        codeRect.offsetMin = Vector2.zero;
        codeRect.offsetMax = Vector2.zero;

        Text codeText = codeObject.GetComponent<Text>();
        codeText.text = completionCode;
        codeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        codeText.fontSize = 132;
        codeText.fontStyle = FontStyle.Bold;
        codeText.alignment = TextAnchor.MiddleCenter;
        codeText.color = new Color(0.055f, 0.012f, 0.012f, 1f);
        codeText.resizeTextForBestFit = true;
        codeText.resizeTextMinSize = 72;
        codeText.resizeTextMaxSize = 140;
        codeText.raycastTarget = false;

        Outline outline = codeObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.48f, 0f, 0f, 0.92f);
        outline.effectDistance = new Vector2(3f, -3f);
        Debug.Log("Phase 2 completion code saved for Phase 3: " + codeText.text);
    }

    private static string GenerateFourDigitCode()
    {
        return UnityEngine.Random.Range(0, 10000).ToString("D4");
    }

    private void QuitFromCompletionScreen()
    {
        if (!completionVisible) return;
        completionVisible = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void QuitGameFromMaze()
    {
        if (instance != null) instance.QuitFromMaze();
    }
}

public class CursedMazeEndTrigger : MonoBehaviour
{
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        CursedFinalExitSequence.QuitGameFromMaze();
    }
}

public class CursedRoom99Portal : MonoBehaviour
{
    public Vector3 mazeSpawn;
    private bool entered;

    private void OnTriggerEnter(Collider other)
    {
        if (entered || !other.CompareTag("Player")) return;
        PlayerScript player = other.GetComponent<PlayerScript>();
        if (player == null) player = other.GetComponentInParent<PlayerScript>();
        if (player == null) return;

        entered = true;
        CharacterController controller = player.cc;
        if (controller != null) controller.enabled = false;
        player.height = mazeSpawn.y;
        player.transform.position = mazeSpawn;
        if (controller != null) controller.enabled = true;
    }
}
