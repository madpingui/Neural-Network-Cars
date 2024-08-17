using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public float spawnRate = 1;
    public int floorScale = 1;
    public GameObject foodPrefab;

    private float timeElapsed = 0;

    void Start()
    {
        // Spawn food at random locations at the start of the game
        for (int i = 0; i < 100; i++)
            SpawnFood();
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        //spawn food every second with timeElapsed
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= spawnRate)
        {
            timeElapsed = timeElapsed % spawnRate;
            SpawnFood();
        }
    }

    void SpawnFood()
    {
        int x = Random.Range(-100, 101)*floorScale;
        int z = Random.Range(-100, 101)*floorScale;
        Instantiate(foodPrefab, new Vector3(x, 0.75f, z), Quaternion.identity, this.transform);
    }
}
