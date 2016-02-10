using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
/*
    [Header("Scripts")]
    public WorldGeneration world;
    public GameCamera gameCamera;

    [Header("Objects")]
    public GameObject player;
    public GameObject camera;

    [Header("Data")]
    public Vector2 spawnPoint;

    [Header("Prefabs")]
    public GameObject playerPrefab;

	void Start ()
    {
        // Calculate spawnpoint for player
        spawnPoint = CalculateSpawn();

        // Set camera to correct position
        camera.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, -12.5f);

        // Spawn player
        player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity) as GameObject;
        gameCamera.player = player;
	}
	
	void Update ()
    {
	    
	}

    Vector2 CalculateSpawn ()
    {
        Vector2 temp = Vector2.zero;

        // Get the x spawnpoint
        temp.x = Mathf.RoundToInt((world.chunkWidth * world.worldWidthInChunks) / 2);
        temp.y = 100; // HAVE TO CALCULATE THE FIRST AVAILABLE Y TO STAND ON

        return temp;
    }*/
}
