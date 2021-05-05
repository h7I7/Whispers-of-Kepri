using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDisplaySwap : MonoBehaviour
{
	private Camera playerCamera;
	private Camera soundCamera;
	private Camera sightCamera;

    public float timeDelayMin;
    public float timeDelayMax;
    private float timeLeft;

    public Controller playerController;
    public Controller nodeController;
    public Controller shopController;
    public Controller boulderController;

	private void Start()
	{
        

		playerCamera = GameObject.Find("Player Camera").GetComponent<Camera>();
		soundCamera = GameObject.Find("Sound Camera").GetComponent<Camera>();
		sightCamera = GameObject.Find("Sight Camera").GetComponent<Camera>();

        playerController = GameObject.Find("Player").GetComponent<PlayerController>().controller;
        nodeController = GameObject.Find("SightGameManager").GetComponent<NodeSelector>().controller;
        shopController = GameObject.Find("SightGameManager").GetComponent<TurretShop>().controller;
        boulderController = GameObject.Find("player boulder").GetComponent<BoulderFallPlayerController>().controller;

        timeLeft = RandomTime();
	}

	private void Update()
	{
		timeLeft -= Time.deltaTime;
		if (timeLeft < 0)
		{
			DisplaySwap();
			timeLeft = RandomTime();
		}
	}

	private void DisplaySwap()
	{
        int tempDis = playerCamera.targetDisplay;
        playerCamera.targetDisplay = soundCamera.targetDisplay;
        soundCamera.targetDisplay = sightCamera.targetDisplay;
        sightCamera.targetDisplay = tempDis;

        Controller tempController = boulderController;
        
        boulderController = nodeController;
        nodeController = playerController;
        shopController = playerController;
        playerController = tempController;

    }

    private float RandomTime()
    {
        float time = Random.Range(timeDelayMin, timeDelayMax);
        return time;
    }
}
