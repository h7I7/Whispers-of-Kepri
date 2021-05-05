using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderFallPlayerController : MonoBehaviour
{
	public float moveSpeed = 10;
    public Controller controller;
    public GameObject gameManager;
    public float bettleTime = 5.0f;
    private float timeRemaining;
    private GameObject player;

    private void Start()
    {
        timeRemaining = bettleTime;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update ()
	{

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            player.GetComponent<PlayerController>().SpawnTracer();
            timeRemaining = bettleTime;
        }

        controller =  gameManager.GetComponent<CameraDisplaySwap>().boulderController;
        if (Input.GetAxisRaw(controller.XAxis) < 0)
		{
			GetComponent<Rigidbody>().AddForce(Vector3.left * moveSpeed);
		}
		if (Input.GetAxisRaw(controller.XAxis) > 0)
		{
			GetComponent<Rigidbody>().AddForce(Vector3.right * moveSpeed);
		}
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Boulder(Clone)")
        {
            timeRemaining = bettleTime;
        }
    }
}
