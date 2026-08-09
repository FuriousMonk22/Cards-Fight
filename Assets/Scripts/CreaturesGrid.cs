using UnityEngine;

public class CreaturesGrid : MonoBehaviour
{
    private const string CreaturePrefabPath = "Creatures/";

    TerrainTilemap TerrainTM;

    public int Width = 3;
    public int Height = 3;
    public float CellSize = 1f;

    public GameObject[,] Creatures;

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
