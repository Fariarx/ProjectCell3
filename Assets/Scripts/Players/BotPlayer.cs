using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BotPlayer : Player
{
    public BotPlayer(int id, GameObject context) : base(id, context)
    {

    }
    public override IEnumerator Move(int playerID, MonoBehaviour myMonoBehaviour)
    {
        StartMove();

        var economics = Globals.territoryHandler.territories[playerID];

        foreach(var economic in economics)
        { 
            var groundsUnit = CheckBorUnitMove(economic);

            for (var i = 0; i < groundsUnit.Count; i++)
            {
                var groundUnit = groundsUnit[i];

                Globals.stepCalculator.CalculateUnitSteps(groundUnit, out var unitPathes, out var spent);

                if (unitPathes.Count > 0)
                {
                    var pathCandidate = CheckBotPathPriority(unitPathes, groundUnit); 
                    if (pathCandidate != null)
                    {
                        if (groundUnit.unitOnCell.stepsLeftPerMove < groundUnit.unitOnCell.stepsPerMoveMax)
                        {
                            i--;
                        }

                        yield return myMonoBehaviour.StartCoroutine(Globals.gameSystem.UnitStepByPath(pathCandidate, groundUnit)); 
                    }
                }
            }

            CheckBotBuildFarmer(economic);
            CheckBotBuildFarm(economic);
        }

        //yield return new WaitForSeconds(0.5f);

        EndMove();
    }
    private int SortForBuildFarm(KeyValuePair<float, GroundController> c1, KeyValuePair<float, GroundController> c2)
    {
        if (c1.Key < c2.Key)
        {
            return 1;
        }
        else if(c1.Key == c2.Key)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
    private void CheckBotBuildFarm(PlayerEconomic playerEconomic)
    {
        const float coffersBuildCof = 0.9f;
        float chance = 0.7f;

        if (Globals.gameSystem.stepIndex < 3)
        {
            chance = 1f;
        }

        if (playerEconomic.coffers * coffersBuildCof > _farmTheCost)
        {
            var bestVariants = new List<KeyValuePair<float, GroundController>>();
            var freeCells = 0;

            foreach (var ground in playerEconomic.cells)
            {
                if (ground.unitOnCell != null)
                {
                    continue;
                }
                else
                {
                    freeCells++;
                }

                var neightbors = Globals.territoryHandler.GetNeighborOfHexagon(ground.hexagonPosition);
                var points = 0f;

                foreach (var neightbor in neightbors)
                {
                    if (Globals.territoryHandler.cells.TryGetValue(neightbor.Key, out var ground1))
                    {
                        if (ground1.playerId != ground.playerId)
                        {
                            points--;
                        }
                        else if (ground1.unitOnCell != null)
                        {
                            points++;
                        }
                        else
                        {
                            //points -= 0.5f;
                        }
                    }
                    else
                    {
                        points += 2f;
                    }
                }

                bestVariants.Add(new KeyValuePair<float, GroundController>(points, ground));
            }

            bestVariants.Sort(SortForBuildFarm);

            if (Random.Range(0f, 1f) <= chance && freeCells > 1)
            {
                BuildFarm(bestVariants[0].Value);
                return;
            }
        }
    }
    private void CheckBotBuildFarmer(PlayerEconomic playerEconomic)
    {
        const float coffersBuildCof = 0.6f;
        float chance = 0.5f;

        if(Globals.gameSystem.stepIndex == 0)
        {
            chance = 1f;
        }

        if (playerEconomic.coffers * coffersBuildCof > Globals.unitCosts[Globals.UnitType.UnitFarmer] && playerEconomic.GetProfitabilityPerMove() > 0)
        {
            foreach (var ground in playerEconomic.cells)
            {
                if (ground.unitOnCell != null)
                {
                    continue;
                } 

                if (Random.Range(0f, 1f) <= chance)
                {
                    BuildFarmer(ground);
                    return;
                }
            }
        }
    }
    private List<GroundController> CheckBorUnitMove(PlayerEconomic playerEconomic)
    {
        List<GroundController> grounds = new List<GroundController>();

        foreach (var ground in playerEconomic.cells)
        {
            if(Globals.stepCalculator.CanStepCell(ground))
            {
                grounds.Add(ground);
            }
        }

        return grounds;
    }
    private List<Vector2Int> CheckBotPathPriority(List<List<Vector2Int>> pathes, GroundController groundInit)
    {
        List<List<Vector2Int>> pathCandidate = null;
        float pathCandidatePoints = 0f;

        foreach(var path in pathes)
        {
            var groundFinish = Globals.territoryHandler.cells[path[path.Count-1]];

            if (groundFinish.playerId == -1)
            {
                if (pathCandidatePoints <= 0.5f)
                { 
                    if(pathCandidatePoints < 0.5f)
                    {
                        pathCandidate = new List<List<Vector2Int>>();
                    }

                    pathCandidate.Add(path);
                    pathCandidatePoints = 0.5f;
                }
            }
            else if(groundFinish.playerId != groundInit.playerId)
            {
                if (pathCandidatePoints <= 1.5f)
                {
                    if (pathCandidatePoints < 1.5f)
                    {
                        pathCandidate = new List<List<Vector2Int>>();
                    }

                    pathCandidate.Add(path);
                    pathCandidatePoints = 1.5f;
                }
            }
            else if(groundFinish.playerId == groundInit.playerId)
            {
                if (groundFinish.unitOnCell != null)
                {
                    if (pathCandidatePoints <= 0.35f)
                    {
                        if (pathCandidatePoints < 0.35f)
                        {
                            pathCandidate = new List<List<Vector2Int>>();
                        }

                        pathCandidate.Add(path);
                        pathCandidatePoints = 0.35f;
                    }
                }
                else
                {
                    if (pathCandidatePoints <= 0.3f)
                    {
                        if (pathCandidatePoints < 0.3f)
                        {
                            pathCandidate = new List<List<Vector2Int>>();
                        }

                        pathCandidate.Add(path);
                        pathCandidatePoints = 0.3f;
                    }
                }
            }
        }

        return pathCandidate[Random.Range(0, pathCandidate.Count - 1)];
    }
}
