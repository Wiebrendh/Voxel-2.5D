using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Tool, Block };

public class Inventory : MonoBehaviour 
{

	public List<Item> items = new List<Item>();

	void Start () 
	{

	}

	void Update () 
	{
		
	}
}

[System.Serializable]
public class Item 
{
	public string name;
	public int id;
	public ItemType type;

	// Tool data
	public float dps;

	// Block data
	public int damage;

	public Item (string name, int id, ItemType type) // Tool
	{
		this.name = name;
		this.id = id;
		this.type = type;
	}
}

[System.Serializable]
public class Tool
{

	public Item item;

	public Tool (Item item)
	{
		this.item = item;
	}
}