using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{

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
               
	}
	
	void Update ()
    {
	    
	}

    public void CalculateSpawn ()
    {
        Vector2 temp = Vector2.zero;

        // Get the x spawnpoint
        temp.x = Mathf.RoundToInt((world.worldWidth * world.chunkSize) / 2);
        
		for (int y = world.worldHeight * world.chunkSize - 1; y > 0; y--)
		{
			if (world.worldBlocks[(int)temp.x, y].id != 0)
			{
				temp.y = y + 1;
				break;
			}
		}

		spawnPoint = temp;
		SpawnPlayer();
    }

	void SpawnPlayer ()
	{
		// Spawn player
		player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity) as GameObject;
		camera.transform.position = new Vector3 (player.transform.position.x, player.transform.position.y + 1.5f, -30f);
		gameCamera.target = player;
	}
}
