using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public float sensitivity = 10f;
	public float walkSpeed = 4f;
	public float sprintSpeed = 10f;
	public float jumpHeight = 18f;
	public float groundDistance = 0.1f;

	public float crouchHeight;
	public float crouchCamHeight;
	public float crouchGroundCheckHeight;
	public float crouchSpeed = 5f;
	public float crouchSpeedMultiplier = 0.5f;
	float standingHeight;
	float standingCamHeight;
	float standingGrounCheckHeight;

	public float health;
	
	bool isCrouching;
	bool wantsToCrouch;
	Coroutine crouchCoroutine;

	public bool canMove = true;

	public Camera cam;
	public GameObject camHolder;
	public GameObject weaponHolder;
	public Transform groundCheck;
	public LayerMask groundMask;
	public LayerMask interactableMask;

	public float Height 
	{
		get => controller.height;
		set => controller.height = value;
	}

	const float JOYSTICK_DEADZONE = 0.125f;

	float xRotation = 0f;
	float yRotation = 0f;

	float speed;

	InputAction lookAction;
	InputAction moveAction;
	InputAction jumpAction;
	InputAction sprintAction;
	InputAction crouchAction;
	InputAction interactAction;

	bool isUsingGamepad;
	float lastInputTime;
	const float INPUT_SWITCH_DELAY = 0.1f;

	bool isSprinting;

	Vector3 velocity;

	CharacterController controller;

	const float GRAVITY = -9.81f * 0.005f;

    void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		controller = GetComponent<CharacterController>();
		standingHeight = Height;
		standingCamHeight = camHolder.transform.localPosition.y;
		standingGrounCheckHeight = groundCheck.transform.localPosition.y;
		InputInit();
    }

    void Update()
    {
		if (!canMove) return;

		DetectInputDevice();
		CameraLook();
		ApplyGravity();
		HandleSprint();
		MovePlayer();

		Vector2 input = moveAction.ReadValue<Vector2>();

		StartCoroutine(Interact());
	}

	void InputInit()
	{
		lookAction = InputSystem.actions.FindAction("Look");
		moveAction = InputSystem.actions.FindAction("Move");
		jumpAction = InputSystem.actions.FindAction("Jump");
		sprintAction = InputSystem.actions.FindAction("Sprint");
		crouchAction = InputSystem.actions.FindAction("Crouch");
		interactAction = InputSystem.actions.FindAction("Interact");
	}

	void DetectInputDevice()
	{
		bool gamepadInput = false;
		bool keyboardMouseInput = false;
		
		if (lookAction.IsPressed())
		{
			Vector2 lookInput = lookAction.ReadValue<Vector2>();
			if (lookInput.magnitude > JOYSTICK_DEADZONE)
			{
				if (lookAction.activeControl?.device is Gamepad)
					gamepadInput = true;
				else
					keyboardMouseInput = true;
			}
		}
		
		if (moveAction.IsPressed())
		{
			Vector2 moveInput = moveAction.ReadValue<Vector2>();
			if (moveInput.magnitude > JOYSTICK_DEADZONE)
			{
				if (moveAction.activeControl?.device is Gamepad)
					gamepadInput = true;
				else
					keyboardMouseInput = true;
			}
		}
		
		if (jumpAction.WasPressedThisFrame() || sprintAction.WasPressedThisFrame() || crouchAction.WasPressedThisFrame())
		{
			InputAction lastPressedAction = jumpAction.WasPressedThisFrame() ? jumpAction :
											sprintAction.WasPressedThisFrame() ? sprintAction : crouchAction;
			
			if (lastPressedAction.activeControl?.device is Gamepad)
				gamepadInput = true;
			else
				keyboardMouseInput = true;
		}
		if ((gamepadInput || keyboardMouseInput) && Time.time - lastInputTime > INPUT_SWITCH_DELAY)
		{
			if (gamepadInput && !keyboardMouseInput)
			{
				isUsingGamepad = true;
				lastInputTime = Time.time;
			}
			else if (keyboardMouseInput && !gamepadInput)
			{
				isUsingGamepad = false;
				lastInputTime = Time.time;
			}
		}
	}

	void CameraLook()
	{
		Vector2 input = lookAction.ReadValue<Vector2>() * sensitivity * (isUsingGamepad ? 25f * Time.deltaTime : 0.01f);

		xRotation -= input.y;
		yRotation += input.x;

		xRotation = Mathf.Clamp(xRotation, -75f, 75f);

		cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
	}

	void MovePlayer()
	{
		Vector2 input = moveAction.ReadValue<Vector2>();

		Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

		float baseSpeed = isSprinting ? sprintSpeed : walkSpeed;
		speed = isCrouching ? baseSpeed * crouchSpeedMultiplier : baseSpeed;
		controller.Move(moveDirection * speed * Time.deltaTime);

		if (isGrounded() && velocity.y < 0)
		{
			velocity.y = 0f;
		}

		if (jumpAction.WasPressedThisFrame() && isGrounded())
		{
			Jump();
		}

		controller.Move(velocity * Time.deltaTime);

		HandleCrouch();
	}

	IEnumerator Interact()
	{
		yield return new WaitForEndOfFrame();

		Ray ray = new Ray(cam.transform.position, cam.transform.forward);

		if (Physics.Raycast(ray, out RaycastHit hit, 2f, interactableMask))
		{
			Interactable interactable = hit.collider.GetComponent<Interactable>();
			if (interactable != null && interactAction.WasPressedThisFrame())
			{
				foreach (Transform child in weaponHolder.transform)
				{
					Destroy(child.gameObject);
				}
				interactable.Interact(weaponHolder.transform);
				interactable.gameObject.layer = LayerMask.NameToLayer("Weapon");
			}
		}
	}

	void HandleCrouch()
	{
		wantsToCrouch = crouchAction.IsPressed();

		if (crouchCoroutine != null) return;

		if (wantsToCrouch && !isCrouching)
		{
			crouchCoroutine = StartCoroutine(CrouchTransition(true));
		}
		else if (!wantsToCrouch && isCrouching && CanUncrouch())
		{
			crouchCoroutine = StartCoroutine(CrouchTransition(false));
		}
	}

	bool CanUncrouch()
	{
		Vector3 capsuleTop = transform.position + Vector3.up * standingHeight;
		float capsuleRadius = controller.radius;
		
		return !Physics.CheckCapsule(
			transform.position + Vector3.up * capsuleRadius,
			capsuleTop - Vector3.up * capsuleRadius,
			capsuleRadius,
			groundMask
		);
	}

	IEnumerator CrouchTransition(bool crouching)
	{
		
		float startHeight = Height;
		float targetHeight = crouching ? crouchHeight : standingHeight;
		
		float startCamY = camHolder.transform.localPosition.y;
		float targetCamY = crouching ? crouchCamHeight : standingCamHeight;
		
		float startGroundY = groundCheck.transform.localPosition.y;
		float targetGroundY = crouching ? crouchGroundCheckHeight : standingGrounCheckHeight;
		
		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos;
		
		if (!crouching)
		{
			float heightDifference = standingHeight - crouchHeight;
			targetPos.y += heightDifference * 0.5f;
		}
		
		float elapsedTime = 0f;
		float duration = 1f / crouchSpeed;
		
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;
			t = Mathf.SmoothStep(0f, 1f, t);
			
			Height = Mathf.Lerp(startHeight, targetHeight, t);
			
			Vector3 camPos = camHolder.transform.localPosition;
			camPos.y = Mathf.Lerp(startCamY, targetCamY, t);
			camHolder.transform.localPosition = camPos;
			
			Vector3 groundPos = groundCheck.transform.localPosition;
			groundPos.y = Mathf.Lerp(startGroundY, targetGroundY, t);
			groundCheck.transform.localPosition = groundPos;
			
			if (!crouching)
			{
				transform.position = Vector3.Lerp(startPos, targetPos, t);
			}
			
			yield return null;
		}
		
		Height = targetHeight;
		
		Vector3 finalCamPos = camHolder.transform.localPosition;
		finalCamPos.y = targetCamY;
		camHolder.transform.localPosition = finalCamPos;
		
		Vector3 finalGroundPos = groundCheck.transform.localPosition;
		finalGroundPos.y = targetGroundY;
		groundCheck.transform.localPosition = finalGroundPos;
		
		if (!crouching)
		{
			transform.position = targetPos;
		}
		
		isCrouching = crouching;
		crouchCoroutine = null;
	}

	void HandleSprint()
	{
		Vector2 input = moveAction.ReadValue<Vector2>();

		if (isUsingGamepad)
		{
			// Toggle Sprint for controller
			if (sprintAction.WasPerformedThisFrame() || sprintAction.IsPressed()) isSprinting = true;
			else if (input.magnitude < JOYSTICK_DEADZONE) isSprinting = false;
		}
		else
		{
			// Hold Sprint for keyboard
			if (sprintAction.IsPressed()) isSprinting = true;
			else isSprinting = false;
		}
	}

	public void TakeDamage(float amount)
	{
		health -= amount;
		if (health <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		Debug.Log("Player has died.");
	}

	void Jump() { velocity.y = Mathf.Sqrt(jumpHeight); }

	public bool isGrounded() { return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); }

	void ApplyGravity() { velocity.y += GRAVITY*(isGrounded() ? 0 : 1); }
}
