using UnityEngine;
using System.Collections.Generic;
using System;

public class Character : MonoBehaviour 
{

    WorldGeneration world;
    float worldWidth;

    // Set the character limits
    public float[] distances = new float[4];

    // Movement data
    public float walkSpeed;
    public Vector2 velocity;
    public float gravity;
    
    void Start ()
    {
        world = GameObject.Find("_WorldGeneration").GetComponent<WorldGeneration>();
        worldWidth = world.chunkWidth * world.worldWidthInChunks;
    }

	void FixedUpdate () 
    {
        Vector2 wantedPos = this.transform.position;

        // Add gravity
        if (wantedPos.y > distances[0])
            velocity.y -= gravity;

        // Set moveDirection
        if (Input.GetAxis("Horizontal") != 0)
            velocity.x = Input.GetAxis("Horizontal") * Time.deltaTime * walkSpeed;
        else
            velocity.x = 0;

        // Apply velocity
        wantedPos += velocity;

        // Check that wantedPos does not go through something
        if (wantedPos.y < distances[0]) // Bottom
        {
            wantedPos.y = distances[0];
        }
        if (wantedPos.x < distances[1] + .15f && distances[1] != 0) // Left
            wantedPos.x = distances[1] + .15f;
        if (wantedPos.x > distances[3] - .15f && distances[3] != 0) // Right
            wantedPos.x = distances[3] - .15f;

        // Make sure the player cannot go out of the map
        if (wantedPos.x < -.35f) // Left
            wantedPos.x = -.35f;
        if (wantedPos.x > world.chunkWidth * world.worldWidthInChunks - .65f)
            wantedPos.x = world.chunkWidth * world.worldWidthInChunks - .65f;

        // Check if the player is grounded, then set velocity to 0
        if (Math.Round(this.transform.position.y, 1) == distances[0])
            velocity.y = 0;

        // Set position to wantedPos
        this.transform.position = wantedPos;
        
        // Check distances
        CheckBottom();
        CheckLeft();
        CheckRight();
    }

    void CheckBottom ()
    {
        float newDistance = 0;
        RaycastHit hit;

        // Check distance from left bottom
        if (Physics.Raycast(new Vector3(this.transform.position.x - .15f, this.transform.position.y + .1f, 0), new Vector3(0, -1, 0), out hit))
        {            
            newDistance = hit.point.y;
        }

        // Check distance from right bottom
        if (Physics.Raycast(new Vector3(this.transform.position.x + .15f, this.transform.position.y + .1f, 0), new Vector3(0, -1, 0), out hit))
        {
            if (hit.point.y > newDistance)
                newDistance = hit.point.y;
        }

        // Apply distance
        distances[0] = newDistance;
    }

    void CheckLeft ()
    {
        float newDistance = 0;
        RaycastHit hit;

        // Check distance from left bottom
        if (Physics.Raycast(new Vector3(this.transform.position.x - .05f, this.transform.position.y + .2f, 0), new Vector3(-1, 0, 0), out hit))
        {
            newDistance = hit.point.x;
        }

        // Check distance from left middle
        if (Physics.Raycast(new Vector3(this.transform.position.x - .05f, this.transform.position.y + 1f, 0), new Vector3(-1, 0, 0), out hit))
        {
            if (hit.point.x > newDistance)
                newDistance = hit.point.x;
        }

        // Check distance from left top
        if (Physics.Raycast(new Vector3(this.transform.position.x - .05f, this.transform.position.y + 1.8f, 0), new Vector3(-1, 0, 0), out hit))
        {
            if (hit.point.x > newDistance)
                newDistance = hit.point.x;
        }

        // Apply distance
        distances[1] = newDistance;
    }

    void CheckRight ()
    {
        float newDistance = 0;
        RaycastHit hit;

        // Check distance from left bottom
        if (Physics.Raycast(new Vector3(this.transform.position.x + .05f, this.transform.position.y + .2f, 0), new Vector3(1, 0, 0), out hit))
        {
            newDistance = hit.point.x;
        }

        // Check distance from left middle
        if (Physics.Raycast(new Vector3(this.transform.position.x + .05f, this.transform.position.y + 1f, 0), new Vector3(1, 0, 0), out hit))
        {
            if (hit.point.x < newDistance)
                newDistance = hit.point.x;
        }

        // Check distance from left top
        if (Physics.Raycast(new Vector3(this.transform.position.x + .05f, this.transform.position.y + 1.8f, 0), new Vector3(1, 0, 0), out hit))
        {
            if (hit.point.x < newDistance)
                newDistance = hit.point.x;
        }

        // Apply distance
        distances[3] = newDistance;
    }
}
