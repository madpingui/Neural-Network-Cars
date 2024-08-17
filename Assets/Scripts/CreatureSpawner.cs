using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    private GameObject[] agentList;
    public int floorScale = 1;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        agentList = GameObject.FindGameObjectsWithTag("Agent");

        // if there are no agents in the scene, spawn one at a random location. 
        // This is to ensure that there is always at least one agent in the scene.
        if (agentList.Length < 1)
            SpawnCreature();
    }

    void SpawnCreature()
    {
        int x = Random.Range(-100, 101) * floorScale;
        int z = Random.Range(-100, 101) * floorScale;
        Instantiate(agentPrefab, new Vector3(x, 0.75f, z), Quaternion.identity, this.transform);
    }
}
