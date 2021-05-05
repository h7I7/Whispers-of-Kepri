using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
	public Transform[] boulderSpawnPoints;
	public GameObject boulderPrefab;

	public float spawnTime = 4.0f;
	private float currentSpawnTime;

	void Start()
	{
		currentSpawnTime = spawnTime;
	}

	void Update()
	{
		currentSpawnTime -= Time.deltaTime;

		if (currentSpawnTime <=0)
		{
			Spawn();
			currentSpawnTime = spawnTime;
		}
	}

	void Spawn()
	{
		int randToNotSpawn = Random.Range(0, boulderSpawnPoints.Length);

		for (int i = 0; i < boulderSpawnPoints.Length; i++)
		{
			if (i != randToNotSpawn)
			{
				GameObject boulder = Instantiate(boulderPrefab, boulderSpawnPoints[i].position, Quaternion.identity);
				boulder.transform.parent = transform;
			}
		}
	}
}
