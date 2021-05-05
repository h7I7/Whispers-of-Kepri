using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
	public ParticleSystem boulderBreakEffect;
	public ParticleSystem playerHit;

	void OnTriggerEnter(Collider other)
	{ 
		if (other.gameObject.name == "player boulder")
		{
			ParticleSystem effectIns = Instantiate(boulderBreakEffect, transform.position, transform.rotation, transform.parent);
			Destroy(effectIns, 1.0f);
			ParticleSystem effect2Ins = Instantiate(playerHit, other.transform.position, other.transform.rotation, transform.parent);
			Destroy(effect2Ins, 1.0f);
			Destroy(this.gameObject);
		}
		else if (other.gameObject.name == "Floor")
		{
			ParticleSystem effectIns = Instantiate(boulderBreakEffect, transform.position, transform.rotation, transform.parent);
			Destroy(effectIns, 1.0f);
			Destroy(this.gameObject);
		}
	}
}
