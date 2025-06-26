using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class AisleManager : MonoBehaviour
{
    // Arrays for each aisle type, set these in the inspector
    public GameObject[] Aisle_Fresh;
    public GameObject[] Aisle_Essentials;
    public GameObject[] Aisle_Others;
    public GameObject[] Aisle_Offers;

    // New array that holds all aisle GameObjects. Populate this manually in the Inspector,
    // or it can be built at runtime by combining the above arrays if desired.
    public GameObject[] AllAisles;

    // List of names of all the aisles
    public List<string> AllAisleNames;

    public AisleDataSO aisleData;

    private void Start()
    {
       CreateAisleNameList();
    }

    // Function to create and return a shopping list.
    public List<string> CreateShoppingList(int minItems, int maxItems)
    {
        List<string> shoppingList = new List<string>();

        // Total weight (Fresh=0.9, Essentials=0.75, Others=0.3, Offers=0.3)
        float totalWeight = 0.9f + 0.75f + 0.3f + 0.3f; // = 2.25

        // Randomly decide the number of items in the shopping list.
        int itemCount = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < itemCount; i++)
        {
            float randomValue = Random.Range(0f, totalWeight);
            GameObject[] selectedAisleArray = null;

            // Determine which aisle type to choose based on weighted probability.
            if (randomValue < 0.9f)
            {
                selectedAisleArray = Aisle_Fresh;
            }
            else if (randomValue < 0.9f + 0.75f)  // < 1.65
            {
                selectedAisleArray = Aisle_Essentials;
            }
            else if (randomValue < 0.9f + 0.75f + 0.3f)  // < 1.95
            {
                selectedAisleArray = Aisle_Others;
            }
            else
            {
                selectedAisleArray = Aisle_Offers;
            }

            // If the selected array is not empty, choose a random aisle from it.
            if (selectedAisleArray != null && selectedAisleArray.Length > 0)
            {
                int index = Random.Range(0, selectedAisleArray.Length);
                string aisleName = selectedAisleArray[index].name;
                shoppingList.Add(aisleName);
            }
            else
            {
                //shoppingList.Add("Nill");
            }
        }

        // Optionally, output the shopping list to the console.
        /*
        for (int i = 0; i < shoppingList.Count; i++)
        {
            Debug.Log(i + ". " + shoppingList[i]);
        }
        */
        return shoppingList;
    }

    // Returns a random point within the bounds of the aisle (GameObject) that matches the provided aisle name.
    public Vector3 GetAisleLocation(string aisleName)
    {
        // Check if the AllAisles array is populated.
        if (AllAisles != null)
        {
            foreach (GameObject aisle in AllAisles)
            {
                if (aisle != null && aisle.name == aisleName)
                {
                    // Attempt to get a collider from the aisle.
                    Collider col = aisle.GetComponent<Collider>();
                    if (col != null)
                    {
                        // Get the bounds of the collider.
                        Bounds bounds = col.bounds;
                        // Generate a random point within the bounds (assuming y is fixed to 0).
                        float randomX = Random.Range(bounds.min.x, bounds.max.x);
                        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
                        return new Vector3(randomX, 0, randomZ);
                    }
                    else
                    {
                        // If no collider is found, fallback to the aisle's center position.
                        return aisle.transform.position;
                    }
                }
            }
        }
        Debug.LogWarning("Aisle with name " + aisleName + " not found.");
        return Vector3.zero;
    }

    // Returns the Tag Index from the aisle name
    public int GetAisleTagIndex(string aisleName)
    {
        string tag;
        int tagIndex = 0;
        for (int i = 0; i < AllAisles.Length; i++)
        {
            // When aisle found, return the index based on the tag name
            if (AllAisles[i].name == aisleName)
            {
                tag = AllAisles[i].tag;
                switch (tag)
                {
                    default:
                    case "Aisle_Fresh":// Aisle_Fresh
                        tagIndex = 0;
                        break;
                    case "Aisle_Essentials":// Aisle_Essentials
                        tagIndex = 1;
                        break;
                    case "Aisle_Others":// Aisle_Others
                        tagIndex = 2;
                        break;
                    case "Aisle_Offers":// Aisle_Offers
                        tagIndex = 3;
                        break;
                }
            }
        }
        return tagIndex;
    }

    public void Record(string aisleName, int decision)
    {
        aisleData.Record(aisleName, decision);
    }
    private void OnApplicationQuit()
    {
        aisleData.ExportToCSV();
    }

    private void CreateAisleNameList()
    {                  
        if (AllAisles != null)
        {
            foreach (GameObject aisle in AllAisles)
            {
                if (aisle != null)
                {
                    AllAisleNames.Add(aisle.name); 
                }
            }
        }

        aisleData.ResetAllData();
        // Make sure that all aisleNames are shared
        if (aisleData.aisleStats.Count != AllAisleNames.Count)
        {
            aisleData.aisleStats.Clear();
            for (int i = 0; i < AllAisleNames.Count; i++)
            {
                AisleStats stat = new AisleStats();
                stat.aisleName = AllAisleNames[i];
                aisleData.aisleStats.Add(stat);
            }
        }
    }
}
