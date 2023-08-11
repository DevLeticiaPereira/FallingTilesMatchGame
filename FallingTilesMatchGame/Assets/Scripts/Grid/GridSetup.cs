using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSetup : MonoBehaviour
{
    [SerializeField] private GridSetupData _gridSetupData;

    public void GenerateGrid()
    {
        // Clear the current grid if it exists
        ClearGrid();

        Vector2 initialPosition = this.transform.position;
        // Loop through rows and columns to create the grid
        for (int row = 0; row < _gridSetupData.Rows; row++)
        {
            for (int col = 0; col < _gridSetupData.Columns; col++)
            {
                // Calculate the position of the current block
                float x = initialPosition.x + _gridSetupData.BlockDimensions.x * col + _gridSetupData.BlockSpaceBetween.x * col;
                float y = initialPosition.y + _gridSetupData.BlockDimensions.y * row +_gridSetupData.BlockSpaceBetween.y * row;
                Vector3 position = new Vector3( x, y, 0);

                // Instantiate the block prefab at the calculated position
                var prefabToInstantiate = _gridSetupData.BlockBackgroundPrefabType1;
                if (( row% 2 == 0 && col % 2 != 0) || (row % 2 != 0 && col % 2 == 0))
                {
                    prefabToInstantiate = _gridSetupData.BlockBackgroundPrefabType2;
                }
                GameObject spawnedBackgroundBlock = Instantiate(prefabToInstantiate, position, Quaternion.identity, this.transform);
                spawnedBackgroundBlock.name = $"TileBackground {row} {col}";
            }
        }
    }
    private void ClearGrid()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        for (int i = 1; i < allChildren.Length; i++)
        {
            DestroyImmediate(allChildren[i].gameObject);
        }
    }
}

