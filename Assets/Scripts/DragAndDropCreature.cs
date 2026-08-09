using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DragAndDropCreature : MonoBehaviour
{
    private Camera cam;
    private Collider2D col;

    private bool dragging;
    private bool lerpingBack;

    private Vector3 offset;
    private Vector3 initial_pos;
    GameObject terrain_tilemap;
    GameObject creature_grid;

    [SerializeField] private float lerpSpeed = 10f;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        initial_pos = transform.position;
        terrain_tilemap = GameObject.FindGameObjectWithTag("TerrainTilemap");
        creature_grid = GameObject.FindGameObjectWithTag("CreaturesGrid");
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        // If we're returning to the initial position, keep doing that
        // regardless of where the mouse is.
        if (lerpingBack)
        {
            LerpToInitialPosition();
            return;
        }

        // Return if mouse is outside the game window
        if (!IsMouseInBounds())
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginDrag();
        }

        if (Mouse.current.leftButton.isPressed)
        {
            IsDragging();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    bool IsMouseInBounds()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        return mousePosition.x >= 0 &&
               mousePosition.x <= Screen.width &&
               mousePosition.y >= 0 &&
               mousePosition.y <= Screen.height;
    }

    void BeginDrag()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();

        if (col != null && col.OverlapPoint(mouseWorld))
        {
            dragging = true;
            offset = transform.position - mouseWorld;

            //Debug.Log("Started dragging " + gameObject.name);
        }
    }

    void IsDragging()
    {
        if (!dragging)
            return;

        Vector3 mouseWorld = GetMouseWorldPosition();

        Vector3 newPosition = mouseWorld + offset;
        newPosition.z = transform.position.z;

        transform.position = newPosition;
    }

    void EndDrag()
    {
        if (!dragging)
            return;

        dragging = false;
        lerpingBack = true;

        //Debug.Log(terrain_tilemap.GetComponent<Tilemap>().WorldToCell(GetMouseWorldPosition()));
        Vector3Int pos = terrain_tilemap.GetComponent<Tilemap>().WorldToCell(GetMouseWorldPosition());

        CreaturesGrid cg = creature_grid.GetComponent<CreaturesGrid>();
        cg.Spawn("Template", pos.x, pos.y);

        //Debug.Log("Stopped dragging " + gameObject.name);
    }

    void LerpToInitialPosition()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            initial_pos,
            lerpSpeed * Time.deltaTime
        );

        // Stop once we're sufficiently close
        if (Vector3.Distance(transform.position, initial_pos) < 0.01f)
        {
            transform.position = initial_pos;
            lerpingBack = false;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector3 screenPosition = new Vector3(
            mouseScreen.x,
            mouseScreen.y,
            Mathf.Abs(cam.transform.position.z - transform.position.z)
        );

        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
        worldPosition.z = transform.position.z;

        return worldPosition;
    }
}