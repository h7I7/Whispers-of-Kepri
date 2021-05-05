using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Cursor : MonoBehaviour
{
	public float speed = 100.0f;


	private void Update()
	{
		Vector3 direction = Vector3.zero;
		
		if (Input.GetKey(KeyCode.I))
		{
			direction = Vector2.up;
		}
		if (Input.GetKey(KeyCode.L))
		{
			direction = Vector2.right;
		}
		if (Input.GetKey(KeyCode.J))
		{
			direction = Vector2.left;
		}
		if (Input.GetKey(KeyCode.K))
		{
			direction = Vector2.down;
		}

		transform.position += direction * (Time.deltaTime * speed);
		
	}

}
	

