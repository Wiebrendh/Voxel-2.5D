using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{

    public GameObject player;
    public float smoothFollowSpeed;

    public WorldGeneration world;
	
	void Update ()
    {
        // Store position in temp position
        Vector3 wantedPos = new Vector3(player.transform.position.x, player.transform.position.y + 2.5f, -12.5f);
        
        // Add limits
        if (wantedPos.x < 12) // Limit left
            wantedPos.x = 12;
            
        if (wantedPos.x > world.chunkWidth * world.worldWidthInChunks - 12.81f) // Limit right
            wantedPos.x = world.chunkWidth * world.worldWidthInChunks - 12.81f;    
            
        if (wantedPos.y < 6.5f) // Limit bottom
            wantedPos.y = 6.5f;
        
        // Lerp to position
        this.transform.position = Vector3.Lerp(this.transform.position, wantedPos, smoothFollowSpeed * Time.deltaTime);
	}
}
