
using UnityEngine;

public class GroundwaterTester : MonoBehaviour
{
    public GroundwaterSystem groundwater;

    void Start()
    {
        groundwater.AddRainfall();
        groundwater.ExtractWater();
        groundwater.ArtificialRecharge();
    }
}
