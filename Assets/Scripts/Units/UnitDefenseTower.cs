using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UnitDefenseTower : Unit, IUnitOnGround
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

    public UnitDefenseTower()
    {
        _stepsLeftPerMove = 0;
        _stepsPerMoveMax = Globals.unitCountMaxStepsPerMove[unitType];
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
        var obj = UnityEngine.Object.Instantiate(Globals.unitsPrefubsCombinator.defenseTower, parent);

        obj.name = "Unit UnitDefenseTower";
        obj.layer = 2;

        obj.transform.LookAt(Globals.territoryHandler.centerOfMap);

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
