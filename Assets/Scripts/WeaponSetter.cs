using UnityEngine;

public class WeaponSetter : MonoBehaviour
{
	public GameObject pistolPrefab;
	public GameObject launcherPrefab;
	public GameObject smgPrefab;
	public PlayerController playerController;

    void Start()
    {
		GameObject weapon = null;

		if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);
		
		switch (GameManager.Instance.currentWeapon)
		{
			case "Pistol":
				weapon = Instantiate(pistolPrefab, transform);
				break;
			case "Launcher":
				weapon = Instantiate(launcherPrefab, transform);
				break;
			case "SMG":
				weapon = Instantiate(smgPrefab, transform);
				break;
			default:
				Debug.LogWarning("Unknown weapon type: " + GameManager.Instance.currentWeapon);
				break;
		}

		if (weapon != null)
		{
			weapon.transform.SetParent(transform);
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
			weapon.GetComponent<Weapon>().playerController = playerController;
		}
    }
}
