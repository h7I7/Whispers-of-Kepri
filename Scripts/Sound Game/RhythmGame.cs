using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGame : MonoBehaviour
{
    public float _blueThreshold;
    public float _greenThreshold;
    public float _redThreshold;
    public float _yellowThreshold;

    public float _offset;

    public GameObject _scarab;

    public Material _yellowScarab;
    public Material _greenScarab;
    public Material _blueScarab;
    public Material _redScarab;

    // Update is called once per frame
    void Update ()
    {
        if (AudioPeer._freq4Band[0] >= _blueThreshold)
        {
            GameObject scarab = Instantiate(_scarab, transform.position, Quaternion.Euler(0, 0, 0));
            scarab.transform.parent = transform;
            scarab.name = "Scarab0";
            scarab.transform.position = scarab.transform.position + scarab.transform.forward * _offset; 
            scarab.transform.eulerAngles = new Vector3(180, 0, 0);
            scarab.GetComponent<Renderer>().material = _blueScarab;
        }

        if (AudioPeer._freq4Band[1] >= _greenThreshold)
        {
            GameObject scarab = Instantiate(_scarab, transform.position, Quaternion.Euler(90, 0, 0));
            scarab.transform.parent = transform;
            scarab.name = "Scarab1";
            scarab.transform.position = scarab.transform.position + scarab.transform.forward * _offset;
            scarab.transform.eulerAngles = new Vector3(270, 0, 0);
            scarab.GetComponent<Renderer>().material = _greenScarab;
        }

        if (AudioPeer._freq4Band[2] >= _redThreshold)
        {
            GameObject scarab = Instantiate(_scarab, transform.position, Quaternion.Euler(180, 0, 0));
            scarab.transform.parent = transform;
            scarab.name = "Scarab2";
            scarab.transform.position = scarab.transform.position + scarab.transform.forward * _offset;
            scarab.transform.eulerAngles = new Vector3(0, 0, 0);
            scarab.GetComponent<Renderer>().material = _redScarab;
        }

        if (AudioPeer._freq4Band[3] >= _yellowThreshold)
        {
            GameObject scarab = Instantiate(_scarab, transform.position, Quaternion.Euler(270, 0, 0));
            scarab.transform.parent = transform;
            scarab.name = "Scarab3";
            scarab.transform.position = scarab.transform.position + scarab.transform.forward * _offset;
            scarab.transform.eulerAngles = new Vector3(90, 0, 0);
            scarab.GetComponent<Renderer>().material = _yellowScarab;
        }
    }
}
