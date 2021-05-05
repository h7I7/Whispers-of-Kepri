using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor {

    #region Variables
    public Vector2 startPos;    // Vector for the start position
    public int length;          // Length of the corridor
    public Direction dir;       // Direction of the corridor

    // Get the end position of the corridor based off its size and direction its facing
    public int EndPosX
    {
        get
        {
            if (dir == Direction.North || dir ==Direction.South)
                return Mathf.RoundToInt(startPos.x);
            if (dir == Direction.East)
                return Mathf.RoundToInt(startPos.x + length - 1);
            return Mathf.RoundToInt(startPos.x - length + 1);
        }
    }

    public int EndPosY
    {
        get
        {
            if (dir == Direction.East || dir == Direction.West)
                return Mathf.RoundToInt(startPos.y);
            if (dir == Direction.North)
                return Mathf.RoundToInt(startPos.y + length - 1);
            return Mathf.RoundToInt(startPos.y - length + 1);
        }
    }

    #endregion

    #region Functions
    public void Setup(Room a_room, Vector2 a_lengthRange, Vector2 a_roomWidthRange, Vector2 a_roomHeightRange, int a_columns, int a_rows, bool a_firstCorridor = false)
    {
        // A set of directions that we can move in, these will be chosen from randomly later and can be removed at any time
        List<Direction> dirWhitelist = new List<Direction>();
        dirWhitelist.Add(Direction.North);
        dirWhitelist.Add(Direction.East);
        dirWhitelist.Add(Direction.South);
        dirWhitelist.Add(Direction.West);

        // A bool for if we need to recalculate the corridor
        bool recalcCorridor = true;

        while (recalcCorridor)
        {
            recalcCorridor = false;

            // Set a random direction
            dir = dirWhitelist[Random.Range(0, dirWhitelist.Count)];

            // Find the direction opposite to the one entering the room this corridor is leaving from.
            // Cast the previous corridor's dirction to an int between 0 and 3 and add 2 (this becomes a number between 2 and 5)
            // Find the remainder when dividing by 4 (if 2 then 2, if 3 then 3, if 4 then 0)
            // Cast this number back to a Direction
            // Overall effect is if the direction was South then that is 2, becomes 4, remainder is 0, which is North.
            Direction oppositeDir = (Direction)(((int)a_room.enteringCorridor + 2) % 4);

            // If this is not the first corridor and the randomly selected direction is opposite to the previous corridor's direction
            if (!a_firstCorridor && dir == oppositeDir)
            {
                // Rotate the direction 90 degrees clockwise (North becomes East, East becomes South, etc.)
                // This is a more broken down version of the opposite direction operation above but instead of adding 2 we're adding 1
                // This means instead of rotating 180 (the opposite direction) we're rotating 90
                int directionInt = (int)dir;
                directionInt++;
                directionInt = directionInt % 4;
                dir = (Direction)directionInt;
            }
            
            // Set a random length
            length = Mathf.RoundToInt(Random.Range(a_lengthRange.x, a_lengthRange.y));

            // Create a cap for how long the length can be (this will be changed based on the direction and position)
            int maxLength = Mathf.FloorToInt(a_lengthRange.y);

            switch (dir)
            {
                // If the direction is north/up
                case Direction.North:
                    {
                        // The starting position in the x axis can be random but within the width of the room
                        startPos.x = Mathf.RoundToInt(Random.Range(a_room.pos.x, a_room.pos.x + a_room.size.x - 1));

                        // The starting position of the y axis must be the top of the room
                        startPos.y = a_room.pos.y + a_room.size.y;

                        // The max length the corridor can be is the height of the board but from the top of the room
                        maxLength = Mathf.FloorToInt(a_rows - startPos.y - a_roomHeightRange.x);
                        break;
                    }
                case Direction.East:
                    {
                        startPos.x = a_room.pos.x + a_room.size.x;
                        startPos.y = Mathf.RoundToInt(Random.Range(a_room.pos.y, a_room.pos.y + a_room.size.y - 1));
                        maxLength = Mathf.FloorToInt(a_columns - startPos.x - a_roomWidthRange.x);
                        break;
                    }
                case Direction.South:
                    {
                        startPos.x = Mathf.RoundToInt(Random.Range(a_room.pos.x, a_room.pos.x + a_room.size.x - 1));
                        startPos.y = a_room.pos.y - 1;
                        maxLength = Mathf.FloorToInt(startPos.y - a_roomHeightRange.x);
                        break;
                    }
                case Direction.West:
                    {
                        startPos.x = a_room.pos.x - 1;
                        startPos.y = Mathf.RoundToInt(Random.Range(a_room.pos.y, a_room.pos.y + a_room.size.y - 1));
                        maxLength = Mathf.FloorToInt(startPos.x - a_roomWidthRange.x);
                        break;
                    }
            }

            // We clamp the length of the corridor to make sure it doesn't go off the board
            length = Mathf.Clamp(length, 1, maxLength - 1);

            //Check if the corridor intersects another object or will place a room inside other objects
            switch (dir)
            {
                case Direction.North:
                    {
                        // Checking the length of the corridor
                        for (int i = 0; i < length; ++i)
                        {
                            int posY = (int)startPos.y + i;
                            // We also want to check 1 unit to the right and one unit to the left for other corridors
                            for (int j = -1; j < 1; ++j)
                            {
                                int posX = (int)startPos.x + j;

                                if (GenerateDungeon.Tiles[posX, posY] == TileType.Corridor || GenerateDungeon.Tiles[posX, posY] == TileType.Floor)
                                {
                                    recalcCorridor = true;
                                    dirWhitelist.Remove(Direction.North);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case Direction.East:
                    {
                        // Checking the length of the corridor
                        for (int i = 0; i < length; ++i)
                        {
                            int posX = (int)startPos.x + i;
                            // We also want to check 1 unit to the right and one unit to the left for other corridors
                            for (int j = -1; j < 1; ++j)
                            {
                                int posY = (int)startPos.y + j;

                                if (GenerateDungeon.Tiles[posX, posY] == TileType.Corridor || GenerateDungeon.Tiles[posX, posY] == TileType.Floor)
                                {
                                    recalcCorridor = true;
                                    dirWhitelist.Remove(Direction.North);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case Direction.South:
                    {
                        // Checking the length of the corridor
                        for (int i = 0; i < length; ++i)
                        {
                            int posY = (int)startPos.y - i;
                            // We also want to check 1 unit to the right and one unit to the left for other corridors
                            for (int j = -1; j < 1; ++j)
                            {
                                int posX = (int)startPos.x + j;

                                if (GenerateDungeon.Tiles[posX, posY] == TileType.Corridor || GenerateDungeon.Tiles[posX, posY] == TileType.Floor)
                                {
                                    recalcCorridor = true;
                                    dirWhitelist.Remove(Direction.North);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case Direction.West:
                    {
                        // Checking the length of the corridor
                        for (int i = 0; i < length; ++i)
                        {
                            int posX = (int)startPos.x - i;
                            // We also want to check 1 unit to the right and one unit to the left for other corridors and walls
                            for (int j = -1; j < 1; ++j)
                            {
                                int posY = (int)startPos.y + j;

                                if (GenerateDungeon.Tiles[posX, posY] == TileType.Corridor || GenerateDungeon.Tiles[posX, posY] == TileType.Floor)
                                {
                                    recalcCorridor = true;
                                    dirWhitelist.Remove(Direction.North);
                                    break;
                                }
                            }
                        }
                        break;
                    }
            }

            // If the whitelist for directions is empty then choose a random room to start the map gen process from again
            if (dirWhitelist.Count == 0)
            {
                recalcCorridor = true;

                // Set the room to a different one
                a_room = GenerateDungeon.Rooms[Random.Range(0, GenerateDungeon.Rooms.Length - 1)];

                // Reset the direction whitelist
                dirWhitelist = new List<Direction>();
                // Add all the directions back in
                dirWhitelist.Add(Direction.North);
                dirWhitelist.Add(Direction.East);
                dirWhitelist.Add(Direction.South);
                dirWhitelist.Add(Direction.West);
            }
        }
    }
    #endregion
}
