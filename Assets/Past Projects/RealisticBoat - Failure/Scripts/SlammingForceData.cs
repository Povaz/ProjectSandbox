using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data that belongs to one triangle in the original boat mesh
// and is needed to calculate the Slamming Force

public class SlammingForceData
{
    // The area of the original triangles 
    // Calculated once in the beginning since it does not change over time
    public float originalArea;

    // How much area of a triangle in the whole boat is submerged
    public float submergedArea;

    // Same as above, but previous time step
    public float previousSubmergedArea;

    // Need to save the center of the triangle to calculate the velocity
    public Vector3 triangleCenter;

    // Velocity
    public Vector3 velocity;

    // Same as above but previous time step
    public Vector3 previousVelocity;
}
