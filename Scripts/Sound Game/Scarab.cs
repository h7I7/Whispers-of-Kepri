using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarab : MonoBehaviour
{

    public float _moveSpeed;

	// Update is called once per frame
	void Update ()
    {
        transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
    }
}
