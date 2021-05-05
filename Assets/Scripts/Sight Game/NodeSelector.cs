using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSelector : MonoBehaviour
{
	public GameObject[] nodes;
	private int randomNumber;
	private int selectedNode;
	public float waitTime = 0.1f;
	private float waitRemaining = 0.1f;
    public Controller controller;
    public GameObject gameManager;

	void Start()
	{
		nodes = GameObject.FindGameObjectsWithTag("TurretNode");
		randomNumber = Random.Range(0, nodes.Length + 1);
		nodes[randomNumber].GetComponent<TurretNode>().Selected();
		selectedNode = randomNumber;
	}

	void Update()
	{
        controller = gameManager.GetComponent<CameraDisplaySwap>().nodeController;
        waitRemaining -= Time.deltaTime;
		if (waitRemaining < 0)
		{
			if (Input.GetAxis(controller.YAxis)<0)//up
			{
				GameObject closestNode = nodes[selectedNode];
				float closestDistance = 99999;
				List<GameObject> sameAxisNodes = new List<GameObject>();
				List<GameObject> correctDirNodes = new List<GameObject>();

				foreach (GameObject node in nodes)
				{
					if (node.transform.position.x == nodes[selectedNode].transform.position.x)
					{
						sameAxisNodes.Add(node);
					}
				}
				foreach (GameObject node in sameAxisNodes)
				{
					if (node.transform.position.z > nodes[selectedNode].transform.position.z)
					{

						correctDirNodes.Add(node);
					}
				}

				foreach (GameObject node in correctDirNodes)
				{
					if (Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position) < closestDistance)
					{
						closestNode = node;
						closestDistance = Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position);
					}
				}
				if (closestNode != nodes[selectedNode])
				{
					closestNode.GetComponent<TurretNode>().Selected();
					nodes[selectedNode].GetComponent<TurretNode>().Deselcted();
					selectedNode = System.Array.IndexOf(nodes, closestNode);

					waitRemaining = waitTime;
				}

			}
			if (Input.GetAxis(controller.XAxis) < 0)//Left
            {
				GameObject closestNode = nodes[selectedNode];
				float closestDistance = 99999;
				List<GameObject> sameAxisNodes = new List<GameObject>();
				List<GameObject> correctDirNodes = new List<GameObject>();

				foreach (GameObject node in nodes)
				{
					if (node.transform.position.z == nodes[selectedNode].transform.position.z)
					{
						sameAxisNodes.Add(node);
					}
				}
				foreach (GameObject node in sameAxisNodes)
				{
					if (node.transform.position.x < nodes[selectedNode].transform.position.x)
					{
						correctDirNodes.Add(node);
					}
				}

				foreach (GameObject node in correctDirNodes)
				{
					if (Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position) < closestDistance)
					{
						closestNode = node;
						closestDistance = Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position);
					}
				}
				if (closestNode != nodes[selectedNode])
				{
					closestNode.GetComponent<TurretNode>().Selected();
					nodes[selectedNode].GetComponent<TurretNode>().Deselcted();
					selectedNode = System.Array.IndexOf(nodes, closestNode);

					waitRemaining = waitTime;
				}
			}
			if (Input.GetAxis(controller.XAxis) > 0)//Right
            {
				GameObject closestNode = nodes[selectedNode];
				float closestDistance = 99999;
				List<GameObject> sameAxisNodes = new List<GameObject>();
				List<GameObject> correctDirNodes = new List<GameObject>();

				foreach (GameObject node in nodes)
				{
					if (node.transform.position.z == nodes[selectedNode].transform.position.z)
					{
						sameAxisNodes.Add(node);
					}
				}
				foreach (GameObject node in sameAxisNodes)
				{
					if (node.transform.position.x > nodes[selectedNode].transform.position.x)
					{
						correctDirNodes.Add(node);
					}
				}

				foreach (GameObject node in correctDirNodes)
				{
					if (Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position) < closestDistance)
					{
						closestNode = node;
						closestDistance = Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position);
					}
				}
				if (closestNode != nodes[selectedNode])
				{
					closestNode.GetComponent<TurretNode>().Selected();
					nodes[selectedNode].GetComponent<TurretNode>().Deselcted();
					selectedNode = System.Array.IndexOf(nodes, closestNode);

					waitRemaining = waitTime;
				}
			}
			if (Input.GetAxis(controller.YAxis) > 0)//Down
            {
				GameObject closestNode = nodes[selectedNode];
				float closestDistance = 99999;
				List<GameObject> sameAxisNodes = new List<GameObject>();
				List<GameObject> correctDirNodes = new List<GameObject>();

				foreach (GameObject node in nodes)
				{
					if (node.transform.position.x == nodes[selectedNode].transform.position.x)
					{
						sameAxisNodes.Add(node);
					}
				}
				foreach (GameObject node in sameAxisNodes)
				{
					if (node.transform.position.z < nodes[selectedNode].transform.position.z)
					{

						correctDirNodes.Add(node);
					}
				}

				foreach (GameObject node in correctDirNodes)
				{
					if (Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position) < closestDistance)
					{

						closestNode = node;
						closestDistance = Vector3.Distance(node.transform.position, nodes[selectedNode].transform.position);
					}
				}
				if (closestNode != nodes[selectedNode])
				{
					closestNode.GetComponent<TurretNode>().Selected();
					nodes[selectedNode].GetComponent<TurretNode>().Deselcted();
					selectedNode = System.Array.IndexOf(nodes, closestNode);

					waitRemaining = waitTime;
				}
			}
		}
		
		

	}

}

