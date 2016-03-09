using System;
using UnityEngine;
using System.Collections;

public class Items : MonoBehaviour 
{

	public Inventory inv;

	void Start () 
	{
		// Blocks
		inv.items.Add (new Block("Air", 0, ItemType.Block));
		inv.items.Add (new Block("Grass", 1, ItemType.Block));
		inv.items.Add (new Block("Dirt", 2, ItemType.Block));
		inv.items.Add (new Block("Stone", 3, ItemType.Block));
		inv.items.Add (new Block("Cobblestone", 4, ItemType.Block));
		inv.items.Add (new Block("Wood", 5, ItemType.Block));
		inv.items.Add (new Block("Planks", 6, ItemType.Block));
		inv.items.Add (new Block("Leaves", 7, ItemType.Block));

		// Tools


		// Items

	}
}
