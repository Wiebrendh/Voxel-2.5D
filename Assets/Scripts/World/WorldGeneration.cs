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
    
    void Start ()
    {
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
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                CalculateChunkMeshData(x, y);
            }
        }
    }
    
    struct ChunkMeshData
    {
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
                CalculateFace (x, y, ref chunkData, ref squareCount);
            }
        }
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

        chunkData.vertices.AddRange(tempVertices);
        chunkData.colliderVertices.AddRange(tempVertices);

        // Collider triangles
        if (worldBlocks[x, y].id != 0)
        {
            chunkData.colliderTriangles.AddRange(new int[]
            {
                squareCount * 4,
                (squareCount * 4) + 1,
                (squareCount * 4) + 3,
                (squareCount * 4) + 1,
                (squareCount * 4) + 2,
                (squareCount * 4) + 3
            });
        }

        // Add triangles
        switch (worldBlocks[x, y].id)
        {
            case 1: // Grass
                {
                    chunkData.triangles[1].AddRange(new int[]
                    {
                        squareCount * 4,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 3,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 2,
                        (squareCount * 4) + 3
                    });
                }
                break;
            case 2: // Dirt
                {
                    chunkData.triangles[2].AddRange(new int[]
                    {
                        squareCount * 4,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 3,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 2,
                        (squareCount * 4) + 3
                    });
                }
                break;
            case 3: // Stone
                {
                    chunkData.triangles[3].AddRange(new int[]
                    {
                        squareCount * 4,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 3,
                        (squareCount * 4) + 1,
                        (squareCount * 4) + 2,
                        (squareCount * 4) + 3
                    });
                }
                break;
        }

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