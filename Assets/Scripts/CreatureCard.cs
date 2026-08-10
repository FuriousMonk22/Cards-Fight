using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class CreatureCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Creature")]
    [SerializeField] private string creatureName;

    [Header("UI")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text ability;
    [SerializeField] private TMP_Text damage;
    [SerializeField] private TMP_Text defense;
    [SerializeField] private TMP_Text speed;

    private CreaturesGrid grid;
    private Tilemap tilemap;
    private LineRenderer line;

    private bool dragging;
    private Vector3 cardWorldPosition;

    private void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("CreaturesGrid").GetComponent<CreaturesGrid>();
        tilemap = GameObject.FindGameObjectWithTag("TerrainTilemap").GetComponent<Tilemap>();

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
    }

    private void Start()
    {
        CreatureData data = CreatureData.Load(creatureName);

        if (data == null)
        {
            Debug.LogError($"Creature '{creatureName}' not found.");
            return;
        }

        title.text = data.Name;
        image.sprite = data.Sprite;
        ability.text = "No Ability";
        damage.text = "STR\n" + data.Attack;
        defense.text = "DEF\n" + data.Shield;
        speed.text = "SPD\n" + data.CooldownMove;
    }

    private void Update()
    {
        if (!dragging)
            return;

        Vector2 mouse = Mouse.current.position.ReadValue();

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z)
        );

        Vector3Int cell = tilemap.WorldToCell(worldPosition);

        if (!grid.IsInsideBounds(cell.x, cell.y))
        {
            line.enabled = false;
            return;
        }

        RectTransform rect = GetComponent<RectTransform>();

        Vector3 cardWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(rect.position.x, rect.position.y, -Camera.main.transform.position.z)
        );

        line.enabled = true;
        line.SetPosition(0, cardWorldPosition);
        line.SetPosition(1, tilemap.GetCellCenterWorld(cell));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;

        cardWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(
                eventData.position.x,
                eventData.position.y,
                -Camera.main.transform.position.z
            )
        );

        Debug.Log($"Dragging {creatureName}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragging)
            return;

        dragging = false;
        line.enabled = false;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(
                eventData.position.x,
                eventData.position.y,
                -Camera.main.transform.position.z
            )
        );

        Vector3Int cell = tilemap.WorldToCell(worldPosition);

        if (grid.IsInsideBounds(cell.x, cell.y))
            grid.Spawn(creatureName, cell.x, cell.y);
    }
}