//\===========================================================================================
//\ File: MapGen.cs
//\ Author: Morgan James
//\ Date Created: 18/09/2017
//\ Brief: Creates a randomly sized map with a random setup out of the building blocks arguments.
//\===========================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGen : MonoBehaviour
{
	//\===========================================================================================
	//\ Variables
	//\===========================================================================================

	#region Variables
    
	[System.Serializable]
	public struct Spawnable
	{
		public GameObject gameObject;
		public float weight;
	}

	private int iMapSize = 4;//How many rows and columns the map will have.
	public int iFloorNumber = 0;//What floor the game is on(how high up the floor is).

	public Spawnable[] corners;//The game object that will be the corner of the map.
	private float _totalCornersSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] sides;//The game object that will be used for the walls of the map.
	private float _totalSidesSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] floors;//The game object that will be used for the floor of the map.
	private float _totalFloorsSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] starts;//The game object that will be used for the start of the map.
	private float _totalStartsSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] exits;//The game object that will be used for the exit of the map.
	private float _totalExitsSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] enemySpawns;//The game object that will be used for the enemy spawn of the map.
	private float _totalEnemySpawnsSpawnWeight;//Track the total weight used in the whole array.

	public Spawnable[] obstacles;//The game object that will be used for the enemy spawn of the map.
	private float _totalObstaclesSpawnsSpawnWeight;//Track the total weight used in the whole array.

	public float startingHeight;//How high the pyramid starts.

	public GameObject goTerrainFolder;

	public GameObject gameManager;

	public GameObject enemy;

	public GameObject player;

	MapNode[,] nGrid;
	private Vector2 v2GridWorldSize;
	private float fNodeRadius = 1;
	private float fNodeDiameter = 2;
	private float fNodeHeight = 2.5f;
	private float fCeilingOffset = 0.06f;
	private int iColumns, iRows;
	public int iObstacleAmount = 1;
    private Vector3 exitPos;
    private Vector3 playerPos;
    private Vector3 enemyPos;
    private bool pathSuccess;

    #endregion

    //\===========================================================================================
    //\ Unity Methods
    //\===========================================================================================

    #region Unity Methods

    void OnValidate()
	{
		_totalCornersSpawnWeight = 0f;
		foreach (var spawnable in corners)
			_totalCornersSpawnWeight += spawnable.weight;

		_totalSidesSpawnWeight = 0f;
		foreach (var spawnable in sides)
			_totalSidesSpawnWeight += spawnable.weight;

		_totalFloorsSpawnWeight = 0f;
		foreach (var spawnable in floors)
			_totalFloorsSpawnWeight += spawnable.weight;

		_totalEnemySpawnsSpawnWeight = 0f;
		foreach (var spawnable in enemySpawns)
			_totalEnemySpawnsSpawnWeight += spawnable.weight;

		_totalExitsSpawnWeight = 0f;
		foreach (var spawnable in exits)
			_totalExitsSpawnWeight += spawnable.weight;

		_totalStartsSpawnWeight = 0f;
		foreach (var spawnable in starts)
			_totalStartsSpawnWeight += spawnable.weight;

		_totalObstaclesSpawnsSpawnWeight = 0f;
		foreach (var spawnable in obstacles)
			_totalObstaclesSpawnsSpawnWeight += spawnable.weight;
	}

	void Awake()
	{
		NewMap();
	}

	public void NewMap()
	{
		DestroyPreviousMap();//Delete the last stuff
		//Create object grid with same size as the floor but more nodes and checking the floor to see if placeable square, spawn a teleporter max one when touched if no enemies teleport player and call this function.
		v2GridWorldSize.x = NewMapSize();

		v2GridWorldSize.y = v2GridWorldSize.x;	

		iColumns = (int)v2GridWorldSize.x;

		iRows = (int)v2GridWorldSize.x;

		if (transform.position.y == 0)//If its the first floor change the starting height.
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + startingHeight, transform.position.z);
		}

		CreateGrid();

        PlaceTiles();

        if (iFloorNumber != 0)
        {
            StopAllCoroutines();
            Grid.instance.gridWorldSize.x += 2f;
            Grid.instance.gridWorldSize.y += 2f;
            Grid.instance.gridHeightOffset -= 2.5f;
            Grid.instance.OnValidate();
         
        }

        iFloorNumber++;//Increases the floor number so that the next floor will be higher.

		transform.position = new Vector3(transform.position.x, transform.position.y - fNodeHeight, transform.position.z);
	}

	void CreateGrid()
	{
		nGrid = new MapNode[iRows, iColumns];
		
		Vector3 worldBottomLeft = transform.position - Vector3.right * v2GridWorldSize.x - Vector3.forward * v2GridWorldSize.y ;

		for (int x = 0; x < iRows; x++)
		{
			for (int y = 0; y < iColumns; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) + Vector3.forward * (y * fNodeDiameter + fNodeRadius);
				nGrid[x, y] = new MapNode(worldPoint);
			}
		}
	}

	void PlaceTiles()
	{
		//Can change this to get a random number between all the types to make random room types.
		int iMapType = 1;//The type of algorithm the map generator uses to put the map together.

		if (iMapType == 1)
		{

			int iStartX = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iRows >> 1), 0f), new WeightedRandom.IntRange((int)(iRows >> 1),(int)(iRows - 2),100f));
			int iStartY = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iColumns >> 1), 0f), new WeightedRandom.IntRange((int)(iColumns >> 1), (int)(iColumns - 2), 100f));

			int iExitX = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iRows >> 1), 100f), new WeightedRandom.IntRange((int)(iRows >> 1), (int)(iRows - 2), 0f));
			int iExitY = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iColumns >> 1), 100f), new WeightedRandom.IntRange((int)(iColumns >> 1), (int)(iColumns - 2), 0f));
			
			int iEnemySpawnX = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iRows >> 1), 0f), new WeightedRandom.IntRange((int)(iRows >> 1), (int)(iRows - 2), 100f));
			int iEnemySpawnY = WeightedRandom.Range(new WeightedRandom.IntRange(1, (int)(iColumns >> 1), 100f), new WeightedRandom.IntRange((int)(iColumns >> 1), (int)(iColumns - 2), 0f));


			//if col and row == 0 corner or one 0 and one max or both max then one 0 and not both or max then wall and fill rest with floor
			for (int x = 0; x < iRows; x++)
			{
				for (int y = 0; y < iColumns; y++)
				{
					if (y == iStartX && x == iStartY)//Player start.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalStartsSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = starts[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < starts.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += starts[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(starts[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 0));
						childObject.transform.parent = goTerrainFolder.transform;

						if (iFloorNumber == 0)
						{
							player = Instantiate(player.gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(0, 90, 0));
							player.transform.position = new Vector3(nGrid[x, y].v3WorldPosition.x, nGrid[x, y].v3WorldPosition.y + 1, nGrid[x, y].v3WorldPosition.z);
							player.transform.parent = transform.parent;
							player.name = "Player";
							nGrid[x, y].bPlaced = true;
						}
						else
						{
							player.transform.position = new Vector3(nGrid[x, y].v3WorldPosition.x, nGrid[x, y].v3WorldPosition.y + 1, nGrid[x, y].v3WorldPosition.z);
							nGrid[x, y].bPlaced = true;
						}

						playerPos = childObject.transform.position;
					}
					else if (y == iExitX && x == iExitY)//Player exit.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalExitsSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = exits[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < exits.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += exits[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(exits[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 0));
						childObject.transform.parent = goTerrainFolder.transform;

                       exitPos = childObject.transform.position;

                        nGrid[x, y].bPlaced = true;
					}
					else if (y == iEnemySpawnX && x == iEnemySpawnY)//enemy start.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalEnemySpawnsSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = enemySpawns[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < enemySpawns.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += enemySpawns[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(enemySpawns[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 0));
						childObject.transform.parent = goTerrainFolder.transform;

						if (iFloorNumber == 0)
						{
							enemy = Instantiate(enemy.gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(0, 90, 0));
							enemy.transform.position = new Vector3(nGrid[x, y].v3WorldPosition.x, nGrid[x, y].v3WorldPosition.y + 1, nGrid[x, y].v3WorldPosition.z);
							enemy.transform.parent = transform.parent;
							enemy.name = "Enemy";
						}
						else
						{
							enemy.transform.position = new Vector3(nGrid[x, y].v3WorldPosition.x, nGrid[x, y].v3WorldPosition.y + 1, nGrid[x, y].v3WorldPosition.z);
						}
						enemyPos = childObject.transform.position;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == 0 && x == 0)//Corner non rotated.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalCornersSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = corners[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += corners[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(corners[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 0));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == iColumns - 1 && x == 0)//Corner 90.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalCornersSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = corners[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += corners[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(corners[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 90));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == iColumns - 1 && x == iRows - 1)//Corner 180.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalCornersSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = corners[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += corners[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(corners[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 180));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == 0 && x == iRows - 1)//Corner 270.
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalCornersSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = corners[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += corners[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(corners[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 270));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == 0)
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalSidesSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = sides[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += sides[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(sides[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 270));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (x == 0)
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalSidesSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = sides[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += sides[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(sides[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 0));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (y == iColumns - 1)
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalSidesSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = sides[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += sides[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(sides[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 90));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
					else if (x == iRows - 1)
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalSidesSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = sides[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += sides[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(sides[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 90, 180));
						childObject.transform.parent = goTerrainFolder.transform;

						nGrid[x, y].bPlaced = true;
					}
				}
			}
			//obstacles
			int iObjectCount = Random.Range((int)(iRows * 0.25f * iObstacleAmount), (int)(iColumns * 0.75f * iObstacleAmount + 1));

			for (int i = 0; i < iObjectCount; i++)
			{
				int x = Random.Range(1, iColumns - 1);
				int y = Random.Range(1, iColumns - 1);

				if (nGrid[x, y].bPlaced == false)
				{
					// Generate a random position in the list.
					float pick = Random.value * _totalObstaclesSpawnsSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = obstacles[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < obstacles.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += obstacles[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(obstacles[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 0, 0));
					childObject.transform.parent = goTerrainFolder.transform;
					nGrid[x, y].bPlaced = true;

                    // Checking paths after map generation, if the player can't exit and the enemy cant get to the player then we need to regen
                    pathSuccess = true;
                    bool rebuildMap = false;

                    Grid.instance.OnValidate();

                    if (PathRequestManager.Instance == null)
                    {
                       
                    }
                    // Checking path from player to exit
                    PathRequestManager.RequestPath(new PathRequest(playerPos, exitPos, MapGenPathSuccess));

                    if (!pathSuccess)
                    {
                        Debug.Log("Path from player to exit could not be found!");
                        pathSuccess = true;
                        rebuildMap = true;
                    }

                    // Checking path from enemy to player
                    PathRequestManager.RequestPath(new PathRequest(enemyPos, playerPos, MapGenPathSuccess));

                    if (!pathSuccess)
                    {
                        Debug.Log("Path from enemy to player could not be found!");
                        rebuildMap = true;
                    }

                    // If a path could not be found on either of the requests
                    if (rebuildMap)
                    {
                        nGrid[x, y].bPlaced = false;
                        Destroy(childObject);
                    }
                }
			}

			for (int x = 0; x < iRows; x++)
			{
				for (int y = 0; y < iColumns; y++)
				{
					if (nGrid[x, y].bPlaced == false)
					{
						// Generate a random position in the list.
						float pick = Random.value * _totalFloorsSpawnWeight;
						int chosenIndex = 0;
						float cumulativeWeight = floors[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < floors.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += floors[chosenIndex].weight;
						}

						GameObject childObject = Instantiate(floors[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 0, 0));
						childObject.transform.parent = goTerrainFolder.transform;
						//ceiling

						// Generate a random position in the list.
						pick = Random.value * _totalFloorsSpawnWeight;
						chosenIndex = 0;
						cumulativeWeight = floors[0].weight;

						// Step through the list until we've accumulated more weight than this.
						// The length check is for safety in case rounding errors accumulate.
						while (pick > cumulativeWeight && chosenIndex < floors.Length - 1)
						{
							chosenIndex++;
							cumulativeWeight += floors[chosenIndex].weight;
						}

						GameObject childObject2 = Instantiate(floors[chosenIndex].gameObject, nGrid[x, y].v3WorldPosition, Quaternion.Euler(-90, 0, 0));
						childObject2.transform.parent = goTerrainFolder.transform;
						childObject2.transform.position = new Vector3(childObject2.transform.position.x, childObject2.transform.position.y + fNodeHeight - fCeilingOffset, childObject2.transform.position.z);

						nGrid[x, y].bPlaced = true;

					}
				}
			}
		}
	}

    public void MapGenPathSuccess(Vector3[] a_path, bool a_pathSuccess)
    {
        pathSuccess = a_pathSuccess;
    }

    //void OnDrawGizmos()
    //{
    //	Gizmos.DrawWireCube(transform.position, new Vector3(v2GridWorldSize.x, 1, v2GridWorldSize.y));
    //
    //	if (nGrid != null)
    //	{
    //		foreach (MapNode n in nGrid)
    //		{
    //			Gizmos.color = Color.cyan;
    //			Gizmos.DrawCube(n.v3WorldPosition, Vector3.one * (fNodeDiameter - .1f));
    //		}
    //	}
    //}

    private int NewMapSize()//Takes the range of room size and returns a random int in between with a rated result towards the lower end of the spectrum.
	{
		iMapSize++;
		return iMapSize;//Returns an int indicating the amount of rows and columns the floor will have.
	}

	void DestroyPreviousMap()
	{
		GameObject[] mapTiles;
		mapTiles = GameObject.FindGameObjectsWithTag("MapTile");
		for (int i = 0; i < mapTiles.Length; i++)
		{
			Destroy(mapTiles[i]);
		}
	}

	#endregion
}