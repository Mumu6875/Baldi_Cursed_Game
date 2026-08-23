using UnityEngine;

public class EndlessNotebookScript : MonoBehaviour
{
	private void Awake()
	{
		// Kept only so old serialized scenes do not report a Missing Script.
		// Endless-only notebooks are disabled in the story-only build.
		gameObject.SetActive(false);
	}
	public float openingDistance;
	public GameControllerScript gc;
	public Transform player;
	public GameObject learningGame;
}
