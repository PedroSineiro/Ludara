using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 20;
    public int height = 12;
    public float tileSize = 35f;

    public List<Vector2Int> spawns;
    public Dictionary<Vector2,Tile> tiles;
    public Vector2 startPosition = new Vector2(-380f, 140f);

    public GameObject inspector;

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, transform);
                RectTransform rect = tile.GetComponent<RectTransform>();

                float posX = startPosition.x + (x * tileSize);
                float posY = startPosition.y - (y * tileSize);

                rect.anchoredPosition = new Vector2(posX, posY);
                tile.name = $"Tile_{x}_{y}";

                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.inspector = inspector;

                Vector2Int pos = new Vector2Int(x, y);
                if (tileScript != null)
                {
                    tileScript.gridPosition = pos;
                    tileScript.spawnable = spawns.Contains(pos);
                }

                tiles[new Vector2(x, y)] = tileScript;
            }
        }
    }

}
