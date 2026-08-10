using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private Tilemap groundTilemap;

    public Tilemap GroundTilemap => groundTilemap;

    private void Awake()
    {
        Instance = this;
    }
}