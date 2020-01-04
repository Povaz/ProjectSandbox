using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTypes
{
    // Sinus waves
    public static float SinXWave(
        Vector3 position,
        float speed,
        float scale,
        float waveDistance,
        float noiseStrength,
        float noiseWalk,
        float timeSinceStart,
        WaterTypesEnumeration waterType)
    {
        float x = position.x;
        float y = 0f;
        float z = position.z;

        // Using only x or z will produce straight waves
        // Using only y will produce an up/down movement
        // x + y + z rolling waves
        // x * z produces a moving sea without rolling waves

        float waveType = 0f;
        switch (waterType)
        {
            case WaterTypesEnumeration.StraightX: waveType = x; break;
            case WaterTypesEnumeration.StraightZ: waveType = z; break;
            case WaterTypesEnumeration.UpDown: waveType = y; break;
            case WaterTypesEnumeration.Rolling: waveType = x + y + z; break;
            case WaterTypesEnumeration.Moving: waveType = x * z; break;
            default: waveType = z; break;
        }

        y += Mathf.Sin((timeSinceStart * speed + waveType / waveDistance)) * scale;

        // Add Noise to make it more realistic
        y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

        return y;
    }
}
