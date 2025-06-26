using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[System.Serializable]
public class AisleStats
{
    public string aisleName;
    public int itemsBought;
    public int itemsBrowsed;
    public int itemsIgnored;
}

[CreateAssetMenu(fileName = "AisleData", menuName = "RetailSim/Aisle Data SO")]
public class AisleDataSO : ScriptableObject
{
    public List<AisleStats> aisleStats = new List<AisleStats>();

    public void Record(string aisleName, int decision)
    {
        var data = aisleStats.Find(a => a.aisleName == aisleName);
        if (data != null)
        {
            switch (decision)
            {
                case 0:
                    data.itemsBought++;
                    break;
                case 1:
                    data.itemsBrowsed++;
                    break;
                case 2:
                    data.itemsIgnored++;
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Aisle '{aisleName}' not found in AisleDataSO!");
        }
    }

    public void ResetAllData()
    {
        foreach (var data in aisleStats)
        {
            data.itemsBought = 0;
            data.itemsBrowsed = 0;
            data.itemsIgnored = 0;
        }
    }

    /// <summary>
    /// Exports the aisle stats to a CSV file in the specified path.
    /// If no path is provided, saves to persistentDataPath/AisleData.csv
    /// </summary>
    public void ExportToCSV(string customPath = null)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Aisle Name,Items Bought,Items Browsed,Items Ignored");

        foreach (var data in aisleStats)
        {
            csv.AppendLine($"{data.aisleName},{data.itemsBought},{data.itemsBrowsed},{data.itemsIgnored}");
        }

        string filePath = string.IsNullOrEmpty(customPath)
            ? Path.Combine(Application.persistentDataPath, "AisleData.csv")
            : customPath;

        try
        {
            File.WriteAllText(filePath, csv.ToString());
            Debug.Log($"[AisleDataSO] CSV export successful: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[AisleDataSO] Failed to export CSV: {e.Message}");
        }
    }
}
