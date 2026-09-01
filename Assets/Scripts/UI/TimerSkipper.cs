using UnityEngine;
using UnityEngine.EventSystems;

public class TimerSkipper : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        GameObject console = GameObject.FindGameObjectWithTag("GameConsole");

        if (console != null)
        {
            Timer timer = console.GetComponent<Timer>();

            if (timer != null)
            {
                timer.SkipTimer();
            }
        }
    }
}
