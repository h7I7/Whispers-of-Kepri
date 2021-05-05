using System;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

using System.Threading;

public class TKGenerator : MonoBehaviour {

	[Header("Room Generation Variables")]
	[Range(1, 100)]
	public int CellsToGenerate = 1;

	[Space(10)]
	[Header("Room Dimension Constraints")]
	public int RoomWidth = 25;
	public int RoomHeight = 25;
	[Space(5)]
	public int MinimumCellWidth = 2;
	public int MaximumCellWidth = 6;
	public int MinimumCellHeight = 2;
	public int MaximumCellHeight = 6;
	public int MinimumCells = 5;

	[Space(10)]
	[Header("Percentage amount of corridors to add in after")]
	[Space(-12)]
	[Header("minimum spanning tree has been created")]
	[Range(0.15f, 0.99f)]
	public float PercentCorridorsToAddInMinimum = 0.15f;
	[Range(0.16f, 1f)]
	public float PercentCorridorsToAddInMaximum = 0.8f;
	private float PercentCorridorsToAddIn;

	[System.Serializable]
	public struct Spawnable
	{
		public GameObject gameObject;
		public float weight;
	}

	[Space(10)]
	[Header("Dungeon prefabs")]
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


	[Space(5)]
	[Tooltip("The object that all the map objects will be placed onto")]
	public Transform MapTransform;

	[Space(10)]
	[Header("Gizmo visualisation")]
	public bool TurnOnDelaunayTriangulation = false;
	public bool TurnOnMinimumSpanningPath = false;
	public bool TurnOnCorridors = true;
	public bool TurnOffAllGizmos = false;

	[Space(10)]
	private List<Cell> _cells;

	[SerializeField]
	private List<Cell> _activeCells = null;

	// Delaunay lists
	private List<Vector2> cellIndices = null;
	private List<LineSegment> m_spanningTree = null;
	private List<LineSegment> m_delaunayTriangulation = null;
	private List<LineSegment> m_corridors = null;

	[Header("If the separation function gets stuck it will try this many")]
	[Space(-12)]
	[Header("times and then restart")]
	public int SeparationTryAmount = 25000;

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
		Generate();
	}

	void Update()
	{
		// If the player presses R then regenerate the level
		if (Input.GetKeyDown(KeyCode.R))
		{
			DestroyMap(MapTransform);
			Generate();
		}
	}

	public void Generate()
	{
		// Redo the rooms creations until we have a good amount of rooms
		do
		{
			// Set up nominal distribution for room sizes
			GenerateCells();
			// Rooms have generated now separate rooms
			SeparateCells();
			// Select the rooms that are above the average size and keeps them while disabling others
			SelectRooms();
		} while (_activeCells.Count < MinimumCells);
		// Relocate all the map items so that the average location of all items is at 0, 0, 0
		RelocateMap();
		// Align all the cells so that they lie on a grid
		AlignCells();
		// Create triangulation for the corridorse between cells
		CreateTriangulation();
		// Calculate corridors
		CalculateCorridors();
		// Add in rooms if they collide with corridors
		AddInCells();

		// Start actually loading in the map
		PlaceCellPrefabs();
		//PlaceCorridorPrefabs();
	}

	// Generate a bunch of cells at random within the confines of the map
	private void GenerateCells()
	{
		// Seed the random fro getting a position on a quad
		PMB_Random.Seed((long)DateTime.Now.Millisecond);

		// Initialise the list of cells
		_cells = new List<Cell>(CellsToGenerate);

		// Create the amount of cells that we need
		for (int i = 0; i < CellsToGenerate; ++i)
		{
			// Add the newely created cell to the cell list
			_cells.Add(new Cell());
			// Set the cell dimension constraints
			_cells[i].SetDimensionConstraints(MinimumCellWidth, MaximumCellWidth, MinimumCellHeight, MaximumCellHeight);
			// Create the random dimensions of the cell
			_cells[i].GenerateCell();

			// Adjust the cells position to be placed somewhere within a box that is 10 times the size of it's footprint
			Vector2 cellPos = Vector2.zero;
			Vector2 min = new Vector2(-RoomWidth, -RoomHeight);
			Vector2 max = new Vector2(RoomWidth, RoomHeight);
			// Get a random position in the dungeon map area
			PMB_Random.RandomPointInQuad(min, max, out cellPos);
			// Set the position of the cell
			_cells[i].Position = new Vector3(cellPos.x, 0f, cellPos.y);
			// Align its position to the nearest integer
			_cells[i].RoundToNearestTilePosition();
		}
	}

	// Separating cells
	private void SeparateCells()
	{
		// An int to count how many times we have tried to separate the cells
		// There is a bug in the code where if two cells are in the exact same spot
		// it does not move then and gets stuck so if we go over a certain amount
		// of attempts to separate the cells we need to restart the dungeon generation
		int iterations = 0;
		// A temporary variable to store which direction a cell should move upon separation
		Vector3 moveVec;
		// A bool for determining whether there is still collision
		bool isInCollision = false;
		do
		{
			// Sort the cells based on their distance to the center of the map
			_cells.Sort((x, y) => x.DistanceToCenter.CompareTo(y.DistanceToCenter));

			isInCollision = false;
			// For each of the cells that we generated
			foreach (Cell currentC in _cells)
			{
				// Get the current position of the cell (center)
				Vector3 crPos = currentC.Position;
				// Get the bounds of the cells (width/height from center)
				Vector3 crBounds = currentC.Size * 0.5f;
				// The velocity of this cell
				Vector3 cellVel = new Vector3(0f, 0f, 0f);
				// An int to store how many neighbors it is colliding with
				int neighbourCount = 0;
				
				// For each other cell in the cell list
				foreach (Cell c in _cells)
				{
					// If this cell is not the current cell
					if (c != currentC)
					{
						// Get the position (center)
						Vector3 cellPos = c.Position;
						// Get the bounds of the cell (width/height from center)
						Vector3 cellBounds = c.Size * 0.5f;
						// AABB collision testing
						if (cellPos.x - cellBounds.x - 0.5f < crPos.x + crBounds.x + 0.5f &&
							cellPos.x + cellBounds.x + 0.5f > crPos.x - crBounds.x - 0.5f &&
							cellPos.z - cellBounds.z - 0.5f < crPos.z + crBounds.z + 0.5f &&
							cellPos.z + cellBounds.z + 0.5f > crPos.z - crBounds.z - 0.5f)
						{
							// Get the direction that this cell is relative to the current cell
							Vector3 separation = (cellPos - crPos).normalized;
							// Calculate the best distance to move so that we are no longer colliding with this cell
							Vector3 optimalDistance = crBounds + cellBounds;

							// Set the velocity so that we are moving the correct distance in the correct direction to separate the cells
							cellVel += Vector3.Scale(separation, optimalDistance);
							// Add a neighbor
							++neighbourCount;
							// If we have too many neighbors then break out of the loop
							if (neighbourCount > 6)
							{
								break;
							}
						}
					}
					// If we have collided then we need to move the cell
					if (neighbourCount > 0)
					{
						// Set the collision to be true so that the loop will continue another iteration
						isInCollision = true;
						// Scale the current velocity by the amount of cells that we are colliding with
						cellVel /= neighbourCount;
						// Normalise the movement vector
						c.MoveVector = cellVel.normalized;

						// Move cell, move here to help prevent overlap issues
						moveVec = new Vector3(Mathf.Ceil(Mathf.Abs(c.MoveVector.x)),
							Mathf.Ceil(Mathf.Abs(c.MoveVector.y)),
							Mathf.Ceil(Mathf.Abs(c.MoveVector.z)));
						moveVec.x *= Mathf.Sign(c.MoveVector.x);
						moveVec.y *= Mathf.Sign(c.MoveVector.y);
						moveVec.z *= Mathf.Sign(c.MoveVector.z);

						// Move the cell
						c.Position += moveVec;
					}
				}

				// Count the iterations of the separation
				iterations++;
			}

			// If the separation of the cells is taking too long then break out of the loop and try to generate a new map
			if (iterations > SeparationTryAmount)
			{
				Generate();
				isInCollision = false;
			}
		} while (isInCollision);
	}

	// Select the cells that are above the average size of all cells in the current cell list
	// and discard the rest
	private void SelectRooms()
	{
		// Initialise the active cells list
		_activeCells = new List<Cell>();
		// Estimate the average volume of cells
		float averageVolume = MaximumCellWidth * MaximumCellHeight * 0.3f;

		// For each cell in the cells list
		foreach (Cell c in _cells)
		{
			// Get the dimensions of the cell
			Vector3 Dimensions = c.Size;
			// Calculate the cell volume
			float cellVolume = (Dimensions.x * Dimensions.z);
			// Compare the two and if the cell is above the average volume them add it the new list
			if (cellVolume > averageVolume)
			{
				_activeCells.Add(c);
			}
		}
	}

	// Changing the average position of all cells to be 0, 0
	private void RelocateMap()
	{
		// A variable for the average location of all cells
		Vector3 averageLocation = Vector3.zero;

		// For each cell in the main cells list add the position to the average locations vector
		foreach (Cell c in _cells)
		{
			averageLocation.x += c.Position.x;
			averageLocation.z += c.Position.z;
		}

		// Then divide the vector by the amount of cells in the list to get an average position
		averageLocation.x = averageLocation.x / _cells.Count;
		averageLocation.z = averageLocation.z / _cells.Count;

		// We can then subtract this position from every cell making the average center of every cell 0, 0
		foreach (Cell c in _cells)
		{
			c.Position -= averageLocation;
		}
	}

	// Align the cells to a grid
	private void AlignCells()
	{
		foreach(Cell c in _cells)
		{
			c.Position.x = Mathf.Round(c.Position.x);
			c.Position.z = Mathf.Round(c.Position.z);

			if (c._width % 2 == 1)
			{
				//c.Position.x += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
				c.Position.x += 0.5f;
			}

			if (c._height % 2 == 1)
			{
				//c.Position.z += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
				c.Position.z += 0.5f;
			}
		}
	}

	// Create Delaunay triangulation for all the active cells in the scene
	private void CreateTriangulation()
	{
		// If we have a list of cells
		if (_activeCells != null)
		{
			// A list for all the cell positions in Vector2 format
			cellIndices = new List<Vector2>();
			// A list of colors that will all be black
			List<uint> colors = new List<uint>();

			// For each of the active cells add their position to cellIndices and add a blank color to colors
			foreach(Cell c in _activeCells)
			{
				cellIndices.Add(new Vector2(c.Position.x, c.Position.z));
				colors.Add(0);
			}

			// Use unity Delaunay generation routines
			Delaunay.Voronoi v = new Delaunay.Voronoi(cellIndices, colors, new Rect(0, 0, 100, 100));

			// Create a minimum spanning tree from the Delaunay triangulation
			m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);
			m_delaunayTriangulation = v.DelaunayTriangulation();

			// Turn on a few of the connections that were present in the delaunay triangulation
			// that aren't in the spanning tree
			int treeTriDiff = m_delaunayTriangulation.Count - m_spanningTree.Count;
			treeTriDiff = (int)((float)treeTriDiff * UnityEngine.Random.Range(PercentCorridorsToAddInMinimum, PercentCorridorsToAddInMaximum));
			foreach(LineSegment dlLine in m_delaunayTriangulation)
			{
				if (!m_spanningTree.Contains(dlLine))
				{
					m_spanningTree.Add(dlLine);
					treeTriDiff--;
					if (treeTriDiff <= 0)
					{
						break;
					}
				}
			}
		}
	}

	// Calculate where the corridors should be
	private void CalculateCorridors()
	{
		// A list to contain all the corridors we will make
		m_corridors = new List<LineSegment>();

		// For each of the lines in the spanning tree we need to find two more lines that are axis aligned
		// We are going to treat these lines like a hypotenuse of a right angle triangle and then attempt
		// to find the opposite and adjacent sides for the rest of the triangle
		foreach (LineSegment stline in m_spanningTree)
		{
			// Get the 'left' position of the line
			Vector2 left = stline.p0.Value;
			// Get the 'right' position of the line
			Vector2 right = stline.p1.Value;

			// Two bools that allow us to early out of the search below
			bool line1Complete = false;
			bool line2Complete = false;

			// Search all of the active cells to see which one has this line connecting it
			// This is to move the line position if it happens to have an even height or width
			// so that the line can be aligned to the center of one of the squares of the cell
			foreach (Cell c in _activeCells)
			{
				if (c.Position == new Vector3(left.x, 0f, left.y))
				{
					if (c._width % 2 == 0)
					{
						left.x += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
					}
					if (c._height % 2 == 0)
					{
						left.y += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
					}
					line1Complete = true;
				}
				if (c.Position == new Vector3(right.x, 0f, right.y))
				{
					if (c._width % 2 == 0)
					{
						right.x += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
					}
					if (c._height % 2 == 0)
					{
						right.y += UnityEngine.Random.Range((int)0, (int)1) - 0.5f;
					}
					line2Complete = true;
				}

				if (line1Complete && line2Complete)
					break;
			}

			// An array for storing the third position of the 'triangle'
			Vector2[] middles = new Vector2[2];

			// This randomly decides of the triangle will point down or up
			if (UnityEngine.Random.Range(0, 1) == 0)
			{
				middles[0] = new Vector2(right.x, left.y);
				middles[1] = new Vector2(left.x, right.y);
			}
			else
			{
				middles[1] = new Vector2(right.x, left.y);
				middles[0] = new Vector2(left.x, right.y);
			}

			// Ints to check how many cells in the _cells list the lines are colliding with
			// If the triangle it colliding with too many _cells cells then we will switch the direction of the triangle
			int middles1Col = 0;
			int middles2Col = 0;

			LineSegment corridor1 = new LineSegment(left, middles[0]);
			LineSegment corridor2 = new LineSegment(middles[0], right);

			foreach (Cell c in _activeCells)
			{
				if (c.CheckCorridorCollision(corridor1, corridor2))
				{
					middles1Col++;
				}
			}

			corridor1 = new LineSegment(left, middles[1]);
			corridor2 = new LineSegment(middles[1], right);

			foreach (Cell c in _activeCells)
			{
				if (c.CheckCorridorCollision(corridor1, corridor2))
				{
					middles2Col++;
				}
			}

			if (middles1Col < middles2Col)
			{
				corridor1 = new LineSegment(left, middles[0]);
				corridor2 = new LineSegment(middles[0], right);
			}
			else
			{
				corridor1 = new LineSegment(left, middles[1]);
				corridor2 = new LineSegment(middles[1], right);
			}

			// Add the corridors to the corridors list
			m_corridors.Add(corridor1);
			m_corridors.Add(corridor2);
		}
	}

	// Add in _cells cells that have corridors running through them
	private void AddInCells()
	{
		foreach(LineSegment cLine in m_corridors)
		{
			foreach(Cell c in _cells)
			{
				if (_activeCells.Contains(c))
				{
					continue;
				}

				if (c.CheckCorridorCollision(cLine))
				{
					_activeCells.Add(c);
				}
			}
		}
	}
	
	// Spawning all the prefabs needed for cells
	private void PlaceCellPrefabs()
	{
		List<GameObject> objects;

		int cellCount = 0;

		GameObject cell;

		foreach (Cell c in _activeCells)
		{
			objects = new List<GameObject>();

			float w = c.floorPositions[0].x;
			float h = c.floorPositions[0].z;

			cell = new GameObject("Cell " + cellCount.ToString());
			cell.transform.SetParent(MapTransform);
			++cellCount;

			foreach (Vector3 pos in c.floorPositions)
			{
				//Bottom left corner
				if (pos.x == w && pos.z == h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalCornersSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = corners[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += corners[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(corners[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 0f));
				}
				//Top left corner
				else if (pos.x == w && pos.z == -h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalCornersSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = corners[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += corners[chosenIndex].weight;
					}

					GameObject corner = Instantiate(corners[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 90f));
				}
				//Left Wall
				else if (pos.x == w)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalSidesSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = sides[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += sides[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(sides[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 0f));
				}

				//Bottom Right corner
				else if (pos.x == -w && pos.z == h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalCornersSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = corners[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += corners[chosenIndex].weight;
					}

					GameObject corner = Instantiate(corners[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 270f));
				}
				//Top Right corner
				else if (pos.x == -w && pos.z == -h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalCornersSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = corners[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < corners.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += corners[chosenIndex].weight;
					}

					GameObject corner = Instantiate(corners[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 180f));
				}
				//Right wall
				else if (pos.x == -w)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalSidesSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = sides[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += sides[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(sides[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 180f));
				}
				//Bottom wall
				else if (pos.z == h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalSidesSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = sides[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += sides[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(sides[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 270f));
				}
				//Top wall
				else if (pos.z == -h)
				{
					// Generate a random position in the list.
					float pick = UnityEngine.Random.value * _totalSidesSpawnWeight;
					int chosenIndex = 0;
					float cumulativeWeight = sides[0].weight;

					// Step through the list until we've accumulated more weight than this.
					// The length check is for safety in case rounding errors accumulate.
					while (pick > cumulativeWeight && chosenIndex < sides.Length - 1)
					{
						chosenIndex++;
						cumulativeWeight += sides[chosenIndex].weight;
					}

					GameObject childObject = Instantiate(sides[chosenIndex].gameObject, c.Position + pos, Quaternion.Euler(-90f, 90f, 90f));
				}

				
			}
		}

	}

	private void PlaceCorridorPrefabs()
	{
		GameObject corridor;
		int corridorCount = 0;

		foreach (LineSegment seg in m_corridors)
		{
			corridor = new GameObject();
			corridor.transform.SetParent(MapTransform);
			corridor.name = "corridor " + corridorCount.ToString();
			corridorCount++;
			
			Vector2 vLength = seg.p0.Value - seg.p1.Value;
			float length = (vLength.x > vLength.y) ? vLength.x : vLength.y;

			if (vLength.x > vLength.y)
			{
				for (int i = 0; i < length; ++i)
				{
					Vector3 pos = new Vector3(seg.p0.Value.x - i, 0f, seg.p0.Value.y);
					GameObject floor = Instantiate(floors[0].gameObject, corridor.transform);
					floor.transform.position = pos;
				}
			}
			else
			{
				
			}
		}
	}

	private void DestroyMap(Transform a_mapTransform, bool DestroySelf = false)
	{
		for (int i = 0; i < a_mapTransform.childCount; ++i)
		{
			if (a_mapTransform.GetChild(i).childCount != 0)
			{
				DestroyMap(a_mapTransform.GetChild(i), true);
			}

			Destroy(a_mapTransform.GetChild(i).gameObject);
		}

		if (DestroySelf)
			Destroy(a_mapTransform.gameObject);
	}

	void OnDrawGizmos()
	{
		if (TurnOffAllGizmos)
			return;

		// Draw cells
		if (_activeCells != null)
		{
			foreach (Cell c in _activeCells)
			{
				Gizmos.color = Color.white;
				//Gizmos.DrawCube(c.Position, c.Size + Vector3.up);

				foreach(Vector3 pos in c.floorPositions)
				{
					Gizmos.DrawCube(c.Position + pos, Vector3.one * 0.75f);
				}

				for (int i = 0; i <= c.Size.x; ++i)
				{
					if (i == 0 || i == c.Size.x)
						Gizmos.color = Color.red;
					else
						Gizmos.color = Color.white;

					Gizmos.DrawLine(new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * 0.5f + c.Position.z), new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * -0.5f + c.Position.z));
				}
				for (int i = 0; i <= c.Size.z; ++i)
				{
					if (i == 0 || i == c.Size.z)
						Gizmos.color = Color.red;
					else
						Gizmos.color = Color.white;

					Gizmos.DrawLine(new Vector3(c.Size.x * 0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z), new Vector3(c.Size.x * -0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z));
				}
			}
		}

		// Draw triangulation
		if (m_delaunayTriangulation != null && TurnOnDelaunayTriangulation)
		{
			Gizmos.color = Color.magenta;
			for (int i = 0; i < m_delaunayTriangulation.Count; ++i)
			{
				Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
				Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
				Gizmos.DrawLine(new Vector3(left.x, 0f, left.y), new Vector3(right.x, 0f, right.y));
			}
		}

		// Draw minimum spanning tree
		if (m_spanningTree != null && TurnOnMinimumSpanningPath)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < m_spanningTree.Count; ++i)
			{
				LineSegment seg = m_spanningTree[i];
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine(new Vector3(left.x, 0f, left.y), new Vector3(right.x, 0f, right.y));
			}
		}

		// Drawing corridors
		if (m_corridors != null && TurnOnCorridors)
		{
			Gizmos.color = Color.blue;
			for (int i = 0; i < m_corridors.Count; ++i)
			{
				LineSegment seg = m_corridors[i];
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine(new Vector3(left.x, 0f, left.y), new Vector3(right.x, 0f, right.y));
			}
		}
	}
}
