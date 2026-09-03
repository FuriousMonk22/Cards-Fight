using UnityEngine;

public enum GamePhase
{
    Preparation,
    Combat
}

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }
    
    [SerializeField] private GameObject TimerSkipInstance;

    [Header("References")]
    [SerializeField] private Timer timer;
    [SerializeField] private CreaturesGrid creaturesGrid;

    [Header("Durations")]
    [SerializeField] private float preparationDuration = 10f;
    [SerializeField] private float combatDuration = 20f;

    public GamePhase CurrentPhase { get; private set; }

    public bool CanPlaceCreatures =>
        CurrentPhase == GamePhase.Preparation;

    public bool IsCombat =>
        CurrentPhase == GamePhase.Combat;

    private void Awake()
    {
        Instance = this;

        // Explicit, ca să nu existe dubii asupra fazei inițiale.
        CurrentPhase = GamePhase.Preparation;
    }

    private void Start()
    {
        timer.OnTimerFinished += OnTimerFinished;
        StartPreparation();
    }

    private void OnDestroy()
    {
        if (timer != null)
            timer.OnTimerFinished -= OnTimerFinished;
    }

    private void OnTimerFinished()
    {
        if (CurrentPhase == GamePhase.Preparation)
        {
            StartCombat();
        }
        else
        {
            StartPreparation();
        }
    }

    private void StartPreparation()
    {
        Debug.Log("===== PREPARATION START =====");

        TimerSkipInstance.SetActive(true);

        CurrentPhase = GamePhase.Preparation;

        foreach(GameObject go in creaturesGrid.Creatures)
            if(go != null)
            {
                Creature creature = go.GetComponent<Creature>();
                creature.ResetCooldown();
            }

        timer.StartTimer(preparationDuration);
    }

    private void StartCombat()
    {
        Debug.Log("===== COMBAT START =====");

        TimerSkipInstance.SetActive(false);

        CurrentPhase = GamePhase.Combat;

        foreach (GameObject go in creaturesGrid.Creatures)
        {
            if (go == null)
                continue;

            Creature creature = go.GetComponent<Creature>();

            creature.ResetShield();
            creature.OnCombatStart();
        }

        timer.StartTimer(combatDuration);
    }
}