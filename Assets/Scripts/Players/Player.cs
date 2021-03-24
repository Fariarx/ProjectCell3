using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    #region Properties
    private GameObject _gameObject;
    private GameObject _divisionOfTerritories;
    protected float _farmTheCost;
    protected State _state;
    protected int _id;
    #endregion

    #region GetSet
    public float farmTheCost { get { return _farmTheCost; } }
    public GameObject gameObject { get { return _gameObject; } }
    public int id { get { return _id; } }
    public State state { get { return state; } } 
    #endregion 

    public enum State
    {
        MyMove,
        WaitMoveOfEnemy
    }

    public Player(int id, GameObject context)
    {
        this._id = id;
        
        this._gameObject = new GameObject();
        this._gameObject.name = "Player #: " + id; 
        this._gameObject.transform.SetParent(context.transform, true);

        this._divisionOfTerritories = new GameObject();
        this._divisionOfTerritories.name = "Division of territories";
        this._divisionOfTerritories.transform.SetParent(this._gameObject.transform, true);

        this._state = State.WaitMoveOfEnemy;
        this._farmTheCost = Globals.initialTheCostOfFarm;
    }
    public void StartMove()
    {
        Dictionary<Vector2Int, GroundController> cells = GetAllCells();

        foreach (var cell in cells)
        {
            if (cell.Value.unitOnCell != null)
            {
                cell.Value.unitOnCell.stepsLeftPerMove = 0;

                ISteppingUnit steppingUnit = cell.Value.unitOnCell as ISteppingUnit;

                if (steppingUnit != null && cell.Value.unitOnCell.stepsPerMoveMax != 0)
                {
                    steppingUnit.StepAnimation(true);
                }
            }
        }

        foreach(var keyValues in Globals.territoryHandler.territories)
        {
            foreach(var playerEconomic in keyValues.Value)
            {
                if(playerEconomic.coffers < 0)
                {
                    foreach(var cell in playerEconomic.cells)
                    {
                        if(cell.unitOnCell != null)
                        {
                            if(Globals.unitCanDieWithEconomic.IndexOf(cell.unitOnCell.unitType) != -1)
                            { 
                                var effect = GameObject.Instantiate(Globals.unitsPrefubsCombinator.unitDieEffect);
                                var position = cell.unitOnCell.createdPrefub.transform.position;
                                position.y += 0.2f;
                                effect.transform.position = position; 

                                cell.unitOnCell.DeletePrefabWithAnimation();
                                cell.unitOnCell = null;

                                cell.unitOnCell = new UnitTree(); 
                                cell.unitOnCell.CreatePrefab(cell.gameObject.transform);
                            }
                        }
                    }
                }
            }
        }
    }
    public void EndMove()
    {
        Dictionary<Vector2Int, GroundController> cells = GetAllCells();

        foreach (var cell in cells)
        {
            if (cell.Value.unitOnCell != null)
            {
                ISteppingUnit steppingUnit = cell.Value.unitOnCell as ISteppingUnit;

                if (steppingUnit != null)
                {
                    if (cell.Value.unitOnCell.stepsPerMoveMax != 0)
                    {
                        steppingUnit.StepAnimation(false);
                    }

                    if (id != Globals.defaultLocalPlayerId)
                    {
                        steppingUnit.StepAnimation(true);
                    }
                }
            }
        }

        CalculateProfitabilityOfEconomicsPerMove();
    }
    public virtual IEnumerator Move(int playerID, MonoBehaviour myMonoBehaviour)
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            break;
        }
    }
    public Player UpdateDivisionOfTerritories()
    {
        while(_divisionOfTerritories.transform.childCount > 0)
        {
            GameObject.DestroyImmediate(_divisionOfTerritories.transform.GetChild(0).gameObject);
        }

        Color color = Globals.GetColorForPlayer(id);

        color.a = Globals.lineDivisionOfTerritoryTrasponent;

        Globals.territoryHandler.SetBorderOfTerritory(id, _divisionOfTerritories, color, Globals.lineDivisionOfTerritoryWidth);

        return this;
    }
    public Player SetBorderSelected(int territoryId, bool selected = true)
    {
        int i = 0;

        foreach (Transform child in _divisionOfTerritories.transform)
        {
            if(i == territoryId)
            {
                if (selected)
                {
                    child.GetComponent<MeshRenderer>().material.SetColor("_Color2", new Color(0, 0, 0, 0));
                }
                else
                {
                    child.GetComponent<MeshRenderer>().material.SetColor("_Color2", child.GetComponent<MeshRenderer>().material.GetColor("_Color1"));
                }
                return this;
            }

            i++;
        }

        return this;
    }
    public Player SetBorderUnselectedAll()
    { 
        foreach (Transform child in _divisionOfTerritories.transform)
        {
            child.GetComponent<MeshRenderer>().material.SetColor("_Color2", child.GetComponent<MeshRenderer>().material.GetColor("_Color1"));
        }

        return this;
    }
    public bool IsLocalPlayer()
    {
        return Globals.defaultLocalPlayerId == _id;
    }
    public Dictionary<Vector2Int, GroundController> GetAllCells()
    {
        Dictionary<Vector2Int, GroundController> count = new Dictionary<Vector2Int, GroundController>();
         
        foreach(var cell in Globals.territoryHandler.cells)
        {
            if(cell.Value.playerId == id)
            {
                count.Add(cell.Key, cell.Value);
            }
        }

        return count;
    }
    public void Delete()
    {
        if (Globals.territoryHandler.territories.ContainsKey(id))
        {
            Debug.Log("Player cant was deleted. He has territory.");
        } 

        Globals.playerSystem.DeletePlayerFromList(id);
        GameObject.Destroy(this._gameObject); 
    }
    public void CalculateProfitabilityOfEconomicsPerMove()
    {
        if (!Globals.territoryHandler.territories.ContainsKey(id)) return;

        foreach (var cell in Globals.territoryHandler.territories[id])
        {
            cell.CalculateProfitabilityPerMove();
        } 
    }
    public virtual bool BuildFarm(GroundController cell)
    {
        PlayerEconomic playerEconomic = null;
        int economicId = Globals.territoryHandler.FindPlayerEconomicByCell(cell, ref playerEconomic);
        
        if(economicId == -1 || playerEconomic.pid != id || cell.unitOnCell != null || Globals.gameSystem.moveOfPlayerID != id)
        {
            return false;
        }

        if (playerEconomic.coffers >= _farmTheCost)
        {
            playerEconomic.coffers -= _farmTheCost;
            _farmTheCost += Globals.initialTheCostOfFarmProgression;

            cell.unitOnCell = new UnitFarm();
            cell.unitOnCell.CreatePrefab(cell.gameObject.transform);

            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual bool BuildDefenseTower(GroundController cell)
    {
        PlayerEconomic playerEconomic = null;
        int economicId = Globals.territoryHandler.FindPlayerEconomicByCell(cell, ref playerEconomic);

        if (economicId == -1 || playerEconomic.pid != id || cell.unitOnCell != null || Globals.gameSystem.moveOfPlayerID != id)
        {
            return false;
        }

        if (playerEconomic.coffers >= Globals.unitCosts[Globals.UnitType.UnitDefenseTower])
        {
            playerEconomic.coffers -= Globals.unitCosts[Globals.UnitType.UnitDefenseTower];

            cell.unitOnCell = new UnitDefenseTower();
            cell.unitOnCell.CreatePrefab(cell.gameObject.transform);

            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual bool BuildFarmer(GroundController cell, bool itIsMerge = false, Globals.UnitType? unitType = null)
    {
        PlayerEconomic playerEconomic = null;
        int economicId = Globals.territoryHandler.FindPlayerEconomicByCell(cell, ref playerEconomic);

        if (economicId == -1 || playerEconomic.pid != id || Globals.gameSystem.moveOfPlayerID != id)
        {
            return false;
        }

        var cost = Globals.unitCosts[Globals.UnitType.UnitFarmer];

        if (playerEconomic.coffers >= cost && !itIsMerge || itIsMerge)
        {
            if (cell.unitOnCell == null)
            {
                cell.unitOnCell = new UnitFarmer();
                cell.unitOnCell.CreatePrefab(cell.gameObject.transform);
            }
            else
            {
                var oldUnit = cell.unitOnCell;
                var oldUnitRotation = oldUnit.createdPrefub.transform.eulerAngles;
                var oldUnitSteps = oldUnit.stepsLeftPerMove;

                if(unitType == null)
                {
                    unitType = cell.unitOnCell.unitType;
                }

                //upgrade
                switch (unitType)
                {
                    case Globals.UnitType.UnitFarmer: 
                        cell.unitOnCell.DeletePrefab();
                        cell.unitOnCell = new UnitFarmerUpgraded(); 
                        break;
                    case Globals.UnitType.UnitFarmerUpgraded: 
                        cell.unitOnCell.DeletePrefab();
                        cell.unitOnCell = new UnitSpearman(); 
                        break;
                    case Globals.UnitType.UnitSpearman: 
                        cell.unitOnCell.DeletePrefab();
                        cell.unitOnCell = new UnitKnight();
                        break;
                    default:
                        return false;
                }

                Transform cellTransform = cell.gameObject.transform;

                cell.unitOnCell.CreatePrefab(cellTransform);
                cell.unitOnCell.createdPrefub.transform.eulerAngles = oldUnitRotation;
                cell.unitOnCell.stepsLeftPerMove = oldUnitSteps;

                GameObject.Instantiate(Globals.unitsPrefubsCombinator.unitUpgradeEffect, cellTransform);

                if(cell.unitOnCell.stepsLeftPerMove >= cell.unitOnCell.stepsPerMoveMax)
                {
                    ISteppingUnit steppingUnit = cell.unitOnCell as ISteppingUnit;

                    if (steppingUnit != null)
                    {
                        steppingUnit.StepAnimation(false);
                    }
                }
            }

            if (!itIsMerge)
            {
                playerEconomic.coffers -= cost;
            }

            return true;
        }
        else
        {
            return false;
        }
    } 
    public System.Action MoveMergeUnits(GroundController from, GroundController to)
    {
        if (from.unitOnCell == null || to.unitOnCell == null || from.playerId != to.playerId) return null;
        
        if (from.unitOnCell.unitType == Globals.UnitType.UnitFarmer || to.unitOnCell.unitType == Globals.UnitType.UnitFarmer)
        {
            Globals.UnitType upUnitType;
            
            if(from.unitOnCell.unitType == Globals.UnitType.UnitFarmer)
            {
                upUnitType = to.unitOnCell.unitType;
            }
            else
            {
                upUnitType = from.unitOnCell.unitType;
            }

            switch (from.unitOnCell.unitType == Globals.UnitType.UnitFarmer ? to.unitOnCell.unitType : from.unitOnCell.unitType)
            {
                case Globals.UnitType.UnitFarmer:
                    break;
                case Globals.UnitType.UnitFarmerUpgraded:
                    break;
                case Globals.UnitType.UnitSpearman:
                    break;
                default:
                    return null;
            }

            System.Action result = () =>
            {
                BuildFarmer(to, true, upUnitType);
            };

            return result;
        }
        else
        {
            return null;
        }    
    }
}
