using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject coinPrefab;
	public GameObject enemyPrefab;

	[Header("Spawn Points")]
	public Transform[] coinSpawnPoints;
	public Transform[] enemySpawnPoints;

	private List<GameObject> spawnedCoins = new List<GameObject>();
	private List<GameObject> spawnedEnemies = new List<GameObject>();

	public void ResetEnvironment()
	{
		// Clear old
		foreach (var coin in spawnedCoins)
			if (coin != null) Destroy(coin);
		spawnedCoins.Clear();

		foreach (var enemy in spawnedEnemies)
			if (enemy != null) Destroy(enemy);
		spawnedEnemies.Clear();

		// Respawn coins
		foreach (var point in coinSpawnPoints)
		{
			var coin = Instantiate(coinPrefab, point.position, Quaternion.identity);
			spawnedCoins.Add(coin);
		}

		// Respawn enemies
		foreach (var point in enemySpawnPoints)
		{
			var enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
			spawnedEnemies.Add(enemy);
		}
	}
}
