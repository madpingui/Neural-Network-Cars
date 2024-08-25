using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;  // Set this in the inspector or via script

    private void Start()
    {
        // Calculate the index based on the position in the hierarchy
        checkpointIndex = transform.parent.GetSiblingIndex();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            other.GetComponentInParent<Creature>().ValidateCheckpoint(checkpointIndex, checkpointIndex == this.transform.parent.parent.childCount - 1);
        }
    }
}

