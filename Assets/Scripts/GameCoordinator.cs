using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public GameObject gameScripts;
    private GameObject gameScriptsObj;

    void Start()
    {
        Globals.gameCoordinator = this;  
        gameScriptsObj = Instantiate(gameScripts); 
    } 

    public void Restart()
    { 
        Globals.territoryHandler.ClearCells();
        DestroyImmediate(gameScriptsObj);

        Globals.lockedCellsForUnitMove = new List<Vector2Int>();

        Start();
    }
}
