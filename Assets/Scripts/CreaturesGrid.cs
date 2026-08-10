using UnityEngine;

public class CreaturesGrid : MonoBehaviour
{
    private const string CreaturePrefabPath = "Creatures/";

    TerrainTilemap TerrainTM;

    public int Width = 3;
    public int Height = 3;
    public float CellSize = 1f;

    public GameObject[,] Creatures;
    private GameObject[,] Reservations;

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
        Width = width;
        Height = height;

        Creatures = new GameObject[width, height];
        Reservations = new GameObject[width, height];
    }

    public bool TryReserveCell(GameObject creature, Vector3Int cell)
    {
        if (!IsInsideBounds(cell.x, cell.y))
            return false;

        // E ocupată de altă creatură
        if (Creatures[cell.x, cell.y] != null &&
            Creatures[cell.x, cell.y] != creature)
        {
            return false;
        }

        // E deja rezervată de altcineva
        if (Reservations[cell.x, cell.y] != null &&
            Reservations[cell.x, cell.y] != creature)
        {
            return false;
        }

        Reservations[cell.x, cell.y] = creature;

        return true;
    }

    public void CompleteMove(
    GameObject creature,
    Vector3Int from,
    Vector3Int to)
    {
        // Eliberăm celula veche
        if (IsInsideBounds(from.x, from.y) &&
            Creatures[from.x, from.y] == creature)
        {
            Creatures[from.x, from.y] = null;
        }

        // Ocupăm celula nouă
        Creatures[to.x, to.y] = creature;

        // Scoatem rezervarea
        if (Reservations[to.x, to.y] == creature)
        {
            Reservations[to.x, to.y] = null;
        }

        Debug.Log($"{creature.name}: {from} -> {to}");
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

        Vector3Int spawnCell = new Vector3Int(x, y, 0);

        Creatures[x, y] = go;

        go.transform.position =
            GridManager.Instance.GroundTilemap.GetCellCenterWorld(spawnCell);

        CreatureMovement movement =
            go.GetComponent<CreatureMovement>();

        if (movement != null)
        {
            movement.Initialize(this, spawnCell);
        }
        else
        {
            Debug.LogError("Prefab-ul nu are CreatureMovement!");
        }

        CreatureData creature =
            go.GetComponent<CreatureData>();

        // Sprite
        SpriteRenderer renderer =
            go.GetComponent<SpriteRenderer>();

        if (renderer == null)
            renderer = go.AddComponent<SpriteRenderer>();

        if (creature.Sprite != null)
        {
            renderer.sprite = creature.Sprite;
            renderer.sortingOrder = 1;
        }

        return true;
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
