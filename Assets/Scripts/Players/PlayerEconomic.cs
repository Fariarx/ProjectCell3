using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEconomic 
{
    public float coffers;
    public int pid;
    public List<GroundController> cells;

    public PlayerEconomic(List<GroundController> cells, int pid)
    {
        this.coffers = Globals.defaultCoffers;
        this.cells = cells;
        this.pid = pid;
    }
    public PlayerEconomic(float coffers, List<GroundController> cells, int pid)
    {
        this.coffers = coffers;
        this.cells = cells;
        this.pid = pid;
    }
    public float GetProfitabilityPerMove()
    {
        var profitability = 0f;

        foreach (var cell in cells)
        {
            if (cell.unitOnCell != null)
            {
                profitability += cell.unitOnCell.farmModifier;
            }

            profitability += Globals.cellFarmModifier;
        }

        return profitability;
    }
    public void CalculateProfitabilityPerMove()
    {
        var profitability = 0f;

        foreach (var cell in cells)
        {
            if (cell.unitOnCell != null)
            {
                profitability += cell.unitOnCell.farmModifier;
            }

            profitability += Globals.cellFarmModifier;
        }

        coffers += profitability;
    }
}
