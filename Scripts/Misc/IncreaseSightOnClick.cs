using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseSightOnClick : MonoBehaviour
{
	public Material border;
	public Material nonBorder;
	public Material pressed;
	private Camera playerCamera;

	private void Start()
	{
		playerCamera = GameObject.Find("Player Camera").GetComponent<Camera>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.E))
		{
			GetComponent<Renderer>().material = border;
		}
		else if (Input.GetKeyUp(KeyCode.E))
		{
			//run code.
			playerCamera.GetComponent<Sight>().IncreaseSight();
			GetComponent<Renderer>().material = pressed;
		}
		else
		{
			GetComponent<Renderer>().material = nonBorder;
		}
	}
}

