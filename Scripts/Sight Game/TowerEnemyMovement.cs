using UnityEngine;

[RequireComponent(typeof(TowerEnemy))]
public class TowerEnemyMovement : MonoBehaviour
{
	private Transform target;
	private int waypointIndex = 0;

	private TowerEnemy enemy;
    public Transform scarab;
    private Camera playerCamera;

	void Start()
	{
		enemy = GetComponent<TowerEnemy>();
		target = TowerWaypoints.points[0];
        playerCamera =  GameObject.Find("Player Camera").GetComponent<Camera>();
    }

	void Update()
	{
		Vector3 dir = target.position - transform.position;
		transform.Translate(dir.normalized * enemy.speed * Time.deltaTime, Space.World);
        scarab.LookAt(target.position);

        if (Vector3.Distance(transform.position, target.position) <= 0.4f)
		{
			GetNextWaypoint();
		}
	}

	void GetNextWaypoint()
	{
		if (waypointIndex >= TowerWaypoints.points.Length - 1)
		{
			EndPath();
			return;
		}

		waypointIndex++;
		target = TowerWaypoints.points[waypointIndex];
	}

	void EndPath()
	{
        playerCamera.GetComponent<Sound>().DecreaseSound();
        playerCamera.GetComponent<Sight>().DecreaseSight();
        WaveSpawner.EnemiesAlive--;
		Destroy(gameObject);
	}
}
