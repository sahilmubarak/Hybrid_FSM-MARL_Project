using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperMarketManager : MonoBehaviour
{              
    [Header("Supermarket Key Points")]
    [Tooltip("The entry point of the supermarket")]
    public GameObject EntryPoint;

    [Tooltip("The exit point of the supermarket")]
    public GameObject ExitPoint;     

    [Tooltip("The place to go before checkingout")]
    public GameObject CheckoutArea;

    [Tooltip("Array of checkout station GameObjects (each with a CheckoutStation script attached)")]
    public CheckoutStation[] checkoutStations;

    [Header("Simulation Settings")]
    public bool speedUP = true;
    [Tooltip("Set the simulation time scale")]
    [Range(1, 3)]
    public float simulationTimeScale = 1.5f;
    public ShopperHeatmapDataSO heatmapDataSO;

    private void Start()
    {
        heatmapDataSO.ClearData();
        if (!speedUP)
        {
            Time.timeScale = 1;
        }
    }
    private void Update()
    {
        if (speedUP)
        {
            // Update the simulation time scale based on the slider value.
            Time.timeScale = simulationTimeScale;
        }
    }

    /// <summary>
    /// Returns the position of the Entry Point.
    /// </summary>
    public Vector3 GetEntryPosition()
    {
        return EntryPoint != null ? EntryPoint.transform.position : Vector3.zero;
    }

    /// <summary>
    /// Returns the position of the Exit Point.
    /// </summary>
    public Vector3 GetExitPosition()
    {
        return ExitPoint != null ? ExitPoint.transform.position : Vector3.zero;
    }

    public Vector3 GetCheckoutAreaPosition()
    {
        // Get the extents (half the size) of the CheckoutArea in local space.
        Vector3 extents = CheckoutArea.transform.localScale / 2f;
        // Generate a random point within the extents.
        Vector3 randomLocalPoint = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );
        // Convert the local point to world space.
        return CheckoutArea.transform.position + CheckoutArea.transform.rotation * randomLocalPoint;
    }


    /// <summary>
    /// Returns the CheckoutStation with the least number of waiting shoppers.
    /// </summary>
    public CheckoutStation GetLeastBusyCheckoutStation(Vector3 shopperPosition)
    {
        if (checkoutStations == null || checkoutStations.Length == 0)
        {
            Debug.LogWarning("No checkout stations available.");
            return null;
        }

        // Determine the minimal queue count.
        int minQueue = int.MaxValue;
        foreach (CheckoutStation station in checkoutStations)
        {
            int queueCount = station.GetQueueCount();
            if (queueCount < minQueue)
            {
                minQueue = queueCount;
            }
        }

        // Among the stations with minimal queue count, select the one closest to shopperPosition.
        CheckoutStation bestStation = null;
        float minDistance = Mathf.Infinity;
        foreach (CheckoutStation station in checkoutStations)
        {
            if (station.GetQueueCount() == minQueue)
            {
                float distance = Vector3.Distance(shopperPosition, station.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestStation = station;
                }
            }
        }

        return bestStation;
    }

    private void OnApplicationQuit()
    {
        heatmapDataSO.ExportToCSVs();
    }
}
