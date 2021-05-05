using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSelection : MonoBehaviour
{
    public Button play;
    public Button quit;

    private bool playSelected;
    private bool quitSelected;

    private void Start()
    {
        playSelected = true;
        quitSelected = false;
        play.Select();
    }

    // Update is called once per frame
    void Update ()
    {
        if(Input.GetAxisRaw("MenuVert") < 0 && quitSelected)
        {
            play.Select();
            playSelected = true;
            quitSelected = false;
        }

        if (Input.GetAxisRaw("MenuVert") > 0 && playSelected)
        {
            quit.Select();
            playSelected = false;
            quitSelected = true;
        }

        if (Input.GetButtonUp("A Button") && playSelected || Input.GetButtonUp("1A Button") && playSelected || Input.GetButtonUp("2A Button") && playSelected)
        {
            play.onClick.Invoke();
        }

        if (Input.GetButtonUp("A Button") && quitSelected || Input.GetButtonUp("1A Button") && quitSelected || Input.GetButtonUp("2A Button") && quitSelected)
        {
            quit.onClick.Invoke();
        }
    }
}
