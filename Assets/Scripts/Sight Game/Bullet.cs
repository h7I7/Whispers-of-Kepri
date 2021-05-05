using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Transform target;


	public void Seek(Transform a_target)
	{
		target = a_target;
	}
	
	void Update()
	{
		if (target == null)
		{
			Destroy(gameObject);
			return;
		}

		Vector3 dir = target.position - transform.position;
		float distanceThisFrame = transform.parent.GetComponent<Turret>().bulletSpeed * Time.deltaTime;

		if (dir.magnitude <= distanceThisFrame)
		{
			HitTarget();
			return;
		}

		transform.Translate(dir.normalized * distanceThisFrame, Space.World);
		transform.LookAt(target);
	}

	void HitTarget()
	{
		GameObject effectIns = (GameObject)Instantiate(transform.parent.GetComponent<Turret>().impactEffect, transform.position, transform.rotation);
		effectIns.transform.parent = transform.parent;

		Destroy(effectIns, 5.0f);

		if (transform.parent.GetComponent<Turret>().explosionRadius > 0.0f)
		{
			Explode();
		}
		else
		{
			Damage(target);
		}
		
		Destroy(gameObject);
	}

	void Damage(Transform enemy)
	{
		TowerEnemy e = enemy.GetComponent<TowerEnemy>();

		if (e != null)
		{
			e.TakeDamage(transform.parent.GetComponent<Turret>().damage);
		}
	}

	void Explode()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, transform.parent.GetComponent<Turret>().explosionRadius);
		float amountOnFire = 0;
		foreach (Collider collider in colliders)
		{
			if (amountOnFire >= transform.parent.GetComponent<Turret>().range * 0.5f)
			{
				return;
			}
			else if (collider.tag == "Enemy")
			{
				Damage(collider.transform);
				collider.transform.GetComponent<TowerEnemy>().Burn(transform.parent.GetComponent<Turret>().damage / 10, transform.parent.GetComponent<Turret>().fireRate * 10);
				amountOnFire++;
			}
		}
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, transform.parent.GetComponent<Turret>().explosionRadius);
	}
}
