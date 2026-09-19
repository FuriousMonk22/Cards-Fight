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
    private RectTransform rect;

    private bool dragging;
    private Vector3 cardWorldPosition;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        grid = GameObject.FindGameObjectWithTag("CreaturesGrid").GetComponent<CreaturesGrid>();
        tilemap = GameObject.FindGameObjectWithTag("TerrainTilemap").GetComponent<Tilemap>();

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;

        title = title != null ? title : FindText("CardParts/Title");
        ability = ability != null ? ability : FindText("CardParts/Ability");
        damage = damage != null ? damage : FindText("CardParts/Damage");
        defense = defense != null ? defense : FindText("CardParts/Defense");
        speed = speed != null ? speed : FindText("CardParts/Speed");

        if (image == null)
        {
            Transform imageTransform = transform.Find("CardParts/Image");
            if (imageTransform != null)
                image = imageTransform.GetComponent<Image>();
        }
    }

    private void Start()
    {
        rect.localPosition = rect.localPosition + Vector3.up * 50f;
        UpdateCard();
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

    public void SetCreature(string name)
    {
        creatureName = name;
        UpdateCard();
    }

    private void UpdateCard()
    {
        if (string.IsNullOrEmpty(creatureName))
            return;

        CreatureData data = CreatureData.Load(creatureName);
        if (data == null)
        {
            Debug.LogError($"Creature '{creatureName}' not found.");
            return;
        }

        if (title != null) title.text = data.Name;
        if (image != null) image.sprite = data.Sprite;
        if (ability != null) ability.text = "No Ability";
        if (damage != null) damage.text = "STR\n" + data.Attack;
        if (defense != null) defense.text = "DEF\n" + data.Shield;
        if (speed != null) speed.text = "SPD\n" + data.CooldownAction;
    }

    private TMP_Text FindText(string path)
    {
        Transform textTransform = transform.Find(path);
        return textTransform == null ? null : textTransform.GetComponent<TMP_Text>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
        {
            return;
        }

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

        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
        {
            return;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(
                eventData.position.x,
                eventData.position.y,
                -Camera.main.transform.position.z
            )
        );

        Vector3Int cell = tilemap.WorldToCell(worldPosition);

        if (grid.IsInsideBounds(cell.x, cell.y))
            if (grid.Spawn(creatureName, cell.x, cell.y))
                transform.parent.GetComponent<CardHolder>().Consume();
    }
}
