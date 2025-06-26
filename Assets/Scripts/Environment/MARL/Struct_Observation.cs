using System;

[Serializable]
public struct DecisionQueueItem
{
    public CombinedObservations observations;
    public ShopperAgentController controller;
    public int decisionType; // 0: Purchase, 1: Navigation, 2: Distraction

    public DecisionQueueItem(CombinedObservations obs, ShopperAgentController ctrl, int type)
    {
        observations = obs;
        controller = ctrl;
        decisionType = type;
    }
}

public struct CombinedObservations
{
    /// <summary>
    /// Total time (in seconds) the shopper has spent in the store.
    /// </summary>
    public int TimeSpent;

    /// <summary>
    /// No of items in the list.
    /// </summary>
    public int ShoppingListCount;

    /// <summary>
    /// A measure of urgency or progress (for example, a normalized value representing 
    /// the fraction of the shopping list completed).
    /// </summary>
    public float ShoppingProgress; 

    /// <summary>
    /// Is the current aisle in the list
    /// </summary>
    public int itemInTheList;

    /// <summary>
    /// Total number of items already purchased.
    /// </summary>
    public int TotalItemsBought;    

    /// <summary>
    /// Total number of items browsed.
    /// </summary>
    public int TotalItemsBrowsed;

    /// <summary>
    /// Total number of Fresh items purchased.
    /// </summary>
    public int TotalFreshItemsBought; 

    /// <summary>
    /// Total number of Essential items purchased.
    /// </summary>
    public int TotalEssentialItemsBought; 

    /// <summary>
    /// Total number of Other items purchased.
    /// </summary>
    public int TotalOtherItemsBought; 

    /// <summary>
    /// Total number of Offer items purchased.
    /// </summary>
    public int TotalOfferItemsBought;   

    /// <summary>
    /// Encoded value for the current aisle's tag.
    /// For example: 0 = Aisle_Fresh, 1 = Aisle_Essentials, etc.
    /// </summary>
    public int CurrentAisleTag;

    /// <summary>
    /// Encoded value for the aisle's tag, which may indicate attractiveness.
    /// For example: 0 = Aisle_Fresh, 1 = Aisle_Essentials, etc.
    /// </summary>
    public int DistractingAsileTag;

    /// <summary>
    /// Distance (using NavMesh path length) to the nearest aisle available in the environment.
    /// </summary>
    public float DistanceToNearestAisle;

    /// <summary>
    /// Distance to the nearest aisle from the shopping list.
    /// </summary>
    public float DistanceToNearestInList;

    /// <summary>
    /// Distance to the next aisle in the shopping list.
    /// </summary>
    public float DistanceToNextAisle;
}
