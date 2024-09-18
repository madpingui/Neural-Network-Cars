using System;
using UnityEngine;

public class CarAgent : MonoBehaviour
{
    public float viewDistance = 10;
    public float rayCastTotalAngle = 180f;
    [HideInInspector] public float mutationAmount = 0.1f;
    [HideInInspector] public float mutationChance = 0.2f;

    public static Action<CarAgent> OnCarBroken;

    private float fuel;
    private float fuelGained;
    private float elapsed = 0f;
    private float FB = 0;
    private float LR = 0;
    private float[] distances = new float[NumberOfRaycasts];
    private PrometeoCarController CarController;
    private const int NumberOfRaycasts = 5;
    private bool isDead = false;

    public NN nn;
    
    void Awake()
    {
        fuel = 10;
        fuelGained = 3;
        nn = new NN();
        CarController = gameObject.GetComponent<PrometeoCarController>();
        distances = new float[NumberOfRaycasts]; // Set up an array to store the distances to the walls detected by the raycasts
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            DestroyAgent();
        }
    }

    private void DestroyAgent()
    {
        if (isDead) return; // Prevent multiple calls

        isDead = true;
        OnCarBroken?.Invoke(this);
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        ManageFuel();

        // Set up a variable to store the angle between raycasts
        float angleBetweenRaycasts = rayCastTotalAngle / (NumberOfRaycasts - 1);

        // Use multiple raycasts to detect walls
        RaycastHit hit;
        for (int i = 0; i < NumberOfRaycasts; i++)
        {
            float angle = -(rayCastTotalAngle / 2) + (i * angleBetweenRaycasts);
            // Rotate the direction of the raycast by the specified angle around the y-axis of the agent
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            // Increase the starting point of the raycast by 0.3 units
            Vector3 rayStart = (transform.position + Vector3.up * 0.3f) + (transform.forward * 2.5f);
            if (Physics.Raycast(rayStart, rayDirection, out hit, viewDistance))
            {
                if (hit.transform.gameObject.CompareTag("Wall"))
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                    // Use the length of the raycast as the distance to the wall
                    distances[i] = hit.distance / viewDistance;
                }
                else
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);
                    // If no wall is detected, set the distance to the maximum length of the raycast
                    distances[i] = 1;
                }
            }
            else
            {
                // Draw a line representing the raycast in the scene view for debugging purposes
                Debug.DrawRay(rayStart, rayDirection * viewDistance, Color.blue);
                // If no wall is detected, set the distance to the maximum length of the raycast
                distances[i] = 1;
            }
        }

        // Setup inputs for the neural network
        float [] inputsToNN = distances;

        // Get outputs from the neural network
        float [] outputsFromNN = nn.Brain(inputsToNN);

        //Store the outputs from the neural network in variables
        FB = outputsFromNN[0];
        LR = outputsFromNN[1];

        //Move the car using the outputs
        CarController.Move(FB, LR);
    }

    public void ManageFuel()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 1f)
        {
            elapsed = elapsed % 1f;

            //subtract 1 per second
            fuel -= 1f;
        }

        float agentY = this.transform.position.y;
        if (fuel <= 0 || agentY < -10)
        {
            DestroyAgent();
        }
    }

    public void Mutate()
    {
        mutationChance = Mathf.Clamp(mutationChance, 0f, 1f);

        if(mutationChance > 0)
            nn.MutateNetwork(mutationChance, mutationAmount);
    }

    //keep track of checkpoints passed and the total amount of correct checkpoints
    private int currentCheckpointIndex = 0;
    [HideInInspector] public int amountOfCorrectCheckpoints = 0;

    public void ValidateCheckpoint(int checkpointIndex, bool isLastChekpoint)
    {
        if (checkpointIndex == currentCheckpointIndex)
        {
            // Update to the next checkpoint
            currentCheckpointIndex++;
            amountOfCorrectCheckpoints++;
            fuel += fuelGained * 1.5f;  // Reward for correct checkpoint

            if (isLastChekpoint)
                currentCheckpointIndex = 0;
        }
        else
        {
            fuel -= fuelGained * 0.5f;  // Smaller penalty for incorrect checkpoint
        }
    }
}
