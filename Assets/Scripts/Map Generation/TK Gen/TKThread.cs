using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

using System.Threading;

//////////////////////////////////////////////////
// This code was originally used to multi-thread the map generation
// however this made it very unstable and would crash the game roughly
// every 3-4 generations. It was very fast however I has to remove
// it so that the game was playable
//////////////////////////////////////////////////


public static class TKThread {

    private static Thread m_thread;
    private static SetThreadNotRunningDelegate m_setThreadDelegate;
    private static TKGenerator gen = null;
   
    static TKThread()
    {
        gen = TKGenerator.instance;
    }

    public static void SetThreadRunningDelegate(SetThreadNotRunningDelegate a_callback)
    {
        m_setThreadDelegate = a_callback;
    }

    public static void Generate()
    {
        if (m_thread != null)
            m_thread.Abort();
        
        m_thread = new Thread(GenerateMap);
        m_thread.Start();
    }

    // Generate a bunch of cells at random within the confines of the map
    private static void GenerateMap()
    {
        if (!gen.UseCustomSeed)
            gen.Seed = PMB_Random.Random();

        // Seed the random fro getting a position on a quad
        PMB_Random.Seed((long)gen.Seed);

        // Initialise the list of cells
        gen.Cells = new List<Cell>();

        // Create the amount of cells that we need
        for (int i = 0; i < gen.CellsToGenerate; ++i)
        {
            // Add the newely created cell to the cell list
            gen.Cells.Add(new Cell());
            // Set the cell dimension constraints
            gen.Cells[i].SetDimensionConstraints(gen.MinimumCellWidth, gen.MaximumCellWidth, gen.MinimumCellHeight, gen.MaximumCellHeight);
            // Create the random dimensions of the cell
            gen.Cells[i].GenerateCell();

            // Adjust the cells position to be placed somewhere within a box that is 10 times the size of it's footprint
            Vector2 cellPos = Vector2.zero;
            Vector2 min = new Vector2(-(gen.RoomWidth * 0.5f), -(gen.RoomHeight * 0.5f));
            Vector2 max = new Vector2(gen.RoomWidth * 0.5f, gen.RoomHeight * 0.5f);
            // Get a random position in the dungeon map area
            PMB_Random.RandomPointInQuad(min, max, out cellPos);
            // Set the position of the cell
            gen.Cells[i].Position = new Vector3(cellPos.x, 0f, cellPos.y);
            // Align its position to the nearest integer
            gen.Cells[i].RoundToNearestTilePosition();
        }

        SeparateCells();
    }

    // Separating cells
    private static void SeparateCells()
    {
        // A temporary variable to store which direction a cell should move upon separation
        Vector3 moveVec;
        // A bool for determining whether there is still collision
        bool isInCollision = false;
        do
        {
            // Sort the cells based on their distance to the center of the map
            gen.Cells.Sort((x, y) => x.DistanceToCenter.CompareTo(y.DistanceToCenter));
            isInCollision = false;
            // For each of the cells that we generated
            foreach (Cell currentCell in gen.Cells)
            {
                // Get the current position of the cell (center)
                Vector3 crPos = currentCell.Position;
                // Get the bounds of the cells (width/height from center)
                Vector3 crBounds = currentCell.Size * 0.5f;
                // The velocity of this cell
                Vector3 cellVel = new Vector3(0f, 0f, 0f);
                // An int to store how many neighbors it is colliding with
                int neighbourCount = 0;

                // For each other cell in the cell list
                foreach (Cell cell in gen.Cells)
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
    private static void SelectRooms()
    {
        // Initialise the active cells list
        gen.ActiveCells = new List<Cell>();
        // Estimate the average volume of cells
        float averageVolume = gen.MaximumCellWidth * gen.MaximumCellHeight * 0.3f;

        // For each cell in the cells list
        foreach (Cell c in gen.Cells)
        {
            // Get the dimensions of the cell
            Vector3 Dimensions = c.Size;
            // Calculate the cell volume
            float cellVolume = (Dimensions.x * Dimensions.z);
            // Compare the two and if the cell is above the average volume them add it the new list
            if (cellVolume > averageVolume)
            {
                gen.ActiveCells.Add(c);
            }
        }

        if (gen.ActiveCells.Count < gen.MinimumCells)
        {
            GenerateMap();
            return;
        }

        // Relocate all the map items so that the average location of all items is at 0, 0, 0
        RelocateMap();
    }

    // Changing the average position of all cells to be 0, 0
    private static void RelocateMap()
    {
        // A variable for the average location of all cells
        Vector3 averageLocation = Vector3.zero;

        // For each cell in the main cells list add the position to the average locations vector
        foreach (Cell c in gen.Cells)
        {
            averageLocation.x += c.Position.x;
            averageLocation.z += c.Position.z;
        }

        // Then divide the vector by the amount of cells in the list to get an average position
        averageLocation.x = averageLocation.x / gen.Cells.Count;
        averageLocation.z = averageLocation.z / gen.Cells.Count;

        // We can then subtract this position from every cell making the average center of every cell 0, 0
        foreach (Cell c in gen.Cells)
        {
            c.Position -= averageLocation;
        }

        // Align all the cells so that they lie on a grid
        AlignCells();
    }

    // Align the cells to a grid
    private static void AlignCells()
    {
        foreach (Cell c in gen.Cells)
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
    private static void CreateTriangulation()
    {
        // If we have a list of cells
        if (gen.ActiveCells != null)
        {
            // A list for all the cell positions in Vector2 format
            gen.CellIndices = new List<Vector2>();
            // A list of colors that will all be black
            List<uint> colors = new List<uint>();

            // For each of the active cells add their position to cellIndices and add a blank color to colors
            foreach (Cell c in gen.ActiveCells)
            {
                gen.CellIndices.Add(new Vector2(c.Position.x, c.Position.z));
                colors.Add(0);

            }

            // Use unity Delaunay generation routines
            Delaunay.Voronoi v = new Delaunay.Voronoi(gen.CellIndices, colors, new Rect(0, 0, 100, 100));

            // Create a minimum spanning tree from the Delaunay triangulation
            gen.SpanningTree = v.SpanningTree(KruskalType.MINIMUM);
            gen.DelaunayTriangulation = v.DelaunayTriangulation();

            // Turn on a few of the connections that were present in the delaunay triangulation
            // that aren't in the spanning tree
            int treeTriDiff = gen.DelaunayTriangulation.Count - gen.SpanningTree.Count;
            treeTriDiff = (int)((float)treeTriDiff * PMB_Random.Range(gen.PercentCorridorsToAddInMinimum, gen.PercentCorridorsToAddInMaximum));
            foreach (LineSegment dlLine in gen.DelaunayTriangulation)
            {
                if (!gen.SpanningTree.Contains(dlLine))
                {
                    gen.SpanningTree.Add(dlLine);
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
    private static void CalculateCorridors()
    {
        // A list to contain all the corridors we will make
        gen.Corridors = new List<LineSegment>();

        // For each of the lines in the spanning tree we need to find two more lines that are axis aligned
        // We are going to treat these lines like a hypotenuse of a right angle triangle and then attempt
        // to find the opposite and adjacent sides for the rest of the triangle
        foreach (LineSegment stline in gen.SpanningTree)
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
            foreach (Cell c in gen.ActiveCells)
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

            foreach (Cell c in gen.ActiveCells)
            {
                if (c.CheckCorridorCollision(corridor1, corridor2))
                {
                    middles1Col++;
                }
            }

            corridor1 = new LineSegment(left, middles[1]);
            corridor2 = new LineSegment(middles[1], right);

            foreach (Cell c in gen.ActiveCells)
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
            gen.Corridors.Add(corridor1);
            gen.Corridors.Add(corridor2);

            //yield return null;
        }

        //CreatePrefabTileMap();
        AddInCells();
    }

    // Add in _cells cells that have corridors running through them
    private static void AddInCells()
    {
        foreach (LineSegment cLine in gen.Corridors)
        {
            foreach (Cell c in gen.Cells)
            {
                if (gen.ActiveCells.Contains(c))
                {
                    continue;
                }

                if (c.CheckCorridorCollision(cLine))
                {
                    gen.ActiveCells.Add(c);
                }
            }

        }

        // Start actually loading in the map
        CreatePrefabTileMap();
    }

    public static Vector2 m_arrayBounds;
    public static Vector2 m_arrayOffset;
    public static int[,] m_tileMap;
    public static int m_arrayLength;

    private static void CreatePrefabTileMap()
    {
        // Variables for calculating how big our array needs to be for the tile map
        float top = 0;
        float right = 0;
        float bottom = 0;
        float left = 0;

        foreach (Cell c in gen.ActiveCells)
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
        foreach (Cell cell in gen.ActiveCells)
        {
            Vector2 cellPos = new Vector2(Mathf.Floor(cell.Position.x) - left, Mathf.Floor(cell.Position.z) - bottom);

            foreach (Vector3 floorPos in cell.floorPositions)
            {
                Vector2 newPos = cellPos + new Vector2(floorPos.x, floorPos.z);
                m_tileMap[(int)newPos.x, (int)newPos.y] = 2;
            }
        }

        // Calculating corridor positions
        foreach (LineSegment corridor in gen.Corridors)
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

        m_setThreadDelegate(false);
    }

}
