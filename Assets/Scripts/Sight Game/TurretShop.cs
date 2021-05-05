using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class TurretShop : MonoBehaviour
{
	TurretBuildManager buildManager;

	public TurretBluprint arrowTurret;
	public TurretBluprint fireTurret;
	public TurretBluprint slowTurret;
	public TurretBluprint beamTurret;

	private TurretNode selectedNode;

	public GameObject PlaceTurrets;
	public GameObject Modify;
	public GameObject PlaceTurretsCost;
	public GameObject ModifyCost;
	public Button X;
	public Button Arrow;
	public Button ArrowCost;
	public Button Power;
	public Button PowerCost;

	public Button Y;
	public Button Fire;
	public Button FireCost;
	public Button Speed;
	public Button SpeedCost;

	public Button A;
	public Button Slow;
	public Button SlowCost;
	public Button Range;
	public Button RangeCost;

	public Button B;
	public Button Beam;
	public Button BeamCost;
	public Button Sell;
	public Button SellPrice;

	public FloatingText floatingText;

    public Controller controller;

    public GameObject gameManager;

	void Start()
	{
		buildManager = TurretBuildManager.instance;
	}

	public void PurchaseArrowTurret(TurretNode node)
	{
		buildManager.SetTurretToBuild(arrowTurret);
		if (buildManager.BuildTurretOn(node))
		{
			floatingText.CreateFloatingText("-$" + arrowTurret.cost.ToString(), node.transform, Color.red);
		}
	}

	public void PurchaseFireTurret(TurretNode node)
	{
		buildManager.SetTurretToBuild(fireTurret);
		if (buildManager.BuildTurretOn(node))
		{
			floatingText.CreateFloatingText("-$" + fireTurret.cost.ToString(), node.transform, Color.red);
		}
	}
	public void PurchaseSlowTurret(TurretNode node)
	{
		buildManager.SetTurretToBuild(slowTurret);
		if (buildManager.BuildTurretOn(node))
		{
			floatingText.CreateFloatingText("-$" + slowTurret.cost.ToString(), node.transform, Color.red);
		}
	}

	public void PurchaseBeamTurret(TurretNode node)
	{
		buildManager.SetTurretToBuild(beamTurret);
		if (buildManager.BuildTurretOn(node))
		{
			floatingText.CreateFloatingText("-$" + beamTurret.cost.ToString(), node.transform, Color.red);
		}
	}

	public void SetNode(TurretNode node)
	{
		selectedNode = node;
	}

	public void ShopButtonsText()
	{
		if (selectedNode.turret == null)
		{
			ArrowCost.GetComponentInChildren<Text>().text = "-$" + arrowTurret.cost.ToString();
			FireCost.GetComponentInChildren<Text>().text = "-$" + fireTurret.cost.ToString();
			SlowCost.GetComponentInChildren<Text>().text = "-$" + slowTurret.cost.ToString();
			BeamCost.GetComponentInChildren<Text>().text = "-$" + beamTurret.cost.ToString();
		}
		else if (selectedNode.turret != null)
		{
			if (selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				PowerCost.GetComponentInChildren<Text>().text = "Maxed!";
			}
			else
			{
				PowerCost.GetComponentInChildren<Text>().text = "-$" + selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade;
			}

			if (selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				RangeCost.GetComponentInChildren<Text>().text = "Maxed!";
			}
			else
			{
				RangeCost.GetComponentInChildren<Text>().text = "-$" + selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade;
			}

			if (selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				SpeedCost.GetComponentInChildren<Text>().text = "Maxed!";
			}
			else
			{
				SpeedCost.GetComponentInChildren<Text>().text = "-$" + selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade;
			}

			SellPrice.GetComponentInChildren<Text>().text = "+$" + selectedNode.turret.GetComponent<Turret>().SalePrice();
		}
	}

	public void ShopButtonsColor()
	{
		if (selectedNode.turret == null)
		{
			if (PlayerStats.Money < arrowTurret.cost)
			{
				Arrow.GetComponentInChildren<Text>().color = Color.red;
				ArrowCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Arrow.GetComponentInChildren<Text>().color = Color.black;
				ArrowCost.GetComponentInChildren<Text>().color = Color.black;
			}
			if (PlayerStats.Money < fireTurret.cost)
			{
				Fire.GetComponentInChildren<Text>().color = Color.red;
				FireCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Fire.GetComponentInChildren<Text>().color = Color.black;
				FireCost.GetComponentInChildren<Text>().color = Color.black;
			}
			if (PlayerStats.Money < slowTurret.cost)
			{
				Slow.GetComponentInChildren<Text>().color = Color.red;
				SlowCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Slow.GetComponentInChildren<Text>().color = Color.black;
				SlowCost.GetComponentInChildren<Text>().color = Color.black;
			}
			if (PlayerStats.Money < beamTurret.cost)
			{
				Beam.GetComponentInChildren<Text>().color = Color.red;
				BeamCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Beam.GetComponentInChildren<Text>().color = Color.black;
				BeamCost.GetComponentInChildren<Text>().color = Color.black;
			}
		}
		else if (selectedNode.turret != null)
		{
			if (selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				Power.GetComponentInChildren<Text>().color = Color.black;
				PowerCost.GetComponentInChildren<Text>().color = Color.black;
			}
			else if (PlayerStats.Money < selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade)
			{
				Power.GetComponentInChildren<Text>().color = Color.red;
				PowerCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Power.GetComponentInChildren<Text>().color = Color.black;
				PowerCost.GetComponentInChildren<Text>().color = Color.black;
			}

			if (selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				Range.GetComponentInChildren<Text>().color = Color.black;
				RangeCost.GetComponentInChildren<Text>().color = Color.black;
			}
			else if (PlayerStats.Money < selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade)
			{
				Range.GetComponentInChildren<Text>().color = Color.red;
				RangeCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Range.GetComponentInChildren<Text>().color = Color.black;
				RangeCost.GetComponentInChildren<Text>().color = Color.black;
			}

			if (selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade >= selectedNode.turret.GetComponent<Turret>().maxUpgrades)
			{
				Speed.GetComponentInChildren<Text>().color = Color.black;
				SpeedCost.GetComponentInChildren<Text>().color = Color.black;
			}
			else if (PlayerStats.Money < selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade)
			{
				Speed.GetComponentInChildren<Text>().color = Color.red;
				SpeedCost.GetComponentInChildren<Text>().color = Color.red;
			}
			else
			{
				Speed.GetComponentInChildren<Text>().color = Color.black;
				SpeedCost.GetComponentInChildren<Text>().color = Color.black;
			}

		}
	}

	void Update()
	{
		ShopButtonsText();
		ShopButtonsColor();

        controller = gameManager.GetComponent<CameraDisplaySwap>().shopController;

        if (selectedNode.turret == null)//Build a selectedNode.turret
		{
			PlaceTurrets.SetActive(true);
			Modify.SetActive(false);
			PlaceTurretsCost.SetActive(true);
			ModifyCost.SetActive(false);

			if (Input.GetButtonDown(controller.XButton))//X arrow 
			{
				X.OnSelect(null);
				Arrow.OnSelect(null);
				Power.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.XButton))//X
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				PurchaseArrowTurret(selectedNode);
			}
			else if (Input.GetButtonDown(controller.YButton))//Y
			{
				Y.OnSelect(null);
				Fire.OnSelect(null);
				Speed.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.YButton))//Y
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				PurchaseFireTurret(selectedNode);
			}
			else if (Input.GetButtonDown(controller.AButton))//A
			{
				A.OnSelect(null);
				Slow.OnSelect(null);
				Range.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.AButton))//A
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				PurchaseSlowTurret(selectedNode);
			}
			else if (Input.GetButtonDown(controller.BButton))//B
			{
				B.OnSelect(null);
				Beam.OnSelect(null);
				Sell.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.BButton))//B
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				PurchaseBeamTurret(selectedNode);
			}
		}
		else if (selectedNode.turret != null)//Upgrade/sell
		{
			PlaceTurrets.SetActive(false);
			Modify.SetActive(true);
			PlaceTurretsCost.SetActive(false);
			ModifyCost.SetActive(true);
			if (Input.GetButtonDown(controller.XButton))//X
			{
				X.OnSelect(null);
				Arrow.OnSelect(null);
				Power.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.XButton))//X
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				if (PlayerStats.Money >= selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade && selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade < selectedNode.turret.GetComponent<Turret>().maxUpgrades)
				{
					//play effect
					floatingText.GetComponent<FloatingText>().CreateFloatingText("-$" + ((int)selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentDamageUpgrade).ToString(), selectedNode.transform, Color.red);

					selectedNode.turret.GetComponent<Turret>().UpgradeDamage(selectedNode);
				}
			}
			else if (Input.GetButtonDown(controller.YButton))//Y
			{
				Y.OnSelect(null);
				Fire.OnSelect(null);
				Speed.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.YButton))//Y
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null); 
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				if (PlayerStats.Money >= selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade && selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade < selectedNode.turret.GetComponent<Turret>().maxUpgrades)
				{
					floatingText.GetComponent<FloatingText>().CreateFloatingText("-$" + ((int)selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentFireRateUpgrade).ToString(), selectedNode.transform, Color.red);

					selectedNode.turret.GetComponent<Turret>().UpgradeFireRate(selectedNode);
				}
			}
			else if (Input.GetButtonDown(controller.AButton))//A
			{
				A.OnSelect(null);
				Slow.OnSelect(null);
				Range.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.AButton))//A
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);
				if (PlayerStats.Money >= selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade && selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade < selectedNode.turret.GetComponent<Turret>().maxUpgrades)
				{
					floatingText.GetComponent<FloatingText>().CreateFloatingText("-$" + ((int)selectedNode.turret.GetComponent<Turret>().initialCost * selectedNode.turret.GetComponent<Turret>().currentRangeUpgrade).ToString(), selectedNode.transform, Color.red);

					selectedNode.turret.GetComponent<Turret>().UpgradeRange(selectedNode);
				}
			}
			else if (Input.GetButtonDown(controller.BButton))//B
			{
				B.OnSelect(null);
				Beam.OnSelect(null);
				Sell.OnSelect(null);
			}
			else if (Input.GetButtonUp(controller.BButton))//B
			{
				X.OnDeselect(null);
				Y.OnDeselect(null);
				A.OnDeselect(null);
				B.OnDeselect(null);
				Arrow.OnDeselect(null);
				Power.OnDeselect(null);
				Fire.OnDeselect(null);
				Speed.OnDeselect(null);
				Slow.OnDeselect(null);
				Range.OnDeselect(null);
				Beam.OnDeselect(null);
				Sell.OnDeselect(null);

				PlayerStats.Money += (int)selectedNode.turret.GetComponent<Turret>().SalePrice();

				floatingText.CreateFloatingText("+$" + ((int)selectedNode.turret.GetComponent<Turret>().SalePrice()).ToString(), selectedNode.transform, Color.green);

				GameObject effect = Instantiate(selectedNode.turret.GetComponent<Turret>().SellEffect, selectedNode.GetBuildPosition(), Quaternion.identity);
				effect.transform.parent = selectedNode.transform;
				Destroy(effect, 5f);

				Destroy(selectedNode.turret);
				selectedNode.turret = null;

			}
		}
	}
}
