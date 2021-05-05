using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSetup : MonoBehaviour {

	private static ScreenSetup m_instance;

    private bool m_screensLoaded = false;

    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;
        else
            Destroy(gameObject);
    }
    // Use this for initialization
    void Start () {

		

        if (m_screensLoaded)
            return;

        DontDestroyOnLoad(this);

        //set up displays :D
        foreach (Display view in Display.displays)
        {
            view.Activate(Camera.main.pixelWidth, Camera.main.pixelHeight, 60);
        }

        m_screensLoaded = true;
    }
}
