using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    // Variable that loads the next level in the background
    AsyncOperation nextLevel;

    // Use this for initialization
    void Start ()
    {
        nextLevel = new AsyncOperation();

    }

    // Quiting the game
    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Loads a level in the background using the Async variable
    public void LoadLevel(int a_nextScene)
    {
        StartCoroutine(LoadLevelEnum(a_nextScene));
    }

    private IEnumerator LoadLevelEnum(int a_nextScene)
    {
        nextLevel = SceneManager.LoadSceneAsync(a_nextScene);
        nextLevel.allowSceneActivation = false;

        while(nextLevel.isDone)
        {
            yield return null;
        }
    }

    // Activates the level that was loaded in the background
    public void Activatelevel()
    {
        nextLevel.allowSceneActivation = true;
    }
}
