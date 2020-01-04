using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public static WaterController current;

    public bool isMoving;

    // Wave height and speed
    public float scale = 0.1f;
    public float speed = 1.0f;

    // The width between the waves
    public float waveDistance = 1f;

    // Noise parameters
    public float noiseStrength = 1f;
    public float noiseWalk = 1f;

    // Start is called before the first frame update
    void Start()
    {
        current = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Get the Y Coordinate from whatever wavetype we are using
    public float GetWaveYPos(Vector3 position, float timeSinceStart)
    {
        return 0f;
    }

    // Find the distance from a vertice to the water
    // Make sure the position is in global coordinates
    // Positive if above water
    // Negative if below water
    public float DistanceToWater(Vector3 position, float timeSinceStart)
    {
        float waterHeight = GetWaveYPos(position, timeSinceStart);

        float distanceToWater = position.y - waterHeight;

        return distanceToWater;
    }
}
