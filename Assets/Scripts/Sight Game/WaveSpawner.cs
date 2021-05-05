using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
	public static int EnemiesAlive = 0;

	public GameObject enemySimple;

	public GameObject enemyFast;

	public GameObject enemyTough;

	public Transform spawnPoint;

	public Text waveCountdownText;

	public float timeBetweenWaves = 5f;

	public float enemyDelay = 0.5f;

	private float countdown = 2f;

	private int waveIndex = 1;

	void Update()
	{
		if (EnemiesAlive > 0)
		{
			return;
		}

		if (countdown <= 0f)
		{
			StartCoroutine(SpawnWave());
			countdown = timeBetweenWaves;
			return;
		}

		countdown -= Time.deltaTime;
		waveCountdownText.text = Mathf.Round(countdown).ToString();
		
	}

	IEnumerator SpawnWave()
	{

		for (int i = 0; i < waveIndex; i++)
		{
			EnemiesAlive++;

			if (waveIndex % 10 == 0)
			{
				SpawnEnemy(enemyTough);
			}
			else if (waveIndex % 5 == 0)
			{
				SpawnEnemy(enemyFast);
			}
			else
			{
				SpawnEnemy(enemySimple);
			}
			
			yield return new WaitForSeconds(enemyDelay);
		}

		waveIndex++; 
	}

	void SpawnEnemy(GameObject enemy)
	{
		GameObject enemyInstance = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
		enemyInstance.transform.parent = transform;
	}
}
