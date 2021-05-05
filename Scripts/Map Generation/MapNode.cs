//\===========================================================================================
//\ File: MapNode.cs
//\ Author: Morgan James
//\ Date Created: 26/01/2017
//\ Brief: A class to define what a node is for map generation.
//\===========================================================================================

using UnityEngine;
using System.Collections;

public class MapNode
{
	//\===========================================================================================
	//\ Variables
	//\===========================================================================================

	#region Variables

	public Vector3 v3WorldPosition;//The position of the node.
	public bool bPlaced = false;//True if an object has been placed there.

	#endregion

	//\===========================================================================================
	//\ Unity Methods
	//\===========================================================================================

	#region Unity Methods

	public MapNode(Vector3 a_v3WorldPosition)
	{
		v3WorldPosition = a_v3WorldPosition;
	}

	public MapNode(Vector3 a_v3WorldPosition, bool a_bPlaced)
	{
		v3WorldPosition = a_v3WorldPosition;
		bPlaced = a_bPlaced;
	}

	#endregion
}