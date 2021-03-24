using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridHelper
{ 
    public const float hexagonSize = 4f;
    public const float defaultHexagonRotation = 30f;

    #region parametrs
    public static float radius = hexagonSize;
    public static float innerRadius = radius * 0.866025404f;
    public static float height = 2 * radius;
    public static float rowHeight = 1.5f * radius;
    public static float halfWidth = Mathf.Sqrt((radius * radius) - ((radius / 2) * (radius / 2)));
    public static float width = 2 * halfWidth;
    public static float extraHeight = height - rowHeight;
    public static float edge = rowHeight - extraHeight;
    #endregion

    public static Vector3[] corners = {
            new Vector3(0f, 0f, GridHelper.radius),
            new Vector3(GridHelper.innerRadius, 0f, 0.5f * GridHelper.radius),
            new Vector3(GridHelper.innerRadius, 0f, -0.5f * GridHelper.radius),
            new Vector3(0f, 0f, -GridHelper.radius),
            new Vector3(-GridHelper.innerRadius, 0f, -0.5f * GridHelper.radius),
            new Vector3(-GridHelper.innerRadius, 0f, 0.5f * GridHelper.radius),
    };

    #region support  
    public static Vector2Int WorldToHexagon(Vector3 position)
    {

        var px = position.x + halfWidth;
        var py = position.z + radius;

        int gridX = (int)Mathf.Floor(px / width);
        int gridY = (int)Mathf.Floor(py / rowHeight);

        float gridModX = Mathf.Abs(px % width);
        float gridModY = Mathf.Abs(py % rowHeight);

        bool gridTypeA = (gridY % 2) == 0;

        var resultY = gridY;
        var resultX = gridX;
        var m = extraHeight / halfWidth; 

        if (gridTypeA)
        {
            // middle
            resultY = gridY;
            resultX = gridX;
            // left
            if (gridModY < (extraHeight - gridModX * m))
            {
                resultY = gridY - 1;
                resultX = gridX - 1;
            }
            // right
            else if (gridModY < (-extraHeight + gridModX * m))
            {
                resultY = gridY - 1;
                resultX = gridX;
            }
        }
        else
        {
            if (gridModX >= halfWidth)
            {
                if (gridModY < (2 * extraHeight - gridModX * m))
                { 
                    // Top
                    resultY = gridY - 1;
                    resultX = gridX;
                }
                else
                {
                    if(gridX < 0)
                    {
                        resultY = gridY;
                        resultX = gridX - 1;
                    }
                    else
                    {
                        // Right
                        resultY = gridY;
                        resultX = gridX;
                    }
                }
            }

            if (gridModX < halfWidth)
            {
                if (gridModY < (gridModX * m))
                { 
                    // Top
                    resultY = gridY - 1;
                    resultX = gridX;
                }
                else
                { 
                    if (gridX < 0)
                    { 
                        resultY = gridY;
                        resultX = gridX;
                    }
                    else
                    {
                        // Left
                        resultY = gridY;
                        resultX = gridX - 1;
                    }
                }
            }
        }

        return new Vector2Int(resultX, resultY);
    } 
    public static Vector3 HexagonToWorld(Vector2Int hexCell)
    {  
        return new Vector3((hexCell.x * width) + ((hexCell.y & 1) * width / 2), 0, (float)(hexCell.y * 1.5 * radius));
    }
    #endregion
}
