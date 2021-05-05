using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReadyUp : MonoBehaviour
{
    public Button ready1;
    public Button ready2;
    public Button ready3;

    private bool red1;
    private bool red2;
    private bool red3;

    public GameObject menuManager;

    private void Start()
    {
        red1 = false;
        red2 = false;
        red3 = false;
        menuManager.GetComponent<MenuManager>().LoadLevel(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("A Button") && red1 == false)
        {
            red1 = true;
            ready1.OnSelect(null);
        }
        if (Input.GetButtonDown("B Button") && red1 == true)
        {
            red1 = false;
            ready1.OnDeselect(null);
        }

        if (Input.GetButtonDown("1A Button") && red2 == false)
        {
            red2 = true;
            ready2.OnSelect(null);
        }
        if (Input.GetButtonDown("1B Button") && red2 == true)
        {
            red2 = false;
            ready2.OnDeselect(null);
        }

        if (Input.GetButtonDown("2A Button") && red3 == false)
        {
            red3 = true;
            ready3.OnSelect(null);
        }
        if (Input.GetButtonDown("2B Button") && red3 == true)
        {
            red3 = false;
            ready3.OnDeselect(null);
        }

        if (red1 && red2 && red3 || Input.GetKeyDown(KeyCode.UpArrow))
        {
            menuManager.GetComponent<MenuManager>().Activatelevel();
        }
    }
}
