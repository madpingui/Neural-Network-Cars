using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private int checkpointIndex;
    private void Start()
    {
        // Calculate the index based on the position in the hierarchy
        checkpointIndex = transform.parent.GetSiblingIndex();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            other.GetComponentInParent<CarAgent>().ValidateCheckpoint(checkpointIndex, checkpointIndex == this.transform.parent.parent.childCount - 1);
        }
    }
}

