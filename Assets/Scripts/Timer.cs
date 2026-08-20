using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TMP_Text timerText;

    [Header("Timer")]
    [SerializeField] public float duration = 60f;
    [SerializeField] private bool startAutomatically = true;

    [Header("Warning")]
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseAmount = 0.15f;


    public float timeRemaining;
    private bool running;

    private Vector3 originalTextScale;

    public event Action OnTimerFinished;

    public float TimeRemaining => timeRemaining;
    public bool IsRunning => running;

    private void Awake()
    {
        originalTextScale = timerText.transform.localScale;

        timerText.color = normalColor;

        timerSlider.minValue = 0f;
        timerSlider.maxValue = duration;
        timerSlider.value = duration;
    }

    private void Start()
    {
        if (startAutomatically)
            StartTimer();
    }

    private void Update()
    {
        if (!running)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            running = false;

            timerSlider.value = 0f;
            timerText.text = "00:00";

            timerText.color = warningColor;
            timerText.transform.localScale = originalTextScale;

            TimerFinished();

            return;
        }

        timerSlider.value = timeRemaining;
        timerText.text = FormatTime(timeRemaining);

        UpdateWarningEffect();
    }

    private void UpdateWarningEffect()
    {
        if (timeRemaining <= warningTime)
        {
            timerText.color = warningColor;

            float pulse =
                1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            timerText.transform.localScale =
                originalTextScale * pulse;
        }
        else
        {
            timerText.color = normalColor;

            timerText.transform.localScale =
                originalTextScale;
        }
    }

    public void StartTimer()
    {
        timeRemaining = duration;
        running = true;

        timerSlider.maxValue = duration;
        timerSlider.value = duration;

        timerText.text = FormatTime(duration);

        timerText.color = normalColor;
        timerText.transform.localScale = originalTextScale;
    }

    public void StartTimer(float seconds)
    {
        duration = seconds;

        StartTimer();
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        running = false;

        timeRemaining = duration;
        timerSlider.value = duration;
    }

    public void SkipTimer()
    {
        timeRemaining = 0f;
        running = false;

        timerSlider.value = 0f;
        timerText.text = "00:00";

        timerText.color = warningColor;
        timerText.transform.localScale = originalTextScale;

        TimerFinished();
    }


    private void TimerFinished()
    {
        Debug.Log("Timer finished!");

        OnTimerFinished?.Invoke();
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.CeilToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}