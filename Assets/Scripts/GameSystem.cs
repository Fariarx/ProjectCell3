using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class GameSystem : MonoBehaviour
{    
    [HideInInspector]
    public bool isPause = false;
    [HideInInspector]
    public GroundController selectedCell = null;
    [HideInInspector]
    public PlayerEconomic selectedEconomic = null;
    [HideInInspector]
    private uint _stepIndex = 0;
    public uint stepIndex
    {
        get
        {
            return _stepIndex;
        }
    }


    private int _moveOfPlayerID = Globals.defaultLocalPlayerId;
    public int moveOfPlayerID
    {
        get
        {
            return _moveOfPlayerID;
        }
    }

    void CameraMoveToLocalPlayer()
    {
        var obj = Globals.territoryHandler.FindMainHouseCellOfPlayer(Globals.defaultLocalPlayerId);

        if (obj != null) {
            Globals.inputController.LookAt(obj.transform, Globals.lookAtLocalPlayerTime);
        }
    }

    private void Awake()
    {
        Globals.gameSystem = this;
    }
    void Start()
    { 
        CameraMoveToLocalPlayer();
        StartCoroutine(CycleSequenceOfGame());
    } 

    private IEnumerator CycleSequenceOfGame()
    {
        while(true)
        {
            Player player;

            foreach (int playerID in Globals.territoryHandler.territories.Keys)
            {
                _moveOfPlayerID = playerID;

                Globals.userInterface.UpdateUI();

                player = Globals.playerSystem.GetPlayerById(playerID);
                
                if (player != null)
                {
                    yield return StartCoroutine(player.Move(playerID, this));
                }
                else
                {
                    Debug.Log("Player already was deleted!");
                }
            }

            yield return new WaitForSeconds(0.1f);

            _stepIndex++;
        }
    }
    public IEnumerator UnitStepByPath(List<Vector2Int> path, GroundController from, bool fromInterface = false)
    {
        bool setIsUnitMove = false;
        if (Globals.defaultLocalPlayerId == from.playerId)
        {
            Globals.isUnitMove = true;
            setIsUnitMove = true;
        }

        GroundController initCell = from;

        Player playerFrom = Globals.playerSystem.GetPlayerById(from.playerId);

        Unit unit = (Unit) from.unitOnCell; 

        unit.isMoving = true;

        IUnitOnGround unitOnGround = from.unitOnCell;
        ISteppingUnit steppingUnit = (ISteppingUnit)unitOnGround;

        float speed;

        if(from.playerId == Globals.defaultLocalPlayerId)
        {
            speed = Globals.animationMoveSpeedForLocalPlayer;
        }
        else
        {
            speed = Globals.animationMoveSpeedForOtherPlayer;
        }

        steppingUnit.StepAnimation(true, speed);


        GroundController to = from;
        GroundController last = from;

        System.Action unitUpdate = null;

        Globals.lockedCellsForUnitMove.Add(path[path.Count - 1]);

        foreach (var hexagonPosition in path)
        {
            to = Globals.territoryHandler.cells[hexagonPosition];

            bool destroyObjectOnCell = false;

            if (to.unitOnCell != null)
            {
                unitUpdate = playerFrom.MoveMergeUnits(initCell, to);
                to.unitOnCell.DeletePrefabWithAnimation();
                destroyObjectOnCell = true;
            }

            unitOnGround.createdPrefub.transform.LookAt(to.transform.position);
            float step = (speed * 2 / (from.transform.position - to.transform.position).magnitude) * Time.fixedDeltaTime;
            float t = 0;
            while (t <= 1.0f)
            {
                t += step; // Goes from 0 to 1, incrementing by step each time
                unitOnGround.createdPrefub.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, t); // Move objectToMove closer to b
                yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
            }

            //steppingUnit.StepAnimation(false);
            //yield return new WaitForSeconds(0.1f);
            //steppingUnit.StepAnimation(true);

            last = from; 
            from = to;
             
            if (destroyObjectOnCell)
            {
                break;
            } 
        }

        unitOnGround.stepsLeftPerMove++;

        if (unitOnGround.stepsLeftPerMove >= unitOnGround.stepsPerMoveMax)
        {
            steppingUnit.StepAnimation(false);
        }
        else
        {
            steppingUnit.StepAnimation(true, 1);
        }

        unitOnGround.createdPrefub.transform.position = new Vector3(to.transform.position.x, unitOnGround.createdPrefub.transform.position.y, to.transform.position.z); 

        initCell.unitOnCell = null;

        var enemyPid = to.playerId;

        if (to.unitOnCell != null && to.unitOnCell.unitType == Globals.UnitType.UnitMainHouse)
        {
            //вычисление захвата территорий при захвате маин хаус
            var neitherbors = Globals.territoryHandler.GetNeighborOfHexagon(to.hexagonPosition);

            foreach(var neither in neitherbors.Keys)
            {
                GroundController groundController;

                if (Globals.territoryHandler.cells.TryGetValue(neither, out groundController))
                {
                    if (groundController.playerId == enemyPid)
                    {
                        groundController.playerId = initCell.playerId;
                        groundController.CalculatePlayerColor();
                    }
                }
            }
        }

        to.unitOnCell = unitOnGround;
        to.playerId = initCell.playerId;
        to.CalculatePlayerColor();

        if (unitUpdate != null)
        {
            unitUpdate();
        }

        if (enemyPid != initCell.playerId)
        {
            Dictionary<int, List<PlayerEconomic>> territories;

            Globals.territoryHandler.CalculateInitialTerritories(out territories);

            //удаление единичных территорий
            bool wasClear = false;
            for (var index1 = 0; index1 < territories.Count; index1++)
            { 
                var pid = territories.ElementAt(index1).Key; 

                for (var index3 = 0; index3 < territories[pid].Count; index3++)
                {
                    var economic = territories[pid][index3];
                    if (economic.cells.Count <= 1)
                    {
                        if (economic.cells.Count == 1)
                        {
                            var cell = economic.cells[0];
                            if (cell.unitOnCell != null)
                            {
                                cell.unitOnCell.DeletePrefabWithAnimation();
                                cell.unitOnCell = null;
                            }
                            cell.playerId = -1;
                            cell.CalculatePlayerColor();
                        }
                        territories[pid].RemoveAt(index3);
                        wasClear = true;
                        index3--;
                    }
                }
            }

            if (wasClear)
            {
                Globals.territoryHandler.CalculateInitialTerritories(out territories);
            }

            //фикс казны и слияние территорий
            for (var index1 = 0; index1 < Globals.territoryHandler.territories.Count; index1++)
            {
                var keyValue = Globals.territoryHandler.territories.ElementAt(index1);
                var pid = keyValue.Key;
                var playerEconomicsOld = keyValue.Value;

                if (!territories.ContainsKey(pid))
                {
                    //событие проигрыша игрока
                    Globals.playerSystem.GetPlayerById(pid).Delete();
                    continue;
                }
                
                for (var index3 = 0; index3 < territories[pid].Count; index3++)
                {
                    var economic = territories[pid][index3];
                    if (economic.cells.Count <= 1)
                    {
                        if(economic.cells.Count == 1)
                        {
                            var cell = economic.cells[0];
                            if (cell.unitOnCell != null)
                            {
                                cell.unitOnCell.DeletePrefabWithAnimation();
                                cell.unitOnCell = null;
                            }
                        }
                        territories[pid].RemoveAt(index3);
                        index3--;
                    }
                } 

                if(playerEconomicsOld.Count == territories[pid].Count)
                {
                    //тут считаем насколько новая территория похожа на старую и даём свою часть казны той которая слилась воедино
                    for (var index2 = 0; index2 < territories[pid].Count; index2++)
                    {
                        territories[pid][index2].coffers = playerEconomicsOld[index2].coffers;
                    }
                    continue;
                }
                if(playerEconomicsOld.Count > territories[pid].Count)
                {
                    //тут считаем насколько новая территория похожа на старую и даём свою часть казны той которая слилась воедино
                    foreach (PlayerEconomic playerEconomicNew in territories[pid])
                    {
                        playerEconomicNew.coffers = 0;

                        foreach (PlayerEconomic playerEconomicOld in playerEconomicsOld)
                        {
                            bool equal = false;

                            foreach (GroundController groundControllerOld in playerEconomicOld.cells)
                            {  
                                foreach (GroundController groundControllerNew in playerEconomicNew.cells)
                                {
                                    if (groundControllerOld == groundControllerNew)
                                    {
                                        equal = true;
                                        break;
                                    }
                                }

                                if (equal) break;
                            }

                            if (equal)
                            {
                                playerEconomicNew.coffers += playerEconomicOld.coffers;
                            }
                        }
                    }
                    continue;
                }  
                if(playerEconomicsOld.Count < territories[pid].Count)
                {  
                    //тут считаем насколько новая территория похожа на старую и даём свою часть казны разделённой
                    foreach (PlayerEconomic playerEconomicOld in playerEconomicsOld)
                    {  
                        foreach (PlayerEconomic playerEconomicNew in territories[pid])
                        {
                            float equalCount = 0;

                            foreach (GroundController groundControllerOld in playerEconomicOld.cells)
                            {
                                foreach(GroundController groundControllerNew in playerEconomicNew.cells)
                                {
                                    if(groundControllerOld == groundControllerNew)
                                    {
                                        equalCount+=1;
                                    } 
                                }
                            }

                            if(playerEconomicOld.cells.Count != equalCount && playerEconomicOld.cells.Count != 0)
                            {
                                playerEconomicNew.coffers = playerEconomicOld.coffers * (equalCount / (float)playerEconomicOld.cells.Count);
                            }
                        } 
                    } 
                }
            }

            Globals.territoryHandler.territories = territories;

            if (enemyPid != -1)
            {
                var enemy = Globals.playerSystem.GetPlayerById(enemyPid);
                
                if (enemy != null)
                {
                    enemy.UpdateDivisionOfTerritories();
                }
            }

            Globals.territoryHandler.CalculateMainHousesAndDeleteNullTerritory();
        }

        if (initCell.playerId != -1)
        {
            Globals.playerSystem.GetPlayerById(initCell.playerId).UpdateDivisionOfTerritories();
        }

        if(fromInterface)
        { 
            if (initCell.playerId == Globals.defaultLocalPlayerId)
            {
                Globals.userInterface.SetCellSelect(to);
            }

            Globals.userInterface.SetHighlightUnitSteps(selectedCell);
        }

        if (setIsUnitMove)
        {
            Globals.isUnitMove = false;
        }

        unit.isMoving = false;

        Globals.lockedCellsForUnitMove.Remove(path[path.Count - 1]);
    }
}
