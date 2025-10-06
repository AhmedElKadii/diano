using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Weapon : MonoBehaviour
{
	[Header("Weapon Stats")]
	public float range = 100f;
	public float damage = 10f;
	public float attackCooldown = 0.1f;
	public string weaponName = "Weapon";
	public LayerMask enemyMask;
	
	[Header("Attack Configuration")]
	public AttackMode attackMode;
	public enum AttackMode { MODE_AUTOMATIC, MODE_SEMI, MODE_SINGLE, MODE_BURST, MODE_MELEE }
	
	[Header("Ammo")]
	public int ammoCapacity = 30;
	public int currentAmmo = 30;
	public float reloadTime = 2f;
	
	[Header("Visuals")]
	public GameObject muzzleFlash;
	public GameObject bulletTracerPrefab;  // Visual tracer only
	public GameObject projectilePrefab;    // For grenades
	public float tracerSpeed = 300f;       // How fast the visual tracer moves
	
	[Header("References")]
	public PlayerController playerController;
	
	[HideInInspector] public bool ads = false;

	private Animator animator;
	private float currentCooldown;
	private bool isReloading = false;
	private Transform playerCamera;
	
	// Debug info for last shot
	private Vector3 lastShotOrigin;
	private Vector3 lastShotEnd;
	private bool hasShot = false;

	private InputAction attackAction;
	private InputAction adsAction;
	private InputAction reloadAction;

	// Cached animator hashes
	private static readonly int ReloadHash = Animator.StringToHash("RELOAD");
	private static readonly int ADSHash = Animator.StringToHash("ADS");
	private static readonly int ADSInHash = Animator.StringToHash("ADS_IN");
	private static readonly int ADSOutHash = Animator.StringToHash("ADS_OUT");
	private static readonly int ADSAttackHash = Animator.StringToHash("ADS_ATTACK");
	private static readonly int AttackHash = Animator.StringToHash("ATTACK");

	void Start()
	{
		currentCooldown = 0;
		animator = GetComponent<Animator>();
		playerCamera = Camera.main.transform;
		currentAmmo = ammoCapacity;
		
		attackAction = InputSystem.actions.FindAction("Attack");
		adsAction = InputSystem.actions.FindAction("ADS");
		reloadAction = InputSystem.actions.FindAction("Reload");
	}

	void Update()
	{
		if (!CanOperate()) return;

		HandleShooting();
		HandleReload();
		HandleADS();
		
		UpdateCooldown();
	}

	bool CanOperate()
	{
		return playerController != null 
			&& playerController.canMove 
			&& transform.parent != null 
			&& transform.parent.gameObject.name == "Weapon Holder";
	}

	void HandleShooting()
	{
		if (currentAmmo <= 0 || isReloading) 
		{
			if (currentAmmo <= 0 && !isReloading && !animator.GetBool(ADSHash) && attackAction.WasPressedThisFrame())
			{
				Reload();
			}
			return;
		}

		if (currentCooldown > 0f) return;

		switch (attackMode)
		{
			case AttackMode.MODE_AUTOMATIC:
				if (attackAction.IsPressed())
				{
					Shoot();
					currentAmmo--;
					currentCooldown = attackCooldown;
				}
				break;
				
			case AttackMode.MODE_SEMI:
			case AttackMode.MODE_SINGLE:
				if (attackAction.WasPressedThisFrame())
				{
					Shoot();
					currentAmmo--;
					currentCooldown = attackCooldown;
				}
				break;
				
			case AttackMode.MODE_BURST:
				if (attackAction.WasPressedThisFrame())
				{
					StartCoroutine(BurstFire());
					currentCooldown = attackCooldown;
				}
				break;
				
			case AttackMode.MODE_MELEE:
				if (attackAction.WasPressedThisFrame())
				{
					MeleeAttack();
					currentCooldown = attackCooldown;
				}
				break;
		}
	}

	void HandleReload()
	{
		if (reloadAction.WasPressedThisFrame() && currentAmmo < ammoCapacity && !isReloading)
		{
			Reload();
		}
	}

	void HandleADS()
	{
		if (adsAction.WasPressedThisFrame())
		{
			ads = true;
			if (attackMode == AttackMode.MODE_SINGLE)
			{
				animator.CrossFade(ADSInHash, 0.5f, 0);
			}
		}
		else if (adsAction.WasReleasedThisFrame())
		{
			ads = false;
			if (attackMode == AttackMode.MODE_SINGLE)
			{
				animator.CrossFade(ADSOutHash, 0.5f, 0);
			}
		}

		animator.SetBool(ADSHash, ads);
	}

	void UpdateCooldown()
	{
		if (currentCooldown > 0f)
		{
			currentCooldown -= Time.deltaTime;
		}
	}

	IEnumerator BurstFire()
	{
		int shotsToFire = Mathf.Min(3, currentAmmo);
		
		for (int i = 0; i < shotsToFire; i++)
		{
			Shoot();
			currentAmmo--;
			
			if (i < shotsToFire - 1)
			{
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	void Shoot()
	{
		// Check if this is a grenade launcher
		if (projectilePrefab != null && projectilePrefab.GetComponent<Grenade>() != null)
		{
			ShootGrenade();
		}
		else
		{
			ShootHitscan();
		}
		
		PlayShootAnimation();
	}

	void ShootHitscan()
	{
		Vector3 shootDirection = playerCamera.transform.forward;
		Vector3 shootOrigin = playerCamera.position;

		RaycastHit hit;
		Vector3 hitPoint;
		bool didHit = Physics.Raycast(shootOrigin, shootDirection, out hit, range, enemyMask);
		
		if (didHit)
		{
			hitPoint = hit.point;

			hit.collider.GetComponent<Entity>().Hit(damage);

			Debug.Log(hit.collider.gameObject.name);
		}
		else
		{
			hitPoint = shootOrigin + shootDirection * range;
		}

		if (bulletTracerPrefab != null && muzzleFlash != null)
		{
			SpawnBulletTracer(muzzleFlash.transform.position, hitPoint);
		}
	}

	void ShootGrenade()
	{
		if (projectilePrefab == null || muzzleFlash == null) return;

		GameObject projectile = Instantiate(projectilePrefab, muzzleFlash.transform.position, Quaternion.identity);
		
		Grenade grenade = projectile.GetComponent<Grenade>();
		if (grenade != null)
		{
			grenade.damage = damage;
			grenade.direction = playerCamera.forward;
		}
	}

	void SpawnBulletTracer(Vector3 startPoint, Vector3 endPoint)
	{
		GameObject tracer = Instantiate(bulletTracerPrefab, startPoint, Quaternion.LookRotation(endPoint - startPoint));
		
		// If the tracer has a Bullet component, use it for visual movement
		Bullet bulletVisual = tracer.GetComponent<Bullet>();
		if (bulletVisual != null)
		{
			bulletVisual.direction = (endPoint - startPoint).normalized;
			bulletVisual.damage = 0; // No damage, purely visual
			
			// Destroy after it would reach the target
			float distance = Vector3.Distance(startPoint, endPoint);
			float lifetime = distance / tracerSpeed;
			Destroy(tracer, lifetime + 0.1f);
		}
		else
		{
			// Fallback: just destroy after a short time
			Destroy(tracer, 0.5f);
		}
	}

	void MeleeAttack()
	{
		Ray weaponRay = new Ray(playerCamera.position, playerCamera.forward);
		
		if (Physics.Raycast(weaponRay, out RaycastHit hitInfo, range))
		{
			if (hitInfo.collider.TryGetComponent(out Entity entity))
			{
				entity.Hit(damage);
			}
		}
		
		PlayShootAnimation();
	}

	void PlayShootAnimation()
	{
		int animHash = ads ? ADSAttackHash : AttackHash;

		if (attackMode == AttackMode.MODE_AUTOMATIC || 
			attackMode == AttackMode.MODE_SEMI || 
			attackMode == AttackMode.MODE_BURST)
		{
			animator.Play(animHash, 0, 0);
		}
		else
		{
			animator.CrossFade(animHash, 0.2f, 0);
		}
	}

	void Reload()
	{
		if (isReloading) return;

		isReloading = true;
		currentCooldown = 0;
		animator.CrossFade(ReloadHash, 0.5f, 0);
		StartCoroutine(ReloadCoroutine());
	}

	IEnumerator ReloadCoroutine()
	{
		yield return new WaitForSeconds(reloadTime);
		currentAmmo = ammoCapacity;
		isReloading = false;
	}

	public void Interacted(Transform initiator)
	{
		transform.parent = initiator;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		GameManager.Instance.currentWeapon = weaponName;
		Debug.Log("Interacted with " + weaponName);
	}

	void OnDrawGizmosSelected()
	{
		if (playerCamera != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(playerCamera.position, playerCamera.forward * range);
		}

		// Draw the last shot trajectory
		if (hasShot)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(lastShotOrigin, lastShotEnd);
			Gizmos.DrawSphere(lastShotEnd, 0.1f);
		}
	}
}
