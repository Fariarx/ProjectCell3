using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

public class MapGroundEditor : EditorWindow
{
    #region Attributes
    // A list containing the available prefabs.
    [SerializeField]
    private List<GameObject> palette = new List<GameObject>();

    [SerializeField]
    private int paletteIndex;

    GameObject groundSimple;

    private string pathToHex = "Assets/Resources/Graphics/prefabs/hex_tile.prefab";
    
    public Vector2 cellSize = new Vector2(1f, 1f);

    private bool paintMode = false;
    #endregion

    #region UI
    // The window is selected if it already exists, else it's created.
    [MenuItem("Window/Map Ground Editor")]
    private static void ShowWindow()
    {
        GetWindow(typeof(MapGroundEditor));
    }

    void OnFocus()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI; // Don't add twice
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;

        RefreshPalette(); // Refresh the palette (can be called uselessly, but there is no overhead.)
    }
     
    bool selection = false;
    int playerID = -1;

    // Called to draw the MapEditor windows.
    private void OnGUI()
    {
        paintMode = GUILayout.Toggle(paintMode, "Start painting", "Button", GUILayout.Height(40f));
        //hexRadius = EditorGUILayout.FloatField("Hexagon radius:", hexRadius);
        //hexRotation = EditorGUILayout.FloatField("Hexagon rotation:", hexRotation);
        selection = EditorGUILayout.Toggle("Select created", selection);
        playerID = EditorGUILayout.IntSlider("Player ID", playerID, -1, Globals.playerCountMax);
       // groundSimple = EditorGUILayout.ObjectField("Ground simple prefab", groundSimple, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        // Get a list of previews, one for each of our prefabs
        List<GUIContent> paletteIcons = new List<GUIContent>();
        foreach (GameObject prefab in palette)
        {
            // Get a preview for the prefab
            Texture2D texture = AssetPreview.GetAssetPreview(prefab);
            paletteIcons.Add(new GUIContent(texture));
        }

        // Display the grid
        paletteIndex = GUILayout.SelectionGrid(paletteIndex, paletteIcons.ToArray(), 6);
    }

    // Does the rendering of the map editor in the scene view.
    private void OnSceneGUI(SceneView sceneView)
    {
        if (paintMode)
        { 
             
            HandleSceneViewInputs();

            // Refresh the view
            sceneView.Repaint();
        }
    }
     
    #endregion

    #region MapEdition
    private void HandleSceneViewInputs()
    { 
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0); 
        }
         
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            //Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green, 10);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.y / ray.direction.y); 
             
            //GridHelper.SetRadius(hexRadius); 

            GameObject ground = GameObject.Find("Ground");
            Vector2Int hexPosition = GridHelper.WorldToHexagon(worldPoint);

            for (int i = 0; i < ground.transform.childCount; i++)
            {
                Transform child = ground.transform.GetChild(i);

                string[] temp = child.name.Split(' ')[1].Split(',');

                if (System.Convert.ToInt32(temp[0]) == hexPosition[0] && System.Convert.ToInt32(temp[1]) == hexPosition[1])
                {
                    Object.DestroyImmediate(child.gameObject);
                    i--;
                }
            } 

            GameObject prefab = AssetDatabase.LoadAssetAtPath(pathToHex, typeof(GameObject)) as GameObject;
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab, ground.transform) as GameObject;
            gameObject.transform.position = GridHelper.HexagonToWorld(hexPosition);
            gameObject.transform.Rotate(0, GridHelper.defaultHexagonRotation, 0, Space.World);
            gameObject.name = "Ground " + Vector2Int.FloorToInt(hexPosition).x + "," + Vector2Int.FloorToInt(hexPosition).y;
            
            GroundController groundController = gameObject.AddComponent<GroundController>();
            if (playerID != -1)
            {
                groundController.playerId = playerID;
            }
            else
            {
                groundController.playerId = -1;
            }
            groundController.CalculatePlayerColor();

            Undo.RegisterCreatedObjectUndo(gameObject, "");

            Selection.activeGameObject = null;
            
            if (selection)
            {
                Selection.activeGameObject = gameObject;
            }
        }
    }
    #endregion

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    private void RefreshPalette()
    {
        palette.Clear();

        /*System.IO.Directory.CreateDirectory(path);
        string[] prefabFiles = System.IO.Directory.GetFiles(path, "*.prefab");
        foreach (string prefabFile in prefabFiles)
        {
            palette.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
        }*/
    }
}
