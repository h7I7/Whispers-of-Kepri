using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////
// This script is going to be a modified version of the tutorial found here
// https://unity3d.com/learn/tutorials/topics/scripting/basic-2d-dungeon-generation
/////////////////////////////////////////////////////////////////////////////////////

// An enum to hold direction for branching off or rooms and corridor facings
public enum Direction
{
    North = 0,
    East,
    South,
    West,
}

// A custom array with an overloaded indexer so that I can access a double array whilst it is still a single axis array
public class SingleAsDouble<T>
{
    // Variables
    private T[] _items;
    private int _width;
    private int _count;
    public int Count { get { return _count; } }

    static readonly T[] _emptyArray = new T[0];

    // Constructors
    public SingleAsDouble()
    {
        _items = _emptyArray;
        _count = 0;
    }

    // Creating the array size we need
    public SingleAsDouble(int a_x, int a_y)
    {
        // Calculating the size of the array
        _count = a_x * a_y;

       _items = new T[_count];
       
        _width = a_x;
    }

    // Accessors
    public T this[int a_x, int a_y]
    {
        get { return _items[a_x + (a_y * _width)]; }
        set { _items[a_x + (a_y * _width)] = value; }
    }
}

// The type of tile that will be laid in a specific position
public enum TileType
{
    Wall = 0,
    Floor,
    Corridor
}

public class GenerateDungeon : MonoBehaviour {

    // Public variables
    // Headers describe variables
    public bool DrawMapAsGizmos = true; // A bool for whether we should draw the map as gizmos
    public bool SpawnPlayer = false;    // A bool for whether we should spawn the player
    [Header("The size of the map")]
    public int columns = 100;
    public int rows = 100;

    [Header("The range of the amount of rooms there can be")]
    public Vector2 numRooms = new Vector2(15, 20);

    [Header("The range of the room widths and heights")]
    public Vector2 roomWidth = new Vector2(3, 10);
    public Vector2 roomHeight = new Vector2(3, 10);

    [Header("The range in length of the corridors")]
    public Vector2 corridorLength = new Vector2(6, 10);

    // Private variables
    private static SingleAsDouble<TileType> tiles; // An array of tile types representing the board, like a grid
    public static SingleAsDouble<TileType> Tiles    // An accessor for the above
    {
        get { return tiles; }
    }

    private static Room[] rooms;            // All the rooms that are created for this board
    public static Room[] Rooms              // A public getter for the above
    {
        get { return rooms; }
    }
    private Corridor[] corridors;           // All the corridors that connect the rooms
    private GameObject boardHolder;         // GameObject that acts as a container for all other tiles 

    [Header("Level prefabs")]
    [SerializeField]
    private GameObject[] CornerWalls;
    [SerializeField]
    private GameObject[] Walls;
    [SerializeField]
    private GameObject[] Floors;

    public GameObject player;               // A reference to the player

    [SerializeField]
    private GameObject BoardHolderParent;   // A parent for boardHolder
    [SerializeField]
    private GameObject PlayerParent;        // A parent for the player

    [SerializeField]
    private int RandSeed;                   // The seed for random

	// Use this for initialization
	void Awake () {
        // We need to create a boardHolder to store all of the board objects in the hierarchy
        boardHolder = new GameObject("BoardHolder");
        boardHolder.transform.SetParent(BoardHolderParent.transform);

        CreateDungeon();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            StopCoroutine(CreateRoomsAndCorridors());
            CreateDungeon();

        }
    }

    void CreateDungeon()
    {
        // Set the tiles array to the right size
        tiles = new SingleAsDouble<TileType>(columns, rows);

        StartCoroutine(CreateRoomsAndCorridors());

       
    }

    IEnumerator CreateRoomsAndCorridors()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        RandSeed = Mathf.FloorToInt(DateTime.Now.Millisecond * Time.deltaTime * DateTime.Now.Second * DateTime.Now.Minute);

        UnityEngine.Random.InitState(RandSeed);
        //UnityEngine.Random.InitState(14018);


        // Create rooms array with random size
        rooms = new Room[UnityEngine.Random.Range(Mathf.FloorToInt(numRooms.x), Mathf.FloorToInt(numRooms.y))];

        // There should be one less corridor that there is rooms
        // .Length returns one more than is in the array so we have to account for that by subtracting two
        corridors = new Corridor[rooms.Length - 2];

        // Create the first room and corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        // Setup the first room, there is no previous corridor so we don't use one
        rooms[0].Setup(roomWidth, roomHeight, columns, rows);

        // Setup the first corridor using the first room
        corridors[0].Setup(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);
        SetTilesValuesForRooms(rooms[0]);
        SetTilesValuesForCorridors(corridors[0]);

        for (int i = 1; i < rooms.Length - 1; ++i)
        {
            // Create a room
            rooms[i] = new Room();

            // Setup the room based on the previous corridor
            rooms[i].Setup(roomWidth, roomHeight, columns, rows, corridors[i - 1]);

            // If we haven't reached the end of the corridors array...
            if (i < corridors.Length)
            {
                // Create a corridor
                corridors[i] = new Corridor();

                // Setup the corridor based on the room that was just created
                corridors[i].Setup(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows);
            }

            if (SpawnPlayer)
            {
                if (i == rooms.Length * 0.5f)
                {
                    Vector3 playerPos = new Vector3(rooms[i].pos.x, rooms[i].pos.y, 0);
                    Instantiate(player, playerPos, Quaternion.identity, PlayerParent.transform);
                }
            }

            // Set the tiles for the rooms
            SetTilesValuesForRooms(rooms[i]);

            // Set the tiles for the corridors however we have one less corridor than rooms so we need to stop processing this one step before the rooms
            if (i < corridors.Length)
                SetTilesValuesForCorridors(corridors[i]);


            yield return wait;
        }

       

        InstantiateTiles();
        yield return wait;

    }

    void SetTilesValuesForRooms(Room currentRoom )
    {
         // and for each room go through its width
        for (int j = 0; j < currentRoom.size.x; ++j)
        {
            int xCoord = Mathf.FloorToInt(currentRoom.pos.x) + j;
                
            // For each horizontal tile, go up vertically through the rooms height
            for (int k = 0; k < currentRoom.size.y; ++k)
            {
                int yCoord = Mathf.FloorToInt(currentRoom.pos.y) + k;

                // The co-ordinates in the tiles array
                tiles[xCoord, yCoord] = TileType.Floor;
            }
        }
        
    }

    void SetTilesValuesForCorridors(Corridor currentCorridor )
    {
        // Go through every corridor
        for (int j = 0; j < currentCorridor.length; ++j)
        {
            // Start the co-ordinates at the start of the corridor
            int xCoord = Mathf.FloorToInt(currentCorridor.startPos.x);
            int yCoord = Mathf.FloorToInt(currentCorridor.startPos.y);

            // Depending on the direction, add or subtract from the appropriate
            // co-ordinate based on how far through the length the loop is
            switch (currentCorridor.dir)
            {
                case Direction.North:
                    {
                        yCoord += j;
                        break;
                    }
                case Direction.East:
                    {
                        xCoord += j;
                        break;
                    }
                case Direction.South:
                    {
                        yCoord -= j;
                        break;
                    }
                case Direction.West:
                    {
                        xCoord -= j;
                        break;
                    }
            }

            // Set the tile at these co-ordinates to floor
            tiles[xCoord, yCoord] = TileType.Corridor;
        }
        
    }

    void InstantiateTiles()
    {
        // Add code to build level from prefabs now we have a tile map
    }

    void OnDrawGizmos()
    {
        if (!DrawMapAsGizmos)
            return;

        Vector3 wireCubePos = new Vector3(transform.position.x + (columns / 2), 1f, transform.position.z + (rows / 2));
        Gizmos.DrawWireCube(wireCubePos, new Vector3(columns, 0f, rows));

        for (int i = 0; i < columns; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                if (tiles[i, j] == TileType.Floor || tiles[i, j] == TileType.Corridor)
                {
                    Gizmos.color = Color.white;
                    if (tiles[i, j] == TileType.Corridor)
                        Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(i, 0f, j), Vector3.one );
                }
            }
        }
    
    }
}
