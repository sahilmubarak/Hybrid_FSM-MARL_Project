using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShopperAgentController : MonoBehaviour
{
    private NavMeshAgent navAgent;
    [SerializeField] private Animator agentAnimator;

    // References assigned via the Inspector.
    [HideInInspector] public AisleManager aisleManager;
    [HideInInspector] public ItemDetectorColloider itemDetector;
    [HideInInspector] public SuperMarketManager superMarketManager;  // Reference to the supermarket manager.
    [HideInInspector] public IShopperBrain brain;  // Reference to the RL Brain.
    [HideInInspector] public AgentSpawner spawner;
    public string shopperType;


    // Flag to check if the agent is busy with an action.
    private bool isPerformingAction = false;
    private bool isNavigating = false;

    private bool purRequested = false;
    private bool navRequested = false;
    private bool disRequested = false;

    [Space(10)]
    // Damping for animator.
    public float animationDampTime = 0.1f;
    // NavMesh stopping distance threshold.
    public float stoppingDistanceThreshold = 1.0f;
    // Rotation speed for turning toward the target when Browse or buying.
    public float rotationSpeed = 5f;
    // Timeout in seconds for rotation to prevent the agent from being stuck.
    public float rotationTimeout = 2f;


    // Logic Variables-----------------------------------------------------------------
    #region Variables for Decision Making
    // Structs that hold the observations
    private CombinedObservations observations;

    [Space(10)]
    // Decisions
    public int purDecision = -1;
    public int navDecision = -1;
    public int disDecision = -1;

    public float timeSpent; // Time spent shopping in Seconds
    public string currentAisle = "-1"; // The aisle the shopper is at
    public int currentAisleTag; // The Tag of the aisle the shopper is at
    public int currentItemInList; // The Tag of the aisle the shopper is at

    // Distraction
    int distractingAisleTag;

    [Space(10)]
    // The shopping list (aisle names) for this shopper - visible in the Inspector.
    [SerializeField] private List<string> shoppingList = new List<string>();
    private int listCountAtStart;

    [Space(10)]
    // Aisle Distances
    private float nearestAisleDistance;// Nearest of all Aisles
    [SerializeField] private string nearestAisleName;
    private float nearestListAisleDistance;// Nearest Aisle in the list
    [SerializeField] private string nearestListAisleName;
    private float nextListAisleDistance;// Next Aisle in the list
    [SerializeField] private string nextListAisleName;

    [Space(10)]
    // Variables for tracking purchases and Browse.
    public int totalItemsBought = 0;
    public int totalItemsBrowsed = 0;
    public int boughtFromFresh = 0;
    public int boughtFromEssentials = 0;
    public int boughtFromOthers = 0;
    public int boughtFromOffers = 0;
    #endregion
    //---------------------------------------------------------------------------------


    // State machine.
    private enum ShopperState { Enter, Shopping, Checkout, Exit }
    private ShopperState currentState = ShopperState.Enter;

    // For data recording
    private List<Vector3> recordedPositions = new List<Vector3>();
    private float positionRecordTimer = 0f;
    private float positionRecordInterval = 1f;

    // Reference to the chosen checkout station during checkout.
    private CheckoutStation currentCheckoutStation;
    // Flags for checkout destination and queue addition.
    private bool checkoutDestinationSet = false;
    private bool hasBeenAddedToQueue = false;
    private bool movedToCheckoutArea = false;

    #region Non State Functions
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        itemDetector = GetComponentInChildren<ItemDetectorColloider>();
    }

    private void Start()
    {
        // Create a shopping list using the AisleManager.
        shoppingList = aisleManager.CreateShoppingList(1, 12);
        listCountAtStart = shoppingList.Count;
        // Start with the Enter state.
        TransitionToState(ShopperState.Enter);

        // Claculate aisle distances
        CalculateAisleDistances();
    }

    private void FixedUpdate()
    {
        // Update the animation based on the agent's speed.
        float speedNormalized = navAgent.velocity.magnitude / navAgent.speed;
        agentAnimator.SetFloat("Movement", speedNormalized, animationDampTime, Time.deltaTime);
        // Update Time
        timeSpent += Time.deltaTime;

        // Update the observation severy frame
        UpdateObservationStructs();

        // Execute state-specific logic.
        switch (currentState)
        {
            case ShopperState.Enter:
                EnterState();
                break;
            case ShopperState.Shopping:
                ShoppingState();
                break;
            case ShopperState.Checkout:
                CheckoutState();
                break;
            case ShopperState.Exit:
                ExitState();
                break;
        }
        RecordPosition();
    }
    private void RecordPosition()
    {
        positionRecordTimer += Time.deltaTime;
        if (positionRecordTimer >= positionRecordInterval)
        {
            recordedPositions.Add(transform.position);
            positionRecordTimer = 0f;
        }
    }


    // Handles state transitions.
    private void TransitionToState(ShopperState newState)
    {
        currentState = newState;
        //Debug.Log("Transitioning to state: " + currentState.ToString());

        // Reset checkout flags when transitioning away from Checkout.
        if (newState != ShopperState.Checkout)
        {
            checkoutDestinationSet = false;
            hasBeenAddedToQueue = false;
            movedToCheckoutArea = false;
        }

        // Execute immediate actions upon entering a new state.
        if (newState == ShopperState.Enter)
        {
            // Move to the supermarket entry point.
            Vector3 entryPos = superMarketManager.GetEntryPosition();
            navAgent.SetDestination(entryPos);
            navAgent.isStopped = false;
        }
        else if (newState == ShopperState.Exit)
        {
            // Move to the supermarket exit.
            navAgent.SetDestination(superMarketManager.GetExitPosition());
            navAgent.isStopped = false;
        }
        else if (newState == ShopperState.Shopping)
        {
            // Start Shopping by going to an aisle
            MoveToAisle(nearestListAisleName);
        }
        else
        {
            if (!movedToCheckoutArea)
            {
                navAgent.SetDestination(superMarketManager.GetCheckoutAreaPosition());
                navAgent.isStopped = false;
                movedToCheckoutArea = true;
            }
        }
    }

    // Helper function: Calculates the NavMesh path distance between two points.
    private float GetNavMeshPathDistance(Vector3 startPos, Vector3 endPos)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path))
        {
            float distance = 0f;
            if (path.corners.Length < 2)
                return distance;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return 9999;// Large Value
    }

    // Calculates distances to all aisles in the given list using NavMesh path distance and returns the name,distance of the closest aisle.
    private (string, float) GetClosestAisleName(List<string> listOfAisleNames)
    {
        //Debug.Log("CurrentAisle name: " + currentAisle);

        if (listOfAisleNames == null)
            return ("Nill", 9999);

        float minDistance = 9999;// Large Value
        string closestAisle = "Empty";
        for (int i = 0; i < listOfAisleNames.Count; i++)
        {
            string aisleName = listOfAisleNames[i];
            Vector3 aisleLocation = aisleManager.GetAisleLocation(aisleName);
            float distance = GetNavMeshPathDistance(transform.position, aisleLocation);
            if (distance < minDistance)// The closest aisle should not be the current Aisle
            {
                minDistance = distance;
                closestAisle = listOfAisleNames[i];
                //Debug.Log("Closest Aisle name: " + closestAisle);
            }
        }
        return (closestAisle, minDistance);
    }

    // Public method to signal that checkout is finished, transitioning the shopper to Exit.
    public void FinishCheckout()
    {
        //Debug.Log(gameObject.name + " finishing checkout, transitioning to Exit state.");
        TransitionToState(ShopperState.Exit);
    }
    #endregion

    #region State Functions
    // Enter state: Move from outside to the supermarket's entry.
    private void EnterState()
    {
        Vector3 entryPos = superMarketManager.GetEntryPosition();
        if (!navAgent.pathPending && Vector3.Distance(transform.position, entryPos) <= stoppingDistanceThreshold)
        {
            //Debug.Log("Entered supermarket.");
            TransitionToState(ShopperState.Shopping);
        }
    }

    // Shopping state:
    #region Shopping State
    private void ShoppingState()
    {
        // Shopper has reached an Aisle and is not doing anything
        if (!navAgent.pathPending && navAgent.remainingDistance <= stoppingDistanceThreshold)
        {
            isNavigating = false;
            // If there is no aisle selected
            if (currentAisle == "Done" && isPerformingAction == false && !isNavigating)
            {
                CalculateAisleDistances();// Calculate distance to the 3 Aisle types
                // Request navigation decision
                if (!navRequested)
                {
                    navDecision = -1;
                    brain.WhereToGo(observations, this);
                    navRequested = true;
                    //Debug.Log("Nav Dis Reqd At: " + DateTime.Now);
                }
                // Process navigation decision if made
                if (navDecision != -1)
                {
                    switch (navDecision)
                    {
                        default:
                            break;
                        case 0:// Move to Nearest Aisle from All available
                            MoveToAisle(nearestAisleName);
                            isNavigating = true; // Set navigating after setting the destination
                            break;
                        case 1:// Move to the Nearest Aisle in the shoppingList
                            if (shoppingList.Count > 0)
                            {
                                MoveToAisle(nearestListAisleName);
                                isNavigating = true; // Set navigating after setting the destination
                            }
                            else
                            {
                                TransitionToState(ShopperState.Checkout);
                            }
                            break;
                        case 2:// Move to Next Aisle in the shoppingList
                            if (shoppingList.Count > 0)
                            {
                                MoveToAisle(nextListAisleName);
                                isNavigating = true; // Set navigating after setting the destination
                            }
                            else
                            {
                                TransitionToState(ShopperState.Checkout);
                            }
                            break;
                        case 3: // Checkout
                            TransitionToState(ShopperState.Checkout);
                            break;
                    }
                    navRequested = false; // Reset request flag after processing decision
                    navDecision = -1;    // Reset decision
                }
            }
            if (currentAisle != "Nill" && !isNavigating && !isPerformingAction)
            {
                // Request purchase decision
                if (!purRequested)
                {
                    purDecision = -1;
                    brain.WhatToDo(observations, this);
                    purRequested = true;
                    //Debug.Log("Pur Dis Reqd At: " + DateTime.Now);
                }
                // Process purchase decision if made
                if (purDecision != -1)
                {
                    isPerformingAction = true;
                    switch (purDecision)
                    {
                        default:
                            isPerformingAction = false;
                            break;
                        case 0: // Buy
                            StartCoroutine(BuyCoroutine());
                            break;
                        case 1: // Browse
                            StartCoroutine(BrowseCoroutine());
                            break;
                        case 2: // Ignore
                            StartCoroutine(IgnoreCoroutine());
                            break;
                    }
                    purRequested = false; // Reset request flag after processing decision
                    purDecision = -1;    // Reset decision
                }
            }
        }
    }

    // Distract the agent, called from the colliderDetector
    public void DistractShopper(int tag, string aisleName)
    {
        if (aisleName != currentAisle && currentState == ShopperState.Shopping && !isPerformingAction)
        {
            // Update the distracting aisle tag
            distractingAisleTag = tag;
            // Request distraction decision
            if (!disRequested)
            {
                disDecision = -1;
                brain.GoToDistraction(observations, this);
                disRequested = true;
                //Debug.Log("Dis Dis Reqd At: " + DateTime.Now);
            }
            // Process distraction decision if made
            if (disDecision != -1)
            {
                switch (disDecision)
                {
                    default:
                        break;
                    case 0:
                        // Ignore destraction
                        //Debug.Log("Shopper Not Distracted!");
                        break;
                    case 1:
                        // Go towards destraction
                        //Debug.Log("Shopper Distracted!");
                        MoveToAisle(aisleName);
                        isNavigating = true; // Set navigating after setting the destination
                        break;
                }
                disRequested = false; // Reset request flag after processing decision
                disDecision = -1;    // Reset decision
            }
        }
    }

    // Calculate distance to 3 Aisle types
    private void CalculateAisleDistances()
    {
        // Closest of all Aisles
        (nearestAisleName, nearestAisleDistance) = GetClosestAisleName(itemDetector.detectedAisleNames);

        // Next in the list
        if (shoppingList != null && shoppingList.Count > 0)
        {
            nextListAisleName = shoppingList[0];
        }
        else
        {
            nextListAisleName = "Nill";
        }
        nextListAisleDistance = GetNavMeshPathDistance(transform.position, aisleManager.GetAisleLocation(nextListAisleName));

        // Closest of List
        (nearestListAisleName, nearestListAisleDistance) = GetClosestAisleName(shoppingList);
    }
    private void MoveToAisle(string aisleName)
    {
        // Sets agent destination to a random point on the "aisleName" Aisle
        if (aisleName == "Nill" || aisleName == "Empty" || aisleName == "" || aisleName == null)
        {
            //Debug.Log("Can't move to " +aisleName+" Aisle");
            //TransitionToState(ShopperState.Checkout);
            navAgent.isStopped = false;
        }
        else
        {
            Vector3 targetLocation = aisleManager.GetAisleLocation(aisleName);
            navAgent.SetDestination(targetLocation);
            currentAisle = aisleName;// Update the current aisle
            currentAisleTag = aisleManager.GetAisleTagIndex(currentAisle);
            currentItemInList = CheckIfAisleInList();
            navAgent.isStopped = false;
        }
    }
    private int CheckIfAisleInList()
    {
        if (shoppingList.Contains(currentAisle))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    private IEnumerator BuyCoroutine()
    {
        // Wait for a random time in seconds before buying.
        int waitTime = Random.Range(8, 12);
        yield return new WaitForSeconds(waitTime);

        totalItemsBought++;
        // Update count based on teh aisle tag
        switch (currentAisleTag)
        {
            default:
            case 0:// Fresh
                boughtFromFresh++;
                break;
            case 1:// Essential
                boughtFromEssentials++;
                break;
            case 2:// Other
                boughtFromOthers++;
                break;
            case 3:// Offer
                boughtFromOffers++;
                break;
        }

        // If the item is present in the shopping list, remove it.
        if (shoppingList.Contains(currentAisle))
        {
            shoppingList.Remove(currentAisle);
        }

        aisleManager.Record(currentAisle, 0);

        currentAisle = "Done"; // Aisle visited.
        isPerformingAction = false;
        //Debug.Log("Item Bought!");
    }

    private IEnumerator BrowseCoroutine()
    {
        // Wait for a random time in seconds for Browse.
        int waitTime = Random.Range(5, 8);
        yield return new WaitForSeconds(waitTime);

        totalItemsBrowsed++;

        // If the item is present in the shopping list, remove it.
        /*
        if (shoppingList.Contains(currentAisle))
        {
            shoppingList.Remove(currentAisle);
        }
        */

        aisleManager.Record(currentAisle, 1);

        currentAisle = "Done"; // Aisle visited.
        isPerformingAction = false;
        //Debug.Log("Item Browsed!");
    }

    private IEnumerator IgnoreCoroutine()
    {
        // Wait for a 0 seconds for ignoring.
        int waitTime = 0;
        yield return new WaitForSeconds(waitTime);

        aisleManager.Record(currentAisle, 2);

        currentAisle = "Done"; // Aisle visited.
        isPerformingAction = false;
        //Debug.Log("Item Ignored!");
    }

    // Update the observation variables that are required to make decisions
    private void UpdateObservationStructs() // !!!This method will be called EVERY FRAME!!!
    {
        // Local _Observation variable decleration using global value
        int timeSpent_Obs = ((int)timeSpent);
        float shoppingLeft = (float)shoppingList.Count / listCountAtStart;// fraction of shopping lift to do

        // updating struct values using local values
        observations.TimeSpent = timeSpent_Obs;
        observations.ShoppingProgress = 1 - shoppingLeft;
        observations.ShoppingListCount = listCountAtStart;
        observations.itemInTheList = currentItemInList;

        observations.TotalItemsBought = totalItemsBought;
        observations.TotalFreshItemsBought = boughtFromFresh;
        observations.TotalEssentialItemsBought = boughtFromEssentials;
        observations.TotalOtherItemsBought = boughtFromOthers;
        observations.TotalOfferItemsBought = boughtFromOffers;

        observations.TotalItemsBrowsed = totalItemsBrowsed;
        observations.CurrentAisleTag = currentAisleTag;

        observations.DistanceToNearestAisle = nearestAisleDistance;
        observations.DistanceToNearestInList = nearestListAisleDistance;
        observations.DistanceToNextAisle = nextListAisleDistance;

        observations.DistractingAsileTag = distractingAisleTag;
    }
    #endregion

    // Checkout state: Process checkout behavior.
    private void CheckoutState()
    {
        if (!isPerformingAction)
        {
            if (!movedToCheckoutArea)
            {
                navAgent.SetDestination(superMarketManager.GetCheckoutAreaPosition());
                movedToCheckoutArea = true;
            }
            if (movedToCheckoutArea && !navAgent.pathPending && navAgent.remainingDistance <= stoppingDistanceThreshold)
            {
                // If no checkout station is assigned, ask the manager.
                if (currentCheckoutStation == null)
                {
                    currentCheckoutStation = superMarketManager.GetLeastBusyCheckoutStation(transform.position);
                    if (currentCheckoutStation != null)
                    {
                        //Debug.Log("Selected checkout station: " + currentCheckoutStation.name);
                    }
                    else
                    {
                        //Debug.LogWarning("No checkout station available!");
                        return;
                    }
                }
                // Set the destination only once.
                if (!checkoutDestinationSet)
                {
                    Vector3 waitingLocation = currentCheckoutStation.GetWaitingLocation();
                    navAgent.SetDestination(waitingLocation);
                    checkoutDestinationSet = true;
                    //Debug.Log("Setting checkout destination to waiting location: " + waitingLocation);
                }
                // Check if the agent has reached the waiting location.
                if (!navAgent.pathPending && navAgent.remainingDistance <= stoppingDistanceThreshold)
                {
                    if (!hasBeenAddedToQueue)
                    {
                        currentCheckoutStation.AddShopper(gameObject);
                        //Debug.Log("Shopper " + gameObject.name + " added to checkout queue at " + currentCheckoutStation.name);
                        hasBeenAddedToQueue = true;
                    }
                }
            }
        }
    }

    // Exit state: Shopper leaves the supermarket and is destroyed upon reaching the exit.
    private void ExitState()
    {
        //Debug.Log("Exiting supermarket.");
        navAgent.SetDestination(superMarketManager.GetExitPosition());

        // Check if the agent has reached the exit location.
        if (!navAgent.pathPending && navAgent.remainingDistance <= stoppingDistanceThreshold)
        {
            //Debug.Log("Shopper " + gameObject.name + " has reached the exit and will be destroyed.");
            spawner.ShopperExited(shopperType);

            superMarketManager.heatmapDataSO.AddPositionList(shopperType, recordedPositions);

            Destroy(gameObject);
        }
    }
    #endregion
}