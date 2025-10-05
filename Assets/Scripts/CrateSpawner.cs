using UnityEngine;
using System.Collections.Generic;

public class CrateSpawner : MonoBehaviour
{
	public List<GameObject> cratePrefabs;

    void Start()
    {
		SpawnCrate();
    }

	void SpawnCrate()
	{
		if (cratePrefabs.Count == 0)
		{
			Debug.LogWarning("No crate prefabs assigned to the CrateSpawner.");
			return;
		}

		int randomIndex = Random.Range(0, cratePrefabs.Count);
		GameObject selectedCrate = cratePrefabs[randomIndex];

		GameObject crate = Instantiate(selectedCrate, transform.position, Quaternion.identity);
		
		crate.transform.SetParent(transform);
		crate.transform.localPosition = Vector3.zero;
		crate.transform.localRotation = Quaternion.identity;
		crate.transform.localScale = Vector3.one * 3f;
	}
}
