using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class GroundController : MonoBehaviour
{
    public int playerId = -1;
    public IUnitOnGround unitOnCell = null;

    public GameObject selectionObj;
    public GameObject selectionStepObj;

    private Vector2Int _hexagonPosition;
    public Vector2Int hexagonPosition { 
        get 
        { 
            return _hexagonPosition; 
        } 
    }  

    void Awake()
    {
        CalculatePlayerColor();
        CalculatePosition();
    }

    public void CalculatePlayerColor()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (playerId == -1)
        {
            meshRenderer.material.SetColor("_Color", Globals.defaultNeutralColor);
        }
        else
        {
            meshRenderer.material.SetColor("_Color", Globals.GetColorForPlayer(playerId));
        }
    }
    private void CalculatePosition()
    {
        var position = this.name.Split(' ')[1].Split(',');
        _hexagonPosition = new Vector2Int(int.Parse(position[0]), int.Parse(position[1]));
    }
    public void SetSelection(bool state)
    {
        if (selectionObj != null)
        {
            if (state)
            {
                selectionObj.SetActive(true);
            }
            else
            {
                selectionObj.SetActive(false);
            }
        }
    }
    public void SetSelectionMove(bool state)
    {
        if (selectionStepObj != null)
        {
            if (state)
            {
                selectionStepObj.SetActive(true); 
            }
            else
            {
                selectionStepObj.SetActive(false);
            }
        }
    }
}
