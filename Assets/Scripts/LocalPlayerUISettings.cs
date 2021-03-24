using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUISettings : MonoBehaviour
{
    public TMPro.TextMeshProUGUI defenseTowerGoldText;
    [Space]
    public TMPro.TextMeshProUGUI farmerGoldText;
    [Space]
    public TMPro.TextMeshProUGUI farmGoldText;
    [Space] 
    public TMPro.TextMeshProUGUI playerGoldLabel;
    [Space]
    public GameObject buildPanel;
    [Space]
    public GameObject skipStep;

    private void Awake()
    {
        Globals.localPlayerUISettings = this; 
    }
    void Start()
    {
        
    } 
    void Update()
    {
        
    }
}
