using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {

    public static PauseManager instance = null;

    [SerializeField]
    private bool m_gameover = false;
    public bool Gameover
    {
        get { return m_gameover; }
    }

    [SerializeField]
    private bool m_paused = false;
    public bool Paused
    {
        get { return m_paused; }
    }

    [SerializeField]
    private GameObject[] m_itemsToHideOnPause;
    [SerializeField]
    private GameObject[] m_itemsToUnHideOnPause;

    [SerializeField]
    private GameObject[] m_itemsToHideOnUnPause;
    [SerializeField]
    private GameObject[] m_itemsToUnHideOnUnPause;

	[SerializeField]
	private GameObject[] m_itemsToHideOnGameOver;
	[SerializeField]
	private GameObject[] m_itemsToUnHideOnGameOver;


	[SerializeField]
	private GameObject menuManager;

    private bool loaded;

	private void Start()
    {
        if (instance == null)
            instance = this;

	}
    private void Update()
    {
        if (Input.GetButtonDown("Cancel") && !Gameover)
        {
            m_paused = !m_paused;
        }
        if (m_paused)
        {
            Pause();
        }
        else if (!m_paused)
        {
            UnPause();
        }

        if(m_gameover)
        {
            EndGame();
        }
        
		if (m_gameover && Input.GetButtonDown("Submit"))
		{
			menuManager.GetComponent<MenuManager>().Activatelevel();
		}
    }

    public void Pause()
    {
        foreach(GameObject obj in m_itemsToHideOnPause)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_itemsToUnHideOnPause)
        {
            obj.SetActive(true);
        }
        Time.timeScale = 0;
        m_paused = true;
    }
    
    public void UnPause()
    {
        foreach (GameObject obj in m_itemsToHideOnUnPause)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in m_itemsToUnHideOnUnPause)
        {
            obj.SetActive(true);
        }

        Time.timeScale = 1;
        m_paused = false;
    }

    public void EndGame()
    {
        m_gameover = true;

		foreach (GameObject obj in m_itemsToHideOnGameOver)
		{
			obj.SetActive(false);
		}

		foreach (GameObject obj in m_itemsToUnHideOnGameOver)
		{
			obj.SetActive(true);
		}
        if (!loaded)
        {
            loaded = true;

            menuManager.GetComponent<MenuManager>().LoadLevel(0);
        }

	}
}
