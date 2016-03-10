using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Tool, Block };


public class Inventory : MonoBehaviour 
{

	public List<Item> items = new List<Item>();

}

[System.Serializable]
public class Item : System.ICloneable
{

	public string name;
	public int id;
	public ItemType type;

	public Item () { }
	public Item (string name, int id, ItemType type)
	{
		this.name = name;
		this.id = id;
		this.type = type;
	}

	public object Clone ()
	{
		return this.MemberwiseClone();
	}
}

[System.Serializable]
public class Tool : Item
{

	public float dps;

	public Tool (string name, int id, ItemType type, float dps)
	{
		this.name = name;
		this.id = id;
		this.type = type;
		this.dps = dps;
	}
}

[System.Serializable]
public class Block : Item
{
	
	public int damage;
	public Vector2 position;
	
	public Block (string name, int id, ItemType type)
	{
		this.name = name;
		this.id = id;
		this.type = type;
		this.damage = 10;
	}

	public void TakeDamage ()
	{
		damage--;
	}
}