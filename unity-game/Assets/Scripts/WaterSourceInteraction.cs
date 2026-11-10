
using UnityEngine;
using TMPro;

public class WaterSourceInteraction : MonoBehaviour
{
    public GroundwaterSystem groundwaterSystem;
    public TMP_Text interactionMessage;

    private bool playerNearby = false;

    public enum WaterAction
    {
        Rainfall,
        Extraction,
        Recharge
    }

    public WaterAction actionType;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (groundwaterSystem == null)
            {
                Debug.LogError("GroundwaterSystem is not assigned!");
                return;
            }

            if (actionType == WaterAction.Rainfall)
                groundwaterSystem.AddRainfall();

            else if (actionType == WaterAction.Extraction)
                groundwaterSystem.ExtractWater();

            else if (actionType == WaterAction.Recharge)
                groundwaterSystem.ArtificialRecharge();

            if (interactionMessage != null)
                interactionMessage.text = "Action performed!";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (interactionMessage != null)
                interactionMessage.text = "Press E to interact";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactionMessage != null)
                interactionMessage.text = "";
        }
    }
}
