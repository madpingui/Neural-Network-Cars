using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int floorScale = 1;

    private void Awake()
    {
        Creature.OnCreatureDead += () => 
        {
            // if there are no agents in the scene, spawn one at a random location. 
            // This is to ensure that there is always at least one agent in the scene.
            if (this.transform.childCount == 1)
                SpawnCreature();
        };
        SpawnCreature();
    }

    void SpawnCreature()
    {
        int x = Random.Range(-100, 101) * floorScale;
        int z = Random.Range(-100, 101) * floorScale;
        Instantiate(agentPrefab, new Vector3(x, 0.75f, z), Quaternion.identity, this.transform);
    }
}
