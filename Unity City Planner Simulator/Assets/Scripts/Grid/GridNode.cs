using UnityEngine;

public class GridNode : MonoBehaviour
{
    private Vector3 worldPosition;
    private Vector2Int gridPosition;
    private bool isWakable;

    private int laneIndex;

    public float gCost;
    public float hCost;
    public GridNode parent;

    public float fCost => gCost + hCost;

    public GridNode(Vector3 worldPosition, Vector2Int gridPosition, bool isWakable, int laneIndex = 0)
    {
        this.worldPosition = worldPosition;
        this.gridPosition = gridPosition;
        this.isWakable = isWakable;
        this.laneIndex = laneIndex;
    }
}
