using UnityEngine;

public class Creature : MonoBehaviour
{
    public bool mutateMutations = true;
    public GameObject creaturePrefab;
    public bool isUser = false;
    public float viewDistance = 20;
    public float energy = 20;
    public float energyGained = 10;
    public float reproductionEnergyGained = 1;
    public float reproductionEnergy = 0;
    public float reproductionEnergyThreshold = 10;
    public int numberOfChildren = 1;
    public float mutationAmount = 0.8f;
    public float mutationChance = 0.2f; 


    private bool isMutated = false;
    private float elapsed = 0f;
    private float FB = 0;
    private float LR = 0;
    private float[] distances = new float[6];
    private NN nn;
    private Movement movement;

    public bool isDead = false;

    // Start is called before the first frame update
    void Awake()
    {
        nn = gameObject.GetComponent<NN>();
        movement = gameObject.GetComponent<Movement>();
        distances = new float[6]; // Set up an array to store the distances to the food objects detected by the raycasts
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
            energy = 20;
        }

        ManageEnergy();

        // This section of code is for the new food detection system (Raycasts)
        // Set up a variable to store the number of raycasts to use
        int numRaycasts = 5;

        // Set up a variable to store the angle between raycasts
        float angleBetweenRaycasts = 30;

        // Use multiple raycasts to detect food objects
        RaycastHit hit;
        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = (i - (numRaycasts - 1) / 2f) * angleBetweenRaycasts;
            // Rotate the direction of the raycast by the specified angle around the y-axis of the agent
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward * -1;
            // Increase the starting point of the raycast by 0.1 units
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayStart, rayDirection, out hit, viewDistance))
            {
                if (hit.transform.gameObject.CompareTag("Food"))
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                    // Use the length of the raycast as the distance to the food object
                    distances[i] = hit.distance/viewDistance;
                }
                else
                {
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.green);
                    // If no food object is detected, set the distance to the maximum length of the raycast
                    distances[i] = 1;
                }
            }
            else
            {
                // Draw a line representing the raycast in the scene view for debugging purposes
                Debug.DrawRay(rayStart, rayDirection * viewDistance, Color.green);
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

    //this function gets called whenever the agent collides with a trigger. (Which in this case is the food)
    void OnTriggerEnter(Collider col)
    {
        //if the agent collides with a food object, it will eat it and gain energy.
        if (col.gameObject.CompareTag("Food"))
        {
            energy += energyGained;
            reproductionEnergy += reproductionEnergyGained;
            Destroy(col.gameObject);
        }
    }

    public void ManageEnergy()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 1f)
        {
            elapsed = elapsed % 1f;

            //subtract 1 energy per second
            energy -= 1f;

            //if agent has enough energy to reproduce, reproduce
            if (reproductionEnergy >= reproductionEnergyThreshold)
            {
                reproductionEnergy = 0;
                Reproduce();
            }
        }

        //Starve
        float agentY = this.transform.position.y;
        if (energy <= 0 || agentY < -10)
        {
            this.transform.Rotate(0, 0, 180);
            //this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 3.5f, this.transform.position.z);
            Destroy(this.gameObject,3);
            GetComponent<Movement>().enabled = false;
        }

    }

    private void MutateCreature()
    {
        if(mutateMutations)
        {
            mutationAmount += Random.Range(-1.0f, 1.0f)/100;
            mutationChance += Random.Range(-1.0f, 1.0f)/100;
        }

        //make sure mutation amount and chance are positive using max function
        mutationAmount = Mathf.Max(mutationAmount, 0);
        mutationChance = Mathf.Max(mutationChance, 0);

        nn.MutateNetwork(mutationAmount, mutationChance);
    }

    public void Reproduce()
    {
        //replicate
        for (int i = 0; i< numberOfChildren; i ++) // I left this here so I could possibly change the number of children a parent has at a time.
        {
            //create a new agent, and set its position to the parent's position + a random offset in the x and z directions (so they don't all spawn on top of each other)
            GameObject child = Instantiate(creaturePrefab, new Vector3(
                (float)this.transform.position.x + Random.Range(-10, 11), 
                0.75f, 
                (float)this.transform.position.z+ Random.Range(-10, 11)), 
                Quaternion.identity);
            
            //copy the parent's neural network to the child
            child.GetComponent<NN>().layers = GetComponent<NN>().copyLayers();
        }
        reproductionEnergy = 0;

    }
}
