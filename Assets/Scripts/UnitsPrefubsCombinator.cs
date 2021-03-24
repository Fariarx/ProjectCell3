using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsPrefubsCombinator : MonoBehaviour
{
    [Space(25)]
    public Material territoryOutlineMaterial;
    public GameObject territoryOutline;
    [Space(25)]
    public GameObject unitUpgradeEffect;
    public GameObject unitDieEffect;
    [Space(25)]
    public GameObject mainHouse;  
    public GameObject farm;  
    public GameObject spearman;
    public GameObject farmer;
    public GameObject farmerUpgraded;
    public GameObject knight;
    public GameObject defenseTower;
    [Space(25)]
    public List<GameObject> treeVariants = new List<GameObject>();  
    [Space(25)]
    public GameObject selectCell; 
    public GameObject selectCellStep; 

    private void Awake()
    {
        Globals.unitsPrefubsCombinator = this;
    }
}
