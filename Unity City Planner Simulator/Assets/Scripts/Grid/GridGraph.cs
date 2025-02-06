using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GridGraph : MonoBehaviour
{
    public Tilemap roadTilemap;
    public Grid grid;


    public Dictionary<Vector3Int, List<GridNode>> nodes = new Dictionary<Vector3Int, List<GridNode>>();

    private void Awake()
    {
        BuildGraph();
    }

    void BuildGraph()
    {
        BoundsInt bounds = roadTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tileBase = roadTilemap.GetTile(cellPosition);

                if (tileBase != null)
                {
                    Vector3 cellCenter = grid.GetCellCenterWorld(cellPosition);
                    List<GridNode> laneNodes = new List<GridNode>();


                    if (tileBase.name == "tilemap_296" || tileBase.name == "tilemap_294")
                    {
                        Vector3 offset0 = new Vector3(-0.4f, 0, 0);
                        laneNodes.Add(new GridNode(cellCenter, new Vector2Int(x, y), true, 0));

                        Vector3 offset1 = new Vector3(0.4f, 0, 0);
                        laneNodes.Add(new GridNode(cellCenter, new Vector2Int(x, y), true, 1));

                        nodes[cellPosition] = laneNodes;
                    }

                    if (tileBase.name == "tilemap_271" || tileBase.name == "tilemap_319")
                    {
                        Vector3 offset0 = new Vector3(0, 0.4f, 0);
                        laneNodes.Add(new GridNode(cellCenter, new Vector2Int(x, y), true, 1));

                        Vector3 offset1 = new Vector3(0, 0.4f, 0);
                        laneNodes.Add(new GridNode(cellCenter, new Vector2Int(x, y), true, 0));

                        nodes[cellPosition] = laneNodes;
                    }

                }
            }
        }
    }
}
