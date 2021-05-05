using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cell {

    public Vector3 Position;
    [SerializeField]
    public List<Vector3> floorPositions = null;
    private int m_minimumWidth = 2;
    private int m_maximumWidth = 8;
    private int m_minimumHeight = 2;
    private int m_maximumHeight = 12;
    public int moves = 0;

    public void SetDimensionConstraints(int a_minWidth, int a_maxWidth, int a_minHeight, int a_maxHeight)
    {
        m_minimumWidth = a_minWidth;      m_maximumWidth = a_maxWidth;
        m_minimumHeight = a_minHeight;    m_maximumHeight = a_maxHeight;
    }

    public int _width;
    public int _height;

    public Vector3 Size
    {
        get { return new Vector3(_width, 0f, _height); }
    }

    public float DistanceToCenter
    {
        get { return Mathf.Sqrt((Position.x * Position.x) + (Position.z * Position.z)); }

    }

    public Vector3 MoveVector;

    public void RoundToNearestTilePosition()
    {
        Position = new Vector3(Mathf.Floor(Position.x), 0f, Mathf.Floor(Position.z));
    }

    public void GenerateCell()
    {
        do
        {
            _width = (int)(Mathf.Abs(PMB_Random.MarsagliaNormalDistribution(0f, 0.3f)) * m_maximumWidth) + m_minimumWidth;
            _height = (int)(Mathf.Abs(PMB_Random.MarsagliaNormalDistribution(0f, 0.3f)) * m_maximumHeight) + m_minimumHeight;
        } while (_width > m_maximumWidth || _height > m_maximumHeight);

        Vector3 offset = new Vector3(-_width * 0.5f, 0f, -_height * 0.5f);
        floorPositions = new List<Vector3>();

        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                floorPositions.Add(new Vector3(offset.x + x + 0.5f, 0f, offset.z + y + 0.5f));
            }
        }
    }

    public bool CheckCorridorCollision(LineSegment a_corridor1, LineSegment a_corridor2 = null)
    {
        // Check if the corridor is coming into this cell and return false if it is
        if ((a_corridor1.p0.Value.x == Position.x && a_corridor1.p0.Value.y == Position.y) ||
            (a_corridor1.p1.Value.x == Position.x && a_corridor1.p1.Value.y == Position.y))
        {
            return false;
        }

        // Same again for corridor 2
        if (a_corridor2 != null)
        {
            if ((a_corridor2.p0.Value.x == Position.x && a_corridor2.p0.Value.y == Position.y) ||
            (a_corridor2.p1.Value.x == Position.x && a_corridor2.p1.Value.y == Position.y))
            {
                return false;
            }
        }

        // We will need the cells bottom left corner position and top right corner position to
        // accurately calculate the collision between the cell and the line
        Vector2 bottom_left = new Vector2(floorPositions[0].x - 0.5f, floorPositions[0].z - 0.5f);
        Vector2 top_right = new Vector2(floorPositions[floorPositions.Count - 1].x + 0.5f, floorPositions[floorPositions.Count - 1].y + 0.5f);

        // Linetype will be 0 for horizontal and 1 for vertical
        int lineType = 0;

        // Check line type for corridor 1
        // If the y co-ordinates are the same then it has to be horizontal
        if (a_corridor1.p0.Value.y == a_corridor1.p1.Value.y)
        {
            lineType = 0;
        }
        else
        {
            // Else its vertical
            lineType = 1;
        }

        // Check for collision on corridor 1
        switch(lineType)
        {
            // If the line is horizontal
            case 0:
                {
                    if (top_right.x < a_corridor1.p0.Value.x)
                    {
                        break;
                    }

                    if (bottom_left.x > a_corridor1.p1.Value.x)
                    {
                        break;
                    }

                    if (bottom_left.y < a_corridor1.p0.Value.y && top_right.y > a_corridor1.p0.Value.y)
                    {
                        return true;
                    }
                    break;
                }
            // If the line is vertical
            case 1:
                {
                    if (top_right.y < a_corridor1.p0.Value.y)
                    {
                        break;
                    }

                    if (bottom_left.y > a_corridor1.p1.Value.y)
                    {
                        break;
                    }

                    if (bottom_left.x < a_corridor1.p0.Value.x && top_right.x > a_corridor1.p0.Value.x)
                    {
                        return true;
                    }
                    break;
                }
        }

        // Check for collision on corridor 1
        if (a_corridor2 != null)
        {
            // Check line type for corridor 2
            // If the y co-ordinates are the same then it has to be horizontal
            if (a_corridor2.p0.Value.y == a_corridor2.p1.Value.y)
            {
                lineType = 0;
            }
            else
            {
                // Else its vertical
                lineType = 1;
            }

            // Check for collision on corridor 2
            switch (lineType)
            {
                // If the line is horizontal
                case 0:
                    {
                        if (top_right.x < a_corridor2.p0.Value.x)
                        {
                            break;
                        }

                        if (bottom_left.x > a_corridor2.p1.Value.x)
                        {
                            break;
                        }

                        if (bottom_left.y < a_corridor2.p0.Value.y && top_right.y > a_corridor2.p0.Value.y)
                        {
                            return true;
                        }
                        break;
                    }
                // If the line is vertical
                case 1:
                    {
                        if (top_right.y < a_corridor2.p0.Value.y)
                        {
                            break;
                        }

                        if (bottom_left.y > a_corridor2.p1.Value.y)
                        {
                            break;
                        }

                        if (bottom_left.x < a_corridor2.p0.Value.x && top_right.x > a_corridor2.p0.Value.x)
                        {
                            return true;
                        }
                        break;
                    }
            }
        }

        return false;
    }
}
