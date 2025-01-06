using UnityEngine;
using TMPro;

public class UpdateIPText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI IPDynamic; // Assign this in the Inspector

    private void Start()
    {
        if (IPDynamic != null)
        {
            IPDynamic.text = $"{GlobalVariables.IPAddress} : {GlobalVariables.Port}";
        }
        else
        {
            Debug.LogError("IPDynamic TextMeshPro object is not assigned in the Inspector.");
        }
    }
}
