using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public List<GameObject> spawnPoints;
    int waveNumber = 1;
    public int totalEnemiesSpawned = 0;
    public int currentEnemies = 0;
    
    public void SpawnEnemies()
    {
        int enemiesToSpawn = waveNumber * 4;
        
        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            shuffledIndices.Add(i);
        }
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (shuffledIndices.Count == 0)
            {
                for (int j = 0; j < spawnPoints.Count; j++)
                {
                    shuffledIndices.Add(j);
                }
            }
            
            int randomIndex = Random.Range(0, shuffledIndices.Count);
            int spawnPointIndex = shuffledIndices[randomIndex];
            shuffledIndices.RemoveAt(randomIndex);
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPoints[spawnPointIndex].transform.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().enemySpawner = this;
            currentEnemies++;
        }
        
        totalEnemiesSpawned = enemiesToSpawn;
        waveNumber++;
    }
}
