using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitOnGround
{
    Globals.UnitType unitType { get; }

    float stepsPerMoveMax { get; }
    float stepsLeftPerMove { get; set; }
    float lenghtOfStep { get; }

    float farmModifier { get; }
    
    GameObject createdPrefub { get; }
    void CreatePrefab(Transform parent);
    void DeletePrefab();
    void DeletePrefabWithAnimation();
}
