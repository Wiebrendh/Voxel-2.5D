using System;
using UnityEngine;
using System.Collections.Generic;

public class WorldGeneration : MonoBehaviour 
{

    // Generation data
    [SerializeField] int chunkSize;
    [SerializeField] int worldWidth;
    [SerializeField] int worldHeight;
    
    // Blocks and biomes 
    [SerializeField] Block[] blocks;
    [SerializeField] Biome[] biomes;
    [SerializeField] Material[] materials;    
    
    // World data
    Biome[] worldBiomes;
    Block[,] worldBlocks;
    GameObject[,] worldChunks;
    [SerializeField] int chunkCount;
     
    // TEST
    public bool doneCreatingChunks;
    public List<ChunkMeshData> queu;
    
    void Start ()
    {
        // Calculate chunkCount
        chunkCount = worldWidth * worldHeight;
        
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
        Biome biomeType = null;
        for (int b = 0; b < worldBiomes.Length; b++)
        {
            // Set random biome length
            if (wantedLength == 0)
            {
                wantedLength = 2;
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
            for (int i = 0; i < chunkSize * worldWidth; i += 8)
            {
                // Create animationcurve for grass
                { 
                    // Get current biome
                    Biome currentBiome = worldBiomes[i / 15];

                    // Determine height
                    int height = 64;
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
                int dirt = (int)worldGenDirt.Evaluate(l);
                while (worldBlocks[l, dirt] == blocks[0])
                {
                    worldBlocks[l, dirt] = blocks[2];
                    dirt++;
                }

                // Stone
                int stone = 0;
                while (worldBlocks[l, stone] == blocks[0])
                {
                    worldBlocks[l, stone] = blocks[3];
                    stone++;
                }
            }
        }
        
        // Loop to create chunks
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                CalculateChunkMeshData(x, y);
            }
        }
    }
    
    void Update ()
    {
        if (queu.Count == chunkCount)
            doneCreatingChunks = true;
    }
    
    [Serializable]
    public struct ChunkMeshData
    {
        public int x, y;
        
        // Visual
        public List<Vector3> vertices;
        public List<int>[] triangles;
        public List<Vector2> uv;
        
        // Collider
        public List<Vector3> colliderVertices;
        public List<int> colliderTriangles;
    }
    
    void CalculateChunkMeshData (int startX, int startY)
    {
        ChunkMeshData chunkData = new ChunkMeshData(); 
        
        // Visual data
        chunkData.vertices = new List<Vector3>();
        chunkData.triangles = new List<int>[8]{ new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        chunkData.uv = new List<Vector2>();
        
        // Collider data
        chunkData.colliderVertices = new List<Vector3>();
        chunkData.colliderTriangles = new List<int >();
        
        int squareCount = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Calculate front
                CalculateFace (startX + x, startY + y, ref chunkData, ref squareCount);
            }
        }
        
        // Add the ChunkMeshData to the queu
        queu.Add(chunkData);
    }   
    
    void CalculateFace (int x, int y, ref ChunkMeshData chunkData, ref int squareCount)
    {
        // Add vertices
        Vector3[] tempVertices = new Vector3[]
        {
            new Vector3(x, y, -.5f),
            new Vector3(x, y + 1f, -.5f),
            new Vector3(x + 1f, y + 1f, -.5f),
            new Vector3(x + 1f, y, -.5f)
        };
        chunkData.vertices.AddRange(tempVertices); // Visual vertices
        chunkData.colliderVertices.AddRange(tempVertices); // Collider vertices
        
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
        
        switch (worldBlocks[x, y].id) // Visual triangles
        {
            case 0: // Grass top
                chunkData.triangles[0].AddRange(tempTriangles.ToArray());
                break;
            case 1: // Grass front
                chunkData.triangles[1].AddRange(tempTriangles.ToArray());
                break;
            case 2: // Dirt
                chunkData.triangles[2].AddRange(tempTriangles.ToArray());
                break;
            case 3: // Stone
                chunkData.triangles[3].AddRange(tempTriangles.ToArray());
                break;
            case 4: // Cobblestone
                chunkData.triangles[4].AddRange(tempTriangles.ToArray());
                break;
            case 5: // Wood top
                chunkData.triangles[5].AddRange(tempTriangles.ToArray());
                break;
            case 6: // Wood front
                chunkData.triangles[6].AddRange(tempTriangles.ToArray());
                break;
            case 7: // Planks
                chunkData.triangles[7].AddRange(tempTriangles.ToArray());
                break;
        }        
        chunkData.colliderTriangles.AddRange(tempTriangles.ToArray()); // Collider triangles
        
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
    }
                        
                    
    void CreateChunkMesh (ChunkMeshData chunkData)
    {
        // Data
        GameObject chunk = new GameObject("Chunk" + chunkData.x);
        Mesh mesh = chunk.AddComponent<MeshFilter>().mesh;
        chunk.transform.position = new Vector3(chunkData.x, chunkData.y, 0);
        chunk.AddComponent<MeshRenderer>().materials = materials;
        chunk.transform.parent = this.transform;
        
        
    } 
}

[Serializable]
public class Block
{

    public string name; // Block name
    public int id; // Block ID

}

[Serializable]
public class Biome
{

    public string name; // Biome name

    public int averageHeight; // The average height
    public int maxHeightDiff; // Highest value
    public int minHeightDiff; // Lowest value

}