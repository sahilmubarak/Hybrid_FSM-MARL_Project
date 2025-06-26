using UnityEngine;

public class DicisionTracker : MonoBehaviour
{
    public static DicisionTracker Instance { get; private set; }
    public bool showConsoleLogs = false;

    // Goal-Oriented Brain decision counts
    [Header("Goal-Oriented Brain decision counts")]
    public int goalPurchaseDecisions;
    public int goalNavigationDecisions;
    public int goalDistractionDecisions;

    // Impulse Shopper Brain decision counts     
    [Header("Impulse Shopper Brain decision counts")]
    public int impulsePurchaseDecisions;
    public int impulseNavigationDecisions;
    public int impulseDistractionDecisions;

    // Wanderer Brain decision counts         
    [Header("Wanderer Brain decision counts")]
    public int wandererPurchaseDecisions;
    public int wandererNavigationDecisions;
    public int wandererDistractionDecisions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TrackGoalDecision(string decisionCategory, int decision)
    {
        if (decisionCategory == "Purchase")
        {
            goalPurchaseDecisions++;
            if (showConsoleLogs) Debug.Log($"[Goal-Oriented] Purchase Decision: {DecisionName(decision, "Purchase")}");
        }
        else if (decisionCategory == "Navigation")
        {
            goalNavigationDecisions++;
            if (showConsoleLogs) Debug.Log($"[Goal-Oriented] Navigation Decision: {DecisionName(decision, "Navigation")}");
        }
        else if (decisionCategory == "Distraction")
        {
            goalDistractionDecisions++;
            if (showConsoleLogs) Debug.Log($"[Goal-Oriented] Distraction Decision: {DecisionName(decision, "Distraction")}");
        }
    }

    public void TrackImpulseDecision(string decisionCategory, int decision)
    {
        if (decisionCategory == "Purchase")
        {
            impulsePurchaseDecisions++;
            if (showConsoleLogs) Debug.Log($"[Impulse] Purchase Decision: {DecisionName(decision, "Purchase")}");
        }
        else if (decisionCategory == "Navigation")
        {
            impulseNavigationDecisions++;
            if (showConsoleLogs) Debug.Log($"[Impulse] Navigation Decision: {DecisionName(decision, "Navigation")}");
        }
        else if (decisionCategory == "Distraction")
        {
            impulseDistractionDecisions++;
            if (showConsoleLogs) Debug.Log($"[Impulse] Distraction Decision: {DecisionName(decision, "Distraction")}");
        }
    }

    public void TrackWandererDecision(string decisionCategory, int decision)
    {
        if (decisionCategory == "Purchase")
        {
            wandererPurchaseDecisions++;
            if (showConsoleLogs) Debug.Log($"[Wanderer] Purchase Decision: {DecisionName(decision, "Purchase")}");
        }
        else if (decisionCategory == "Navigation")
        {
            wandererNavigationDecisions++;
            if (showConsoleLogs) Debug.Log($"[Wanderer] Navigation Decision: {DecisionName(decision, "Navigation")}");
        }
        else if (decisionCategory == "Distraction")
        {
            wandererDistractionDecisions++;
            if (showConsoleLogs) Debug.Log($"[Wanderer] Distraction Decision: {DecisionName(decision, "Distraction")}");
        }
    }

    private string DecisionName(int decision, string category)
    {
        switch (category)
        {
            case "Purchase":
                return decision == 0 ? "Buy" : decision == 1 ? "Browse" : decision == 2 ? "Ignore" : decision.ToString();

            case "Navigation":
                return decision == 0 ? "Nearest Aisle" :
                       decision == 1 ? "Nearest Aisle in List" :
                       decision == 2 ? "Next Aisle in List" :
                       decision == 3 ? "Checkout"
                        : decision.ToString();

            case "Distraction":
                return decision == 0 ? "Ignore Distraction" : decision == 1 ? "Go to Distraction" : decision.ToString();

            default:
                return "Unknown Decision";
        }
    }
}
