using UnityEngine;

public class CanvasUI : MonoBehaviour
{
    public void SetUIVisible(string uiName)
    {
        Debug.Log($"SetUIVisible {uiName}");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(child.name == uiName);
        }
        Transform backButton = transform.Find("BackButton");
        if (backButton != null)
            backButton.gameObject.SetActive(uiName != "MainMenu");
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
