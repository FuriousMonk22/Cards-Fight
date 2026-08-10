using UnityEngine;
using UnityEngine.InputSystem;

public class CreaturesGrid : MonoBehaviour
{
    private const string CreaturePrefabPath = "Creatures/";

    TerrainTilemap TerrainTM;

    public int Width = 3;
    public int Height = 3;
    public float CellSize = 1f;

    public GameObject[,] Creatures;
    private bool dragging;
    private Vector2Int dragStart;

    private void Awake()
    {
        TerrainTM = GameObject.FindWithTag("TerrainTilemap").GetComponent<TerrainTilemap>();
    }

    void Start()
    {
        InitializeGrid(10, 8);
    }

    public void InitializeGrid(int width, int height)
    {
        Debug.Log($"Initializing grid: width={width}, height={height}");

        Width = width;
        Height = height;
        Creatures = new GameObject[width, height];
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z)
            );

            Vector3Int cell = TerrainTM.tilemap.WorldToCell(world);

            if (IsInsideBounds(cell.x, cell.y) && Creatures[cell.x, cell.y] != null)
            {
                dragging = true;
                dragStart = new Vector2Int(cell.x, cell.y);
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && dragging)
        {
            dragging = false;

            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z)
            );

            Vector3Int cell = TerrainTM.tilemap.WorldToCell(world);

            if (IsInsideBounds(cell.x, cell.y))
            {
                Swap(
                    dragStart.x,
                    dragStart.y,
                    cell.x,
                    cell.y
                );
            }
        }
    }

    public void Swap(int x1, int y1, int x2, int y2)
    {
        if (!IsInsideBounds(x1, y1) || !IsInsideBounds(x2, y2))
        {
            Debug.Log("Failed to swap creatures, outside of bounds.");
            return;
        }

        GameObject temp = Creatures[x1, y1];

        Creatures[x1, y1] = Creatures[x2, y2];
        Creatures[x2, y2] = temp;

        if (Creatures[x1, y1] != null)
            Creatures[x1, y1].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x1, y1, 0));

        if (Creatures[x2, y2] != null)
            Creatures[x2, y2].transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x2, y2, 0));
        
        Debug.Log($"Swapped ({x1}, {y1}) with ({x2}, {y2})");
    }

    public bool Spawn(string creatureName, int x, int y)
    {
        if (Creatures == null){
            Debug.Log("Failed to add creature, creature grid uninitialized.");
            return false;
        }
        if (!IsInsideBounds(x, y)){
            Debug.Log("Failed to add creature, outside of bounds." + x.ToString() + y.ToString());
            return false;
        }
        if (Creatures[x, y] != null) {
            Debug.Log("Failed to add creature, slot occupied.");
            return false;
        }

        GameObject prefab = Resources.Load<GameObject>(CreaturePrefabPath + creatureName);

        if (prefab == null)
        {
            Debug.LogError($"Creature prefab '{creatureName}' not found.");
            return false;
        }

        GameObject go = Instantiate(prefab, transform);
        Creatures[x, y] = go;
        go.transform.position = TerrainTM.GetWorldPosition(new Vector3Int(x, y, 0));
        CreatureData creature = go.GetComponent<CreatureData>();

        // ################################
        // Temporary code fix for showing creature sprite, to be added in separate class
        go.AddComponent<SpriteRenderer>();
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        
        if (creature.Sprite != null)
            renderer.sprite = creature.Sprite;
            renderer.sortingOrder = 1;
        // ################################

        return true;
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
