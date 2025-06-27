using UnityEngine;

public class LapTrigger : MonoBehaviour
{
    [Tooltip("Drag your GameManager (with LapManager) here")]
    public LapManager lapManager;

    // Only count a lap if we've left the trigger first
    private bool readyToCount = false;

    void OnTriggerExit(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
            readyToCount = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!readyToCount) return;

        if (other.transform.root.CompareTag("Player"))
        {
            lapManager.CompleteLap();
            readyToCount = false;
        }
    }
}
