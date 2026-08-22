using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
	public void StartGame()
	{
		if (currentMode == StartButton.Mode.Story)
		{
			PlayerPrefs.SetString("CurrentMode", "story");
		}
		else
		{
			PlayerPrefs.SetString("CurrentMode", "endless");
		}
		SceneManager.LoadSceneAsync(LoadScene);
	}
	public StartButton.Mode currentMode;
	public enum Mode
	{
		Story,
		Endless
	}
	public string LoadScene;
}
