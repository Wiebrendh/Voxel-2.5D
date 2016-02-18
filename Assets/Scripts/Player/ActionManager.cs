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

			if ((currentSelectedBlock.x % 1) == 0) // Get correct block
			{

			}
			else // Remove decimals
				currentSelectedBlock.x = (int)currentSelectedBlock.x;


		}
	}

	void OnDrawGizmos ()
	{
		Gizmos.DrawSphere (new Vector3(currentSelectedBlock.x + .5f, currentSelectedBlock.y + .5f, -.5f), .5f);
	}
}
