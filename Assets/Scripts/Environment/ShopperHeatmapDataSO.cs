using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopperHeatmapData", menuName = "RetailSim/Heatmap Data SO")]
public class ShopperHeatmapDataSO : ScriptableObject
{
    [Tooltip("Continuous path positions for Goal-oriented shoppers.")]
    public List<Vector3> goalShopperPositions = new List<Vector3>();

    [Tooltip("Continuous path positions for Impulse shoppers.")]
    public List<Vector3> impulseShopperPositions = new List<Vector3>();

    [Tooltip("Continuous path positions for Wanderer shoppers.")]
    public List<Vector3> wandererShopperPositions = new List<Vector3>();

    /// <summary>
    /// Adds a list of position values continuously to the correct shopper type’s list.
    /// </summary>
    /// <param name="shopperType">"GoalOriented", "Impulse", or "Wanderer"</param>
    /// <param name="path">List of positions recorded by an agent</param>
    public void AddPositionList(string shopperType, List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("Attempted to add an empty or null position list.");
            return;
        }

        // Add all positions from the provided list into the correct list.
        switch (shopperType.ToLower())
        {
            case "goaloriented":
                goalShopperPositions.AddRange(path);
                break;
            case "impulse":
                impulseShopperPositions.AddRange(path);
                break;
            case "wanderer":
                wandererShopperPositions.AddRange(path);
                break;
            default:
                Debug.LogWarning($"Unknown shopper type '{shopperType}' — must be 'GoalOriented', 'Impulse', or 'Wanderer'.");
                break;
        }
    }

    /// <summary>
    /// Clears all stored position data.
    /// </summary>
    public void ClearData()
    {
        goalShopperPositions.Clear();
        impulseShopperPositions.Clear();
        wandererShopperPositions.Clear();
    }

    /// <summary>
    /// Exports continuous position data for each shopper type to separate CSV files.
    /// Each line in the CSV is in the format: x,y,z
    /// </summary>
    public void ExportToCSVs()
    {
        ExportListToCSV(goalShopperPositions, "GoalShopperHeatmap.csv");
        ExportListToCSV(impulseShopperPositions, "ImpulseShopperHeatmap.csv");
        ExportListToCSV(wandererShopperPositions, "WandererShopperHeatmap.csv");
        Debug.Log($"CSV files saved to {Application.persistentDataPath}");
    }

    private void ExportListToCSV(List<Vector3> positions, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var pos in positions)
            {
                writer.WriteLine($"{pos.x},{pos.y},{pos.z}");
            }
        }
    }
}
