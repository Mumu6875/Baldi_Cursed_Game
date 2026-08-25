using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.GetInt("AnalogMove") == 1)
		{
			sensitivityActive = true;
		}
		height = transform.position.y;
		stamina = maxStamina;
		playerRotation = transform.rotation;
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
		flipaturn = 1f;
	}
	private void Update()
	{
		transform.position = new Vector3(transform.position.x, height, transform.position.z);
		MouseMove();
		PlayerMove();
		StaminaCheck();
		GuiltCheck();
		if (cc.velocity.magnitude > 0f)
		{
			gc.LockMouse();
		}
	}
	private void MouseMove()
	{
		playerRotation.eulerAngles = new Vector3(playerRotation.eulerAngles.x, playerRotation.eulerAngles.y, fliparoo);
		// Android can emulate Mouse X from the same finger that drives the custom
		// look pad. Never add both sources or a single swipe is applied twice.
		float horizontalLook = CursedMobileInput.IsActive
			? CursedMobileInput.ConsumeLookDeltaX()
			: Input.GetAxis("Mouse X") * mouseSensitivity * Time.timeScale;
		playerRotation.eulerAngles = playerRotation.eulerAngles + Vector3.up * horizontalLook * flipaturn;
		transform.rotation = playerRotation;
	}
	private void PlayerMove()
	{
		Vector3 movement = Vector3.zero;
		Vector3 lateralMovement = Vector3.zero;
		bool mobileMovement = CursedMobileInput.IsActive;
		float inputMagnitude;
		if (mobileMovement)
		{
			Vector2 mobileInput = CursedMobileInput.GetMoveVector();
			movement = transform.forward * mobileInput.y;
			lateralMovement = transform.right * mobileInput.x;
			inputMagnitude = Mathf.Clamp01(mobileInput.magnitude);
		}
		else
		{
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveForward)) movement = transform.forward;
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveBackward)) movement = -transform.forward;
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveLeft)) lateralMovement = -transform.right;
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveRight)) lateralMovement = transform.right;
			inputMagnitude = Mathf.Clamp01((movement + lateralMovement).magnitude);
		}
		if (stamina > 0f)
		{
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Run))
			{
				playerSpeed = runSpeed;
				sensitivity = mobileMovement ? inputMagnitude : 1f;
				if (cc.velocity.magnitude > 0.1f)
				{
					ResetGuilt("running", 0.1f);
				}
			}
			else
			{
				playerSpeed = walkSpeed;
				if (mobileMovement || sensitivityActive)
				{
					sensitivity = inputMagnitude;
				}
				else
				{
					sensitivity = 1f;
				}
			}
		}
		else
		{
			playerSpeed = walkSpeed;
			if (mobileMovement || sensitivityActive)
			{
				sensitivity = inputMagnitude;
			}
			else
			{
				sensitivity = 1f;
			}
		}
		playerSpeed *= Time.deltaTime;
		moveDirection = (movement + lateralMovement).normalized * playerSpeed * sensitivity;
		cc.Move(moveDirection);
	}
	private void StaminaCheck()
	{
		if (cc.velocity.magnitude > 0.1f)
		{
			if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Run) & stamina > 0f)
			{
				stamina -= staminaRate * Time.deltaTime;
			}
			if (stamina < 0f & stamina > -5f)
			{
				stamina = -5f;
			}
		}
		else if (stamina < maxStamina)
		{
			stamina += staminaRate * Time.deltaTime;
		}
		staminaBar.value = stamina / maxStamina * 100f;
	}
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.name == "Baldi" & !gc.debugMode)
		{
			gameOver = true;
			RenderSettings.skybox = blackSky; //Sets the skybox black
			StartCoroutine(KeepTheHudOff()); //Hides the Hud
		}
	}
	public IEnumerator KeepTheHudOff()
	{
		while (gameOver)
		{
			hud.enabled = false;
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.transform.name == "Office Trigger")
		{
			ResetGuilt("escape", door.lockTime);
		}
	}
	public void ResetGuilt(string type, float amount)
	{
		if (amount >= guilt)
		{
			guilt = amount;
			guiltType = type;
		}
	}
	private void GuiltCheck()
	{
		if (guilt > 0f)
		{
			guilt -= Time.deltaTime;
		}
	}
	public void ActivateBoots()
	{
		bootsActive = true;
		StartCoroutine(BootTimer());
	}
	private IEnumerator BootTimer()
	{
		float time = 15f;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		bootsActive = false;
		yield break;
	}
	public GameControllerScript gc;
	public BaldiScript baldi;
	public DoorScript door;
	public bool gameOver;
	public bool bootsActive;
	public float fliparoo;
	public float flipaturn;
	private Quaternion playerRotation;
	private bool sensitivityActive;
	private float sensitivity;
	public float mouseSensitivity;
	public float walkSpeed;
	public float runSpeed;
	public float slowSpeed;
	public float maxStamina;
	public float staminaRate;
	public float guilt;
	public float initGuilt;
	private Vector3 moveDirection;
	private float playerSpeed;
	public float stamina;
	public CharacterController cc;
	public Slider staminaBar;
	public float db;
	public string guiltType;
	public float height;
	public Material blackSky;
	public Canvas hud;
}
