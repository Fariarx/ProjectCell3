using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepCalculator : MonoBehaviour
{
    private void Awake()
    {
        Globals.stepCalculator = this;
    }

    public void CalculateUnitSteps(GroundController initCell, out List<List<Vector2Int>> unitPathes, out List<Vector2Int> spent)
    {
        IUnitOnGround initUnitOnGround = initCell.unitOnCell;

        unitPathes = new List<List<Vector2Int>>();
        spent = new List<Vector2Int>();

        if (initUnitOnGround == null)
        {
            return;
        }

        int steps = Globals.unitLenghOfStep[initUnitOnGround.unitType];

        if (steps == 0)
        {
            return;
        }

        System.Func<GroundController, bool> checkCanMoveOnCell = (checkCell) => {
            return CanHijackCell(initCell, checkCell);
        };
        System.Func<GroundController, bool> checkLastCanMoveOnCell = (checkCell) => {
            return checkCell.playerId == initCell.playerId;
        };

        Globals.territoryHandler.CalculateUnitSteps(checkCanMoveOnCell, checkLastCanMoveOnCell, initCell, steps, unitPathes, spent);
    }

    public bool CanHijackCell(GroundController from, GroundController to)
    {
        if(Globals.lockedCellsForUnitMove.Contains(to.hexagonPosition))
        {
            return false;
        }

        if(from.unitOnCell == null)
        {
            return false;
        }
        if (Globals.playerSystem.GetPlayerById(from.playerId).MoveMergeUnits(from, to) != null)
        {
            return true;
        }
        if ((from.unitOnCell.unitType == Globals.UnitType.UnitFarmer || from.unitOnCell.unitType == Globals.UnitType.UnitFarmerUpgraded) && to.playerId != from.playerId && to.playerId != -1)
        {
            return false;
        } 

        var neightbors = Globals.territoryHandler.GetNeighborOfHexagon(to.hexagonPosition);

        if (from.unitOnCell.unitType < Globals.UnitType.UnitDefenseTower)
        {
            foreach (var neightbor in neightbors)
            {
                if (Globals.territoryHandler.cells.TryGetValue(neightbor.Key, out var groundController))
                {
                    if (groundController.playerId != from.playerId && groundController.unitOnCell != null && groundController.unitOnCell.unitType == Globals.UnitType.UnitDefenseTower)
                    {
                        return false;
                    }
                }
            }
        }

        if (to.unitOnCell == null)
        {
            return true;
        }
        if ((from.unitOnCell.unitType != Globals.UnitType.UnitFarmer && from.unitOnCell.unitType != Globals.UnitType.UnitFarmerUpgraded) && to.unitOnCell.unitType == Globals.UnitType.UnitTree)
        {
            return false;
        }
        if(Globals.unitLenghOfStep[from.unitOnCell.unitType] == 0)
        {
            return false;
        }
        if(from.playerId == to.playerId && to.unitOnCell.unitType != Globals.UnitType.UnitTree)
        {
            return false;
        }  

        return (int)from.unitOnCell.unitType > (int)to.unitOnCell.unitType;
    }
    public bool CanStepCell(GroundController ground)
    {
        return ground.unitOnCell != null && ground.unitOnCell.stepsLeftPerMove < ground.unitOnCell.stepsPerMoveMax && Globals.unitLenghOfStep[ground.unitOnCell.unitType] != 0 && !((Unit)ground.unitOnCell).isMoving; 
    }
}
