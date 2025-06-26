using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Linq;

public class GoalShopperBrain : Agent, IShopperBrain
{
    // Tracking variables
    [Tooltip("No of decisions at which an Episode Ends")]
    public int endEpisodeAt = 50;
    [Space(10)]
    [SerializeField] int decisionsRequested = 0;
    [SerializeField] int liveStepCount = 0;
    [SerializeField] int totalDecisionsRequested = 0;

    [Header("Last Decisions Taken")]
    [SerializeField] private int lastWhatToDoDecision = -1;
    [SerializeField] private int lastWhereToGoDecision = -1;
    [SerializeField] private int lastGoToDistractionDecision = -1;

    [Header("Queue references")]
    public int agentCount;

    private CombinedObservations currentObservations;

    // Flags for controlling decision requests.
    private bool decisionRequested = false;
    private bool canEndEpisode = false;
    private bool purDecisionRequested = false; // Purchase
    private bool navDecisionRequested = false; // Navigation
    private bool disDecisionRequested = false; // Distraction

    // Decision Queue for all types
    [SerializeField] private List<DecisionQueueItem> decisionQueue;
    private bool isProcessingQueue = false;

    private DecisionQueueItem? currentProcessingItem = null;

    private void Start()
    {
        decisionQueue = new List<DecisionQueueItem>();
    }

    private void Update()
    {
        liveStepCount = StepCount;
        ProcessDecisionQueueStep();
        if (canEndEpisode && !decisionRequested)
        { 
            CanEndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        // Optionally: Reset or reposition agents here.
        Debug.Log("Episode Began");

        // Reset decision tracking variables
        decisionsRequested = 0;

        decisionRequested = false; 
        canEndEpisode = false;
        purDecisionRequested = false;
        navDecisionRequested = false;
        disDecisionRequested = false;

        lastWhatToDoDecision = -1;
        lastWhereToGoDecision = -1;
        lastGoToDistractionDecision = -1;

        isProcessingQueue = false;
        currentProcessingItem = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Common observations
        sensor.AddObservation(currentObservations.TimeSpent);                  // int
        sensor.AddObservation(currentObservations.ShoppingProgress);             // float
        sensor.AddObservation(currentObservations.ShoppingListCount);             // int
        sensor.AddObservation(currentObservations.TotalItemsBought);              // int

        if (purDecisionRequested && !navDecisionRequested && !disDecisionRequested)
        {
            // Purchase decision-specific observations (total 14 observations)
            sensor.AddObservation(currentObservations.TotalFreshItemsBought);      // int
            sensor.AddObservation(currentObservations.TotalEssentialItemsBought); // int
            sensor.AddObservation(currentObservations.TotalOtherItemsBought);      // int
            sensor.AddObservation(currentObservations.TotalOfferItemsBought);      // int
            sensor.AddObservation(currentObservations.CurrentAisleTag);            // int
            sensor.AddObservation(currentObservations.TotalItemsBrowsed);          // int
            sensor.AddObservation(currentObservations.itemInTheList);              // int
            // Dummies to fill space
            sensor.AddObservation(-1.0f); // float
            sensor.AddObservation(-1.0f); // float
            sensor.AddObservation(-1.0f); // float
            sensor.AddObservation(-1);    // int
        }
        else if (!purDecisionRequested && navDecisionRequested && !disDecisionRequested)
        {
            // Navigation decision: Use 7 dummy observations then 3 decision-specific observations and a dummy.
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);  // int dummy
            sensor.AddObservation(currentObservations.DistanceToNearestAisle);   // float
            sensor.AddObservation(currentObservations.DistanceToNearestInList);    // float
            sensor.AddObservation(currentObservations.DistanceToNextAisle);        // float
            sensor.AddObservation(-1);    // dummy
        }
        else if (!purDecisionRequested && !navDecisionRequested && disDecisionRequested)
        {
            // Distraction decision: 4 decision-specific, 7 dummies.
            sensor.AddObservation(currentObservations.TotalFreshItemsBought);
            sensor.AddObservation(currentObservations.TotalEssentialItemsBought);
            sensor.AddObservation(currentObservations.TotalOtherItemsBought);
            sensor.AddObservation(currentObservations.TotalOfferItemsBought);
            sensor.AddObservation(-1); // dummy
            sensor.AddObservation(-1); // dummy
            sensor.AddObservation(-1); // dummy (int)
            sensor.AddObservation(-1.0f); // dummy (float)
            sensor.AddObservation(-1.0f); // dummy (float)
            sensor.AddObservation(-1.0f); // dummy (float)
            sensor.AddObservation(currentObservations.DistractingAsileTag);
        }
        else // No decision requested.
        {
            // Provide a full set of dummy observations.
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
            sensor.AddObservation(-1.0f);
            sensor.AddObservation(-1.0f);
            sensor.AddObservation(-1.0f);
            sensor.AddObservation(-1);
        }
    }

    // This function is used to request a decision (it simply calls RequestDecision() if not already requested).
    private void RequestSingleDecision()
    {
        if (!decisionRequested)
        {
            if (decisionsRequested >= endEpisodeAt)
            {
                canEndEpisode = true;
            }
            RequestDecision();
            decisionRequested = true;
            decisionsRequested++;
        }
    }

    // This function adds or updates an entry in the decision queue for a given shopper and decision type.
    private void RequestDecision(CombinedObservations observations, ShopperAgentController shopper, int decisionType)
    {
        // Look for an existing queue item.
        for (int i = 0; i < decisionQueue.Count; i++)
        {
            if (decisionQueue[i].controller == shopper && decisionQueue[i].decisionType == decisionType)
            {
                // Update the observations.
                DecisionQueueItem updatedItem = decisionQueue[i];
                updatedItem.observations = observations;
                decisionQueue[i] = updatedItem;
                return;
            }
        }
        // Not found; add new entry.
        DecisionQueueItem newItem = new DecisionQueueItem(observations, shopper, decisionType);
        decisionQueue.Add(newItem);

        // Start processing the queue if not already processing.
        if (!isProcessingQueue)
        {
            isProcessingQueue = true;
        }
    }

    private void ProcessDecisionQueueStep()
    {
        if (!isProcessingQueue || decisionRequested || decisionQueue.Count == 0)
        {
            return;
        }

        currentProcessingItem = decisionQueue.First();
        decisionQueue.RemoveAt(0);

        // Set decision flags based on decision type.
        switch (currentProcessingItem.Value.decisionType)
        {
            case 0:
                purDecisionRequested = true;
                navDecisionRequested = false;
                disDecisionRequested = false;
                break;
            case 1:
                purDecisionRequested = false;
                navDecisionRequested = true;
                disDecisionRequested = false;
                break;
            case 2:
                purDecisionRequested = false;
                navDecisionRequested = false;
                disDecisionRequested = true;
                break;
            default:
                currentProcessingItem = null;
                return;
        }

        // Update current observations.
        currentObservations = currentProcessingItem.Value.observations;

        // Request a decision.
        RequestSingleDecision();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (currentProcessingItem.HasValue)
        {
            int decision = -1;
            int decisionType = currentProcessingItem.Value.decisionType;
            ShopperAgentController shopper = currentProcessingItem.Value.controller;

            if (decisionType == 0 && purDecisionRequested)
            {
                lastWhatToDoDecision = actions.DiscreteActions[0];
                decision = lastWhatToDoDecision;
                ComputePurchaseRewardAndTrack(decision);
                shopper.purDecision = decision;
                purDecisionRequested = false;
                lastWhatToDoDecision = -1;
            }
            else if (decisionType == 1 && navDecisionRequested)
            {
                lastWhereToGoDecision = actions.DiscreteActions[1];
                decision = lastWhereToGoDecision;
                ComputeNavigationRewardAndTrack(decision);
                shopper.navDecision = decision;
                navDecisionRequested = false;
                lastWhereToGoDecision = -1;
            }
            else if (decisionType == 2 && disDecisionRequested)
            {
                lastGoToDistractionDecision = actions.DiscreteActions[2];
                decision = lastGoToDistractionDecision;
                ComputeDistractionRewardAndTrack(decision);
                shopper.disDecision = decision;
                disDecisionRequested = false;
                lastGoToDistractionDecision = -1;
            }
            currentProcessingItem = null;
        }

        // Reset the general decision requested flag.
        decisionRequested = false;
    }

    // Decision functions for the different decision types.
    // For Purchase decisions.
    public void WhatToDo(CombinedObservations observations, ShopperAgentController shopper)
    {
        RequestDecision(observations, shopper, 0);
    }

    private void ComputePurchaseRewardAndTrack(int decision)
    {
        float reward;
        switch (decision)
        {
            case 0: // Buy
                reward = 1;
                if (currentObservations.itemInTheList == 0) reward -= 2; // Penalty for buying item not from list
                if (currentObservations.TotalItemsBought > currentObservations.ShoppingListCount) reward -= 0.5f; // Penalty for over-purchasing
                break;
            case 1: // Browse
                reward = 0.5f;
                if (currentObservations.TotalItemsBrowsed > 1.5f * currentObservations.ShoppingListCount) reward = -0.5f; // Penalty for excessive Browse
                if (currentObservations.itemInTheList == 1) reward -= 2; // Penalty for ignoring Shopping list
                break;
            case 2: // Ignore
                reward = -0.25f;
                if (currentObservations.itemInTheList == 1) reward = -1; // Penalty for ignoring Shopping list
                break;
            default:
                reward = 0f;
                break;
        }
        GiveReward(reward);
        DicisionTracker.Instance.TrackGoalDecision("Purchase", decision);
    }


    // For Navigation decisions.
    public void WhereToGo(CombinedObservations observations, ShopperAgentController shopper)
    {
        RequestDecision(observations, shopper, 1);
    }

    private void ComputeNavigationRewardAndTrack(int decision)
    {
        float reward;
        switch (decision)
        {
            case 0: // Nearest aisle overall
                reward = 1;
                if (currentObservations.ShoppingProgress < 0.5f) reward = -0.5f;
                break;
            case 1: // Nearest aisle from list
                reward = 1;
                break;
            case 2: // First aisle in list
                reward = 0.5f;
                if (currentObservations.DistanceToNearestInList < currentObservations.DistanceToNextAisle) reward = -0.5f;
                break;
            case 3: // Checkout
                reward = 5.0f;
                if (currentObservations.ShoppingProgress < 0.90f) reward = -5.0f; // Penalty for early checkout
                break;
            default:
                reward = 0f;
                break;
        }
        GiveReward(reward);
        DicisionTracker.Instance.TrackGoalDecision("Navigation", decision);
    }


    // For Distraction decisions.
    public void GoToDistraction(CombinedObservations observations, ShopperAgentController shopper)
    {
        RequestDecision(observations, shopper, 2);
    }

    private void ComputeDistractionRewardAndTrack(int decision)
    {
        float reward;
        switch (decision)
        {
            case 0: // Not getting distracted
                reward = 0.5f;
                break;
            case 1: // Getting distracted
                reward = 1.0f; // Impulsive shopper gets rewarded for distraction
                if (currentObservations.DistractingAsileTag == 4) reward += 0.25f;
                else reward -= 0.5f;
                break;
            default:
                reward = 0f;
                break;
        }
        GiveReward(reward);
        DicisionTracker.Instance.TrackGoalDecision("Distraction", decision);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Random.Range(0, 3); // WhatToDo
        discreteActions[1] = Random.Range(0, 4); // WhereToGo
        discreteActions[2] = Random.Range(0, 2); // GoToDistraction
        /*
        //Skibidi Heuristics
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // WhatToDo
        discreteActions[1] = 2; // WhereToGo
        discreteActions[2] = 0; // GoToDistraction
        */
    }

    // This function adds the provided reward using AddReward().
    private void GiveReward(float reward)
    {
        AddReward(reward);
        //Debug.Log("Rewarded: " + reward);
    }

    // Called from the ShopperSpawner when a specified number of shoppers exit the market.
    public void CanEndEpisode()
    {
        totalDecisionsRequested += decisionsRequested;
        decisionsRequested = 0;
        canEndEpisode = false;
        Debug.Log("Episode Ended");
        EndEpisode();
    }
}