using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	public GameObject floatingTextPrefab;
	public Canvas canvas;

	public void CreateFloatingText(string text, Transform location, Color color)
	{
		Vector3 screenPosition = (new Vector3(location.position.x + Random.Range(-5, 5), 5, location.position.z + Random.Range(-5, 5)));

		GameObject effect = Instantiate(floatingTextPrefab);
		effect.transform.SetParent(canvas.transform);
		effect.transform.position = screenPosition;
		effect.transform.GetChild(0).gameObject.GetComponent<Text>().text = text;
		effect.transform.GetChild(0).gameObject.GetComponent<Text>().color = color;
		Destroy(effect, 1.45f);
	}
}
