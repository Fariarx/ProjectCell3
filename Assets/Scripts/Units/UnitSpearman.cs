using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UnitSpearman : Unit, IUnitOnGround, ISteppingUnit
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

    public UnitSpearman()
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
        var obj = UnityEngine.Object.Instantiate(Globals.unitsPrefubsCombinator.spearman, parent);

        obj.name = "Unit UnitSpearman";
        obj.layer = 2;

        obj.transform.LookAt(Globals.territoryHandler.centerOfMap);

        obj.GetComponent<PlayableDirector>().enabled = true;
        obj.GetComponent<PlayableDirector>().Stop();

        _createdPrefub = obj;

        if (_stepsLeftPerMove < _stepsPerMoveMax)
        {
            StepAnimation(true, 1);
        }
    }
    public void StepAnimation(bool work, float speed)
    {
        var playableDirector = _createdPrefub.GetComponent<PlayableDirector>();

        if (work)
        {
            playableDirector.RebuildGraph(); // the graph must be created before getting the playable graph
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
            playableDirector.Play();
        }
        else
        {
            playableDirector.Stop();
        }
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

