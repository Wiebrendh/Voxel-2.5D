using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

public class WorldGenerationOld : MonoBehaviour
{
   /* 
    // Generation data
    [SerializeField] int worldMaxHeight;
    [SerializeField] public int chunkWidth;
    [SerializeField] public int worldWidthInChunks;
    [SerializeField] Block[] blocks;
    [SerializeField] Biome[] biomes;
    [SerializeField] Material[] materials;

    // Generated data
    [SerializeField] Biome[] worldBiomes;
    [SerializeField] public Block[,] worldBlocks;
    [SerializeField] List<GameObject> chunks = new List<GameObject>();

    // TEST
    int currX, currY;
	            
	void Start ()
    {   
        // Calculate width and assign world 2D array
        int width = worldWidthInChunks * chunkWidth;
        worldBlocks = new Block[width, worldMaxHeight];

        // Set all blocks in world to air at start
        for (int y = 0; y < worldMaxHeight; y++)
        {
            for (int x = 0; x < width; x++)
            {
                worldBlocks[x, y] = blocks[0];
            }
        }

        // Set biomes
        worldBiomes = new Biome[worldWidthInChunks];
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
            for (int i = 0; i < chunkWidth * worldWidthInChunks; i += 8)
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
            for (int l = 0; l < chunkWidth * worldWidthInChunks; l++)
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

        // Create chunk meshes
        for (int i = 0; i < worldWidthInChunks; i++)
        {
            StartCoroutine(CreateChunkMesh(i * chunkWidth));
        }
	}

    IEnumerator CreateChunkMesh (int start)
    {
        // Data
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int>[] triangles = new List<int>[8]{ new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        GameObject chunk;
        Mesh mesh;

        // Set data
        chunk = new GameObject("Chunk" + start);
        mesh = chunk.AddComponent<MeshFilter>().mesh;
        chunk.transform.position = new Vector3(start, 0, 0);
        chunk.AddComponent<MeshRenderer>().materials = materials;
        chunk.transform.parent = this.transform;

        // Collider
        MeshCollider collider = chunk.AddComponent<MeshCollider>();
        List<Vector3> colliderVertices = new List<Vector3>();
        List<int> colliderTriangles = new List<int>();
        Mesh colliderMesh = new Mesh();

        // Set submesh data
        mesh.Clear();
        mesh.subMeshCount = 8;

        // Insert data to lists
        int squareCount = 0;
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < worldMaxHeight - 1; y++)
            {

                #region Front

                // Check if the current x&y is not air
                if (worldBlocks[start + x, y].id != 0)
                {                    
                    // Add vertices
                    Vector3[] tempVertices = new Vector3[]
                    {
                        new Vector3(x - .5f, y - .5f, -.5f),
                        new Vector3(x - .5f, y + .5f, -.5f),
                        new Vector3(x + .5f, y + .5f, -.5f),
                        new Vector3(x + .5f, y - .5f, -.5f)
                    };

                    vertices.AddRange(tempVertices);
                    colliderVertices.AddRange(tempVertices);

                    // Collider triangles
                    if (worldBlocks[start + x, y].id != 0)
                    {
                        colliderTriangles.AddRange(new int[]
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
                    switch (worldBlocks[start + x, y].id)
                    {
                        case 1: // Grass
                            {
                                triangles[1].AddRange(new int[]
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
                                triangles[2].AddRange(new int[]
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
                                triangles[3].AddRange(new int[]
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

                    uv.AddRange(new Vector2[]
                    {
                        new Vector3(0f, 0f),
                        new Vector3(0f, 1f),
                        new Vector3(1f, 1f),
                        new Vector3(1f, 0f)
                    });

                    // Increase squarecount
                    squareCount++;
                }

                #endregion

                #region Top

                // Check if the current x&y is not air
                if (worldBlocks[start + x, y + 1].id == 0)
                {
                    // Add vertices
                    Vector3[] tempVertices = new Vector3[]
                    {
                        new Vector3(x - .5f, y + .5f, -.5f),
                        new Vector3(x - .5f, y + .5f, .5f),
                        new Vector3(x + .5f, y + .5f, .5f),
                        new Vector3(x + .5f, y + .5f, -.5f)
                    };

                    vertices.AddRange(tempVertices);
                    colliderVertices.AddRange(tempVertices);

                    // Collider triangles
                    if (worldBlocks[start + x, y].id != 0)
                    {
                        colliderTriangles.AddRange(new int[]
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
                    switch (worldBlocks[start + x, y].id)
                    {
                        case 1: // Grass
                            {
                                triangles[0].AddRange(new int[]
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
                                triangles[2].AddRange(new int[]
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
                                triangles[3].AddRange(new int[]
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

                    uv.AddRange(new Vector2[]
                    {
                        new Vector3(0f, 0f),
                        new Vector3(0f, 1f),
                        new Vector3(1f, 1f),
                        new Vector3(1f, 0f)
                    });

                    // Increase squarecount
                    squareCount++;
                }

                #endregion

                #region Bottom

                // Check if the current x&y is not air
                if (y > 0 && worldBlocks[start + x, y - 1].id == 0 || y == 0)
                {
                    // Add vertices
                    Vector3[] tempVertices = new Vector3[]
                    {
                        new Vector3(x - .5f, y - .5f, .5f),
                        new Vector3(x + .5f, y - .5f, .5f),
                        new Vector3(x + .5f, y - .5f, -.5f),
                        new Vector3(x - .5f, y - .5f, -.5f)
                    };

                    vertices.AddRange(tempVertices);
                    colliderVertices.AddRange(tempVertices);

                    // Collider triangles
                    if (worldBlocks[start + x, y].id != 0)
                    {
                        colliderTriangles.AddRange(new int[]
                        {
                            (squareCount * 4) + 3,
                            (squareCount * 4) + 1,
                            squareCount * 4,
                            (squareCount * 4) + 3,
                            (squareCount * 4) + 2,
                            (squareCount * 4) + 1
                        });
                    }

                    // Add triangles
                    switch (worldBlocks[start + x, y].id)
                    {
                        case 1: // Grass
                            {
                                triangles[1].AddRange(new int[]
                                {
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 1,
                                    squareCount * 4,
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 2,
                                    (squareCount * 4) + 1
                                });


                            }
                            break;
                        case 2: // Dirt
                            {
                                triangles[2].AddRange(new int[]
                                {
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 1,
                                    squareCount * 4,
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 2,
                                    (squareCount * 4) + 1
                                });
                            }
                            break;
                        case 3: // Stone
                            {
                                triangles[3].AddRange(new int[]
                                {
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 1,
                                    squareCount * 4,
                                    (squareCount * 4) + 3,
                                    (squareCount * 4) + 2,
                                    (squareCount * 4) + 1
                                });
                            }
                            break;
                    }

                    uv.AddRange(new Vector2[]
                    {
                        new Vector3(0f, 0f),
                        new Vector3(0f, 1f),
                        new Vector3(1f, 1f),
                        new Vector3(1f, 0f)
                    });

                    // Increase squarecount
                    squareCount++;
                }

                #endregion

                #region Left

                // Check if the current x&y is not air
                if (start + x > 0 && worldBlocks[start + x - 1, y].id == 0 || start + x == 0)
                {
                    // Add vertices
                    Vector3[] tempVertices = new Vector3[]
                    {
                        new Vector3(x - .5f, y - .5f, .5f),
                        new Vector3(x - .5f, y + .5f, .5f),
                        new Vector3(x - .5f, y + .5f, -.5f),
                        new Vector3(x - .5f, y - .5f, -.5f)
                    };

                    vertices.AddRange(tempVertices);
                    colliderVertices.AddRange(tempVertices);

                    // Collider triangles
                    if (worldBlocks[start + x, y].id != 0)
                    {
                        colliderTriangles.AddRange(new int[]
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
                    switch (worldBlocks[start + x, y].id)
                    {
                        case 1: // Grass
                            {
                                triangles[1].AddRange(new int[]
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
                                triangles[2].AddRange(new int[]
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
                                triangles[3].AddRange(new int[]
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

                    uv.AddRange(new Vector2[]
                    {
                        new Vector3(0f, 0f),
                        new Vector3(0f, 1f),
                        new Vector3(1f, 1f),
                        new Vector3(1f, 0f)
                    });

                    // Increase squarecount
                    squareCount++;
                }

                #endregion

                #region Right

                // Check if the current x&y is not air
                if (start + x < chunkWidth * worldWidthInChunks - 1 && worldBlocks[start + x + 1, y].id == 0 || start + x == chunkWidth * worldWidthInChunks - 1)
                {
                    // Add vertices
                    Vector3[] tempVertices = new Vector3[]
                    {
                        new Vector3(x + .5f, y - .5f, -.5f),
                        new Vector3(x + .5f, y + .5f, -.5f),
                        new Vector3(x + .5f, y + .5f, .5f),
                        new Vector3(x + .5f, y - .5f, .5f)
                    };

                    vertices.AddRange(tempVertices);
                    colliderVertices.AddRange(tempVertices);

                    // Collider triangles
                    if (worldBlocks[start + x, y].id != 0)
                    {
                        colliderTriangles.AddRange(new int[]
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
                    switch (worldBlocks[start + x, y].id)
                    {
                        case 1: // Grass
                            {
                                triangles[1].AddRange(new int[]
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
                                triangles[2].AddRange(new int[]
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
                                triangles[3].AddRange(new int[]
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

                    uv.AddRange(new Vector2[]
                    {
                        new Vector3(0f, 0f),
                        new Vector3(0f, 1f),
                        new Vector3(1f, 1f),
                        new Vector3(1f, 0f)
                    });

                    // Increase squarecount
                    squareCount++;
                }

                #endregion
            }
        }

        #region Apply mesh data

        mesh.vertices = vertices.ToArray();
        mesh.SetTriangles(triangles[0], 0);
        mesh.SetTriangles(triangles[1], 1);
        mesh.SetTriangles(triangles[2], 2);
        mesh.SetTriangles(triangles[3], 3);
        mesh.SetTriangles(triangles[4], 4);
        mesh.SetTriangles(triangles[5], 5);
        mesh.SetTriangles(triangles[6], 6);
        mesh.SetTriangles(triangles[7], 7);
        mesh.uv = uv.ToArray();
        c

        #endregion

        #region Apply collider data

        colliderMesh.vertices = colliderVertices.ToArray();
        colliderMesh.triangles = colliderTriangles.ToArray();
        collider.sharedMesh = colliderMesh;

        #endregion
        
        yield return 0;
    }*/
}
/*
[Serializable]
public class Biome
{

    public string name; // Biome name

    public int averageHeight; // The average height
    public int maxHeightDiff;
    public int minHeightDiff;

}

[Serializable]
public class Block
{

    public string name;
    public int id;

}
*/