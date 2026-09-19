using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class ModifierCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Modifier")]
    private Ability modifier;

    [Header("UI")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text ability;

    private RectTransform rect;
    private CreaturesGrid grid;
    private Tilemap tilemap;
    private LineRenderer line;
    private bool dragging;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        grid = GameObject.FindGameObjectWithTag("CreaturesGrid").GetComponent<CreaturesGrid>();
        tilemap = GameObject.FindGameObjectWithTag("TerrainTilemap").GetComponent<Tilemap>();
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;

        if (title == null)
        {
            Transform titleTransform = transform.Find("CardParts/Title");
            if (titleTransform != null)
                title = titleTransform.GetComponent<TMP_Text>();
        }

        if (ability == null)
        {
            Transform abilityTransform = transform.Find("CardParts/Ability");
            if (abilityTransform != null)
                ability = abilityTransform.GetComponent<TMP_Text>();
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
            new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z));
        Vector3Int cell = tilemap.WorldToCell(worldPosition);

        if (!grid.IsInsideBounds(cell.x, cell.y))
        {
            line.enabled = false;
            return;
        }

        Vector3 cardWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(rect.position.x, rect.position.y, -Camera.main.transform.position.z));

        line.enabled = true;
        line.SetPosition(0, cardWorldPosition);
        line.SetPosition(1, tilemap.GetCellCenterWorld(cell));
    }

    public void SetAbility(Ability ability)
    {
        modifier = ability;
        UpdateCard();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
            return;

        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragging)
            return;

        dragging = false;
        line.enabled = false;

        if (GamePhaseManager.Instance != null &&
            !GamePhaseManager.Instance.CanPlaceCreatures)
            return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(eventData.position.x, eventData.position.y, -Camera.main.transform.position.z));
        Vector3Int cell = tilemap.WorldToCell(worldPosition);

        if (!grid.IsInsideBounds(cell.x, cell.y) ||
            grid.Creatures[cell.x, cell.y] == null ||
            modifier == null)
            return;

        Creature creature = grid.Creatures[cell.x, cell.y].GetComponent<Creature>();
        if (creature == null)
            return;

        modifier.SetOwner(creature);
        creature.abilities.Add(modifier);
        transform.parent.GetComponent<CardHolder>().Consume();
    }

    private void UpdateCard()
    {
        if (modifier == null)
            return;

        if (title != null)
            title.text = modifier.Name;

        if (ability != null)
            ability.text = modifier.Description;
    }
}
