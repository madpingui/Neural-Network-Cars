using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NN;

public class CreatureSpawner : MonoBehaviour
{
    public GameObject agentPrefab;

    public int numberOfCreatures = 5;
    private Layer[] BestLayers = null;
    private int BestCheckpointReach = 0;
    private List<Creature> activeCreatures = new List<Creature>(); // List to keep track of active creatures

    private void Awake()
    {
        Creature.OnCreatureDead += OnCreatureDead;
        SpawnCreatures();
    }

    private void OnDestroy()
    {
        Creature.OnCreatureDead -= OnCreatureDead;
    }

    void OnCreatureDead(Creature creature)
    {
        // Remove the dead creature from the list
        activeCreatures.Remove(creature);

        // If no left take the last one and copy its genes.
        if (activeCreatures.Count == 0)
            TakeBestCreatureAndReproduce(creature);
    }

    void TakeBestCreatureAndReproduce(Creature creature)
    {
        if(creature.amountOfCorrectCheckpoints > BestCheckpointReach)
        {
            BestLayers = creature.GetComponent<NN>().copyLayers();
            BestCheckpointReach = creature.amountOfCorrectCheckpoints;
        }
        SpawnCreatures();
    }

    void SpawnCreatures()
    {
        // Clear the active creatures list
        activeCreatures.Clear();

        // Spawn new creatures
        for (int i = 0; i < numberOfCreatures; i++)
        {
            var child = Instantiate(agentPrefab, this.transform.position, this.transform.rotation, this.transform);
            var childCreature = child.GetComponent<Creature>();

            if (BestLayers != null)
                child.GetComponent<NN>().layers = BestLayers;

            // Apply elitism: keep the best creature unmutated in the next generation
            if (i == 0 && BestLayers != null)
            {
                childCreature.mutationAmount = 0f;
                childCreature.mutationChance = 0f;
            }

            // Add the new creature to the list of active creatures
            activeCreatures.Add(childCreature);
        }
    }
}
