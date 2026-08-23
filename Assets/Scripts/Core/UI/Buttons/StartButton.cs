using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
	public void StartGame()
	{
		PlayerPrefs.SetString("CurrentMode", "story");
		SceneManager.LoadSceneAsync(LoadScene);
	}
	public string LoadScene;
}
