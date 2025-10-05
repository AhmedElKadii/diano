using UnityEngine;

public class Referencer : MonoBehaviour
{
	public GameObject player;
	public GameObject enemySpawner;

    void Start()
    {
		if (GameManager.Instance == null)
		{
			Debug.LogError("GameManager instance not found!");
			return;
		}

		if (player != null) GameManager.Instance.player = player;
		if (enemySpawner != null) GameManager.Instance.enemySpawner = enemySpawner.GetComponent<EnemySpawner>();
    }
}
