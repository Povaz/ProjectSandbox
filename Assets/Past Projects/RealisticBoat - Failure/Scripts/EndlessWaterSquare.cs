﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class EndlessWaterSquare : MonoBehaviour
{
    // Controls whether water update is done by Threads or Single execution
    public bool threadedExecution;

    // The object the water will follow
    public GameObject boatObj;

    // Central water square
    public GameObject waterSqrObj;

    // Water square data
    [SerializeField]
    private float squareWidth = 800f;
    [SerializeField]
    private float innerSquareResolution = 6f;
    [SerializeField]
    private float outerSquareResolution = 25f;

    // The list with all water mesh squares == the entire ocean we can see
    List<WaterSquare> waterSquares = new List<WaterSquare>();

    // THREAD VARIABLES
    // The timer that keeps track of seconds since start to update the water 
    // because we cant use Time.time in a thread
    float secondsSinceStart;
    // The position of the boat;
    Vector3 boatPos;
    // The position of the ocean has to be updated in the thread because it follows 
    // the boat, but it is not the same as position of the boat because it moves 
    // with the same resolution as the smallest water square resolution
    Vector3 oceanPos;
    // Has the thread finished updating the water so we can add the stuff from the 
    // thread to the main thread?
    bool hasThreadUpdatedWater;
    

    // Start is called before the first frame update
    void Start()
    {
        // Create the Sea
        CreateEndlessSea();

        // Init the time 
        secondsSinceStart = Time.time;

        if (threadedExecution)
        {
            // Update the water in the thread
            ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateWaterWithThreadPooling));

            // Start the coroutine
            StartCoroutine(UpdateWater());
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if (!threadedExecution)
        {
            UpdateWaterNoThread();
        }

        // Update these as often as possibile because we don't know when the thread
        // will run because of pooling and we always need to latest version

        // Update the time since start to get correct wave height
        // which depends on time since start
        secondsSinceStart = Time.time;

        // Update the position of the boat to see if we should move the water
        boatPos = boatObj.transform.position;
    }

    void UpdateWaterNoThread()
    {
        // Update the position of the boat
        boatPos = boatObj.transform.position;

        // Move the Water to the boat
        MoveWaterToBoat();

        // Add the new position of the ocean to this transform
        transform.position = oceanPos;

        // Update the vertices
        for (int i = 0; i < waterSquares.Count; i++)
        {
            waterSquares[i].MoveSea(oceanPos, Time.time);
        }
    }

    // The loop that gives the updated vertices from the thread to the meshes
    // which we can't do in its own thread
    IEnumerator UpdateWater()
    {
        while(true)
        {
            // Has the thread finished updating the water?
            if (hasThreadUpdatedWater)
            {
                // Move the water to the boat
                transform.position = oceanPos;

                // Add the updated vertices to the water meshes
                for (int i = 0; i < waterSquares.Count; i++)
                {
                    waterSquares[i].terrainMeshFilter.mesh.vertices = waterSquares[i].vertices;
                    waterSquares[i].terrainMeshFilter.mesh.RecalculateNormals();
                }

                // Stop looping until we have updated the water in the thread
                hasThreadUpdatedWater = false;

                // Update the water in the thread
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateWaterWithThreadPooling));
            }

            // Don't need to update the Water every frame
            yield return new WaitForSeconds(Time.deltaTime * 3f);
        }
    }

    // The thread that updates the water vertices
    void UpdateWaterWithThreadPooling(object state)
    {
        //Move the water to the boat
        MoveWaterToBoat();

        // Loop through all water squares
        for (int i = 0; i < waterSquares.Count; i++)
        {
            // The local center pos of this square
            Vector3 centerPos = waterSquares[i].centerPos;
            // All the vertices this squares consist of
            Vector3[] vertices = waterSquares[i].vertices;

            // Update the vertices in this square
            for (int j = 0; j < vertices.Length; j++)
            {
                // The Local Position of the vertex
                Vector3 vertexPos = vertices[j];

                // Can't use transformpoint in a thread, so to find the global position of the vertex
                // we just add the position of the ocean and the square because rotation and scale are
                // respectively always 0 and 1
                Vector3 vertexPosGlobal = vertexPos + centerPos + oceanPos;

                // Get the Water height
                vertexPos.y = WaterController.current.GetWaveYPos(vertexPosGlobal, secondsSinceStart);

                // Save the new y coordinate, but x and z are still in local position
                vertices[j] = vertexPos;
            }
        }

        hasThreadUpdatedWater = true;
    }

    // Move the endless water to the boat's position in steps 
    // that are the same as the water's resolution
    void MoveWaterToBoat()
    {
        // Round to the nearest resolution
        float x = innerSquareResolution * (int)Mathf.Round(boatPos.x / innerSquareResolution);
        float z = innerSquareResolution * (int)Mathf.Round(boatPos.z / innerSquareResolution);

        // Should we move the water?
        if (oceanPos.x != x || oceanPos.z != z)
        {
            Debug.Log("Moved Sea");
            oceanPos = new Vector3(x, oceanPos.y, z);
        }
    }

    // Init the endless sea by creating all squares
    void CreateEndlessSea()
    {
        // The center piece
        AddWaterPlane(0f, 0f, 0f, squareWidth, innerSquareResolution);

        // The 8 squares around the center square
        for (int x = -1; x <= 1; x += 1)
        {
            for (int z = -1; z <= 1; z += 1)
            {
                // Ignore the center pos
                if (x == 0 && z == 0)
                {
                    continue;
                }

                // The y-Pos should be lower than the square with high resolution
                // to avoid any ugly seam
                float yPos = -0.5f;
                AddWaterPlane(x * squareWidth, z * squareWidth, yPos, squareWidth, outerSquareResolution);
            }
        }
    }

    // Add one water plane
    void AddWaterPlane(float xCoord, float zCoord, float yPos, float squareWidth, float spacing)
    {
        GameObject waterPlane = Instantiate(waterSqrObj, transform.position, transform.rotation) as GameObject;
        waterPlane.SetActive(true);
         
        // Change its position
        Vector3 centerPos = transform.position;

        centerPos.x += xCoord;
        centerPos.y = yPos;
        centerPos.z += zCoord;

        waterPlane.transform.position = centerPos;
        waterPlane.transform.parent = transform;

        // Give it moving properties and set its width and resolution to generate the water mesh
        WaterSquare newWaterSquare = new WaterSquare(waterPlane, squareWidth, spacing);
        waterSquares.Add(newWaterSquare);
    }
}
