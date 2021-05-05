using UnityEngine;

public class TurretBuildManager : MonoBehaviour
{
	public static TurretBuildManager instance;

	public GameObject buildEffect;

	void Awake()
	{
		if (instance != null)
		{
			Debug.LogError("More than one TurretBuildManager in scene!");
			return;
		}
		instance = this;
	}

	private TurretBluprint turretToBuild;

	public void SetTurretToBuild(TurretBluprint turret)
	{
		turretToBuild = turret;
	}

	public TurretBluprint GetTurretToBuild()
	{
		return turretToBuild;
	}

	public bool BuildTurretOn(TurretNode node)
	{
		if (PlayerStats.Money < turretToBuild.cost)
		{
			//Not enough money.
			return false;
		}

		PlayerStats.Money -= turretToBuild.cost;

		GameObject turret = (GameObject)Instantiate(turretToBuild.prefab, node.GetBuildPosition(), Quaternion.identity);
		node.turret = turret;

		GameObject effect = Instantiate(buildEffect, node.GetBuildPosition(), Quaternion.identity);
		effect.transform.parent = node.transform;
		Destroy(effect, 5f);

		turret.transform.parent = node.transform;

		turret.GetComponent<Turret>().ShowRange();
		return true;
		//turret built money left.
	}
}
