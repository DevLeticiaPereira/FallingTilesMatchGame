using UnityEngine;

public class GridSetup : MonoBehaviour
{
    [SerializeField] private GridSetupData _gridSetupData;

    private void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Clear the current grid if it exists
        ClearGrid();

        Vector2 initialPosition = transform.position;
        // Loop through rows and columns to create the grid
        for (var row = 0; row < _gridSetupData.Rows; row++)
        for (var col = 0; col < _gridSetupData.Columns; col++)
        {
            // Calculate the position of the current block
            var x = initialPosition.x + _gridSetupData.BlockDimensions.x * col * transform.parent.localScale.x +
                    _gridSetupData.BlockSpaceBetween.x * col;
            var y = initialPosition.y + _gridSetupData.BlockDimensions.y * row * transform.parent.localScale.y +
                    _gridSetupData.BlockSpaceBetween.y * row;
            var position = new Vector3(x, y, 0);

            // Instantiate the block prefab at the calculated position
            var prefabToInstantiate = _gridSetupData.BlockBackgroundPrefabType1;
            if ((row % 2 == 0 && col % 2 != 0) || (row % 2 != 0 && col % 2 == 0))
                prefabToInstantiate = _gridSetupData.BlockBackgroundPrefabType2;
            var spawnedBackgroundBlock = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
            spawnedBackgroundBlock.name = $"TileBackground {row} {col}";
        }
    }

    private void ClearGrid()
    {
        var allChildren = GetComponentsInChildren<Transform>();

        for (var i = 1; i < allChildren.Length; i++) DestroyImmediate(allChildren[i].gameObject);
    }
}