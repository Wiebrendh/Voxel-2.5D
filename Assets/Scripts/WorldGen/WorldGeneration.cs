using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum LoadingActions { CreatingTerrain, CreatingChunks };

public class WorldGeneration : MonoBehaviour 
{

	GameController game;

	[Header("Generation data")]
    [SerializeField] public int chunkSize;
    [SerializeField] public int worldWidth;
    [SerializeField] public int worldHeight;
    
	[Header("Blocks and biomes")]
    [SerializeField] public Block[] blocks;
    [SerializeField] Biome[] biomes;
    [SerializeField] Material[] materials;    
    
    // World data
    Biome[] worldBiomes;
    public Block[,] worldBlocks;
    GameObject[,] worldChunks;
     
	[Header("Chunk queu")]
	[SerializeField] int maxChunkUpdatesPerFrame;
	[SerializeField] List<ChunkMeshData> queu = new List<ChunkMeshData>();

	[Header("Loading stuff")]
	public Text text;
	public Slider slider;
	LoadingActions currentAction;
	public FadeOut loadingUIFade;
	public int chunksCreated;
	bool doneStartup;
    
	void Start ()
	{
		game = GetComponentInParent<GameController>();
		CreateWorld ();

	} // Start

	void Update ()
	{
		// Create/update chunks
		for (int i = 0; i < maxChunkUpdatesPerFrame; i++) 
		{
			// Return if queu is empty
			if (queu.Count <= 0)
				break;
			
			CreateChunkMesh(queu[0]);
			queu.RemoveAt(0);
			chunksCreated++;
		}

		// Check if script is done creating chunks
		if (chunksCreated == worldHeight * worldWidth && !doneStartup)
		{
			doneStartup = true;
			game.CalculateSpawn();
			loadingUIFade.fade = true;
		}

		if (!doneStartup)
			UpdateLoadingScreen ();
		
	} // Update

    void CreateWorld ()
    {
		// Update action
		currentAction = LoadingActions.CreatingTerrain;
		UpdateLoadingScreen ();

        // Calculate width and assign world 2D array
        int width = worldWidth * chunkSize;
        worldBlocks = new Block[width, worldHeight * chunkSize];

        // Set all blocks in world to air at start
        for (int y = 0; y < worldHeight * chunkSize; y++)
        {
            for (int x = 0; x < width; x++)
            {
                worldBlocks[x, y] = blocks[0];
            }
        }

        // Set biomes
        worldBiomes = new Biome[worldWidth];
        int wantedLength = 0;
        Biome biomeType = new Biome();
        for (int b = 0; b < worldBiomes.Length; b++)
        {
            // Set random biome length
            if (wantedLength == 0)
            {
				wantedLength = UnityEngine.Random.Range (3, 5);
                biomeType = biomes[UnityEngine.Random.Range(0, biomes.Length)];
            }

            // Set data
            worldBiomes[b] = biomeType;
            wantedLength--;
        }

        // Create chunks
        {
            // Animation curves
            AnimationCurve worldGenGrass = new AnimationCurve();
            AnimationCurve worldGenDirt = new AnimationCurve();

            // Add keyframes in the animationcurve and generate height
			for (int i = 0; i < worldWidth * chunkSize; i += chunkSize)
            {
                // Create animationcurve for grass
                { 
                    // Get current biome
					Biome currentBiome = worldBiomes[i / chunkSize];

                    // Determine height
                    int height = 0;
                    if (UnityEngine.Random.Range(0, 1) == 0)
                        height = currentBiome.averageHeight + UnityEngine.Random.Range(currentBiome.minHeightDiff, currentBiome.maxHeightDiff + 1);
                    else
                        height = -currentBiome.averageHeight + UnityEngine.Random.Range(currentBiome.minHeightDiff, currentBiome.maxHeightDiff + 1);

                    // Insert the key
                    worldGenGrass.AddKey(i, height);
                }

                // Create animationcurve for dirt layer thickness
                {
                    // Insert the key
                    worldGenDirt.AddKey(i, worldGenGrass.Evaluate(i) - UnityEngine.Random.Range(5, 8));
                }
            }

            // Use keyframe data to insert blocks into world array
            for (int l = 0; l < chunkSize * worldWidth; l++)
            {
				// Grass
				worldBlocks[l, (int)worldGenGrass.Evaluate(l)] = blocks[1];

				// Dirt
				for (int d = 0; d < (int)worldGenGrass.Evaluate (l); d++) 
				{
					worldBlocks [l, d] = blocks [2];
				}

				// Stone
				for (int s = 0; s < (int)worldGenDirt.Evaluate (l); s++) 
				{
					worldBlocks [l, s] = blocks [3];
				}                
            }

			// Insert some trees
			for (int i = 0; i < worldWidth; i++)
			{
				if (worldBiomes[i].forest)
				{
					int posX = UnityEngine.Random.Range(3, 4) + (i * chunkSize);
					int posY = (int)worldGenGrass.Evaluate(posX) + 1;

					InsertTree(posX, posY);
				}
			}
        }
        
		// Loop to create chunks
		currentAction = LoadingActions.CreatingChunks;
		UpdateLoadingScreen ();
		for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                CalculateChunkMeshData(x, y, false);
            }
        }

    } // CreateWorld
    
	public void UpdateChunk (int chunkX, int chunkY)
	{
		CalculateChunkMeshData (chunkX, chunkY, true);

		// Check if it is on the side of a chunk
	}   
       
    void CalculateChunkMeshData (int chunkX, int chunkY, bool destroyOld)
    {
        ChunkMeshData chunkData = new ChunkMeshData();

        // Set chunkData position
		chunkData.x = chunkX * chunkSize;
		chunkData.y = chunkY * chunkSize;
	
		// Destroy old chunk
		chunkData.destroyOld = destroyOld;

        // Visual data
        chunkData.vertices = new List<Vector3>();
		chunkData.triangles = new List<int>[9]{ new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        chunkData.uv = new List<Vector2>();
        
        // Collider data
        chunkData.colliderVertices = new List<Vector3>();
        chunkData.colliderTriangles = new List<int >();
        
        int squareCount = 0;
		int squareCountCollider = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
				Block thisBlock = worldBlocks [chunkData.x + x, chunkData.y + y];
				if (thisBlock.id != 0) // Dont do these calculations if the block is air 
				{
	                // Calculate front
					CalculateFace (x, y, ref chunkData, ref squareCount, ref squareCountCollider, 0);

					// Calculate top
					if (worldHeight * chunkSize == chunkData.y + y + 1 || worldBlocks[chunkData.x + x, chunkData.y + y + 1].id == 0 || worldBlocks[chunkData.x + x, chunkData.y + y + 1].id == 7 && worldBlocks[chunkData.x + x, chunkData.y + y].id != 7)
						CalculateFace (x, y, ref chunkData, ref squareCount, ref squareCountCollider, 1);

					// Calculate bottom
					if (chunkData.y + y  == 0 || worldBlocks[chunkData.x + x, chunkData.y + y - 1].id == 0)
						CalculateFace (x, y, ref chunkData, ref squareCount, ref squareCountCollider, 2);

					// Calculate left 
					if (chunkData.x + x == 0 || worldBlocks[chunkData.x + x - 1, chunkData.y + y].id == 0)
						CalculateFace (x, y, ref chunkData, ref squareCount, ref squareCountCollider, 3);
					
					// Calculate right 
					if (worldWidth * chunkSize == chunkData.x + x + 1 || worldBlocks[chunkData.x + x + 1, chunkData.y + y].id == 0)
						CalculateFace (x, y, ref chunkData, ref squareCount, ref squareCountCollider, 4);
				}
            }
        }
        
        // Add the ChunkMeshData to the queu
        queu.Add(chunkData);

    } // CalculateMeshData
    
	void CalculateFace (int x, int y, ref ChunkMeshData chunkData, ref int squareCount, ref int squareCountCollider, int side)
    {
		Vector3[] tempVertices =  new Vector3[0];
		switch (side)
		{
			case 0:
			{
				// Add vertices
				tempVertices = new Vector3[] 
				{
					new Vector3 (x, y, -.5f),
					new Vector3 (x, y + 1f, -.5f),
					new Vector3 (x + 1f, y + 1f, -.5f),
					new Vector3 (x + 1f, y, -.5f)
				};
			}
			break;
			case 1:
			{
				// Add vertices
				tempVertices = new Vector3[] {
					new Vector3 (x, y + 1f, -.5f),
					new Vector3 (x, y + 1f, .5f),
					new Vector3 (x + 1f, y + 1f, .5f),
					new Vector3 (x + 1f, y + 1f, -.5f)
				};
			}
			break;
			case 2:
			{
				// Add vertices
				tempVertices = new Vector3[] 
				{
					new Vector3 (x, y, .5f),
					new Vector3 (x, y, -.5f),
					new Vector3 (x + 1f, y, -.5f),
					new Vector3 (x + 1f, y, .5f)
				};
			}
			break;
			case 3:
			{
				// Add vertices
				tempVertices = new Vector3[] 
				{
					new Vector3 (x, y, .5f),
					new Vector3 (x, y + 1f, .5f),
					new Vector3 (x, y + 1f, -.5f),
					new Vector3 (x, y, -.5f)
				};
			}
			break;
			case 4:
			{
				// Add vertices
				tempVertices = new Vector3[] 
				{
					new Vector3 (x + 1f, y, -.5f),
					new Vector3 (x + 1f, y + 1f, -.5f),
					new Vector3 (x + 1f, y + 1f, .5f),
					new Vector3 (x + 1f, y, .5f)
				};
			}
			break;
		}

        chunkData.vertices.AddRange(tempVertices); // Visual vertices
		chunkData.colliderVertices.AddRange(tempVertices); // Collider vertices        

		// Triangles for collider
		List<int> colliderTriangles = new List<int>();
		colliderTriangles.AddRange(new int[]
		{
			squareCountCollider * 4,
			(squareCountCollider * 4) + 1,
			(squareCountCollider * 4) + 3,
			(squareCountCollider * 4) + 1,
			(squareCountCollider * 4) + 2,
			(squareCountCollider * 4) + 3
		}); 
		chunkData.colliderTriangles.AddRange(colliderTriangles.ToArray()); // Collider triangles
		

		// Add triangles
		List<int> tempTriangles = new List<int>();
		tempTriangles.AddRange(new int[]
		{
			squareCount * 4,
			(squareCount * 4) + 1,
			(squareCount * 4) + 3,
			(squareCount * 4) + 1,
			(squareCount * 4) + 2,
			(squareCount * 4) + 3
		}); 

		switch (worldBlocks [chunkData.x + x, chunkData.y + y].id) // Insert triangles into correct chunkData.triangles
		{
			case 1: // Grass
				{	
					if (side == 1)
						chunkData.triangles [0].AddRange (tempTriangles.ToArray ());
					else if (side == 2)
						chunkData.triangles [2].AddRange (tempTriangles.ToArray ());
					else
						chunkData.triangles [1].AddRange (tempTriangles.ToArray ());
				}
				break;
			case 2: // Dirt
				{
					chunkData.triangles [2].AddRange (tempTriangles.ToArray ());
				}
				break;
			case 3: // Stone
				{
					chunkData.triangles [3].AddRange (tempTriangles.ToArray ());
				}	
				break;
			case 4: // Cobblestone
				{
					chunkData.triangles [4].AddRange (tempTriangles.ToArray ());
				}
				break;
			case 5: // Wood
				{			
					if (side == 1 || side == 2)
						chunkData.triangles [5].AddRange (tempTriangles.ToArray ());
					else
						chunkData.triangles [6].AddRange (tempTriangles.ToArray ());
				}
				break;
			case 6: // Planks
				{
					chunkData.triangles [7].AddRange (tempTriangles.ToArray ());
				}
				break;
			case 7: // Leaves
				{
					chunkData.triangles [8].AddRange (tempTriangles.ToArray ());
				}
				break;
		}
        
        // Add UV
        chunkData.uv.AddRange(new Vector2[]
        {
            new Vector3(0f, 0f),
            new Vector3(0f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 0f)
        });
        
        // Increase squareCount                
        squareCount++;
		squareCountCollider++;

    } // CalculateFace 
                    
    void CreateChunkMesh (ChunkMeshData chunkData)
    {
        // Data
        GameObject chunk = new GameObject("Chunk(" + chunkData.x + "|" + chunkData.y + ")");
        Mesh mesh = chunk.AddComponent<MeshFilter>().mesh;
        chunk.transform.position = new Vector3(chunkData.x, chunkData.y, 0);
        chunk.AddComponent<MeshRenderer>().materials = materials;
        chunk.transform.parent = this.transform;

        // Set submesh data
        mesh.subMeshCount = 9;

        // Insert data
        mesh.vertices = chunkData.vertices.ToArray();
        mesh.SetTriangles(chunkData.triangles[0], 0);
        mesh.SetTriangles(chunkData.triangles[1], 1);
        mesh.SetTriangles(chunkData.triangles[2], 2);
        mesh.SetTriangles(chunkData.triangles[3], 3);
        mesh.SetTriangles(chunkData.triangles[4], 4);
		mesh.SetTriangles(chunkData.triangles[5], 5);
		mesh.SetTriangles(chunkData.triangles[6], 6);
		mesh.SetTriangles(chunkData.triangles[7], 7);
		mesh.SetTriangles(chunkData.triangles[8], 8);
        mesh.uv = chunkData.uv.ToArray();

		// Collider mesh
		Mesh colliderMesh = new Mesh();
		colliderMesh.vertices = chunkData.colliderVertices.ToArray();
		colliderMesh.triangles = chunkData.colliderTriangles.ToArray ();

		// Collider object
		GameObject chunkCollider = new GameObject ();
		chunkCollider.AddComponent<MeshCollider> ().sharedMesh = colliderMesh;
		chunkCollider.transform.parent = chunk.transform;
		chunkCollider.transform.position = chunk.transform.position;
		chunkCollider.name = "Collider";
		chunkCollider.tag = "Chunk";

        // Do other shit
        mesh.RecalculateNormals();

		// Remove old chunk, if destroyOld == true
		if (chunkData.destroyOld)
		{
			string name = "Chunk(" + chunkData.x + "|" + chunkData.y + ")";
			Destroy(GameObject.Find(name));
		}

    } // CreateChunkMesh

	void InsertTree (int x, int y)
	{
		// Sometimes randomly skip tree
		if (UnityEngine.Random.Range (0, 11) == 0)
			return;

		// List of leave blocks
		List<Vector2> temp = new List<Vector2> ();
		temp.AddRange (new Vector2[] 
		{
			new Vector2(x - 2, y + 3),
			new Vector2(x - 2, y + 4),
			new Vector2(x - 2, y + 5),
			new Vector2(x - 1, y + 3),
			new Vector2(x - 1, y + 4),
			new Vector2(x - 1, y + 5),
			new Vector2(x - 1, y + 6),
			new Vector2(x, y + 3),
			new Vector2(x, y + 4),
			new Vector2(x, y + 5),
			new Vector2(x, y + 6),
			new Vector2(x + 1, y + 3),
			new Vector2(x + 1, y + 4),
			new Vector2(x + 1, y + 5),
			new Vector2(x + 1, y + 6),
			new Vector2(x + 2, y + 3),
			new Vector2(x + 2, y + 4),
			new Vector2(x + 2, y + 5)
		});

		// Check if there is a block there
		foreach (Vector2 v in temp)
		{
			if (worldBlocks[(int)v.x, (int)v.y].id != 0)
				return;
		}

		// Insert wood
		worldBlocks[x, y] = blocks[5];
		worldBlocks[x, y + 1] = blocks[5];
		worldBlocks[x, y + 2] = blocks[5];

		// Insert leaves
		foreach (Vector2 v in temp)
		{
			worldBlocks[(int)v.x, (int)v.y] = blocks[7];
		}
	}

	void UpdateLoadingScreen ()
	{
		// Update LoadingGame UI
		if (!doneStartup)
		{
			switch (currentAction)
			{
				case LoadingActions.CreatingTerrain:
				{
					slider.value = 0;
					text.text = "Creating terrain.";	
				}
					break;
				case LoadingActions.CreatingChunks:
				{
					slider.value = chunksCreated;
					slider.maxValue = worldWidth * worldHeight;
					text.text = "Creating chunks. (" + chunksCreated + "/" + worldHeight * worldWidth + ")";	
				}
				break;
			}
		}
	}
}

[Serializable]
public struct ChunkMeshData
{
	public int x, y;
	public bool destroyOld;
	
	// Visual
	public List<Vector3> vertices;
	public List<int>[] triangles;
	public List<Vector2> uv;
	
	// Collider
	public List<Vector3> colliderVertices;
	public List<int> colliderTriangles;
	
}

[Serializable]
public struct Block
{

    public string name; // Block name
    public int id; // Block ID

	public float damage;	

}

[Serializable]
public struct Biome
{

    public string name; // Biome name

    public int averageHeight; // The average height
    public int maxHeightDiff; // Highest value
    public int minHeightDiff; // Lowest value

	public bool forest; // Bool to toggle forest

}