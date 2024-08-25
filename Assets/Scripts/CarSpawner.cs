using System.Collections.Generic;
using UnityEngine;
using static NN;

public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public int numberOfSpawns = 5;

    private Layer[] BestLayers = null;
    private int BestCheckpointReach = 0;
    private List<CarAgent> activeCars = new List<CarAgent>();

    private void Awake()
    {
        CarAgent.OnCarBroken += OnCreatureDead;
        Spawn();
    }

    private void OnDestroy()
    {
        CarAgent.OnCarBroken -= OnCreatureDead;
    }

    void OnCreatureDead(CarAgent car)
    {
        // Remove the dead creature from the list
        activeCars.Remove(car);

        // If no left take the last one and copy its genes.
        if (activeCars.Count == 0)
            TakeBestAndReproduce(car);
    }

    void TakeBestAndReproduce(CarAgent car)
    {
        if(car.amountOfCorrectCheckpoints > BestCheckpointReach)
        {
            BestLayers = car.GetComponent<NN>().copyLayers();
            BestCheckpointReach = car.amountOfCorrectCheckpoints;
        }
        Spawn();
    }

    void Spawn()
    {
        // Clear the active list
        activeCars.Clear();

        // Spawn new cars
        for (int i = 0; i < numberOfSpawns; i++)
        {
            var car = Instantiate(carPrefab, this.transform.position, this.transform.rotation, this.transform);
            var carAgent = car.GetComponent<CarAgent>();

            if (BestLayers != null)
                car.GetComponent<NN>().layers = BestLayers;

            // Apply elitism: keep the best creature unmutated in the next generation
            if (i == 0 && BestLayers != null)
            {
                carAgent.mutationAmount = 0f;
                carAgent.mutationChance = 0f;
            }

            // Add the new creature to the list of active creatures
            activeCars.Add(carAgent);
        }
    }
}
