using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Delaunay;
using Delaunay.Geo;

using System.Threading;

public delegate void SetThreadNotRunningDelegate(bool a_threadState);

public class FloorPosition
{
    public Vector3 Position;

    public bool North = false;
    public bool East = false;
    public bool South = false;
    public bool West = false;

    public int WallCount = 0;
}

public class TKGenerator : MonoBehaviour
{

    [HideInInspector]
    public static TKGenerator instance = null;

    [Header("Room Generation Variables")]
    [SerializeField]
    private int m_cellsToGenerate = 150;
    public int CellsToGenerate
    {
        get { return m_cellsToGenerate; }
    }


    [Space(10)]
    [Header("Room Dimension Constraints")]
    [SerializeField]
    private int m_roomWidth = 50;
    public int RoomWidth
    {
        get { return m_roomWidth; }
    }

    [SerializeField]
    private int m_roomHeight = 50;
    public int RoomHeight
    {
        get { return m_roomHeight; }
    }

    [Space(5)]
    [SerializeField]
    private int m_minimumCellWidth = 2;
    public int MinimumCellWidth
    {
        get { return m_minimumCellWidth; }
    }

    [SerializeField]
    private int m_maximumCellWidth = 6;
    public int MaximumCellWidth
    {
        get { return m_maximumCellWidth; }
    }

    [SerializeField]
    private int m_minimumCellHeight = 2;
    public int MinimumCellHeight
    {
        get { return m_minimumCellHeight; }
    }

    [SerializeField]
    private int m_maximumCellHeight = 6;
    public int MaximumCellHeight
    {
        get { return m_maximumCellHeight; }
    }

    [SerializeField]
    private int m_minimumCells = 5;
    public int MinimumCells
    {
        get { return m_minimumCells; }
    }


    [Space(5)]
    [Header("Changes inbetween levels")]
    [SerializeField]
    private int m_mapSizeIncrease;
    [SerializeField]
    private int m_cellAmountIncrease;
    [SerializeField]
    private int m_level;
    [SerializeField]
    private int m_levelToStartSpawningEnemy;

    [Space(10)]
    [Header("Seeding Properties")]
    [SerializeField]
    private bool m_useCustomSeed = false;
    public bool UseCustomSeed
    {
        get { return m_useCustomSeed; }
    }

    [SerializeField]
    private float m_seed;
    public float Seed
    {
        get { return m_seed; }
        set { m_seed = value; }
    }


    [Space(10)]
    [Header("Percentage amount of corridors to add in after")]
    [Space(-12)]
    [Header("minimum spanning tree has been created")]
    [Range(0.15f, 0.99f)]
    [SerializeField]
    private float m_percentCorridorsToAddInMinimum = 0.15f;
    public float PercentCorridorsToAddInMinimum
    {
        get { return m_percentCorridorsToAddInMinimum; }
    }

    [Range(0.16f, 1f)]
    [SerializeField]
    private float m_percentCorridorsToAddInMaximum = 0.8f;
    public float PercentCorridorsToAddInMaximum
    {
        get { return m_percentCorridorsToAddInMaximum; }
    }


    [Space(10)]
    [Header("Loading screen settings")]
    [SerializeField]
    private Canvas m_loadingCanvas;
	[SerializeField]
	private Canvas m_loadingCanvas2;
	[SerializeField]
	private Canvas m_loadingCanvas3;

	[Header("Game over text settings")]
	[SerializeField]
	private Text m_gameOverText;
	[SerializeField]
	private Text m_gameOverText2;
	[SerializeField]
	private Text m_gameOverText3;


	[System.Serializable]
    public class Spawnable
    {
        [SerializeField]
        private GameObject gameObject;
        public GameObject GameObject
        {
            get { return gameObject; }
        }

        [SerializeField]
        private float percentChanceToDrop;
        public float PercentChanceToDrop
        {
            get { return percentChanceToDrop; }
        }

        [SerializeField]
        private Vector3 m_offSetPosition;
        public Vector3 OffsetPosition
        {
            get { return m_offSetPosition; }
        }

        [SerializeField]
        private Vector3 m_offSetRotation;
        public Vector3 OffSetRotation
        {
            get { return m_offSetRotation; }
        }

        [SerializeField]
        private bool m_doNotClearPositionWhenPlaced;
        public bool DoNotClearPositionWhenPlaced
        {
            get { return m_doNotClearPositionWhenPlaced; }
        }
    }


    [Space(10)]
    [Header("Dungeon prefabs")]
    [SerializeField]
    private Spawnable[] m_walls;

    [SerializeField]
    private Spawnable[] m_floors;           // The game object that will be used for the floor of the map.

    [SerializeField]
    private GameObject m_exitPrefab;            // The game object that will be used for the exit of the map.

    [SerializeField]
    private Spawnable[] m_cellEdgeWallPrefabs;

    [SerializeField]
    private Spawnable[] m_cellEdgeFloorPrefabs;

    [SerializeField]
    private Spawnable[] m_cellMiddleFloorPrefabs;


    [Space(5)]
    public Transform m_player;
    private Rigidbody m_playerRbdy;

    public Transform m_enemy;
    private Rigidbody m_enemyRbdy;
    public GameObject m_exit;
    private Unit m_enemyUnit;
    [Tooltip("The object that all the map objects will be placed onto")]
    public Transform m_mapTransform;


    [Space(10)]
    [Header("Gizmo visualisation")]
    [SerializeField]
    private bool m_turnOnCells = false;
    [SerializeField]
    private bool m_turnOnActiveCells = false;
    [SerializeField]
    private bool m_turnOnTileMapVisualisation = false;
    [SerializeField]
    private bool m_turnOnDelaunayTriangulation = false;
    [SerializeField]
    private bool m_turnOnMinimumSpanningPath = false;
    [SerializeField]
    private bool m_turnOnCorridors = false;
    [SerializeField]
    private bool m_turnOffAllGizmos = false;
    [SerializeField]
    private float m_yAxisGizmoOffset = 1f;

    [Space(10)]
    private List<Cell> m_cells;
    public List<Cell> Cells
    {
        get { return m_cells; }
        set { m_cells = value; }
    }

    [SerializeField]
    private List<Cell> m_activeCells = null;
    public List<Cell> ActiveCells
    {
        get { return m_activeCells; }
        set { m_activeCells = value; }
    }


    // Delaunay lists
    private List<Vector2> m_cellIndices = null;
    public List<Vector2> CellIndices
    {
        get { return m_cellIndices; }
        set { m_cellIndices = value; }
    }

    private List<LineSegment> m_spanningTree = null;
    public List<LineSegment> SpanningTree
    {
        get { return m_spanningTree; }
        set { m_spanningTree = value; }
    }

    private List<LineSegment> m_delaunayTriangulation = null;
    public List<LineSegment> DelaunayTriangulation
    {
        get { return m_delaunayTriangulation; }
        set { m_delaunayTriangulation = value; }
    }

    private List<LineSegment> m_corridors = null;
    public List<LineSegment> Corridors
    {
        get { return m_corridors; }
        set { m_corridors = value; }
    }

    private List<FloorPosition> m_emptyCellPositions;
    public List<FloorPosition> EmptyCellPositions
    {
        get { return m_emptyCellPositions; }
        set { m_emptyCellPositions = value; }
    }

    private List<FloorPosition> m_emptyCorridorPositions;
    public List<FloorPosition> EmptyCorridorPositions
    {
        get { return m_emptyCorridorPositions; }
        set { m_emptyCorridorPositions = value; }
    }

    void Start()
    {
        m_playerRbdy = m_player.GetComponent<Rigidbody>();
        m_enemyRbdy = m_enemy.GetComponent<Rigidbody>();
        m_enemyUnit = m_enemy.GetComponent<Unit>();

		m_gameOverText.text = m_level + " Floors!";
		m_gameOverText2.text = m_level + " Floors!";
		m_gameOverText3.text = m_level + " Floors!";

		if (instance == null)
            instance = this;

        // Sorting the floor and wall prefab arrays so they can be used as a drop table later
        Array.Sort(m_cellEdgeFloorPrefabs, delegate (Spawnable x, Spawnable y) { return x.PercentChanceToDrop.CompareTo(y.PercentChanceToDrop); });
        Array.Sort(m_cellEdgeWallPrefabs, delegate (Spawnable x, Spawnable y) { return x.PercentChanceToDrop.CompareTo(y.PercentChanceToDrop); });

        // Generating the map
        Generate();
    }

    void Update()
    {
        // If the player presses R then regenerate the level
        if (Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }

        if (m_loadingCanvas.gameObject.activeInHierarchy == true)
        {
            if (m_enemyCanReachPlayer && m_playerCanReachExit)
            {
                // Unpause the game
                //PauseManager.instance.UnPause();

                // Iterate the level variables
                ++m_level;

				m_gameOverText.text = m_level + " Floors!";
				m_gameOverText2.text = m_level + " Floors!";
				m_gameOverText3.text = m_level + " Floors!";

				m_roomWidth += m_cellAmountIncrease;
                m_roomHeight += m_cellAmountIncrease;

                m_cellsToGenerate += m_cellAmountIncrease;

                // Disable the loading canvas
                m_loadingCanvas.gameObject.SetActive(false);
				m_loadingCanvas2.gameObject.SetActive(false);
				m_loadingCanvas3.gameObject.SetActive(false);

			}
        }
    }

    public void Generate()
    {
        //StopAllCoroutines();
        StartGenerate();
    }

    private bool m_enemyCanReachPlayer;
    private bool m_playerCanReachExit;

    private void StartGenerate()
    {
        // Pause the game
        //if (PauseManager.instance != null)
        //    PauseManager.instance.Pause();

        m_enemyCanReachPlayer = false;
        m_playerCanReachExit = false;

        m_loadingCanvas.gameObject.SetActive(true);
		m_loadingCanvas2.gameObject.SetActive(true);
		m_loadingCanvas3.gameObject.SetActive(true);

		// Setting the delegate for the TKThread generator
		//SetThreadNotRunningDelegate del = new SetThreadNotRunningDelegate(SetThreadNotRunning);
		//SetThreadRunningDelegate(del);

		m_playerRbdy.isKinematic = true;
        m_enemyRbdy.isKinematic = true;
        m_enemyUnit.enabled = false;

        // Delete all the objects inside of the map transform if there is any
        DestroyMap(m_mapTransform);

        // I'm using a thread here to load everything in because I want there to be a loading screen
        // which is updated by the main thread whilst everything loads.
        // This could have been achieved with a coroutine however it was taking up to 13 seconds
        // to load a 100 cell map.
        // With the thread it now takes, usually, less than 1/2.
        GenerateMap();
    }

    private void EnemyCanReachPlayer(Vector3[] a_arg1, bool a_pathSuccess)
    {
        if (a_pathSuccess)
            m_enemyCanReachPlayer = true;
        else
            Generate();
    }

    private void PlayerCanReachExit(Vector3[] a_arg1, bool a_pathSuccess)
    {
        if (a_pathSuccess)
            m_playerCanReachExit = true;
        else
            Generate();
    }

    private void DestroyMap(Transform a_mapTransform, bool DestroySelf = false)
    {
        for (int i = a_mapTransform.childCount - 1; i >= 0; --i)
        {
            if (a_mapTransform.GetChild(i).childCount != 0)
            {
                DestroyMap(a_mapTransform.GetChild(i), true);
            }

            Destroy(a_mapTransform.GetChild(i).gameObject);
        }

        // Destroying all map objects
        // Search all of the children in the reference
        for (int i = a_mapTransform.childCount - 1; i >= 0; --i)
        {
            // Destroy the object
            Destroy(m_mapTransform.GetChild(i).gameObject);
        }

        if (DestroySelf)
            Destroy(a_mapTransform.gameObject);
    }

    // Generate a bunch of cells at random within the confines of the map
    private void GenerateMap()
    {
        if (!UseCustomSeed)
            Seed = PMB_Random.Random();

        // Seed the random fro getting a position on a quad
        PMB_Random.Seed((long)Seed);

        // Initialise the list of cells
        Cells = new List<Cell>();

        // Create the amount of cells that we need
        for (int i = 0; i < CellsToGenerate; ++i)
        {
            // Add the newely created cell to the cell list
            Cells.Add(new Cell());
            // Set the cell dimension constraints
            Cells[i].SetDimensionConstraints(MinimumCellWidth, MaximumCellWidth, MinimumCellHeight, MaximumCellHeight);
            // Create the random dimensions of the cell
            Cells[i].GenerateCell();

            // Adjust the cells position to be placed somewhere within a box that is 10 times the size of it's footprint
            Vector2 cellPos = Vector2.zero;
            Vector2 min = new Vector2(-(RoomWidth * 0.5f), -(RoomHeight * 0.5f));
            Vector2 max = new Vector2(RoomWidth * 0.5f, RoomHeight * 0.5f);
            // Get a random position in the dungeon map area
            PMB_Random.RandomPointInQuad(min, max, out cellPos);
            // Set the position of the cell
            Cells[i].Position = new Vector3(cellPos.x, 0f, cellPos.y);
            // Align its position to the nearest integer
            Cells[i].RoundToNearestTilePosition();
        }

        SeparateCells();
    }

    // Separating cells
    private void SeparateCells()
    {
        // Some random amount of moves a cell can make before being deleted
        int maxCellMoves = (m_cellsToGenerate * m_cellsToGenerate) * 4;

        // A temporary variable to store which direction a cell should move upon separation
        Vector3 moveVec;
        // A bool for determining whether there is still collision
        bool isInCollision = false;
        do
        {
            // Sort the cells based on their distance to the center of the map
            Cells.Sort((x, y) => x.DistanceToCenter.CompareTo(y.DistanceToCenter));
            isInCollision = false;
            // For each of the cells that we generated
            foreach (Cell currentCell in Cells)
            {
                // If a cell has moved too many times its stuck so remove it
                if (currentCell.moves > maxCellMoves)
                {
                    Cells.Remove(currentCell);
                    isInCollision = true;
                    break;
                }

                // Get the current position of the cell (center)
                Vector3 crPos = currentCell.Position;
                // Get the bounds of the cells (width/height from center)
                Vector3 crBounds = currentCell.Size * 0.5f;
                // The velocity of this cell
                Vector3 cellVel = new Vector3(0f, 0f, 0f);
                // An int to store how many neighbors it is colliding with
                int neighbourCount = 0;

                // For each other cell in the cell list
                foreach (Cell cell in Cells)
                {

                    // If this cell is not the current cell
                    if (cell != currentCell)
                    {
                        // Get the position (center)
                        Vector3 cellPos = cell.Position;
                        // Get the bounds of the cell (width/height from center)
                        Vector3 cellBounds = cell.Size * 0.5f;
                        // AABB collision testing
                        if (cellPos.x - cellBounds.x < crPos.x + crBounds.x &&
                            cellPos.x + cellBounds.x > crPos.x - crBounds.x &&
                            cellPos.z - cellBounds.z < crPos.z + crBounds.z &&
                            cellPos.z + cellBounds.z > crPos.z - crBounds.z)
                        {
                            // Get the direction that this cell is relative to the current cell
                            Vector3 separation = (cellPos - crPos).normalized;
                            if (separation.sqrMagnitude < 0.001f)
                            {
                                //separation = new Vector3(UnityEngine.Random.Range(-cellBounds.x, cellBounds.x), 0f, UnityEngine.Random.Range(-cellBounds.z, cellBounds.z)).normalized;
                                separation = ((cellPos + (cellBounds * 0.5f)) - crPos).normalized;
                            }
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
                }

                // If we have collided then we need to move the cell
                if (neighbourCount > 0)
                {
                    // Set the collision to be true so that the loop will continue another iteration
                    isInCollision = true;
                    cellVel /= neighbourCount;
                    // Normalise the movement vector
                    currentCell.MoveVector = -cellVel.normalized;

                    // Move cell, move here to help prevent overlap issues
                    moveVec = new Vector3(Mathf.Ceil(Mathf.Abs(currentCell.MoveVector.x)),
                        Mathf.Ceil(Mathf.Abs(currentCell.MoveVector.y)),
                        Mathf.Ceil(Mathf.Abs(currentCell.MoveVector.z)));
                    moveVec.x *= Mathf.Sign(currentCell.MoveVector.x);
                    moveVec.y *= Mathf.Sign(currentCell.MoveVector.y);
                    moveVec.z *= Mathf.Sign(currentCell.MoveVector.z);

                    moveVec.Scale(currentCell.Size);
                    // Move the cell
                    currentCell.Position += moveVec;

                    ++currentCell.moves;
                }
            }
        } while (isInCollision);

        SelectRooms();
    }

    // Select the cells that are above the average size of all cells in the current cell list
    // and discard the rest
    private void SelectRooms()
    {
        // Initialise the active cells list
        ActiveCells = new List<Cell>();
        // Estimate the average volume of cells
        float averageVolume = MaximumCellWidth * MaximumCellHeight * 0.3f;

        // For each cell in the cells list
        foreach (Cell c in Cells)
        {
            // Get the dimensions of the cell
            Vector3 Dimensions = c.Size;
            // Calculate the cell volume
            float cellVolume = (Dimensions.x * Dimensions.z);
            // Compare the two and if the cell is above the average volume them add it the new list
            if (cellVolume > averageVolume)
            {
                ActiveCells.Add(c);
            }
        }

        if (ActiveCells.Count < MinimumCells)
        {
            Generate();
            return;
        }

        // Relocate all the map items so that the average location of all items is at 0, 0, 0
        RelocateMap();
    }

    // Changing the average position of all cells to be 0, 0
    private void RelocateMap()
    {
        // A variable for the average location of all cells
        Vector3 averageLocation = Vector3.zero;

        // For each cell in the main cells list add the position to the average locations vector
        foreach (Cell c in Cells)
        {
            averageLocation.x += c.Position.x;
            averageLocation.z += c.Position.z;
        }

        // Then divide the vector by the amount of cells in the list to get an average position
        averageLocation.x = averageLocation.x / Cells.Count;
        averageLocation.z = averageLocation.z / Cells.Count;

        // We can then subtract this position from every cell making the average center of every cell 0, 0
        foreach (Cell c in Cells)
        {
            c.Position -= averageLocation;
        }

        // Align all the cells so that they lie on a grid
        AlignCells();
    }

    // Align the cells to a grid
    private void AlignCells()
    {
        foreach (Cell c in Cells)
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

        CreateTriangulation();
    }

    // Create Delaunay triangulation for all the active cells in the scene
    private void CreateTriangulation()
    {
        // If we have a list of cells
        if (ActiveCells != null)
        {
            // A list for all the cell positions in Vector2 format
            CellIndices = new List<Vector2>();
            // A list of colors that will all be black
            List<uint> colors = new List<uint>();

            // For each of the active cells add their position to cellIndices and add a blank color to colors
            foreach (Cell c in ActiveCells)
            {
                CellIndices.Add(new Vector2(c.Position.x, c.Position.z));
                colors.Add(0);

            }

            // Use unity Delaunay generation routines
            Delaunay.Voronoi v = new Delaunay.Voronoi(CellIndices, colors, new Rect(0, 0, 100, 100));

            // Create a minimum spanning tree from the Delaunay triangulation
            SpanningTree = v.SpanningTree(KruskalType.MINIMUM);
            DelaunayTriangulation = v.DelaunayTriangulation();

            // Turn on a few of the connections that were present in the delaunay triangulation
            // that aren't in the spanning tree
            int treeTriDiff = DelaunayTriangulation.Count - SpanningTree.Count;
            treeTriDiff = (int)((float)treeTriDiff * PMB_Random.Range(PercentCorridorsToAddInMinimum, PercentCorridorsToAddInMaximum));
            foreach (LineSegment dlLine in DelaunayTriangulation)
            {
                if (!SpanningTree.Contains(dlLine))
                {
                    SpanningTree.Add(dlLine);
                    treeTriDiff--;
                    if (treeTriDiff <= 0)
                    {
                        break;
                    }
                }
                //yield return null;
            }
        }

        // Create triangulation for the corridorse between cells
        CalculateCorridors();
    }

    // Calculate where the corridors should be
    private void CalculateCorridors()
    {
        // A list to contain all the corridors we will make
        Corridors = new List<LineSegment>();

        // For each of the lines in the spanning tree we need to find two more lines that are axis aligned
        // We are going to treat these lines like a hypotenuse of a right angle triangle and then attempt
        // to find the opposite and adjacent sides for the rest of the triangle
        foreach (LineSegment stline in SpanningTree)
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
            foreach (Cell c in ActiveCells)
            {
                if (c.Position == new Vector3(left.x, 0f, left.y))
                {
                    if (c._width % 2 == 0)
                    {
                        left.x += PMB_Random.Range((int)0, (int)1) - 0.5f;
                    }
                    if (c._height % 2 == 0)
                    {
                        left.y += PMB_Random.Range((int)0, (int)1) - 0.5f;
                    }
                    line1Complete = true;
                }
                if (c.Position == new Vector3(right.x, 0f, right.y))
                {
                    if (c._width % 2 == 0)
                    {
                        right.x += PMB_Random.Range((int)0, (int)1) - 0.5f;
                    }
                    if (c._height % 2 == 0)
                    {
                        right.y += PMB_Random.Range((int)0, (int)1) - 0.5f;
                    }
                    line2Complete = true;
                }

                if (line1Complete && line2Complete)
                    break;
            }

            // An array for storing the third position of the 'triangle'
            Vector2[] middles = new Vector2[2];

            // This randomly decides of the triangle will point down or up
            if (PMB_Random.Range(0, 1) == 0)
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

            foreach (Cell c in ActiveCells)
            {
                if (c.CheckCorridorCollision(corridor1, corridor2))
                {
                    middles1Col++;
                }
            }

            corridor1 = new LineSegment(left, middles[1]);
            corridor2 = new LineSegment(middles[1], right);

            foreach (Cell c in ActiveCells)
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
            Corridors.Add(corridor1);
            Corridors.Add(corridor2);

            //yield return null;
        }

        //CreatePrefabTileMap();
        AddInCells();
    }

    // Add in _cells cells that have corridors running through them
    private void AddInCells()
    {
        foreach (LineSegment cLine in Corridors)
        {
            foreach (Cell c in Cells)
            {
                if (ActiveCells.Contains(c))
                {
                    continue;
                }

                if (c.CheckCorridorCollision(cLine))
                {
                    ActiveCells.Add(c);
                }
            }

        }

        // Start actually loading in the map
        CreatePrefabTileMap();
    }

    private Vector2 m_arrayBounds;
    private Vector2 m_arrayOffset;
    private int[,] m_tileMap;
    private int m_arrayLength;

    private void CreatePrefabTileMap()
    {
        // Variables for calculating how big our array needs to be for the tile map
        float top = 0;
        float right = 0;
        float bottom = 0;
        float left = 0;

        foreach (Cell c in ActiveCells)
        {
            Vector2 cPos = new Vector2(c.Position.x, c.Position.z) + new Vector2(-0.5f, -0.5f);

            // Top and bottom of array bounds
            float offset = c._height * 0.5f;
            if (top < cPos.y + offset)
            {
                top = cPos.y + offset;
            }
            if (bottom > cPos.y - offset)
            {
                bottom = cPos.y - offset;
            }

            // Right and left of array bounds
            offset = c._width * 0.5f;
            if (right < cPos.x + offset)
            {
                right = cPos.x + offset;
            }
            if (left > cPos.x - offset)
            {
                left = cPos.x - offset;
            }
        }

        // Since some of our cells have non integar positions such as 5.5 or -15.5 we need to ceil and floor the values respectively
        top = Mathf.Ceil(top);
        right = Mathf.Ceil(right);
        bottom = Mathf.Floor(bottom);
        left = Mathf.Floor(left);

        // Calculating the bounds for the array (array size) and the offset we will need when converting corridor and cells from array space into world space
        m_arrayBounds = new Vector2(-left + right, -bottom + top);
        m_arrayOffset = new Vector2(left + (m_arrayBounds.x * 0.5f), bottom + (m_arrayBounds.y * 0.5f));

        // Initialising the array
        m_tileMap = new int[(int)m_arrayBounds.x, (int)m_arrayBounds.y];

        // Setting all values in the array to 0
        m_arrayLength = (int)m_arrayBounds.x * (int)m_arrayBounds.y;
        for (int i = 0, x = 0, y = 0; i < m_arrayLength; i++, x++)
        {
            if (x >= m_arrayBounds.x)
            {
                x = 0;
                y++;
            }

            m_tileMap[x, y] = 0;
        }

        //// Calculating cell positions
        foreach (Cell cell in ActiveCells)
        {
            Vector2 cellPos = new Vector2(Mathf.Floor(cell.Position.x) - left, Mathf.Floor(cell.Position.z) - bottom);

            foreach (Vector3 floorPos in cell.floorPositions)
            {
                Vector2 newPos = cellPos + new Vector2(floorPos.x, floorPos.z);
                m_tileMap[(int)newPos.x, (int)newPos.y] = 2;
            }
        }

        // Calculating corridor positions
        foreach (LineSegment corridor in Corridors)
        {
            // Corridor length
            int corridorLength = Mathf.CeilToInt(Vector2.Distance(corridor.p0.Value, corridor.p1.Value));
            // The direction that the corridor is moving in
            Vector2 moveDir = -(corridor.p0.Value - corridor.p1.Value) / corridorLength;

            moveDir = moveDir.normalized;

            // The initial position of the corridor in 
            Vector2 corPos = new Vector2(Mathf.Floor(corridor.p0.Value.x) - left, Mathf.Ceil(corridor.p0.Value.y) - bottom);

            // Iterating through the corridor positions
            for (int i = 0; i < corridorLength; ++i)
            {
                Vector2 newPos = corPos + (i * moveDir);
                m_tileMap[(int)newPos.x, (int)newPos.y] = 1;
            }

        }

        PlaceFloorPrefabs();
    }

    private void PlaceFloorPrefabs()
    {
        m_emptyCellPositions = new List<FloorPosition>();
        m_emptyCorridorPositions = new List<FloorPosition>();

        // Going through the array to place all the floor prefabs
        for (int i = 0, x = 0, y = 0; i < m_arrayLength; i++, x++)
        {
            if (x >= m_arrayBounds.x)
            {
                x = 0;
                y++;
            }

            if (m_tileMap[x, y] > 0)
            {
                Vector3 tilePos = new Vector3(x + m_arrayOffset.x, 0f, y + m_arrayOffset.y);
                Instantiate(m_floors[0].GameObject, tilePos, m_floors[0].GameObject.transform.rotation, m_mapTransform);

                FloorPosition floorPos = new FloorPosition();
                floorPos.Position = tilePos;

                bool placeUp = false;
                bool placeRight = false;
                bool placeDown = false;
                bool placeLeft = false;

                // Checking all neighbors off this cell, if it is empty then we need to place a wall
                // We need to do two checks as some of the walls are going to need to be placed outside the bounds of the array
                // which if we checked normally would give an out-of-bounds error
                // Above
                if (y + 1 < m_arrayBounds.y)
                {
                    if (m_tileMap[x, y + 1] == 0)
                    {
                        placeUp = true;
                    }
                }
                else if (y + 1 == m_arrayBounds.y)
                {
                    // We know that this is out of bounds so we need to place a wall here
                    placeUp = true;
                }

                // Right
                if (x + 1 < m_arrayBounds.x)
                {
                    if (m_tileMap[x + 1, y] == 0)
                    {
                        placeRight = true;
                    }
                }
                else if (x + 1 == m_arrayBounds.x)
                {
                    placeRight = true;
                }

                // Below
                if (y - 1 >= 0)
                {
                    if (m_tileMap[x, y - 1] == 0)
                    {
                        placeDown = true;
                    }
                }
                else if (y == 0)
                {
                    placeDown = true;
                }

                // Left
                if (x - 1 >= 0)
                {
                    if (m_tileMap[x - 1, y] == 0)
                    {
                        placeLeft = true;
                    }
                }
                else if (x == 0)
                {
                    placeLeft = true;
                }

                // Placing tiles
                // Above
                if (placeUp)
                {
                    Instantiate(m_walls[0].GameObject, tilePos + (Vector3.forward * 0.5f), m_walls[0].GameObject.transform.rotation, m_mapTransform);

                    floorPos.North = placeUp;
                    ++floorPos.WallCount;
                }

                // Right
                if (placeRight)
                {
                    Instantiate(m_walls[0].GameObject, tilePos + (Vector3.right * 0.5f), m_walls[0].GameObject.transform.rotation * Quaternion.Euler(Vector3.up * 90f), m_mapTransform);

                    floorPos.East = placeRight;
                    ++floorPos.WallCount;
                }

                // Below
                if (placeDown)
                {
                    Instantiate(m_walls[0].GameObject, tilePos + (Vector3.back * 0.5f), m_walls[0].GameObject.transform.rotation * Quaternion.Euler(Vector3.up * 180f), m_mapTransform);

                    floorPos.South = placeDown;
                    ++floorPos.WallCount;
                }

                // left
                if (placeLeft)
                {
                    Instantiate(m_walls[0].GameObject, tilePos + (Vector3.left * 0.5f), m_walls[0].GameObject.transform.rotation * Quaternion.Euler(Vector3.up * 270f), m_mapTransform);

                    floorPos.West = placeLeft;
                    ++floorPos.WallCount;
                }

                if (m_tileMap[x, y] == 1)
                {
                    m_emptyCorridorPositions.Add(floorPos);
                }
                else if (m_tileMap[x, y] == 2)
                {
                    m_emptyCellPositions.Add(floorPos);
                }
            }
        }

        if (m_emptyCellPositions.Count < 1 || m_emptyCorridorPositions.Count < 1)
        {
            Generate();
            return;
        }

        // Add in map scenery
        for (int i = m_emptyCellPositions.Count - 1; i >= 0; --i)
        { 
            if (m_emptyCellPositions[i].WallCount == 0)
            {
                Spawnable obj = ReturnRandomItemInArray(m_cellMiddleFloorPrefabs);
                if (obj.GameObject != null)
                {
                    Quaternion rot = Quaternion.identity;
                    rot.SetLookRotation(new Vector3(UnityEngine.Random.Range(-1, 1), 0f, UnityEngine.Random.Range(-1, 1)).normalized, Vector3.up);
                    rot = Quaternion.Euler(rot.eulerAngles.x + obj.OffSetRotation.x, rot.eulerAngles.y + obj.OffSetRotation.y, rot.eulerAngles.z + obj.OffSetRotation.z);

                    Instantiate(obj.GameObject, m_emptyCellPositions[i].Position + obj.OffsetPosition, rot, m_mapTransform);

                    //if (!obj.DoNotClearPositionWhenPlaced)
                        // This crashes the game, I don't know why
                        //m_emptyCellPositions.RemoveAt(i);
                }
            }

            if (m_emptyCellPositions[i].WallCount != 0)
            {
                Spawnable wall = ReturnRandomItemInArray(m_cellEdgeWallPrefabs);
                if (wall.GameObject != null)
                {
                    Vector3 lookOffset = Vector3.zero;
                    if (m_emptyCellPositions[i].North)
                        lookOffset.z += 1f;
                    if (m_emptyCellPositions[i].South)
                        lookOffset.z -= 1f;
                    if (m_emptyCellPositions[i].East)
                        lookOffset.x += 1f;
                    if (m_emptyCellPositions[i].West)
                        lookOffset.x -= 1f;

                    Quaternion rot = Quaternion.identity;
                    rot.SetLookRotation(-lookOffset.normalized, Vector3.up);

                    Vector3 posOffset;
                    if (m_emptyCellPositions[i].WallCount == 1)
                    {
                        posOffset = rot * Vector3.forward * 0.5f;
                    }
                    else
                    {
                        posOffset = rot * Vector3.forward * 0.75f;
                    }

                    rot = Quaternion.Euler(rot.eulerAngles.x + wall.OffSetRotation.x, rot.eulerAngles.y + wall.OffSetRotation.y, rot.eulerAngles.z + wall.OffSetRotation.z);

                    Instantiate(wall.GameObject, m_emptyCellPositions[i].Position - posOffset + wall.OffsetPosition, rot, m_mapTransform);
                }

                Spawnable floor = ReturnRandomItemInArray(m_cellEdgeFloorPrefabs);
                if (floor.GameObject != null)
                {
                    Vector3 lookOffset = Vector3.zero;
                    if (m_emptyCellPositions[i].North)
                        lookOffset.z += 1f;
                    if (m_emptyCellPositions[i].South)
                        lookOffset.z -= 1f;
                    if (m_emptyCellPositions[i].East)
                        lookOffset.x += 1f;
                    if (m_emptyCellPositions[i].West)
                        lookOffset.x -= 1f;

                    lookOffset.y = m_emptyCellPositions[i].Position.y;

                    Quaternion rot = Quaternion.identity;
                    rot.SetLookRotation(-lookOffset.normalized, Vector3.up);

                    Vector3 posOffset;
                    if (m_emptyCellPositions[i].WallCount == 1)
                    {
                        posOffset = rot * Vector3.forward * 0.3f;
                    }
                    else
                    {
                        posOffset = rot * Vector3.forward * 0.4f;
                    }

                    rot = Quaternion.Euler(rot.eulerAngles.x + floor.OffSetRotation.x, rot.eulerAngles.y + floor.OffSetRotation.y, rot.eulerAngles.z + floor.OffSetRotation.z);

                    Instantiate(floor.GameObject, m_emptyCellPositions[i].Position - posOffset + floor.OffsetPosition, rot, m_mapTransform);
                }

                //if (wall.GameObject != null || floor.GameObject != null)
                    //m_emptyCellPositions.Remove(m_emptyCellPositions[i]);
            }
        }

        // Fit the pathfinding to the map
        FitPathfinder();
    }

    private void FitPathfinder()
    {
        Grid.instance.gridWorldSize = m_arrayBounds;
        Grid.instance.gridWorldPosition = transform.position + new Vector3(m_arrayOffset.x + (Grid.instance.gridWorldSize.x * 0.5f), 0f, m_arrayOffset.y + (Grid.instance.gridWorldSize.y * 0.5f));
        Grid.instance.OnValidate();

        SetPlayerAndEnemy();
    }

    float avgGridSize;
    private void PlacePlayer()
    {
        FloorPosition randFloorPos;
        int iter = 0;

        // Random position for the player
        do
        {
            if (iter > 500)
            {
                Generate();
                return;
            }

            randFloorPos = m_emptyCellPositions[UnityEngine.Random.Range(0, m_emptyCellPositions.Count - 1)];
            iter++;
        } while (Vector3.Distance(randFloorPos.Position, Grid.instance.gridWorldPosition) < avgGridSize * 0.35f || randFloorPos.WallCount != 0);
        m_playerRbdy.isKinematic = false;
        m_player.transform.position = randFloorPos.Position + Vector3.up;
        m_emptyCellPositions.Remove(randFloorPos);
    }

    public void PlaceEnemy()
    {
        FloorPosition randFloorPos;
        int iter = 0;

        if (m_level >= m_levelToStartSpawningEnemy)
        {
            // Random position for the enemy
            do
            {
                if (iter > 500)
                {
                    Generate();
                    return;
                }

                randFloorPos = m_emptyCellPositions[UnityEngine.Random.Range(0, m_emptyCellPositions.Count - 1)];
                iter++;
            } while (Vector3.Distance(randFloorPos.Position, m_player.transform.position) < avgGridSize * 0.35f || randFloorPos.WallCount != 0);
            m_enemyRbdy.isKinematic = false;
            m_enemy.transform.position = randFloorPos.Position + Vector3.up;
            m_emptyCellPositions.Remove(randFloorPos);
        }
    }

    private void PlaceExit()
    {
        FloorPosition randFloorPos;
        int iter = 0;

        // Setting exit position
        do
        {
            if (iter > 500)
            {
                Generate();
                return;
            }

            randFloorPos = m_emptyCellPositions[UnityEngine.Random.Range(0, m_emptyCellPositions.Count - 1)];
            iter++;
        } while (Vector3.Distance(randFloorPos.Position, m_player.transform.position) < avgGridSize * 0.35f || randFloorPos.WallCount != 1);
        m_exit = Instantiate(m_exitPrefab, randFloorPos.Position, m_exitPrefab.transform.rotation, m_mapTransform);
        m_emptyCellPositions.Remove(randFloorPos);
    }

    private void SetPlayerAndEnemy()
    {
        avgGridSize = (Grid.instance.gridWorldSize.x + Grid.instance.gridWorldSize.y) * 0.5f;

        PlacePlayer();

        PlaceEnemy();

        PlaceExit();

        // Enable the enemy pathfinding
        m_enemyUnit.enabled = true;

        CheckPaths();
    }

    private void CheckPaths()
    {
        if (m_level >= m_levelToStartSpawningEnemy)
            PathRequestManager.RequestPath(new PathRequest(m_enemy.transform.position, m_player.transform.position, instance.EnemyCanReachPlayer));
        else
            m_enemyCanReachPlayer = true;

        PathRequestManager.RequestPath(new PathRequest(m_player.transform.position, m_exit.transform.position, instance.PlayerCanReachExit));
    }

    // Drop table-ish function to return an item based of its percent chance to drop
    // Requires the array to be sorted
    private Spawnable ReturnRandomItemInArray(Spawnable[] a_array)
    {
        float range = 0;
        for (int i = 0; i < a_array.Length; ++i)
            range += a_array[i].PercentChanceToDrop;

        float rand = UnityEngine.Random.Range(0f, range);
        float top = 0f;

        for (int i = 0; i < a_array.Length; ++i)
        {
            top += a_array[i].PercentChanceToDrop;
            if (rand < top)
                return a_array[i];
        }

        return new Spawnable();
    }

    void OnDrawGizmos()
    {
        if (m_turnOffAllGizmos)
            return;

        Vector3 globalOffset = new Vector3(0f, m_yAxisGizmoOffset, 0f);

        Vector3 halfArrayBounds = new Vector3(m_arrayBounds.x * 0.5f, 0f, m_arrayBounds.y * 0.5f);
        Vector3 arrayOffset = new Vector3(m_arrayOffset.x, 0f, m_arrayOffset.y) + globalOffset;

        if (m_turnOnTileMapVisualisation)
        {
            //Gizmos.DrawCube(transform.position + halfArrayBounds + arrayOffset, new Vector3(m_arrayBounds.x, 1f, m_arrayBounds.y));

            for (int i = 0, x = 0, y = 0; i < m_arrayLength; i++, x++)
            {
                if (x >= m_arrayBounds.x)
                {
                    x = 0;
                    y++;
                }

                if (m_tileMap[x, y] == 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(transform.position + arrayOffset + new Vector3(x, 0f, y), Vector3.one * 0.5f);
                }
                else if (m_tileMap[x, y] == 2)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(transform.position + arrayOffset + new Vector3(x, 0f, y), Vector3.one * 0.5f);
                }
            }
        }

        Vector3 trueCellPos = transform.position + halfArrayBounds + globalOffset - new Vector3(0.5f, 0f, 0.5f);

        // draw cells
        if (m_cells != null && m_turnOnCells)
        {
            foreach (Cell c in m_cells)
            {
                Gizmos.color = Color.magenta;
                //Gizmos.DrawCube(c.Position, c.Size + Vector3.up);

                foreach (Vector3 pos in c.floorPositions)
                {
                    Gizmos.DrawCube(trueCellPos + c.Position + pos, Vector3.one * 0.75f);
                }

                for (int i = 0; i <= c.Size.x; ++i)
                {
                    if (i == 0 || i == c.Size.x)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.white;

                    Gizmos.DrawLine(trueCellPos + new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * 0.5f + c.Position.z), trueCellPos + new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * -0.5f + c.Position.z));
                }
                for (int i = 0; i <= c.Size.z; ++i)
                {
                    if (i == 0 || i == c.Size.z)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.white;

                    Gizmos.DrawLine(trueCellPos + new Vector3(c.Size.x * 0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z), trueCellPos + new Vector3(c.Size.x * -0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z));
                }
            }
        }

        // Draw active cells
        if (m_activeCells != null && m_turnOnActiveCells)
        {
            foreach (Cell c in m_activeCells)
            {
                Gizmos.color = Color.white;
                //Gizmos.DrawCube(c.Position, c.Size + Vector3.up);

                foreach (Vector3 pos in c.floorPositions)
                {
                    Gizmos.DrawCube(trueCellPos + c.Position + pos, Vector3.one * 0.75f);
                }

                for (int i = 0; i <= c.Size.x; ++i)
                {
                    if (i == 0 || i == c.Size.x)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.white;

                    Gizmos.DrawLine(trueCellPos + new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * 0.5f + c.Position.z), trueCellPos + new Vector3(-c.Size.x * 0.5f + i + c.Position.x, 0.5f, c.Size.z * -0.5f + c.Position.z));
                }
                for (int i = 0; i <= c.Size.z; ++i)
                {
                    if (i == 0 || i == c.Size.z)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.white;

                    Gizmos.DrawLine(trueCellPos + new Vector3(c.Size.x * 0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z), trueCellPos + new Vector3(c.Size.x * -0.5f + c.Position.x, 0.5f, -c.Size.z * 0.5f + i + c.Position.z));
                }
            }
        }

        // Draw triangulation
        if (m_delaunayTriangulation != null && m_turnOnDelaunayTriangulation)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < m_delaunayTriangulation.Count; ++i)
            {
                Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
                Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
                Gizmos.DrawLine(trueCellPos + new Vector3(left.x, 0f, left.y), trueCellPos + new Vector3(right.x, 0f, right.y));
            }
        }

        // Draw minimum spanning tree
        if (m_spanningTree != null && m_turnOnMinimumSpanningPath)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < m_spanningTree.Count; ++i)
            {
                LineSegment seg = m_spanningTree[i];
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Gizmos.DrawLine(trueCellPos + new Vector3(left.x, 0f, left.y), trueCellPos + new Vector3(right.x, 0f, right.y));
            }
        }

        // Drawing corridors
        if (m_corridors != null && m_turnOnCorridors)
        {
            for (int i = 0; i < m_corridors.Count; ++i)
            {
                Gizmos.color = Color.blue;
                LineSegment seg = m_corridors[i];
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Gizmos.DrawLine(trueCellPos + new Vector3(left.x, 0f, left.y), trueCellPos + new Vector3(right.x, 0f, right.y));

                Vector3 arrowPos = trueCellPos + ((new Vector3(left.x, 0f, left.y) + new Vector3(right.x, 0f, right.y)) * 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(trueCellPos + new Vector3(left.x, 0f, left.y), arrowPos);
            }
        }
    }
}
