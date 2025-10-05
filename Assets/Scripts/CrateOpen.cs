using UnityEngine;
using System.Collections.Generic;

public class CrateOpen : MonoBehaviour
{
	public List<GameObject> possibleItems;
	public Transform itemSpawnPoint;

	private GameObject weapon;

	public void OpenCrate()
	{
		if (weapon != null) return;

		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.Play("CRATE_OPEN", 0, 0f);
		}
		if (possibleItems.Count > 0 && itemSpawnPoint != null)
		{
			Debug.Log("Spawning loot from crate");
			int randomIndex = Random.Range(0, possibleItems.Count);
			GameObject loot = Instantiate(possibleItems[randomIndex], itemSpawnPoint.position, itemSpawnPoint.rotation, itemSpawnPoint);
			loot.GetComponent<Interactable>().enabled = false;
			loot.transform.localPosition = Vector3.zero;
			loot.transform.localRotation = Quaternion.identity;
			weapon = loot;
		}

		GetComponent<Interactable>().enabled = false;
	}
}
