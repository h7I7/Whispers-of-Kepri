using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
	private Transform target;
	private TowerEnemy targetEnemy;
	private float fireCountdown = 0f;

	[Header("General Attributes")]
	public float range = 15f;
	public float damage = 1f;
	public float fireRate = 1f;

	[Header("Use Bullets Attributes")]
	public GameObject bulletPrefab;
	public GameObject impactEffect;
	public float explosionRadius = 0.0f;

	[Header("Use Laser Attributes")]
	public bool useLaser = false;
	public LineRenderer lineRenderer;
	public ParticleSystem laserImpactEffect;
	public Light impactLight;

	[Header("Use Slow Attributes")]
	public bool useSlow = false;
	public ParticleSystem slowEffect;

	[Header("Upgrade Modifiers")]
	public float rangeUpgrade = 2f;
	public float fireRateUpgrade = 0.5f;
	public float damageUpgrade = 0.5f;
	public int maxUpgrades = 5;

	[Header("Current Upgrade Modifiers")]
	public int currentRangeUpgrade = 1;
	public int currentFireRateUpgrade = 1;
	public int currentDamageUpgrade = 1;

	[Header("Sell Amount")]
	[Range(0, 1)]
	public float initialSellAmountPercentage = 0.5f;

	[Header("Upgrade Costs")]
	public float initialCost = 100;
	[Range(0, 10)]
	public float increaseCostPercentage = 2;

	[Header("Unity Setup Fields")]
	public Transform partToRotate;
	public string enemyTag = "Enemy";
	public float turnSpeed = 10f;
	public float bulletSpeed = 70f;
	public Transform firePoint;
	public ParticleSystem rangeEffect;

	[Header("Upgrade Effects")]
	public GameObject PowerUpgradeEffect;
	public GameObject RangeUpgradeEffect;
	public GameObject FireRateUpgradeEffect;
	public GameObject SellEffect;

	void Start()
	{
		InvokeRepeating("UpdateTarget", 0f, 0.5f);
	}

	void Update()
	{
		if (target == null)
		{
			if (useLaser)
			{
				if (lineRenderer.enabled)
				{
					lineRenderer.enabled = false;
					laserImpactEffect.Stop();
					impactLight.enabled = false;
				}
			}
			return;
		}
		
		if (useLaser)
		{
			LockOnTarget();
			Laser();
		}
		else if (useSlow)
		{
			if (fireCountdown <= 0f)
			{
				Slow();
				fireCountdown = 1f / fireRate;
			}

			fireCountdown -= Time.deltaTime;
		}
		else
		{
			LockOnTarget();
			if (fireCountdown <= 0f)
			{
				Shoot();
				fireCountdown = 1f / fireRate;
			}

			fireCountdown -= Time.deltaTime;
		}
	}

	public void ShowRange()
	{
		ParticleSystem.ShapeModule shape = rangeEffect.shape;
		shape.radius = range;

		rangeEffect.Play();
	}

	public void StopShowingRange()
	{
		rangeEffect.Stop();
	}

	public void UpgradeRange(TurretNode node)
	{
		PlayerStats.Money -= (int)initialCost * currentRangeUpgrade;
		range += rangeUpgrade;
		currentRangeUpgrade++;

		node.turret.GetComponent<Turret>().ShowRange();

		GameObject effect = Instantiate(RangeUpgradeEffect, node.GetBuildPosition(), Quaternion.identity);
		effect.transform.parent = node.transform;
		Destroy(effect, 5f);
	}

	public void UpgradeFireRate(TurretNode node)
	{
		PlayerStats.Money -= (int)initialCost * currentFireRateUpgrade;
		fireRateUpgrade += fireRateUpgrade;
		currentFireRateUpgrade++;

		GameObject effect = Instantiate(FireRateUpgradeEffect, node.GetBuildPosition(), Quaternion.identity);
		effect.transform.parent = node.transform;
		Destroy(effect, 5f);
	}

	public void UpgradeDamage(TurretNode node)
	{
		PlayerStats.Money -= (int)initialCost * currentDamageUpgrade;
		damage += damageUpgrade;
		currentDamageUpgrade++;

		GameObject effect = Instantiate(PowerUpgradeEffect, node.GetBuildPosition(), Quaternion.identity);
		effect.transform.parent = node.transform;
		Destroy(effect, 5f);
	}

	public float SalePrice()
	{
		return	((initialCost * (currentDamageUpgrade - 1) * (currentDamageUpgrade - 1)) * initialSellAmountPercentage)
		+ ((initialCost * (currentFireRateUpgrade - 1) * (currentFireRateUpgrade - 1)) * initialSellAmountPercentage)
		+ ((initialCost * (currentRangeUpgrade - 1) * (currentRangeUpgrade - 1)) * initialSellAmountPercentage)
		+ initialCost * initialSellAmountPercentage;
	}

	void LockOnTarget()
	{
		Vector3 dir = target.position - transform.position;
		Quaternion lookRotation = Quaternion.LookRotation(dir);
		Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
		partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
	}

	void Slow()
	{
		slowEffect.Play();
		Collider[] colliders = Physics.OverlapSphere(transform.position, range);
		float amountSlowed = 0;

        foreach (Collider collider in colliders)
		{
			if (amountSlowed >= range * 0.5f)
			{
				return;
			}
			else if (collider.tag == "Enemy")
			{
				collider.transform.GetComponent<TowerEnemy>().Slow(damage, fireRate * 10);
				amountSlowed++;
			}
		}
	}

	void Laser()
	{
		targetEnemy.TakeDamage(damage * Time.deltaTime);
        if (!lineRenderer.enabled)
		{
			lineRenderer.enabled = true;
			laserImpactEffect.Play();
			impactLight.enabled = true;
		}
		
		lineRenderer.SetPosition(0, firePoint.position);
		lineRenderer.SetPosition(1, target.position);

		Vector3 dir = firePoint.position - target.position;

		laserImpactEffect.transform.position = target.transform.position + dir.normalized;

		laserImpactEffect.transform.rotation = Quaternion.LookRotation(dir);
	}

	void Shoot()
	{
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
		bulletGO.transform.parent = transform;
		Bullet bullet = bulletGO.GetComponent<Bullet>();

		if (bullet != null)
		{
			bullet.Seek(target);
		}

	}

	void UpdateTarget()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

		float shortestDistance = Mathf.Infinity;

		GameObject nearestEnemy = null;

		foreach (GameObject enemy in enemies)
		{
			float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
			if(distanceToEnemy < shortestDistance)
			{
				shortestDistance = distanceToEnemy;
				nearestEnemy = enemy;
			}
		}

		if (nearestEnemy != null && shortestDistance <= range)
		{
			target = nearestEnemy.transform;
			targetEnemy = nearestEnemy.GetComponent<TowerEnemy>();
		}
		else
		{
			target = null;
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, range);
	}
}
