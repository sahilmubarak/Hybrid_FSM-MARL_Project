using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovementController : MonoBehaviour
{
    [HideInInspector] public InputActions inputActions; // For getting inputs from the User

    public Transform agentTransform;
    [SerializeField] private Animator agentAnimator;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float animationDampTime = 0.1f; // Damping time for smooth animation transitions

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    /*Controlled by hurestic in ML Agent
    void Update()
    {
        // Example of using the Move() function directly within Update:
        Vector2 moveInput = inputActions.Land.Movement.ReadValue<Vector2>();
        Move(moveInput);
    }
    */ 

    /// <summary>
    /// Moves and rotates the agent based on the input vector.
    /// </summary>
    /// <param name="moveInput">A Vector3 representing movement direction on the X and Z axes.</param>
    public void Move(Vector3 moveInput)
    {
        Debug.Log("moveInput: " + moveInput);
        Vector3 moveDirection = moveInput.normalized;
        float movementMagnitude = moveInput.magnitude;

        // Smoothly update the "Movement" parameter in the animator using damping
        agentAnimator.SetFloat("Movement", movementMagnitude, animationDampTime, Time.deltaTime);

        if (movementMagnitude > 0.1f)
        {
            // Move the agent smoothly
            agentTransform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Rotate smoothly to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            agentTransform.rotation = Quaternion.Slerp(agentTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
