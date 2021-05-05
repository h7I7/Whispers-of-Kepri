using UnityEngine;

public class TurretNode : MonoBehaviour
{
	public Color hoverColor;

	private Renderer rend;

	private Color startColor;

	[Header("Optional")]
	public GameObject turret;

	public Vector3 positionOffSet;

	public TurretShop turretShop;

	void Start()
	{
		rend = GetComponent<Renderer>();
		startColor = rend.material.color;
	}

	public Vector3 GetBuildPosition()
	{
		return transform.position + positionOffSet;
	}

	public void Selected()
	{
		rend.material.color = hoverColor;
		turretShop.SetNode(this);

		if (turret != null)
		{
			turret.GetComponent<Turret>().ShowRange();
		}
	}
	public void Deselcted()
	{
		rend.material.color = startColor;

		if (turret != null)
		{
			turret.GetComponent<Turret>().StopShowingRange();
		}
	}
}
