using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFarm : Unit, IUnitOnGround
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

    public UnitFarm()
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
        var obj = UnityEngine.Object.Instantiate(Globals.unitsPrefubsCombinator.farm, parent); 

        var rotation = obj.transform.eulerAngles;
        rotation[1] = UnityEngine.Random.Range(20, 50);
        obj.transform.eulerAngles = rotation;

        obj.name = "Unit UnitFarm";
        obj.layer = 2;
        _createdPrefub = obj;
    }
    public void DeletePrefab()
    {
        UnityEngine.Object.Destroy(_createdPrefub);
    }
    public void DeletePrefabWithAnimation()
    {
        _createdPrefub.AddComponent<UnitDeleteAnimation>();
    }
}
