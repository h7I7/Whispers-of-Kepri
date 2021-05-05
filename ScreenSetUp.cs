using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSetUp : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //set up displays :D
        foreach (Display view in Display.displays)
        {
            view.Activate(Camera.main.pixelWidth, Camera.main.pixelHeight, 60);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
