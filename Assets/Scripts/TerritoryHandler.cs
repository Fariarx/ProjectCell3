using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerritoryHandler : MonoBehaviour
{
    public Dictionary<int, List<PlayerEconomic>> territories;
    public Dictionary<Vector2Int, GroundController> cells;

    private Vector3 _centerOfMap;
    public Vector3 centerOfMap
    {
        get
        {
            return _centerOfMap;
        }
    }
     
    public void CalculateUnitSteps(System.Func<GroundController, bool> checkCell, System.Func<GroundController, bool> checkLastCell, GroundController initCell, int unitMoveCount, List<List<Vector2Int>> pathes, List<Vector2Int> spent, List<Vector2Int> path = null, GroundController parentCell = null, bool init = true)
    {
        if (unitMoveCount < 0) return;

        List<Vector2Int> newPath = new List<Vector2Int>();

        if (!init)
        {
            if (checkCell(initCell))
            {
                if (!spent.Contains(initCell.hexagonPosition))
                {
                    spent.Add(initCell.hexagonPosition);
                }

                newPath.Add(initCell.hexagonPosition);
            }
            else
            {
                return;
            }
        } 

        if (newPath.Count != 0)
        {
            if (path != null)
            {
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    newPath.Insert(0, path[i]);
                }
            }

            pathes.Add(newPath);
        }

        if (!checkLastCell(initCell)) return;
        if (unitMoveCount - 1 < 0) return;

        List<Vector2Int> neighbors = GetNeighborOfHexagon(initCell.hexagonPosition).Keys.ToList<Vector2Int>();

        foreach (Vector2Int neighbor in neighbors)
        {
            if (!cells.TryGetValue(neighbor, out var neighborCell) || parentCell != null && neighbor == parentCell.hexagonPosition)
            {
                continue;
            } 

            CalculateUnitSteps(checkCell, checkLastCell, neighborCell, unitMoveCount - 1, pathes, spent, newPath, initCell, false);
        }
    }

    #region border
    private void SetBorderOfTerritoryRecursion(int side, GroundController initCell, List<string> spent, LineRenderer lineRenderer, PlayerEconomic playerEconomic, bool wasDraw = false)
    {  
        Dictionary<Vector2Int, int> neighbors = GetNeighborOfHexagon(initCell.hexagonPosition);

        while(true)
        { 
            KeyValuePair<Vector2Int, int> neighborKV = neighbors.ElementAt(side); 

            string uniqID = initCell.hexagonPosition.x.ToString() + initCell.hexagonPosition.y.ToString() + side.ToString();

            /*if(uniqID.IndexOf("-335") != -1)
            {
                 int a = 0;
            }*/

            if (spent.Contains(uniqID))
            {
                return;
            }
            else
            {
                if (wasDraw)
                {
                    spent.Add(uniqID);
                }
            }

            GroundController find = playerEconomic.cells.Find((GroundController x) => x.hexagonPosition == neighborKV.Key);
             
            if (find == null)
            {
                Vector3 position = initCell.transform.position + GridHelper.corners[side] + Globals.lineOffsetZ;

                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);

                if (lineRenderer.positionCount > 1)
                {
                    GameObject cylinder = Instantiate(Globals.unitsPrefubsCombinator.territoryOutline, lineRenderer.transform);

                    Vector3 positionNew = (position + lineRenderer.GetPosition(lineRenderer.positionCount - 2)) / 2;
                    positionNew.y = 3.05f;
                    cylinder.transform.position = positionNew;
                    cylinder.transform.localScale = new Vector3(0.7f, 1f, 0.08f);
                    cylinder.transform.LookAt(position );
                    cylinder.transform.eulerAngles = new Vector3(0, cylinder.transform.eulerAngles.y + 90, cylinder.transform.eulerAngles.z); 
                }
                
                wasDraw = true;
            }
            else
            {
                if (!wasDraw)
                {
                    SetBorderOfTerritoryRecursion(0, find, spent, lineRenderer, playerEconomic, wasDraw);
                }
                else
                {
                    SetBorderOfTerritoryRecursion(neighborKV.Value, find, spent, lineRenderer, playerEconomic, wasDraw);
                }

                return;
            }

            if (side >= 5)
            {
                side = 0;
            }
            else
            {
                side++;
            }
        }
    }
    public void SetBorderOfTerritory(int pid, GameObject context, Color color, float width)
    { 
        if (territories.ContainsKey(pid))
        {  
            foreach (PlayerEconomic playerEconomic in territories[pid])
            {
                if(playerEconomic.cells.Count == 0)
                {
                    CalculateMainHousesAndDeleteNullTerritory();
                    continue;
                }
                  
                GameObject container = new GameObject();
                container.name = "Territory line";
                container.transform.SetParent(context.transform, true); 
                
                var line = container.AddComponent<LineRenderer>();
                line.loop = true;
                line.positionCount = 0;
                line.receiveShadows = false; 

                line.startWidth = 0;
                line.endWidth = 0; 

                SetBorderOfTerritoryRecursion(0, playerEconomic.cells[0], new List<string>(), line, playerEconomic);

                container.AddComponent<MeshCombiner>();

                var renderer = container.GetComponent<MeshRenderer>();
                renderer.material = new Material(Globals.unitsPrefubsCombinator.territoryOutlineMaterial);
                renderer.material.SetColor("_Color1", color);

                if (playerEconomic == Globals.gameSystem.selectedEconomic)
                {
                    renderer.material.SetColor("_Color2", new Color(0, 0, 0, 0));
                }
                else
                {
                    renderer.material.SetColor("_Color2", color);
                }

                Destroy(line);
            }
        } 
    }
    #endregion

    #region methods 
    public void ClearCells()
    {
        GameObject ground = GameObject.Find("Ground");
        foreach (Transform child in ground.transform)
        {
            var groundChild = child.GetComponent<GroundController>();

            if(groundChild != null)
            {
                if (groundChild.unitOnCell != null)
                {
                    groundChild.unitOnCell = null;
                    groundChild.selectionObj = null;
                    groundChild.selectionStepObj = null;
                }
            }

            for (var i = 0; i < child.childCount; i++)
            {
                GameObject.DestroyImmediate(child.GetChild(i).gameObject);
                i--;
            }
        }
    }
    public void SetSelectedStepCellsByPosition(List<Vector2Int> selectedCells = null)
    {
        foreach(KeyValuePair<Vector2Int, GroundController> cellKv in cells)
        {
            if(selectedCells != null && selectedCells.Contains(cellKv.Key))
            {
                cellKv.Value.selectionStepObj.SetActive(true);
            }
            else
            {
                cellKv.Value.selectionStepObj.SetActive(false);
            }
        }
    }
    public Dictionary<Vector2Int, int> GetNeighborOfHexagon(Vector2Int hexagonPosition)
    {
        Dictionary<Vector2Int, int> outNeighbous = new Dictionary<Vector2Int, int>();

        int a = 0;

        while (a <= 5)
        {
            Vector2Int neighbour;
            int neighbourIterator;

            switch (a)
            {
                case 0:
                    neighbourIterator = 4;
                    if (hexagonPosition.y % 2 == 0)
                    {
                        neighbour = new Vector2Int(hexagonPosition.x, hexagonPosition.y + 1);
                    }
                    else
                    {
                        neighbour = new Vector2Int(hexagonPosition.x + 1, hexagonPosition.y + 1);
                    }
                    break;
                case 1:
                    neighbourIterator = 5;
                    neighbour = new Vector2Int(hexagonPosition.x + 1, hexagonPosition.y);
                    break;
                case 2:
                    neighbourIterator = 0;
                    if (hexagonPosition.y % 2 == 0)
                    {
                        neighbour = new Vector2Int(hexagonPosition.x, hexagonPosition.y - 1);
                    }
                    else
                    {
                        neighbour = new Vector2Int(hexagonPosition.x + 1, hexagonPosition.y - 1);
                    }
                    break;
                case 3:
                    neighbourIterator = 1;
                    if (hexagonPosition.y % 2 == 0)
                    {
                        neighbour = new Vector2Int(hexagonPosition.x - 1, hexagonPosition.y - 1);
                    }
                    else
                    {
                        neighbour = new Vector2Int(hexagonPosition.x, hexagonPosition.y - 1);
                    }
                    break;
                case 4:
                    neighbourIterator = 2;
                    neighbour = new Vector2Int(hexagonPosition.x - 1, hexagonPosition.y);
                    break;
                case 5:
                    neighbourIterator = 3;
                    if (hexagonPosition.y % 2 == 0)
                    {
                        neighbour = new Vector2Int(hexagonPosition.x - 1, hexagonPosition.y + 1);
                    }
                    else
                    {
                        neighbour = new Vector2Int(hexagonPosition.x, hexagonPosition.y + 1);
                    }
                    break;
                default:
                    neighbourIterator = 4;
                    if (hexagonPosition.y % 2 == 0)
                    {
                        neighbour = new Vector2Int(hexagonPosition.x, hexagonPosition.y + 1);
                    }
                    else
                    {
                        neighbour = new Vector2Int(hexagonPosition.x + 1, hexagonPosition.y + 1);
                    }
                    break;
            }

            outNeighbous.Add(neighbour, neighbourIterator);
            a++;
        }

        return outNeighbous;
    }
    public GroundController FindMainHouseCellOfPlayer(int pid)
    {
        if (territories.ContainsKey(pid))
        {
            foreach (PlayerEconomic playerEconomic in territories[pid])
            {  
                foreach (GroundController cell in playerEconomic.cells)
                {
                    if (cell.unitOnCell != null && cell.unitOnCell.createdPrefub != null && cell.unitOnCell.GetType().Name == "UnitMainHouse")
                    {
                        return cell; 
                    }
                } 
            }
        }

        return null;
    } 
    public int FindPlayerEconomicByCell(GroundController ground, ref PlayerEconomic playerEconomic)
    {
        foreach (int pid in territories.Keys)
        {
            for (int i = 0; i < territories[pid].Count; i++)
            {
                PlayerEconomic _playerEconomic = territories[pid][i]; 

                foreach (GroundController cell in _playerEconomic.cells)
                {
                    if (cell == ground)
                    {
                        playerEconomic = _playerEconomic;
                        return i;
                    }
                }
            }
        }

        return -1;
    }
    public int FindIdByPlayerEconomic(int pid, PlayerEconomic playerEconomic)
    {
        for (int i = 0; i < territories[pid].Count; i++)
        {
            PlayerEconomic _playerEconomic = territories[pid][i];

            if (playerEconomic == _playerEconomic)
            {
                return i;
            }
        }

        return -1;
    }
    public int FindPlayerIdByPlayerEconomic(int pid, PlayerEconomic playerEconomic)
    {
        for (int i = 0; i < territories[pid].Count; i++)
        {
            PlayerEconomic _playerEconomic = territories[pid][i];

            if (playerEconomic == _playerEconomic)
            {
                return pid;
            }
        }

        return -1;
    }
    public PlayerEconomic FindPlayerEconomicById(int pid, int playerEconomicId)
    {
        return territories[pid][playerEconomicId];
    }
    public void CalculateMainHousesAndDeleteNullTerritory()
    {
        foreach (int key in territories.Keys)
        {
            List<PlayerEconomic> value = territories[key];

            foreach (PlayerEconomic playerEconomic in value)
            {
                if (playerEconomic.cells.Count == 0)
                {
                    value.Remove(playerEconomic);
                    continue;
                }

                var wasMainHouse = false;

                foreach (GroundController cell in playerEconomic.cells)
                {
                    if (cell.unitOnCell != null && cell.unitOnCell.createdPrefub != null && cell.unitOnCell.GetType().Name == "UnitMainHouse")
                    {
                        wasMainHouse = true;
                        break;
                    }
                }

                if (!wasMainHouse)
                {
                    foreach (GroundController cell in playerEconomic.cells)
                    {
                        if (cell.unitOnCell == null)
                        {
                            cell.unitOnCell = new UnitMainHouse();
                            cell.unitOnCell.CreatePrefab(cell.gameObject.transform);
                            break;
                        }
                    }
                }
            }

            if (value.Count == 0)
            {
                territories.Remove(key);
            }
        }
    }
    public void CalculateNewTrees(bool initialSpawn)
    {
        foreach (var cell in cells.Values)
        {
            if (cell.playerId == -1)
            {
                if (cell.unitOnCell == null && (initialSpawn && Random.Range(0, Globals.maxRandomChanceInitialSpawnTree) == 0 || !initialSpawn && Random.Range(0, Globals.maxRandomChanceSpawnTree) == 0))
                {
                    cell.unitOnCell = new UnitTree();
                    cell.unitOnCell.CreatePrefab(cell.gameObject.transform);
                }
            }
            else
            {
                if (cell.unitOnCell == null && Random.Range(0, Globals.maxRandomChanceSpawnTreeOnPlayerTerritory) == 0)
                {
                    cell.unitOnCell = new UnitTree();
                    cell.unitOnCell.CreatePrefab(cell.gameObject.transform);
                }
            }
        }
    }
    #endregion

    #region start 
    private void GetMapCenter()
    {
        _centerOfMap = Vector3.zero;

        foreach (var cell in cells.Values)
        {
            _centerOfMap += cell.transform.position;
        }

        _centerOfMap /= cells.Values.Count;
    }
    private void GetRecursionTerritory(List<GroundController> territory, int pid, GroundController cell, Vector2Int hexagonPosition, Dictionary<Vector2Int, GroundController> cells, List<Vector2Int> spent)
    {
        if (spent.Contains(hexagonPosition))
        {
            return;
        }

        if (cell.playerId == pid)
        {
            territory.Add(cell);
            spent.Add(hexagonPosition);
        }
        else
        {
            return;
        }

        List<Vector2Int> checks = GetNeighborOfHexagon(hexagonPosition).Keys.ToList<Vector2Int>();

        foreach (Vector2Int check in checks)
        {
            GroundController cell_check;

            if (cells.TryGetValue(check, out cell_check))
            {
                GetRecursionTerritory(territory, pid, cell_check, cell_check.hexagonPosition, cells, spent);
            }
        }
    }
    private void CalculatePlayerTerritory(List<GroundController> territory, int pid, Dictionary<int, List<PlayerEconomic>> territories)
    {
        if (territories.ContainsKey(pid))
        {
            List<PlayerEconomic> playerEconomics = territories[pid];
            playerEconomics[0].coffers /= 2;
            playerEconomics.Add(new PlayerEconomic(playerEconomics[0].coffers, territory, pid));
        }
        else
        {
            List<PlayerEconomic> playerEconomics = new List<PlayerEconomic>();
            territories.Add(pid, playerEconomics);
            playerEconomics.Add(new PlayerEconomic(Globals.defaultCoffers, territory, pid));
        } 
    }
    public void CalculateInitialTerritories(out Dictionary<int, List<PlayerEconomic>> territories)
    {
        territories = new Dictionary<int, List<PlayerEconomic>>();
        var spent = new List<Vector2Int>();

        foreach (Vector2Int hexagonPosition in cells.Keys)
        {
            GroundController cell = cells[hexagonPosition];

            if (cell.playerId == -1)
            {
                continue;
            }
            if (spent.IndexOf(hexagonPosition) != -1)
            {
                continue;
            }

            var territory = new List<GroundController>();
            GetRecursionTerritory(territory, cell.playerId, cell, hexagonPosition, cells, spent);
            CalculatePlayerTerritory(territory, cell.playerId, territories);
        }

        if (!territories.ContainsKey(Globals.defaultLocalPlayerId))
        {
            Debug.LogError("Where is local player territory???");
        }
    }
    private void CalculateCells()
    {
        GameObject ground = GameObject.Find("Ground");
        foreach (Transform child in ground.transform)
        {
            GroundController cell = child.GetComponent<GroundController>();
            cells.Add(cell.hexagonPosition, cell);

            GameObject outline = Instantiate(Globals.unitsPrefubsCombinator.selectCell, child);
            outline.name = "Select outline";
            outline.SetActive(false);
            cell.selectionObj = outline;

            GameObject outlineMove = Instantiate(Globals.unitsPrefubsCombinator.selectCellStep, child);
            outlineMove.name = "Select outline step";
            outlineMove.SetActive(false);
            cell.selectionStepObj = outlineMove;
        }
    }
    private void CalculatePlayers()
    { 
        foreach (int pid in territories.Keys)
        {
            var player = Globals.playerSystem.AddPlayer(pid).UpdateDivisionOfTerritories();

            if (pid == Globals.defaultLocalPlayerId)
            {
                Globals.gameSystem.selectedEconomic = territories[Globals.defaultLocalPlayerId][0];
                player.SetBorderSelected(0);
            }
        }
    }
    #endregion

    private void Awake()
    {
        Globals.territoryHandler = this;
         
        cells = new Dictionary<Vector2Int, GroundController>(); 
    }
    private void Start()
    {
        CalculateCells();
        CalculateInitialTerritories(out territories);
        CalculateMainHousesAndDeleteNullTerritory();
        CalculateNewTrees(true);
        CalculatePlayers();
        GetMapCenter();
         
        Globals.userInterface.UpdateUI();
    }
}
