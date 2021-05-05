using UnityEngine;
using UnityEngine.UI;

public class TowerEnemy : MonoBehaviour
{
	[HideInInspector]
	public float speed;

	public float startSpeed = 10f;
	private float health;
	public float startHealth = 100;
	public int worth = 50;
	private float burnTime = 0;
	private float burnDamage = 0;
	private float slowTime = 0;
	private float slowAmount = 0;
	public ParticleSystem burningEffect;
	public Light burningLight;
	public ParticleSystem slowEffect;
	public Light slowingLight;
    private Camera playerCamera;
    public GameObject deathEffect;

	[Header("Unity Stuff")]
	public Image healthBar;

	void Start()
	{
		speed = startSpeed;
		health = startHealth;
        playerCamera = GameObject.Find("Player Camera").GetComponent<Camera>();
    }

	public void TakeDamage(float amount)
	{
		health -= amount;

		healthBar.fillAmount = health / startHealth;
	}

	void Die()
	{
		GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
		effect.transform.parent = transform.parent;
		Destroy(effect, 5f);

		WaveSpawner.EnemiesAlive--;
        playerCamera.GetComponent<Sound>().IncreaseSound();
        playerCamera.GetComponent<Sight>().IncreaseSight();
        PlayerStats.Money += worth;
		Destroy(gameObject);
	}

	public void Slow(float amount, float time)
	{
		slowTime = time;
		slowAmount = amount;
	}

	public void Burn(float damage, float time)
	{
		burnTime = time;
		burnDamage = damage;
	}

	void Update()
	{
		slowTime -= Time.deltaTime;
		burnTime -= Time.deltaTime;

		if (health <= 0)
		{
			Die();
		}

		if (burnTime > 0f)
		{
			TakeDamage(burnDamage);
			burningEffect.Play();
			burningLight.enabled = true;
		}
		else
		{
			burningLight.enabled = false;
			burningEffect.Stop();
		}

		if (slowTime > 0f)
		{
			speed = startSpeed * (1f - slowAmount);
			slowEffect.Play();
			slowingLight.enabled = true;
		}
		else
		{
			slowingLight.enabled = false;
			speed = startSpeed;
			slowEffect.Stop();
		}
	}
}
