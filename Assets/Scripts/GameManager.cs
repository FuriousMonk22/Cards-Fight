using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool pause = true;

    private float tick_duration = 0.25f;
    private float tick_cooldown = 0f;

    [SerializeField] private CreaturesGrid creaturesGrid;
    [SerializeField] private Timer timer;
    [SerializeField] private TerrainTilemap terrainTilemap;
    [SerializeField] private int tileFillID = 1;
    [SerializeField] private int waterLayout = 0;

    public void SetTileFillID(int tileID)
    {
        tileFillID = tileID;
    }

    public void SetWaterLayout(int layout)
    {
        waterLayout = layout;
    }

    public void StartGame()
    {
        pause = false;
        timer.StartTimer();
        creaturesGrid.InitializeGrid(10, 8);
        terrainTilemap.InitializeTilemap(
            creaturesGrid.Width,
            creaturesGrid.Height,
            tileFillID,
            waterLayout);
    }

    public void StopGame()
    {
        pause = true;
        timer.ClearTimer();
        terrainTilemap.ClearTilemap();
        creaturesGrid.ClearGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if(!GamePhaseManager.Instance.IsCombat) return;

        tick_cooldown -= Time.deltaTime;

        if(tick_cooldown <= 0f)
        {
            TickProcess();
            tick_cooldown = tick_duration;
        }
    }

    // Go through every Creature and perform actions or clear if dead.
    void TickProcess()
    {
        if(creaturesGrid.getCreatureCount(0) == 0 || creaturesGrid.getCreatureCount(1) == 0)
            timer.SkipTimer();

        for(int i = 0; i < creaturesGrid.Width; i++)
            for(int j = 0; j < creaturesGrid.Height; j++)
                if(creaturesGrid.Creatures[i, j] != null)
                {
                    Creature creature = creaturesGrid.Creatures[i, j].GetComponent<Creature>();

                    creature.Tick();
                }
    }
}
