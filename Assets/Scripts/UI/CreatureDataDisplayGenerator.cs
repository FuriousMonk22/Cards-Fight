using UnityEngine;

public class CreatureDataDisplayGenerator : MonoBehaviour
{
    [SerializeField] private GameObject template;

    private void Start()
    {
        CreatureData[] creatureDataAssets = Resources.LoadAll<CreatureData>("Creatures");

        foreach (CreatureData creatureDataAsset in creatureDataAssets)
        {
            CreatureData creatureData = CreatureData.Load(creatureDataAsset.Name);
            GameObject displayObject = Instantiate(template, transform);

            displayObject.name = creatureData.Name;
            displayObject.GetComponent<CreatureDataDisplay>().Initialize(creatureData);
        }

        template.SetActive(false);
    }
}
