using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarabCatcher : MonoBehaviour
{
	public GameObject game;
	public Camera cam;
	public GameObject scarab;

	private bool moving;


	IEnumerator Wait()
	{
		yield return new WaitForSeconds(cam.GetComponent<AudioPeer>()._delayTime);
		moving = false;
	}

	void Start()
	{
		moving = true;
		transform.position = transform.position + transform.forward * (game.GetComponent<RhythmGame>()._offset * 0.5f);
		StartCoroutine(Wait());
	}

	void LateUpdate()
	{
		if (moving == true)
		{
			transform.Translate(Vector3.forward * scarab.GetComponent<Scarab>()._moveSpeed * Time.deltaTime);
		}
	}


	void OnTriggerStay(Collider other)
	{
		if (gameObject.name == "Scarab Catcher Yellow" && Input.GetKeyDown(KeyCode.UpArrow))
		{
			Destroy(other.gameObject);
		}
		if (gameObject.name == "Scarab Catcher Blue" && Input.GetKeyDown(KeyCode.LeftArrow))
		{
			Destroy(other.gameObject);
		}
		if (gameObject.name == "Scarab Catcher Green" && Input.GetKeyDown(KeyCode.DownArrow))
		{
			Destroy(other.gameObject);
		}
		if (gameObject.name == "Scarab Catcher Red" && Input.GetKeyDown(KeyCode.RightArrow))
		{
			Destroy(other.gameObject);
		}
		
	}
}
