using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

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

	public float dashSpeed = 20f;
	public float dashDuration = 0.2f;
	public float dashCooldown = 1f;
	public float dashStaminaCost = 25f;
	private bool isDashing = false;
	private bool canDash = false;
	private Vector3 dashDirection;

	private bool isDead = false;

	private Leaderboard leaderboard;

	public HUD hud;

	public int maxJumps = 1;
	private int jumpsRemaining;
	private float speedBoostMultiplier = 1f;

	public float maxStamina = 100f;
	public float currentStamina;
	public float staminaRegenRate = 10f;
	public float staminaRegenDelay = 2f;
	public float sprintStaminaCostPerSecond = 15f;
	private float lastStaminaUseTime;

	public float health;

	InputAction adsAction;
	bool isADS;

	public bool aimAssistEnabled = true;
	public float aimAssistStrength = 0.5f;
	public float aimAssistADSMultiplier = 1.5f;
	public float aimAssistRange = 50f;
	public float aimAssistAngle = 15f;
	public LayerMask enemyLayer;
	
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

	[HideInInspector]
	public float xRotation = 0f;

	[HideInInspector]
	public float yRotation = 0f;

	float speed;

	InputAction lookAction;
	InputAction moveAction;
	InputAction jumpAction;
	InputAction sprintAction;
	InputAction crouchAction;
	InputAction interactAction;
	InputAction dashAction;

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

		leaderboard = GameManager.Instance.GetComponent<Leaderboard>();

		hud = GetComponent<HUD>();

		hud.ShowHUD();

		sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

		health = 100f;
		controller = GetComponent<CharacterController>();
		standingHeight = Height;
		standingCamHeight = camHolder.transform.localPosition.y;
		standingGrounCheckHeight = groundCheck.transform.localPosition.y;

		InputInit();

		speedBoostMultiplier = GameManager.Instance.speedBoostMultiplier;
		canDash = GameManager.Instance.canDash;
		maxJumps = GameManager.Instance.maxJumps;
		aimAssistEnabled = PlayerPrefs.GetInt("AimAssist", 1) == 1;
		maxStamina = GameManager.Instance.maxStamina;
		health = GameManager.Instance.maxHealth;

		jumpsRemaining = maxJumps;
		currentStamina = maxStamina;
		
		if (enemyLayer == 0)
		{
			enemyLayer = LayerMask.GetMask("Enemy");
		}
    }

    void Update()
    {
		if (!canMove) return;

		hud.UpdateHealth(health, 100f);
		hud.UpdateStamina(currentStamina, maxStamina);
		hud.UpdateAmmo(weaponHolder.transform.childCount > 0 ? weaponHolder.transform.GetChild(0).GetComponent<Weapon>().currentAmmo : 0,
			weaponHolder.transform.childCount > 0 ? weaponHolder.transform.GetChild(0).GetComponent<Weapon>().ammoCapacity : 0);

		DetectInputDevice();
		CameraLook();
		ApplyGravity();
		HandleSprint();
		HandleDash();
		MovePlayer();
		RegenerateStamina();

		Vector2 input = moveAction.ReadValue<Vector2>();

		if (adsAction.WasPressedThisFrame())
		{
			hud.ToggleCrosshair(false);
		}
		else if (adsAction.WasReleasedThisFrame())
		{
			hud.ToggleCrosshair(true);
		}

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
		adsAction = InputSystem.actions.FindAction("ADS");
		dashAction = InputSystem.actions.FindAction("Dash");
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
		
		if (jumpAction.WasPressedThisFrame() || sprintAction.WasPressedThisFrame() || crouchAction.WasPressedThisFrame() || adsAction.WasPressedThisFrame() || dashAction.WasPressedThisFrame())
		{
			InputAction lastPressedAction = jumpAction.WasPressedThisFrame() ? jumpAction :
				sprintAction.WasPressedThisFrame() ? sprintAction : 
				crouchAction.WasPressedThisFrame() ? crouchAction : 
				dashAction.WasPressedThisFrame() ? dashAction : adsAction;

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

		if (aimAssistEnabled && isUsingGamepad && weaponHolder.transform.childCount > 0)
		{
			Vector2 aimAssist = GetAimAssist();
			input += aimAssist;
		}

		xRotation -= input.y;
		yRotation += input.x;

		xRotation = Mathf.Clamp(xRotation, -75f, 75f);

		cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
	}

	Vector2 GetAimAssist()
	{
		Transform closestEnemy = FindClosestEnemyInView();

		if (closestEnemy == null)
			return Vector2.zero;

		Vector3 targetPoint = closestEnemy.position + Vector3.up * 1.5f;
		Vector3 directionToTarget = (targetPoint - cam.transform.position).normalized;

		float angleToEnemy = Vector3.Angle(cam.transform.forward, directionToTarget);

		if (angleToEnemy > aimAssistAngle)
			return Vector2.zero;

		Vector3 localTarget = cam.transform.InverseTransformDirection(directionToTarget);

		float horizontalAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
		float verticalAngle = Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;

		float falloff = 1f - (angleToEnemy / aimAssistAngle);
		falloff = Mathf.SmoothStep(0f, 1f, falloff);

		float horizontalPull = horizontalAngle * aimAssistStrength * falloff;
		float verticalPull = verticalAngle * aimAssistStrength * falloff;

		return new Vector2(horizontalPull, verticalPull);
	}

	Transform FindClosestEnemyInView()
	{
		Collider[] enemiesInRange = Physics.OverlapSphere(cam.transform.position, aimAssistRange, enemyLayer);

		Transform closestEnemy = null;
		float closestAngle = aimAssistAngle;

		foreach (Collider enemyCollider in enemiesInRange)
		{
			if (!enemyCollider.CompareTag("Ghost")) continue;

			Vector3 directionToEnemy = (enemyCollider.transform.position - cam.transform.position).normalized;
			float angleToEnemy = Vector3.Angle(cam.transform.forward, directionToEnemy);

			if (angleToEnemy < closestAngle)
			{
				if (Physics.Raycast(cam.transform.position, directionToEnemy, out RaycastHit hit, aimAssistRange))
				{
					if (hit.collider == enemyCollider)
					{
						closestEnemy = enemyCollider.transform;
						closestAngle = angleToEnemy;
					}
				}
			}
		}

		return closestEnemy;
	}

	void HandleDash()
	{
		if (dashAction.WasPressedThisFrame() && canDash && !isDashing && currentStamina >= dashStaminaCost)
		{
			Vector2 input = moveAction.ReadValue<Vector2>();
			
			if (input.magnitude > JOYSTICK_DEADZONE)
			{
				dashDirection = (transform.right * input.x + transform.forward * input.y).normalized;
			}
			else
			{
				dashDirection = transform.forward;
			}
			
			UseStamina(dashStaminaCost);
			StartCoroutine(PerformDash());
		}
	}

	IEnumerator PerformDash()
	{
		isDashing = true;
		canDash = false;
		
		float dashTimer = 0f;
		
		float originalYVelocity = velocity.y;
		
		while (dashTimer < dashDuration)
		{
			controller.Move(dashDirection * dashSpeed * Time.deltaTime);
			
			dashTimer += Time.deltaTime;
			yield return null;
		}
		
		isDashing = false;
		
		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	void MovePlayer()
	{
		if (isDashing)
		{
			controller.Move(velocity * Time.deltaTime);
			return;
		}

		Vector2 input = moveAction.ReadValue<Vector2>();

		Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

		float baseSpeed = isSprinting ? sprintSpeed : walkSpeed;
		speed = isCrouching ? baseSpeed * crouchSpeedMultiplier : baseSpeed * speedBoostMultiplier;
		controller.Move(moveDirection * speed * Time.deltaTime);

		if (isGrounded() && velocity.y < 0)
		{
			velocity.y = 0f;
			jumpsRemaining = maxJumps;
		}

		if (jumpAction.WasPressedThisFrame() && jumpsRemaining > 0)
		{
			Jump();
			jumpsRemaining--;
		}

		controller.Move(velocity * Time.deltaTime);

		HandleCrouch();
	}

	IEnumerator Interact()
	{
		yield return new WaitForEndOfFrame();

		Ray ray = new Ray(cam.transform.position, cam.transform.forward);

		if (Physics.Raycast(ray, out RaycastHit hit, 10f, interactableMask))
		{
			Interactable interactable = hit.collider.GetComponent<Interactable>();
			if (interactable != null && interactAction.WasPressedThisFrame())
			{
				Weapon weapon = interactable.GetComponent<Weapon>();
				if (weapon != null) 
				{
					foreach (Transform child in weaponHolder.transform)
					{
						Destroy(child.gameObject);
					}
					interactable.gameObject.layer = LayerMask.NameToLayer("Weapon");
					weapon.playerController = this;
				}

				interactable.Interact(weaponHolder.transform);
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
			if ((sprintAction.WasPerformedThisFrame() || sprintAction.IsPressed()) && currentStamina > 0f)
			{
				isSprinting = true;
			}
			else if (input.magnitude < JOYSTICK_DEADZONE || currentStamina <= 0f)
			{
				isSprinting = false;
			}
		}
		else
		{
			if (sprintAction.IsPressed() && currentStamina > 0f)
			{
				isSprinting = true;
			}
			else
			{
				isSprinting = false;
			}
		}

	if (isSprinting) UseStamina(sprintStaminaCostPerSecond * Time.deltaTime);
	}

	void UseStamina(float amount)
	{
		currentStamina = Mathf.Max(0f, currentStamina - amount);
		lastStaminaUseTime = Time.time;
	}

	void RegenerateStamina()
	{
		if (Time.time - lastStaminaUseTime >= staminaRegenDelay && currentStamina < maxStamina)
		{
			currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
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
		if (isDead) return;

		canMove = false;
		isDead = true;
		GameManager.Instance.gameStarted = false;
		GameManager.Instance.gameOver = true;

		Debug.LogError("Player has died.");
		hud.HideHUD();

		weaponHolder.SetActive(false);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		hud.ShowInputContainer();
	}

	public IEnumerator TransitionScenes()
	{
		yield return new WaitForEndOfFrame();
		SceneManager.LoadScene(1);
	}

	void Jump() { velocity.y = Mathf.Sqrt(jumpHeight); }

	public bool isGrounded() { return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); }

	void ApplyGravity() { velocity.y += GRAVITY*(isGrounded() ? 0 : 1); }
}
