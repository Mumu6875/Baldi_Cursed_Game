using TMPro;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
	private void Awake()
	{
		// Kept as a compatibility shell for scenes serialized with this component.
		// Endless mode has been removed, so its score panel must never appear.
		if (scoreText != null) scoreText.SetActive(false);
		enabled = false;
	}
	public GameObject scoreText;
	public TMP_Text text;
}
