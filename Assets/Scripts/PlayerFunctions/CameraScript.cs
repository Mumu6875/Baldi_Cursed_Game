using UnityEngine;

public class CameraScript : MonoBehaviour
{
	private void Start()
	{
		offset = transform.position - player.transform.position; //Defines the offset
	}
	private void Update()
	{
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.LookBehind))
		{
			lookBehind = 180; //Look behind you
		}
		else
		{
			lookBehind = 0; //Don't look behind you
		}
	}
	private void LateUpdate()
	{
		transform.position = player.transform.position + offset; //Teleport to the player, then move based on the offset vector(if all other statements fail)
		if (!ps.gameOver)
		{
			transform.position = player.transform.position + offset; //Teleport to the player, then move based on the offset vector
			transform.rotation = player.transform.rotation * Quaternion.Euler(0f, (float)lookBehind, 0f); //Rotate based on player direction + lookbehind
		}
		else if (ps.gameOver)
		{
			transform.position = baldi.transform.position + baldi.transform.forward * BaldiOffset.z + new Vector3(0f, BaldiOffset.y, 0f);//Puts the camera in front of Baldi
			transform.LookAt(new Vector3(baldi.position.x, baldi.position.y + BaldiOffset.y, baldi.position.z));//Makes the player look at baldi with an offset so the camera doesn't look at the feet
		}
	}
	public GameObject player;
	public PlayerScript ps;
	public Transform baldi;
	public Vector3 BaldiOffset;
	private int lookBehind;
	public Vector3 offset;
}
