using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseSoundOnClick : MonoBehaviour
{
	public Material border;
	public Material nonBorder;
	public Material pressed;
	private Camera playerCamera;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			playerCamera = GameObject.Find("Player Camera").GetComponent<Camera>();
		}
		if (Input.GetKey(KeyCode.Q))
		{
			GetComponent<Renderer>().material = border;
		}
		else if (Input.GetKeyUp(KeyCode.Q))
		{
			//run code.
			playerCamera.GetComponent<Sound>().IncreaseSound();
			GetComponent<Renderer>().material = pressed;
		}
		else
		{
			GetComponent<Renderer>().material = nonBorder;
		}
	}
}
