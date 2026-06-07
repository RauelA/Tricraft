using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public static int seed;

    public GameObject chunk;
    
    System.Random random;

    
    // Use this for initialization
    void Start ()
    {
        seed = 0;
        random = new System.Random(seed);

        Vector3 position;
        ChunkGenerator generator = GetComponent<ChunkGenerator>();

        int worldSize = 0;
        
        /*
        for (int i = -worldSize; i <= worldSize; i++)
        {
            for (int j = -worldSize; j <= worldSize; j++)
            {
                
                position =  i * ChunkGenerator.chunkLength * ChunkGenerator.blockOffsetLength
                          + j * ChunkGenerator.chunkWidth * ChunkGenerator.blockOffsetWidth;
                
                GameObject newChunk = Instantiate(chunk, Vector3.zero, Quaternion.identity);
                newChunk.GetComponent<ChunkGO>().SetPosition(position);
                newChunk.GetComponent<ChunkGO>().SetIDs(generator.CreateChunk(position));
                newChunk.GetComponent<ChunkGO>().SetMesh(generator.GetComponent<MeshFilter>().mesh);
                generator.GetComponent<MeshFilter>().mesh = new Mesh();
                
            }
        }
        */
    }
}
