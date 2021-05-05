using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

    #region Variables
    public Vector2 pos;                 // Vector for the position
    public Vector2 size;                // Vector for the height and width of the room
    public Direction enteringCorridor;   // The direction of the corridor that is entering the room
    #endregion

    #region Functions
    // This is used for the first room
    public void Setup(Vector2 a_widthRange, Vector2 a_heightRange, int a_columns, int a_rows)
    {
        // Room width and height
        size.x = Mathf.RoundToInt(Random.Range(a_widthRange.x, a_widthRange.y));
        size.y = Mathf.RoundToInt(Random.Range(a_heightRange.x, a_heightRange.y));

        // Set the x and y co-ordinates so the room is roughly in the middle of the board
        pos.x = Mathf.RoundToInt(a_columns * 0.5f - size.x * 0.5f);
        pos.y = Mathf.RoundToInt(a_rows * 0.5f - size.y * 0.5f);
    }

    // An overload for setup that includes a corridor parameter
    public void Setup(Vector2 a_widthRange, Vector2 a_heightRange, int a_columns, int a_rows, Corridor a_corridor)
    {
        // Set the entering corridor direction
        enteringCorridor = a_corridor.dir;

        bool recalcRoom = true;

        while (recalcRoom)
        {
            recalcRoom = false;

            // Set the random values for width and height
            size.x = Mathf.RoundToInt(Random.Range(a_widthRange.x, a_widthRange.y));
            size.y = Mathf.RoundToInt(Random.Range(a_heightRange.x, a_heightRange.y));

            switch (enteringCorridor)
            {
                // If the corridor entering the room is going north
                case Direction.North:
                    {
                        // The height of the room must not go beyond
                        // the board so it must be clamped based on the
                        // height of the board and the end of the
                        // corridor that leads to the room
                        size.y = Mathf.Clamp(size.y, 1, a_rows - a_corridor.EndPosX);

                        // The y co-ordinate of the room must be at the end of the corridor
                        pos.y = a_corridor.EndPosY + 1;

                        // The x co-ordinate can be random but the left-most possibility is no further than the width
                        // and the right-most possibility is that the end of the corridor is at the position of the room
                        pos.x = Mathf.RoundToInt(Random.Range(a_corridor.EndPosX - size.x + 1, a_corridor.EndPosX));

                        // This must be clamped to ensure that the room doesn't go off the board
                        pos.x = Mathf.Clamp(pos.x, 0, a_columns - size.x);

                        if (CheckBounds(a_columns, a_rows))
                            recalcRoom = true;

                        break;
                    }
                case Direction.East:
                    {
                        size.x = Mathf.Clamp(size.x, 1, a_columns - a_corridor.EndPosX);
                        pos.x = a_corridor.EndPosX + 1;

                        pos.y = Mathf.RoundToInt(Random.Range(a_corridor.EndPosY - size.y + 1, a_corridor.EndPosY));
                        pos.y = Mathf.Clamp(pos.y, 0, a_rows - size.y);

                        if (CheckBounds(a_columns, a_rows))
                            recalcRoom = true;

                        break;
                    }
                case Direction.South:
                    {
                        size.y = Mathf.Clamp(size.y, 1, a_corridor.EndPosY);
                        pos.y = a_corridor.EndPosY - size.y;

                        pos.x = Mathf.RoundToInt(Random.Range(a_corridor.EndPosX - size.x + 1, a_corridor.EndPosX));
                        pos.x = Mathf.Clamp(pos.x, 0, a_columns - size.x);

                        if (CheckBounds(a_columns, a_rows))
                            recalcRoom = true;

                        break;
                    }
                case Direction.West:
                    {
                        size.x = Mathf.Clamp(size.x, 1, a_corridor.EndPosX);
                        pos.x = a_corridor.EndPosX - size.x;

                        pos.y = Mathf.RoundToInt(Random.Range(a_corridor.EndPosY - size.y + 1, a_corridor.EndPosY));
                        pos.y = Mathf.Clamp(pos.y, 0, a_rows - size.y);

                        if (CheckBounds(a_columns, a_rows))
                            recalcRoom = true;

                        break;
                    }
            }
        }
    }

    bool CheckBounds(int a_columns, int a_rows)
    {
        // Function to check if the room goes outside the bounds of the room
        // Retruns true if it does, retrns false if it doesn't

        // Checking above
        if (pos.y + size.y >= a_rows)
            return true;

        // Checking left
        if (pos.x < 0)
            return true;

        // Checking right
        if (pos.x + size.x > a_columns)
            return true;

        // Checking down
        if (pos.y < 0)
            return true;

        // If we are inside the bounds of the map return false
        return false;
    }

    #endregion
}
