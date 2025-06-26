using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Agent_MoveToExit : Agent
{
    [SerializeField] private Transform targetTransform;// Where the agent should go
    [SerializeField] private AgentMovementController agentMovementController;
    [Space(5)]
    [SerializeField] Material winMaterial;
    [SerializeField] Material looseMaterial;
    [SerializeField] MeshRenderer floorMeshRenderer;

    public override void OnEpisodeBegin()
    {
        agentMovementController.agentTransform.localPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        targetTransform.localPosition = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentMovementController.agentTransform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        agentMovementController.Move(new Vector3(moveX, 0, moveZ));
        //base.OnActionReceived(actions);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        Vector2 moveInput = agentMovementController.inputActions.Land.Movement.ReadValue<Vector2>();
        continuousActions[0] = moveInput.x;// X
        continuousActions[1] = moveInput.y;// Z
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenderer.material = looseMaterial;
            EndEpisode();
        }
    }
}
