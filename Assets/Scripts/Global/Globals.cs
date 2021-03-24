using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static int maxRandomChanceSpawnTree = 12;
    public static int maxRandomChanceSpawnTreeOnPlayerTerritory = 24;
    public static int maxRandomChanceInitialSpawnTree = 2;
    public static int defaultLocalPlayerId = 0;
    public static float defaultCoffers = 15f;
    public static float lookAtLocalPlayerTime = 0.65f;

    public static Vector3 lineOffsetZ = new Vector3(0, 0.04f, 0);
    public static float lineDivisionOfTerritoryWidth = 0.3f;
    public static float lineDivisionOfTerritoryTrasponent = 0.65f;  

    public static bool isUnitMove = false; 

    public enum UnitType
    {
        UnitTree,
        UnitFarmer,
        UnitFarmerUpgraded,
        UnitFarm,
        UnitSpearman,
        UnitMainHouse,
        UnitDefenseTower,
        UnitKnight,
    }

    public static float initialTheCostOfFarm = 4f;
    public static float initialTheCostOfFarmProgression = 6f;
    public static Dictionary<UnitType, float> unitCosts = new Dictionary<UnitType, float> {
        { UnitType.UnitFarmer, 10 },
        { UnitType.UnitDefenseTower, 30 },
    };

    public static float cellFarmModifier = 1f;

    public static Dictionary<UnitType, float> unitFarmModifier = new Dictionary<UnitType, float> {
        { UnitType.UnitTree, -4},
        { UnitType.UnitFarm, 4},
        { UnitType.UnitFarmer, -51},
        { UnitType.UnitFarmerUpgraded, -8},
        { UnitType.UnitSpearman, -10},
        { UnitType.UnitMainHouse, 2},
        { UnitType.UnitDefenseTower, -6 },
        { UnitType.UnitKnight, -13},
    };
    public static Dictionary<UnitType, int> unitLenghOfStep = new Dictionary<UnitType, int> {
        { UnitType.UnitTree, 0},
        { UnitType.UnitFarm, 0},
        { UnitType.UnitFarmer, 1},
        { UnitType.UnitFarmerUpgraded, 1},
        { UnitType.UnitSpearman, 2},
        { UnitType.UnitMainHouse, 0},
        { UnitType.UnitDefenseTower, 0 },
        { UnitType.UnitKnight, 2},
    };
    public static Dictionary<UnitType, int> unitCountMaxStepsPerMove = new Dictionary<UnitType, int> {
        { UnitType.UnitTree, 0},
        { UnitType.UnitFarm, 0},
        { UnitType.UnitFarmer, 1},
        { UnitType.UnitFarmerUpgraded, 2},
        { UnitType.UnitSpearman, 1},
        { UnitType.UnitMainHouse, 0},
        { UnitType.UnitDefenseTower, 0 },
        { UnitType.UnitKnight, 1},
    };
    public static List<UnitType> unitCanDieWithEconomic = new List<UnitType> { 
        UnitType.UnitFarmer,
        UnitType.UnitFarmerUpgraded,
        UnitType.UnitSpearman, 
        UnitType.UnitKnight, 
    };

    public static Color defaultNeutralColor = new Color(243f / 255f, 255f / 255f, 55f / 155f, 1f);

    private static List<Color> playersColor = new List<Color>()
    {
        Color.cyan,
        Color.blue, 
        Color.white,
        Color.magenta,
        Color.black,
        new Color(123f/255f, 79f/255f, 55f/255f)
    };

    public static float animationMoveSpeedForLocalPlayer = 7f;
    public static float animationMoveSpeedForOtherPlayer = 12f;
    public static float animationSpeedForLocalPlayer = 1.5f;
    public static float animationSpeedForOtherPlayer = 15f;

    public static LocalPlayerUISettings localPlayerUISettings = null;
    public static UnitsPrefubsCombinator unitsPrefubsCombinator = null;
    public static TerritoryHandler territoryHandler = null;
    public static GameCoordinator gameCoordinator = null;
    public static InputController inputController = null;
    public static StepCalculator stepCalculator = null;
    public static UserInterface userInterface = null;
    public static PlayersSystem playerSystem = null;
    public static GameSystem gameSystem = null;
    public static Utils utils = new Utils();

    public static List<Vector2Int> lockedCellsForUnitMove = new List<Vector2Int>();

    public static int playerCountMax { get { return playersColor.Count - 1; } }
    public static Color GetColorForPlayer(int playerId)
    {
        if(playerId < 0 || playerId > playersColor.Count - 1)
        {
            throw new Exception("Player not valid");
        }
        
        Color color = playersColor[(int)playerId];   

        return new Color(color.r, color.g, color.b, color.a);
    }
}
