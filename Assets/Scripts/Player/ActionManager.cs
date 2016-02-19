using UnityEngine;
using System.Collections;

public class ActionManager : MonoBehaviour 
{

	public WorldGeneration worldGen;

	public Vector2 currentSelectedBlock;

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
			if ((currentSelectedBlock.y % 1) == 0) // Get correct block
			{
				if (worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y].id == worldGen.blocks[0].id)
					currentSelectedBlock.y = (int)currentSelectedBlock.y - 1;
			}
			else // Remove decimals
				currentSelectedBlock.y = (int)currentSelectedBlock.y;

			// X Axis
			if ((currentSelectedBlock.x % 1) == 0) // Get correct block
			{
				if (worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y].id == worldGen.blocks[0].id)
					currentSelectedBlock.x = (int)currentSelectedBlock.x - 1;
			}
			else // Remove decimals
				currentSelectedBlock.x = (int)currentSelectedBlock.x;

			// // Do damage to the block
			if (Input.GetButtonDown("Fire1"))
			{
				worldGen.worldBlocks[(int)currentSelectedBlock.x, (int)currentSelectedBlock.y] = worldGen.blocks[0];
				worldGen.UpdateChunk((int)(currentSelectedBlock.x / worldGen.chunkSize), (int)(currentSelectedBlock.y / worldGen.chunkSize));
			}
		}
	}

	void OnDrawGizmos ()
	{
		// Draw currentSelectedBlock
		Gizmos.DrawSphere (new Vector3(currentSelectedBlock.x + .5f, currentSelectedBlock.y + .5f, -.5f), .5f);
	}
}
