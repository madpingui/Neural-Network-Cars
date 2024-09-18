using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private int numberOfSpawns = 5;

    private NeuralNetwork bestNetwork;
    private int bestCheckpointReach = 0;
    private List<CarAgent> activeCars = new List<CarAgent>();

    private void Awake()
    {
        CarAgent.OnCarDestroyed += OnCarDestroyed;
        SpawnCars();
    }

    private void OnDestroy()
    {
        CarAgent.OnCarDestroyed -= OnCarDestroyed;
    }

    // When a car is destroyed, remove it from the list and trigger evaluation/reproduction if all cars are destroyed
    private void OnCarDestroyed(CarAgent car)
    {
        activeCars.Remove(car);

        if (activeCars.Count == 0)
        {
            EvaluateAndReproduce(car); // Reproduce new cars after evaluating the best car's performance
        }
    }

    // Evaluate the best performing network and use it for reproduction
    private void EvaluateAndReproduce(CarAgent car)
    {
        // If the car passed more checkpoints than the current best, update the best network
        if (car.CorrectCheckpointsPassed > bestCheckpointReach)
        {
            bestNetwork = car.GetNeuralNetworkCopy();
            bestCheckpointReach = car.CorrectCheckpointsPassed;
        }
        SpawnCars(); // Respawn cars after evaluating
    }

    // Spawns new cars and assigns neural networks, mutating them except for the best performing car
    private void SpawnCars()
    {
        foreach (var car in activeCars)
        {
            if (car != null)
            {
                Destroy(car.gameObject); // Cleanup old car objects
            }
        }
        activeCars.Clear(); // Clear the car list before spawning new ones

        for (int i = 0; i < numberOfSpawns; i++)
        {
            GameObject carObject = Instantiate(carPrefab, transform.position, transform.rotation, transform);
            CarAgent carAgent = carObject.GetComponent<CarAgent>();

            if (bestNetwork != null)
            {
                carAgent.InitializeNeuralNetwork(bestNetwork); // Assign the best-performing network
            }

            // Mutate all cars except for the first one if there’s no best network
            if (i != 0 || bestNetwork == null)
            {
                carAgent.Mutate(); // Mutate the newly spawned cars
            }

            activeCars.Add(carAgent); // Add car to the active car list
        }
    }
}

