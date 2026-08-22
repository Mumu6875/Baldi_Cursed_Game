using TMPro;
using UnityEngine;

public class EndlessTextScript : MonoBehaviour
{
	private void Start()
	{
		text.text = string.Concat(new object[]
		{
			text.text,
			"\nHigh Score: ",
			PlayerPrefs.GetInt("HighBooks"),
			" Notebooks"
		});
	}
	public TMP_Text text;
}
