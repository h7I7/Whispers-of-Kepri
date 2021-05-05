using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
	private bool activated = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Player")
		{
			if (activated == false)
			{
				activated = true;
                other.GetComponent<PlayerController>().ResetLife();
				TKGenerator.instance.Generate();
			}
		}
	}
}
