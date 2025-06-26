using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{          
    [Header("Agent Prefabs (Bodies)")]
    [Tooltip("List of two body prefabs to choose from randomly")]
    public GameObject[] agentPrefabs; // Ensure this array has exactly 2 elements

    [Header("Brain Components")]
    public DicisionTracker tracker;
    [Tooltip("Reference to the Goal-Oriented Shopper Brain")]
    [SerializeField] GameObject goalOrientedBrainGameObject;
    private IShopperBrain goalOrientedBrain;
    [Tooltip("Reference to the Impulse Shopper Brain")]
    [SerializeField] GameObject impulseBrainGameObject;
    private IShopperBrain impulseBrain;
    [Tooltip("Reference to the Wanderer Shopper Brain")]
    [SerializeField] GameObject wandererBrainGameObject;
    private IShopperBrain wandererBrain;

    [Header("Spawning Weights (Relative Probabilities)")]
    [Tooltip("Relative weight for spawning Goal-Oriented agents")]
    public float weightGoalOriented = 0.33f;
    [Tooltip("Relative weight for spawning Impulse agents")]
    public float weightImpulse = 0.33f;
    [Tooltip("Relative weight for spawning Wanderer agents")]
    public float weightWanderer = 0.34f;

    [Header("Agent Settings")]
    [Tooltip("Desired number of agents to have spawned")]
    [Range(1,75)]
    public int agentCount = 10;
    [Space(10)]
    public Color  goalColor;
    public Color  impulseColor;
    public Color  wandererColor;

    [Header("Spawn Area Bounds")]
    [Tooltip("Lower bound of the spawn area (inclusive)")]
    public Vector3 spawnAreaMin;
    [Tooltip("Upper bound of the spawn area (inclusive)")]
    public Vector3 spawnAreaMax;

    [Header("Parent Object")]
    [Tooltip("Parent GameObject under which spawned agents are organized")]
    public Transform agentsParent;

    // Counters to track the number of each agent type spawned.
    [Header("Spawn Counters")]
    [SerializeField] private int spawnedGoalOriented = 0;
    [SerializeField] private int spawnedImpulse = 0;
    [SerializeField] private int spawnedWanderer = 0;

    // Counters to track the number of each agent type exited.
    [Header("Exit Counters")]
    [SerializeField] private int exitedGoalOriented = 0;
    [SerializeField] private int exitedImpulse = 0;
    [SerializeField] private int exitedWanderer = 0;

    private void Start()
    {
        // Get brains from the brainsGameObject.
        goalOrientedBrain = goalOrientedBrainGameObject.GetComponent<GoalShopperBrain>();
        impulseBrain = impulseBrainGameObject.GetComponent<ImpulseShopperBrain>();
        wandererBrain = wandererBrainGameObject.GetComponent<WandererShopperBrain>();

        SpawnInitialAgents();
    }

    private void Update()
    {
        // Continuously check if the number of spawned agents is less than desired.
        int currentCount = agentsParent.childCount;
        if (currentCount < agentCount)
        {
            int missing = agentCount - currentCount;
            for (int i = 0; i < missing; i++)
            {
                SpawnAgent();
            }
        }
        // Optional: Despawn extras if more than desired.
        else if (currentCount > agentCount)
        {
            int excess = currentCount - agentCount;
            for (int i = 0; i < excess; i++)
            {
                Destroy(agentsParent.GetChild(0).gameObject);
            }
        }
    }

    private void SpawnInitialAgents()
    {
        for (int i = 0; i < agentCount; i++)
        {
            SpawnAgent();
        }
    }

    private void SpawnAgent()
    {
        // Choose a random spawn position within the bounds.
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );

        // Randomly select one of the two prefabs.
        int prefabIndex = Random.Range(0, agentPrefabs.Length);
        GameObject agent = Instantiate(agentPrefabs[prefabIndex], spawnPos, Quaternion.identity, agentsParent);

        // Get the ShopperAgentController from the spawned agent.
        ShopperAgentController shopperController = agent.GetComponent<ShopperAgentController>();

        // Assign common references.
        shopperController.aisleManager = FindObjectOfType<AisleManager>();
        shopperController.superMarketManager = FindObjectOfType<SuperMarketManager>();
        shopperController.spawner = this;

        // Assign color
        Material agentMat = agent.GetComponentsInChildren<Renderer>()[1].material;

        // Select a brain based on weighted probabilities.
        (shopperController.brain,shopperController.shopperType, agentMat.color) = ChooseBrain();
    }

    // Chooses a brain based on the relative weight factors and updates spawn counters.
    private (IShopperBrain,string, Color) ChooseBrain()
    {
        float totalWeight = weightGoalOriented + weightImpulse + weightWanderer;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < weightGoalOriented)
        {
            spawnedGoalOriented++;
            return (goalOrientedBrain, "goalOriented", goalColor);
        }
        else if (randomValue < weightGoalOriented + weightImpulse)
        {
            spawnedImpulse++;
            return (impulseBrain, "impulse", impulseColor);
        }
        else
        {
            spawnedWanderer++;
            return (wandererBrain, "wanderer", wandererColor);
        }
    }

    public void ShopperExited(string type)
    {
        switch (type)
        {
            default:
                break;
            case "goalOriented":
                exitedGoalOriented++;
                break;      
            case "impulse":
                exitedImpulse++;
                break;      
            case "wanderer":
                exitedWanderer++;
                break;
        }
        /* OLD EndEpisode WAY
        // when the Episode target is reached for one type of shopper, end episode for that shopper
        if (exitedGoalOriented >= goalOrientedBrainGameObject.GetComponent<GoalShopperBrain>().shopperCountToEndEpisode)
        {
            goalOrientedBrain.CanEndEpisode();
            exitedGoalOriented = 0;
        }
        
        if (exitedImpulse >= impulseBrainGameObject.GetComponent<ImpulseShopperBrain>().shopperCountToEndEpisode)
        {
            impulseBrain.CanEndEpisode();
            exitedImpulse = 0;
        }
        
        if (exitedWanderer >= wandererBrainGameObject.GetComponent<WandererShopperBrain>().shopperCountToEndEpisode)
        {
            wandererBrain.CanEndEpisode();
            exitedWanderer = 0;
        }
        */
    }       
}
