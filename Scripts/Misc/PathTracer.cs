using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class PathTracer : MonoBehaviour {

    private Unit unit;

	// Use this for initialization
	void Start () {
        if (unit == null)
            unit = GetComponent<Unit>();
	}
	
	// Update is called once per frame
	void Update () {
        if (unit.Path.Length <= 2)
            Destroy(gameObject);
	}
}
