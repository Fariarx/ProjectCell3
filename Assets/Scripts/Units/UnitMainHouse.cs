using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMainHouse : Unit, IUnitOnGround
{
    private float _stepsPerMoveMax;
    private float _stepsLeftPerMove;
    public Globals.UnitType unitType
    {
        get
        {
            System.Enum.TryParse(this.GetType().Name, out Globals.UnitType _unitType);
            return _unitType;
        }
    }
    public float stepsPerMoveMax
    {
        get
        {
            return _stepsPerMoveMax;
        }
    }
    public float stepsLeftPerMove
    {
        get
        {
            return _stepsLeftPerMove;
        }
        set
        {
            _stepsLeftPerMove = value;
        }
    }
    public float lenghtOfStep
    {
        get
        { 
            return Globals.unitLenghOfStep[unitType];
        }
    }
    public float farmModifier
    {
        get
        { 
            return Globals.unitFarmModifier[unitType];
        }
    }

    public UnitMainHouse()
    {
        _stepsLeftPerMove = 0;
        _stepsPerMoveMax = 1;
    }

    private GameObject _createdPrefub = null;
    public GameObject createdPrefub
    {
        get
        {
            return _createdPrefub;
        } 
    }
    public void CreatePrefab(Transform parent)
    {
        var obj =  Object.Instantiate(Globals.unitsPrefubsCombinator.mainHouse, parent); 
        obj.name = "Unit UnitMainHouse";
        obj.layer = 2;
        _createdPrefub = obj;
    }
    public void DeletePrefab()
    {
        Object.Destroy(_createdPrefub);
    }
    public void DeletePrefabWithAnimation()
    {
        _createdPrefub.AddComponent<UnitDeleteAnimation>();
    }
}
