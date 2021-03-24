using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    private void Awake()
    {
        Globals.userInterface = this;
    }

    void Start()
    { 
        Globals.localPlayerUISettings.skipStep.GetComponent<Button>().onClick.AddListener(() => LocalPlayerSkipMove());
    }

    void LocalPlayerSkipMove()
    { 
        if (Globals.isUnitMove)
        {
            Globals.localPlayerUISettings.playerGoldLabel.text = "Wait unit moving";
            return;
        }

        Globals.playerSystem.GetLocalPlayer().SkipMove();
    }

    void Update()
    {
        
    }


    private int _indexSelectCell = 0;
    public int indexSelectCell
    {
        get
        {
            return _indexSelectCell;
        }
    }
    public void SetCellSelect(GroundController groundController, bool isBuySelect = false)
    {
        if(Globals.gameSystem.moveOfPlayerID != Globals.defaultLocalPlayerId)
        {
            return;
        }

        _indexSelectCell++;

        if (Globals.gameSystem.selectedCell != null)
        {
            if(Globals.stepCalculator.CanStepCell(Globals.gameSystem.selectedCell) && !isBuySelect)
            {
                Globals.stepCalculator.CalculateUnitSteps(Globals.gameSystem.selectedCell, out var unitPathes, out var spent);

                List<Vector2Int> truthPath = null;

                foreach(var path in unitPathes)
                { 
                    if (path[path.Count-1] == groundController.hexagonPosition)
                    {
                        if (truthPath == null || truthPath.Count > path.Count)
                        {
                            truthPath = path;
                        }
                    }
                }

                if (truthPath != null) {
                    StartCoroutine(Globals.gameSystem.UnitStepByPath(truthPath, Globals.gameSystem.selectedCell, true));
                    return;
                }
                else
                {
                    Debug.Log("No path to unit move");
                }
            }

            Globals.gameSystem.selectedCell.SetSelection(false);
            Globals.gameSystem.selectedCell = null; 
        }

        Globals.gameSystem.selectedCell = groundController;
        Globals.gameSystem.selectedCell.SetSelection(true);

        SetHighlightUnitSteps(groundController);

        if (Globals.gameSystem.selectedEconomic != null)
        { 
            Globals.playerSystem.GetPlayerById(Globals.gameSystem.selectedEconomic.pid).SetBorderUnselectedAll();
        }

        if (groundController.playerId != -1)
        {
            PlayerEconomic playerEconomic = null;
            int territoryIdOfPlayer = Globals.territoryHandler.FindPlayerEconomicByCell(groundController, ref playerEconomic); 
            Globals.gameSystem.selectedEconomic = playerEconomic;
            Globals.playerSystem.GetPlayerById(groundController.playerId).SetBorderSelected(territoryIdOfPlayer, true);
        }
        else
        {
            Globals.gameSystem.selectedEconomic = null;
        }

        UpdateUI();
    }
    public void SetHighlightUnitSteps(GroundController groundController)
    {
        if (groundController.playerId == Globals.defaultLocalPlayerId && Globals.stepCalculator.CanStepCell(groundController))
        {
            Globals.stepCalculator.CalculateUnitSteps(groundController, out var unitPathes, out var spent);
            Globals.territoryHandler.SetSelectedStepCellsByPosition(spent);
        }
        else
        {
            Globals.territoryHandler.SetSelectedStepCellsByPosition();
        }
    }
     
    public void UpdateUI()
    {
        Globals.localPlayerUISettings.defenseTowerGoldText.text = Globals.unitCosts[Globals.UnitType.UnitDefenseTower].ToString("0");
        Globals.localPlayerUISettings.farmerGoldText.text = Globals.unitCosts[Globals.UnitType.UnitFarmer].ToString("0");

        if (Globals.playerSystem.GetLocalPlayer() != null)
        {
            Globals.localPlayerUISettings.farmGoldText.text = Globals.playerSystem.GetLocalPlayer().farmTheCost.ToString("0");
        }

        PlayerEconomic playerEconomic = Globals.gameSystem.selectedEconomic;
         
        Globals.localPlayerUISettings.buildPanel.SetActive(Globals.gameSystem.moveOfPlayerID == Globals.defaultLocalPlayerId);
        Globals.localPlayerUISettings.skipStep.SetActive(Globals.gameSystem.moveOfPlayerID == Globals.defaultLocalPlayerId);

        if (Globals.gameSystem.moveOfPlayerID != Globals.defaultLocalPlayerId)
        {
            Globals.localPlayerUISettings.playerGoldLabel.text = "Move of player #" + Globals.gameSystem.moveOfPlayerID.ToString();
        }
        else if (playerEconomic == null)
        {
            Globals.localPlayerUISettings.playerGoldLabel.text = "Neutral economic!";
        }
        else if (playerEconomic.pid != Globals.defaultLocalPlayerId)
        {
            Globals.localPlayerUISettings.playerGoldLabel.text = "Enemy economic!";
        }
        else
        {
            var selectedEconomicId = Globals.territoryHandler.FindIdByPlayerEconomic(Globals.defaultLocalPlayerId, playerEconomic);
            var profitability = playerEconomic.GetProfitabilityPerMove();

            Globals.localPlayerUISettings.playerGoldLabel.text = "Territory #" + selectedEconomicId.ToString() + "\n" + "Gold: " + playerEconomic.coffers.ToString("0.00") + "\n" + "Profitability: " + profitability.ToString("0.00");
        }
    }
}
