using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckoutStation : MonoBehaviour
{                         
    // List to track all shoppers waiting at this checkout.
    public List<GameObject> waitingShoppers = new List<GameObject>();

    // Flag to indicate if this checkout is currently processing a shopper.
    private bool isProcessing = false;

    [Header("Processing Settings")]
    [Tooltip("The fixed location where shoppers are processed (e.g., the checkout counter).")]
    public Vector3 ProcessingLocationOffset; 
    [Tooltip("The fixed location where shoppers are Waiting.")]
    public Vector3 waitingLocationOffset;   
    [Tooltip("Offset from the laiting shopper.")]
    public Vector3 queueDistance;
    [Tooltip("Distance threshold to consider that a shopper has reached the processing location.")]
    private float processingStoppingDistance = 2.5f;

    /// <summary>
    /// Adds a shopper to the checkout queue.
    /// </summary>
    /// <param name="shopper">The shopper GameObject to add.</param>
    public void AddShopper(GameObject shopper)
    {
        waitingShoppers.Add(shopper);
        // Start processing if not already.
        if (!isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
        UpdateShopperPosition();
    }

    /// <summary>
    /// Returns the position of the last shopper in the queue.
    /// If no shopper is waiting, returns the station's position.
    /// </summary>
    public Vector3 GetWaitingLocation()
    {
        if (waitingShoppers.Count == 0)
        {
            return transform.position + waitingLocationOffset;
        }
        return transform.position + waitingLocationOffset + (queueDistance * (waitingShoppers.Count - 1));
    }

    /// <summary>
    /// Returns the current number of shoppers waiting.
    /// </summary>
    public int GetQueueCount()
    {
        return waitingShoppers.Count;
    }

    /// <summary>
    /// Coroutine that processes the checkout queue.
    /// For each shopper, first sets its destination to ProcessingLocation,
    /// waits until it reaches that location, then processes the shopper (waiting for a time
    /// based on the number of items bought) before removing it from the queue.
    /// After processing, signals the shopper to finish checkout.
    /// </summary>
    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        while (waitingShoppers.Count > 0)
        {
            GameObject currentShopper = waitingShoppers[0];

            // Get the ShopperAgentController component.
            ShopperAgentController sac = currentShopper.GetComponent<ShopperAgentController>();
            if (sac == null)
            {
                //Debug.LogWarning("ShopperAgentController not found on " + currentShopper.name);
                waitingShoppers.RemoveAt(0);
                continue;
            }

            // Get the shopper's NavMeshAgent and set its destination to ProcessingLocation.
            UnityEngine.AI.NavMeshAgent agent = currentShopper.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                Vector3 ProcessingLocation = transform.position + ProcessingLocationOffset;
                agent.SetDestination(ProcessingLocation);
                UpdateShopperPosition();

                // Wait until the shopper reaches the ProcessingLocation.
                while (Vector3.Distance(currentShopper.transform.position, ProcessingLocation) > processingStoppingDistance)
                {
                    yield return null;
                }
                //Debug.Log(currentShopper.name + " reached processing location at " + ProcessingLocation);
            }
            else
            {
                //Debug.LogWarning("NavMeshAgent not found on " + currentShopper.name);
            }

            // Determine processing time: for each item bought, wait 1-3 seconds.
            int itemsBought = sac.totalItemsBought;
            float totalWaitTime = 0f;
            if (itemsBought > 0)
            {
                for (int i = 0; i < itemsBought; i++)
                {
                    totalWaitTime += Random.Range(1f, 3f);
                }
            }
            else
            {
                // Default wait time if no items bought.
                totalWaitTime = 2f;
            }

            //Debug.Log("Processing " + currentShopper.name + " at checkout for " + totalWaitTime + " seconds.");
            yield return new WaitForSeconds(totalWaitTime);

            // Processing complete: remove the shopper from the queue.
            waitingShoppers.RemoveAt(0);
            //Debug.Log(currentShopper.name + " has been processed at checkout.");

            // Signal the shopper to finish checkout (transition to Exit state).
            sac.FinishCheckout();
        }
        isProcessing = false;
    }

    private void UpdateShopperPosition()
    {
        // Make the shoppers in the queue come forward
        if (waitingShoppers.Count > 0)
            for (int i = 1; i < waitingShoppers.Count; i++)
            {
                UnityEngine.AI.NavMeshAgent nextShopper = waitingShoppers[i].GetComponent<UnityEngine.AI.NavMeshAgent>();
                Vector3 destination = transform.position + waitingLocationOffset + (queueDistance * (i-1));
                nextShopper.SetDestination(destination);
            }
    }  
}
