using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    public bool mutateMutations = true;
    public bool isUser = false;
    public float viewDistance = 30;
    private float energy;
    private float energyGained;
    [HideInInspector] public float mutationAmount = 0.01f;
    [HideInInspector] public float mutationChance = 0.2f;

    public static Action<Creature> OnCreatureDead;

    public float rayCastTotalAngle = 180f;

    private bool isMutated = false;
    private float elapsed = 0f;
    private float FB = 0;
    private float LR = 0;
    private float[] distances = new float[NumberOfRaycasts];
    private NN nn;
    private PrometeoCarController movement;
    private const int NumberOfRaycasts = 5;

    // Start is called before the first frame update
    void Awake()
    {
        energy = 10;
        energyGained = 3;
        nn = gameObject.GetComponent<NN>();
        movement = gameObject.GetComponent<PrometeoCarController>();
        distances = new float[NumberOfRaycasts]; // Set up an array to store the distances to the food objects detected by the raycasts
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //only do this once
        if(!isMutated)
        {
            //call mutate function to mutate the neural network
            MutateCreature();
            isMutated = true;
        }

        ManageEnergy();

        // This section of code is for the new food detection system (Raycasts)
        // Set up a variable to store the angle between raycasts
        float angleBetweenRaycasts = rayCastTotalAngle / (NumberOfRaycasts - 1);

        // Use multiple raycasts to detect food objects
        RaycastHit hit;
        for (int i = 0; i < NumberOfRaycasts; i++)
        {
            float angle = -(rayCastTotalAngle / 2) + (i * angleBetweenRaycasts);
            // Rotate the direction of the raycast by the specified angle around the y-axis of the agent
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            // Increase the starting point of the raycast by 0.1 units
            Vector3 rayStart = (transform.position + Vector3.up * 0.3f) + (transform.forward * 3f);
            if (Physics.Raycast(rayStart, rayDirection, out hit, viewDistance))
            {
                if (hit.transform.gameObject.CompareTag("Enemy"))
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                    // Use the length of the raycast as the distance to the food object
                    distances[i] = hit.distance / viewDistance;
                }
                else
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);
                    // If no food object is detected, set the distance to the maximum length of the raycast
                    distances[i] = 1;
                }
            }
            else
            {
                // Draw a line representing the raycast in the scene view for debugging purposes
                Debug.DrawRay(rayStart, rayDirection * viewDistance, Color.blue);
                // If no food object is detected, set the distance to the maximum length of the raycast
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

        //if the agent is the user, use the inputs from the user instead of the neural network
        if (isUser)
        {
            FB = Input.GetAxis("Vertical");
            LR = Input.GetAxis("Horizontal")/10;
        }

        //Move the agent using the move function
        movement.Move(FB, LR);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        energy -= 1;
    //    }
    //}

    public void ManageEnergy()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 1f)
        {
            elapsed = elapsed % 1f;

            //subtract 1 energy per second
            energy -= 1f;
        }

        //Starve
        float agentY = this.transform.position.y;
        if (energy <= 0 || agentY < -10)
        {
            OnCreatureDead?.Invoke(this);
            Destroy(this.gameObject);
        }
    }

    private void MutateCreature()
    {
        if (mutateMutations)
        {
            mutationAmount += Random.Range(-0.01f, 0.01f);
            mutationChance += Random.Range(-0.01f, 0.01f);
        }

        // Limit mutation amount and chance within a reasonable range
        mutationAmount = Mathf.Clamp(mutationAmount, 0f, 0.1f);
        mutationChance = Mathf.Clamp(mutationChance, 0f, 0.1f);

        nn.MutateNetwork(mutationChance, mutationAmount);
    }

    private int currentCheckpointIndex = 0;
    [HideInInspector] public int amountOfCorrectCheckpoints = 0;

    public void ValidateCheckpoint(int checkpointIndex, bool isLastChekpoint)
    {
        if (checkpointIndex == currentCheckpointIndex)
        {
            // Update to the next checkpoint
            currentCheckpointIndex++;
            amountOfCorrectCheckpoints++;
            energy += energyGained * 1.5f;  // Reward energy for correct checkpoint

            if (isLastChekpoint)
                currentCheckpointIndex = 0;
        }
        else
        {
            energy -= energyGained * 0.5f;  // Smaller penalty for incorrect checkpoint
        }
    }
}
