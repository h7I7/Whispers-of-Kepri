using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instansiate512Cubes : MonoBehaviour

{
	public GameObject _sampleCubePrefab;
	GameObject[] _sampleCube = new GameObject[512];
	public float _maxScale;
	public float _rotation = -0.703125f;
	public float _movedDistance;
	public float _scale;

	// Use this for initialization
	void Start ()
	{
		for (int i = 0; i < 512; i++)
		{
			GameObject _instanceSampleCube = (GameObject)Instantiate(_sampleCubePrefab);
			_instanceSampleCube.transform.position = this.transform.position;
			_instanceSampleCube.transform.parent = this.transform;
			_instanceSampleCube.name = "SampleCube" + i;
			this.transform.eulerAngles = new Vector3(0, _rotation * i, 0);
			_instanceSampleCube.transform.position = Vector3.forward * _movedDistance;
			_sampleCube[i] = _instanceSampleCube;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i =0; i < 512; i++)
		{
			if (_sampleCube != null)
			{
				_sampleCube[i].transform.localScale = new Vector3((AudioPeer._Samples[i] * _maxScale) + 2, _scale,  _scale);
			}
		}
	}
}
