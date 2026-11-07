using UnityEngine;
using TMPro;

public class GroundwaterSystem : MonoBehaviour
{
    public float groundwaterLevel = 70f;
    public float maxGroundwater = 100f;

    public TMP_Text groundwaterValue;
    public TMP_Text groundwaterMessage;

    void Start()
    {
        UpdateUI();
    }

    public void AddRainfall()
    {
        groundwaterLevel += 8f;
        groundwaterLevel = Mathf.Clamp(groundwaterLevel, 0f, maxGroundwater);
        groundwaterMessage.text = "Rainfall added!";
        UpdateUI();
    }

    public void ExtractWater()
    {
        groundwaterLevel -= 5f;
        groundwaterLevel = Mathf.Clamp(groundwaterLevel, 0f, maxGroundwater);
        groundwaterMessage.text = "Water extracted!";
        UpdateUI();
    }

    public void ArtificialRecharge()
    {
        groundwaterLevel += 12f;
        groundwaterLevel = Mathf.Clamp(groundwaterLevel, 0f, maxGroundwater);
        groundwaterMessage.text = "Artificial recharge done!";
        UpdateUI();
    }

    void UpdateUI()
    {
        groundwaterValue.text = groundwaterLevel + " / " + maxGroundwater;

        if (groundwaterLevel >= 60)
            groundwaterMessage.text = "Groundwater is at a healthy level";
        else if (groundwaterLevel >= 30)
            groundwaterMessage.text = "Warning: Groundwater is decreasing!";
        else
            groundwaterMessage.text = "Critical: Save groundwater!";
    }
}