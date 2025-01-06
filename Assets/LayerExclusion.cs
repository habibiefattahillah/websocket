using UnityEngine;

public class LayerExclusion : MonoBehaviour
{
    public LayerMask excludedLayer;

    void Start()
    {
        // Example functionality to exclude layers (adjust as needed)
        gameObject.layer = excludedLayer.value;
    }
}
