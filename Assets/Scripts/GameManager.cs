using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float tick_duration = 0.25f;
    private float tick_cooldown = 0f;

    [SerializeField] private CreaturesGrid creaturesGrid;

    // Update is called once per frame
    void Update()
    {
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
        for(int i = 0; i < creaturesGrid.Width; i++)
            for(int j = 0; j < creaturesGrid.Height; j++)
                if(creaturesGrid.Creatures[i, j] != null)
                {
                    Creature creature = creaturesGrid.Creatures[i, j].GetComponent<Creature>();

                    creature.Tick();
                }
    }
}
