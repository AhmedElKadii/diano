using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
	public float range;
	public float damage;
	public float attackCooldown;
	public enum AttackMode { MODE_AUTOMATIC, MODE_SINGLE, MODE_BURST, MODE_MELEE }
	public AttackMode attackMode;
	private Animator animator;
	public GameObject muzzleFlash;
	public GameObject projectilePrefab;

	private float currentCooldown;

	private InputAction attackAction;
	private InputAction adsAction;

	private Transform playerCamera;

    void Start()
    {
       currentCooldown = attackCooldown; 
	   attackAction = InputSystem.actions.FindAction("Attack");
	   adsAction = InputSystem.actions.FindAction("ADS");
	   playerCamera = Camera.main.transform;
	   animator = GetComponent<Animator>();
    }

    void Update()
    {
		switch (attackMode)
		{
			case AttackMode.MODE_AUTOMATIC:
				if (attackAction.IsPressed())
				{
					if (currentCooldown <= 0f)
					{
						Shoot();
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

		currentCooldown -= Time.deltaTime;

		if (adsAction.IsPressed())
		{
			animator.SetBool("ADS", true);
		}
		else
		{
			animator.SetBool("ADS", false);
		}
    }

	void Shoot()
	{
		GameObject bullet = Instantiate(projectilePrefab, muzzleFlash.transform.position, muzzleFlash.transform.rotation);
		bullet.GetComponent<Bullet>().damage = damage;
		animator.Play(adsAction.IsPressed() ? "ADS_ATTACK" : "ATTACK", 0, 0f);
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
