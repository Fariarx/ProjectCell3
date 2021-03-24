using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocalPlayer : Player
{   
    public LocalPlayer(int id, GameObject context) : base(id, context)
    {  

    }

    private bool _skipMove = false;

    public bool SkipMove()
    {
        if (_skipMove || Globals.gameSystem.moveOfPlayerID != id)
        {
            return false;
        }
        else
        {
            _skipMove = true;

            return true;
        }
    }

    public override IEnumerator Move(int playerID, MonoBehaviour myMonoBehaviour)
    {
        StartMove();

        _skipMove = false;

        while (true)
        { 
            if(_skipMove)
            {
                _skipMove = false; 
                SetBorderUnselectedAll();

                if (Globals.gameSystem.selectedCell != null)
                {
                    Globals.gameSystem.selectedCell.SetSelection(false);
                    Globals.gameSystem.selectedCell = null;
                }

                Globals.territoryHandler.SetSelectedStepCellsByPosition(); 
                break;
            }

            yield return new WaitForSeconds(0.1f); 
        }

        EndMove();
    }
}
