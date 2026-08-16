using UnityEngine;

public enum GamePhase
{
    Preparation,
    Combat
}

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

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
        if (timer == null)
        {
            Debug.LogError("GamePhaseManager: Timer is NOT assigned!");
            return;
        }

        if (creaturesGrid == null)
        {
            Debug.LogError("GamePhaseManager: CreaturesGrid is NOT assigned!");
            return;
        }

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

        CurrentPhase = GamePhase.Preparation;

        creaturesGrid.StopCombat();

        timer.StartTimer(preparationDuration);
    }

    private void StartCombat()
    {
        Debug.Log("===== COMBAT START =====");

        CurrentPhase = GamePhase.Combat;

        creaturesGrid.StartCombat();

        timer.StartTimer(combatDuration);
    }
}