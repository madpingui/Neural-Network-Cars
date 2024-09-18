using System;
using UnityEngine;

public class CarAgent : MonoBehaviour
{
    // Settings for the sensors, mutation behavior, and fuel management
    [Header("Sensor Settings")]
    [SerializeField] private float viewDistance = 10f; // Max distance the raycast can sense
    [SerializeField] private float raycastTotalAngle = 180f; // Angle span for all raycasts
    [SerializeField] private int numberOfRaycasts = 5; // Total number of raycasts
    [SerializeField] private Material lineMaterial; // Material for the LineRenderer (set in Inspector)

    [Header("Mutation Settings")]
    [SerializeField] private float mutationAmount = 0.1f; // Mutation intensity for NN weights
    [SerializeField] private float mutationChance = 0.2f; // Probability of a mutation occurring

    [Header("Fuel Settings")]
    [SerializeField] private float initialFuel = 10f; // Initial fuel amount for the agent
    [SerializeField] private float fuelGainedPerCheckpoint = 3f; // Fuel added upon checkpoint pass
    [SerializeField] private float fuelConsumptionRate = 1f; // Rate at which fuel is consumed

    [Header("Idle Destruction Settings")]
    [SerializeField] private float idleSpeedThreshold = 0.1f; // Speed below which the car is considered idle
    [SerializeField] private float idleTimeLimit = 1f; // Time limit for how long the car can remain idle

    public static event Action<CarAgent> OnCarDestroyed; // Event triggered when a car is destroyed

    private float idleTime = 0f; // Tracks how long the car has been idle

    private float fuel;
    private float elapsedTime = 0f; // Tracks time passed for fuel consumption
    private float[] distances; // Stores distances from raycasts
    private PrometeoCarController carController; // Reference to the car controller script
    private bool isDead = false; // Tracks whether the car is destroyed
    private int currentCheckpointIndex = 0;

    private NeuralNetwork neuralNetwork; // Neural Network instance
    public int CorrectCheckpointsPassed { get; private set; } // Checkpoints passed correctly

    private Vector3 lastPosition;
    private LineRenderer[] lineRenderers; // LineRenderer array for ray visuals

    // Initialize agent with fuel and neural network
    private void Awake()
    {
        fuel = initialFuel;
        neuralNetwork = new NeuralNetwork(numberOfRaycasts, 4, 3, 2); // Neural network with 3 hidden layers
        carController = GetComponent<PrometeoCarController>();
        distances = new float[numberOfRaycasts]; // Array for raycast distances
        lastPosition = transform.position; // Initialize position

        // Initialize LineRenderers for visualizing the rays
        lineRenderers = new LineRenderer[numberOfRaycasts];
        for (int i = 0; i < numberOfRaycasts; i++)
        {
            GameObject lineObj = new GameObject($"LineRenderer_{i}");
            lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2; // Start and end points
            lineRenderers[i].startWidth = 0.2f;
            lineRenderers[i].endWidth = 0.2f;
            lineRenderers[i].material = lineMaterial; // Assign material (set it in Inspector)
            lineRenderers[i].transform.parent = transform; // Set as child of the car for organization
        }
    }

    // Handles collisions (e.g., with walls)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            DestroyAgent(); // Destroy car when hitting a wall
        }
    }

    // Destroys the car and invokes the destruction event
    private void DestroyAgent()
    {
        if (isDead) return;

        isDead = true;
        OnCarDestroyed?.Invoke(this); // Invoke destruction event
        Destroy(gameObject); // Destroy the car object
    }

    // Called every physics update
    private void FixedUpdate()
    {
        if (isDead) return;

        ManageFuel(); // Handle fuel consumption
        PerformSensorReadings(); // Gather sensor (raycast) data
        MoveBasedOnNeuralNetworkOutput(); // Move car using NN's decision
        CheckIfIdle(); // Check if the car is idle
    }

    // Initializes this agent's neural network from a source network
    public void InitializeNeuralNetwork(NeuralNetwork sourceNetwork)
    {
        neuralNetwork = sourceNetwork.DeepCopy(); // Create a deep copy of the source network
    }

    // Returns a copy of this agent's neural network
    public NeuralNetwork GetNeuralNetworkCopy()
    {
        return neuralNetwork.DeepCopy(); // Return deep copy of the current neural network
    }

    // Manage fuel consumption over time and destroy the agent if fuel runs out
    private void ManageFuel()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 1f)
        {
            elapsedTime = elapsedTime % 1f;
            fuel -= fuelConsumptionRate; // Decrease fuel based on consumption rate
        }

        if (fuel <= 0 || transform.position.y < -10) // Check if car falls off map
        {
            DestroyAgent(); // Destroy the car if fuel is depleted
        }
    }

    // Uses raycasts to get distances from obstacles and walls
    private void PerformSensorReadings()
    {
        float angleBetweenRaycasts = raycastTotalAngle / (numberOfRaycasts - 1);

        for (int i = 0; i < numberOfRaycasts; i++)
        {
            float angle = -(raycastTotalAngle / 2) + (i * angleBetweenRaycasts);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            Vector3 rayStart = (transform.position + Vector3.up * 0.3f) + (transform.forward * 2.5f);

            if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, viewDistance))
            {
                distances[i] = hit.transform.CompareTag("Wall") ? hit.distance / viewDistance : 1f;
                Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);

                // Update LineRenderer to match the raycast
                lineRenderers[i].SetPosition(0, rayStart);
                lineRenderers[i].SetPosition(1, hit.point);
                lineRenderers[i].startColor = Color.red;
                lineRenderers[i].endColor = Color.red;
            }
            else
            {
                distances[i] = 1f;
                Debug.DrawRay(rayStart, rayDirection * viewDistance, Color.blue);

                // Update LineRenderer for a full-length ray
                lineRenderers[i].SetPosition(0, rayStart);
                lineRenderers[i].SetPosition(1, rayStart + rayDirection * viewDistance);
                lineRenderers[i].startColor = Color.green;
                lineRenderers[i].endColor = Color.green;
            }
        }
    }

    // Process sensor inputs through the neural network and move the car accordingly
    private void MoveBasedOnNeuralNetworkOutput()
    {
        float[] outputsFromNN = neuralNetwork.ProcessInputs(distances); // Feed sensor inputs to NN
        float forwardBackward = outputsFromNN[0]; // Output 1: forward/backward movement
        float leftRight = outputsFromNN[1]; // Output 2: left/right steering

        carController.Move(forwardBackward, leftRight); // Move car based on NN outputs
    }

    private void CheckIfIdle()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position; // Update position for the next check

        // If the car is moving slower than the idle speed threshold
        if (distanceMoved < idleSpeedThreshold)
        {
            idleTime += Time.deltaTime;

            // If the car has been idle for too long, destroy it
            if (idleTime >= idleTimeLimit)
            {
                DestroyAgent();
            }
        }
        else
        {
            idleTime = 0f; // Reset idle time if the car is moving
        }
    }

    // Applies mutation to the car's neural network
    public void Mutate()
    {
        mutationChance = Mathf.Clamp(mutationChance, 0f, 1f);
        neuralNetwork.Mutate(mutationChance, mutationAmount); // Mutate NN based on chance
    }

    // Validates a checkpoint pass, updates checkpoint index, and adjusts fuel accordingly
    public void ValidateCheckpoint(int checkpointIndex, bool isLastCheckpoint)
    {
        if (checkpointIndex == currentCheckpointIndex)
        {
            currentCheckpointIndex++;
            CorrectCheckpointsPassed++;
            fuel += fuelGainedPerCheckpoint * 1.5f; // Reward fuel for correct checkpoint

            if (isLastCheckpoint)
            {
                currentCheckpointIndex = 0;
            }
        }
        else
        {
            fuel -= fuelGainedPerCheckpoint * 0.5f; // Penalize fuel for incorrect checkpoint
        }
    }
}
