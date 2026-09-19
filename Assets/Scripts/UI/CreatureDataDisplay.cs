using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureDataDisplay : MonoBehaviour
{
    public void Initialize(CreatureData creatureData)
    {
        transform.Find("Image").GetComponent<Image>().sprite = creatureData.Sprite;
        transform.Find("Name").GetComponent<TMP_Text>().text = $"{creatureData.Name}";
        transform.Find("Ability").GetComponent<TMP_Text>().text = "Ability: ???";
        transform.Find("Health").GetComponent<TMP_Text>().text = $"HP: {creatureData.Health}";
        transform.Find("Attack").GetComponent<TMP_Text>().text = $"ATK: {creatureData.Attack}";
        transform.Find("Shield").GetComponent<TMP_Text>().text = $"SHD: {creatureData.Shield}";
        transform.Find("Cooldown").GetComponent<TMP_Text>().text = $"Cooldown: {creatureData.CooldownAction}";
        transform.Find("Range").GetComponent<TMP_Text>().text = $"RNG: {creatureData.AttackRange}";
    }
}
