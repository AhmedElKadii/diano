using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Weapon : MonoBehaviour
{
	public float range;
	public float damage;
	public float attackCooldown;
	public enum AttackMode { MODE_AUTOMATIC, MODE_SEMI , MODE_SINGLE, MODE_BURST, MODE_MELEE }
	public AttackMode attackMode;
	private Animator animator;
	public GameObject muzzleFlash;
	public GameObject projectilePrefab;
	public int ammoCapacity;
	private int currentAmmo = 3;
	public float reloadTime;

	private float currentCooldown;
	private bool isReloading = false;

	private InputAction attackAction;
	private InputAction adsAction;
	private InputAction reloadAction;

	private Transform playerCamera;

	private static readonly int ReloadHash = Animator.StringToHash("RELOAD");
	private static readonly int ADSHash = Animator.StringToHash("ADS");
	private static readonly int ADSInHash = Animator.StringToHash("ADS_IN");
	private static readonly int ADSOutHash = Animator.StringToHash("ADS_OUT");
	private static readonly int ADSAttackHash = Animator.StringToHash("ADS_ATTACK");
	private static readonly int AttackHash = Animator.StringToHash("ATTACK");

	void Start()
	{
		currentCooldown = 0; 
		attackAction = InputSystem.actions.FindAction("Attack");
		adsAction = InputSystem.actions.FindAction("ADS");
		reloadAction = InputSystem.actions.FindAction("Reload");
		playerCamera = Camera.main.transform;
		animator = GetComponent<Animator>();
		currentAmmo = ammoCapacity;
	}

	void Update()
	{
		if (transform.parent.gameObject.name != "Hand") return;

		if (currentAmmo > 0 && !isReloading)
		{
			switch (attackMode)
			{
				case AttackMode.MODE_AUTOMATIC:
					if (attackAction.IsPressed())
					{
						if (currentCooldown <= 0f)
						{
							Shoot();
							currentAmmo--;
							currentCooldown = attackCooldown;
						}
					}
					break;
				case AttackMode.MODE_SEMI:
					if (attackAction.WasPressedThisFrame())
					{
						if (currentCooldown <= 0f)
						{
							Shoot();
							currentAmmo--;
							currentCooldown = attackCooldown;
						}
					}
					break;
				case AttackMode.MODE_SINGLE:
					if (attackAction.WasPressedThisFrame())
					{
						if (currentCooldown <= 0f)
						{
							Shoot();
							currentAmmo--;
							currentCooldown = attackCooldown;
						}
					}
					break;
				case AttackMode.MODE_BURST:
					if (attackAction.WasPressedThisFrame())
					{
						if (currentCooldown <= 0f)
						{
							Invoke("Shoot", 0.15f);
							Invoke("Shoot", 0.3f);
							Shoot();
							currentAmmo -= 3;
							currentCooldown = attackCooldown;
						}
					}
					break;
				case AttackMode.MODE_MELEE:
					if (attackAction.WasPressedThisFrame())
					{
						if (currentCooldown <= 0f)
						{
							Attack();
							currentCooldown = attackCooldown;
						}
					}
					break;
				default:
					break;
			}

			currentCooldown -= currentCooldown > 0 ? Time.deltaTime : 0f;
		}
		else if (currentAmmo <= 0 && !isReloading && !animator.GetBool(ADSHash) && attackAction.WasPressedThisFrame())
		{
			Reload();
		}

		if (reloadAction.WasPressedThisFrame() && currentAmmo < ammoCapacity && !isReloading)
		{
			Reload();
		}

		if (adsAction.WasPressedThisFrame())
		{
			if (attackMode == AttackMode.MODE_SINGLE) animator.CrossFade(ADSInHash, 0.5f, 0);
			animator.SetBool(ADSHash, true);
		}
		else if (adsAction.WasReleasedThisFrame())
		{
			if (attackMode == AttackMode.MODE_SINGLE) animator.CrossFade(ADSOutHash, 0.5f, 0);
			animator.SetBool(ADSHash, false);
		}

		if (animator.GetBool(ADSHash))
		{
			if (attackAction.IsPressed() && attackMode == AttackMode.MODE_AUTOMATIC)
			{
				animator.Play(ADSAttackHash, 0, 0.1f);
			}
			else if (attackAction.WasPressedThisFrame() && attackMode == AttackMode.MODE_SEMI)
			{
				animator.Play(ADSAttackHash, 0, 0.1f);
			}
			else if (attackAction.WasPressedThisFrame() && attackMode == AttackMode.MODE_SINGLE)
			{
				animator.Play(ADSAttackHash, 0, 0.2f);
			}
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

	void Shoot()
	{
		GameObject projectile = Instantiate(projectilePrefab, muzzleFlash.transform.position, Quaternion.identity);

		// super disgusting way but i'm short on time, should be done with interfaces or inheritance (OOP)
		var bullet = projectile.GetComponent<Bullet>();
		if (bullet != null) 
		{ 
			bullet.damage = damage; 

			if (animator.GetBool(ADSHash))
			{
				bullet.direction = playerCamera.forward;
			}
			else
			{
				Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

				Vector3 targetPoint;
				if (Physics.Raycast(ray, out RaycastHit hit))
				{
					targetPoint = hit.point;
				}
				else
				{
					targetPoint = ray.GetPoint(100f);
				}

				bullet.direction = (targetPoint - muzzleFlash.transform.position).normalized;
			}
			bullet.transform.rotation = Quaternion.LookRotation(bullet.direction);
		}

		var grenade = projectile.GetComponent<Grenade>();
		if (grenade != null) 
		{ 
			grenade.damage = damage; 
			grenade.direction = playerCamera.forward;
		}

		int animHash = adsAction.IsPressed() ? ADSAttackHash : AttackHash;

		if (attackMode == AttackMode.MODE_AUTOMATIC || attackMode == AttackMode.MODE_SEMI || attackMode == AttackMode.MODE_BURST)
		{
			animator.Play(animHash, 0, 0.1f);
		}
		else
		{
			animator.CrossFade(animHash, 0.2f, 0);
		}
	}

	void Attack()
	{
		Ray weaponRay = new Ray(playerCamera.position, playerCamera.forward);
		if (Physics.Raycast(weaponRay, out RaycastHit hitInfo, range))
		{
			if (hitInfo.collider.gameObject.TryGetComponent(out Entity entity))
			{
				entity.Hit(damage);
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		if (playerCamera != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(playerCamera.position, playerCamera.forward * range);
		}
	}
}
