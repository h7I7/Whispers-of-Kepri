using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
	public static int Money;
	public int startMoney = 500;

	public Text money;

	void Start()
	{
		Money = startMoney;
	}

	void Update()
	{
		money.text =  "$" + Money.ToString();
	}
}
