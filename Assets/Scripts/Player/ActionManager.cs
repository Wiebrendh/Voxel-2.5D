using UnityEngine;
using System.Collections;

public class ActionManager : MonoBehaviour 
{

	public WorldGeneration worldGen;

	public Vector2 currentSelectedBlock;

	[Header("Test data")]
	public int test;

	void Start () 
	{
		
	}
	
	void Update () 
	{
		// Functions
		GetSelectedBlock ();
	}

	void GetSelectedBlock ()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if (Physics.Raycast(ray, out hit) && hit.transform.tag == "Chunk") 
		{
			currentSelectedBlock = hit.point;

			// Y Axis
			if ((currentSelectedBlock.y % 1) == 0) // Get correct block y axis
			{
				if (worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y].id == worldGen.blocks[0].id)
					currentSelectedBlock.y = (int)currentSelectedBlock.y - 1;
			}
			else // Remove decimals
				currentSelectedBlock.y = (int)currentSelectedBlock.y;

			// X Axis
			if ((currentSelectedBlock.x % 1) == 0) // Get correct block x axis
			{
				if (worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y].id == worldGen.blocks[0].id)
					currentSelectedBlock.x = (int)currentSelectedBlock.x - 1;
			}
			else // Remove decimals
				currentSelectedBlock.x = (int)currentSelectedBlock.x;

			// // Do damage to the block
			if (Input.GetButtonDown("Fire1"))
			{
				int x = (int)currentSelectedBlock.x, z = (int)currentSelectedBlock.y;
				worldGen.worldBlocks[x, z].TakeDamage();

				// Check if block is destroyed
				if (worldGen.worldBlocks[x, z].damage <= 0)
				{
					// Insert air block
					worldGen.worldBlocks[x, z] = (Block)worldGen.blocks[0].Clone();

					// Update chunk				
					worldGen.UpdateChunk((int)(currentSelectedBlock.x / worldGen.chunkSize), (int)(currentSelectedBlock.y / worldGen.chunkSize), currentSelectedBlock.x / 8, currentSelectedBlock.y / 8);
				}

			}
			test = (int)worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y].damage;
		}
	}
}
