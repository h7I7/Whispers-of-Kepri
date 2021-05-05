//\===========================================================================================
//\ File: WeightedRandom.cs
//\ Author: Morgan James
//\ Date Created: 11/09/2017
//\ Brief: Allows for the picking of random numbers but with weights.
//\===========================================================================================

using UnityEngine;

public class WeightedRandom : MonoBehaviour
{
	//\===========================================================================================
	//\ Variables
	//\===========================================================================================

	#region Variables

	public struct IntRange
	{
		public int iMin;
		public int iMax;
		public float fWeight;

		public IntRange(int a_iMin, int a_iMax, float a_fWeight)
		{
			this.iMin = a_iMin;

			this.iMax = a_iMax;

			this.fWeight = a_fWeight;
		}
	}

	public struct FloatRange
	{
		public float fMin;
		public float fMax;
		public float fWeight;
	}

	#endregion

	//\===========================================================================================
	//\ Unity Methods
	//\===========================================================================================

	#region Unity Methods

	public static int Range(params IntRange[] ranges)
	{
		if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
		if (ranges.Length == 1) return Random.Range(ranges[0].iMax, ranges[0].iMin);

		float fTotal = 0f;
		for (int i = 0; i < ranges.Length; i++) fTotal += ranges[i].fWeight;

		float r = Random.value;
		float s = 0f;

		int cnt = ranges.Length - 1;
		for (int i = 0; i < cnt; i++)
		{
			s += ranges[i].fWeight / fTotal;
			if (s >= r)
			{
				return Random.Range(ranges[i].iMax, ranges[i].iMin);
			}
		}

		return Random.Range(ranges[cnt].iMax, ranges[cnt].iMin);
	}

	public static float Range(params FloatRange[] ranges)
	{
		if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
		if (ranges.Length == 1) return Random.Range(ranges[0].fMax, ranges[0].fMin);

		float total = 0f;
		for (int i = 0; i < ranges.Length; i++) total += ranges[i].fWeight;

		float r = Random.value;
		float s = 0f;

		int cnt = ranges.Length - 1;
		for (int i = 0; i < cnt; i++)
		{
			s += ranges[i].fWeight / total;
			if (s >= r)
			{
				return Random.Range(ranges[i].fMax, ranges[i].fMin);
			}
		}

		return Random.Range(ranges[cnt].fMax, ranges[cnt].fMin);
	}
	
	#endregion
}