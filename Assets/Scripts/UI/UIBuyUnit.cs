using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuyUnit : MonoBehaviour, IPointerDownHandler 
{
    private GameObject toBuyMove = null;
    private Vector2? toBuyMoveLastScreenPoint = null;
     
    public Globals.UnitType unitType = Globals.UnitType.UnitFarm;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (toBuyMove != null || Globals.isUnitMove)
        {
            return;
        }

        toBuyMove = Instantiate(this.gameObject, GameObject.Find("Canvas").transform);
        Globals.inputController.setCameraLock = true;
        toBuyMove.GetComponent<UIBuyUnit>().enabled = false;
    }

    public void Update()
    {
        if(toBuyMove != null)
        {
            if (Input.touchCount >= 1)
            {
                Touch touch = Input.GetTouch(0);
                toBuyMoveLastScreenPoint = touch.position;
                toBuyMove.transform.position = (Vector3)toBuyMoveLastScreenPoint;
            }
            else
            {
                if (toBuyMoveLastScreenPoint != null) 
                {
                    var ray = Camera.main.ScreenPointToRay((Vector2)toBuyMoveLastScreenPoint);

                    if (Physics.Raycast(ray, out RaycastHit hit, 150f))
                    {
                        GroundController ground = hit.transform.GetComponent<GroundController>();

                        //Debug.Log(hit.transform.gameObject.name);
                        if (ground != null)
                        {
                            BuyUnit(ground);
                        }
                    }

                    toBuyMoveLastScreenPoint = null;
                }

                Globals.inputController.setCameraLock = false;
                Destroy(toBuyMove);
                toBuyMove = null;
            }
        }

        if(Globals.isUnitMove)
        {
            Globals.inputController.setCameraLock = false;
            Destroy(toBuyMove);
            toBuyMove = null;
        }
    }
    private void BuyUnit(GroundController onGround)
    {
        bool result;

        switch (unitType)
        {
            case Globals.UnitType.UnitFarm:
                result = Globals.playerSystem.GetLocalPlayer().BuildFarm(onGround);
                Globals.userInterface.UpdateUI();
                break;
            case Globals.UnitType.UnitFarmer:
                result = Globals.playerSystem.GetLocalPlayer().BuildFarmer(onGround);
                Globals.userInterface.UpdateUI();
                break;
            case Globals.UnitType.UnitDefenseTower:
                result = Globals.playerSystem.GetLocalPlayer().BuildDefenseTower(onGround);
                Globals.userInterface.UpdateUI();
                break;
        }

        Globals.userInterface.SetCellSelect(onGround, true);
    }
}
