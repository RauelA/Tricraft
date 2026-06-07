using System;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    enum BlockPart
    {
        TetraUp,
        Octa,
        TetraDown
    }

    public Texture2D[] blockIdTexture = new Texture2D[0];

    public GameObject tetraeder;
    public GameObject octaeder;
    public GameObject currentChunk;

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * PARAMETERS
     * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    // Size of Chunks
    public static int chunkLength = 16;
    public static int chunkWidth = 16;
    public static int chunkHeight = 20;

    // Distance between blocks
    // A "block" is an octaeder with a tetraeder at its top (peak to top) and another tetraeder at the bottom (peak to bottom).
    public static Vector3 blockOffsetLength = new Vector3(0, 0, 1);
    public static Vector3 blockOffsetWidth = new Vector3(0.866f, 0, 0.5f);
    public static Vector3 blockOffsetHeight = new Vector3(0.578f, 0.8156f, 1);
    
    // Distance between octaeder (in the center) and the tetraeder to the top
    // (Distance between octaeder and tetraeder to bottom is the same but negative!)
    static Vector3 tetraederPositionOffset = new Vector3(0, 0.6144f, 0);

    // Distance to direct neighbors of the tetraeder to the top
    static Vector3 neighborOfTetraUp1 = -tetraederPositionOffset;
    static Vector3 neighborOfTetraUp2 = new Vector3(0.578f, 0.2012f, 0);
    static Vector3 neighborOfTetraUp3 = new Vector3(-0.288f, 0.2012f, 0.5f);
    static Vector3 neighborOfTetraUp4 = new Vector3(-0.288f, 0.2012f, -0.5f);
    
    // Distance to direct neighbors of the tetraeder to the bottom
    static Vector3 neighborOfTetraDown1 = tetraederPositionOffset;
    static Vector3 neighborOfTetraDown2 = -neighborOfTetraUp2;
    static Vector3 neighborOfTetraDown3 = -neighborOfTetraUp3;
    static Vector3 neighborOfTetraDown4 = -neighborOfTetraUp4;

    // Scaling of block components
    static Vector3 tetraederScale = new Vector3(0.5f, 0.5f, 0.47f);
    static Vector3 octaederScale = new Vector3(0.5f, 0.5f, 0.5f);

    // Ingame rotation of all block components
    static Quaternion tetraederUpQuaternion = Quaternion.Euler(new Vector3(-90, 0, 0));
    static Quaternion octaederQuaternion = Quaternion.Euler(new Vector3(-35.3f, -90, 90));
    static Quaternion tetraederDownQuaternion = Quaternion.Euler(new Vector3(90, 60, 0));
    
    System.Random random;

    // Use this for initialization
    void Start ()
    {
        random = new System.Random(MapGenerator.seed);

        CreateChunk(Vector3.zero);
    }



    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * CREATE CHUNK
     * 
     * A Chunk represents 16 x 16 x 60 Blocks.
     * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    public void CreateChunk(Vector3 position)
    {
        ChunkGO chunkGO = GetComponent<ChunkGO>();
        chunkGO.SetPosition(position);

        SetBlockIDs(chunkGO, position);
        CreateMesh(chunkGO);
    }


    public int[,,] CreateChunkOBSOLETE(Vector3 position)
    {
        ChunkGO chunkGO = GetComponent<ChunkGO>();

        chunkGO.SetPosition(position);

        // Alle Block IDs werden gesetzt
        SetBlockIDs(chunkGO, position);
        
        // Alle "active" Meshes werden erstellt und kombiniert (Combine)
        CreateMesh(chunkGO);

        //return chunkGO;

        int[,,] finalBlockIDs = new int[chunkLength, chunkWidth, chunkHeight * 3]; ;

        Vector3 currentPosition, perlinPosition;
        int tetraUpID, octaID, tetraDownID;
        
        for (int i = 0; i < chunkLength; i++)
        {
            for (int j = 0; j < chunkWidth; j++)
            {
                for (int k = 0; k < chunkHeight; k++)
                {
                    currentPosition = position + (i * blockOffsetLength) + (j * blockOffsetWidth) + (k * blockOffsetHeight);

                    perlinPosition = PerlinOfPosition(currentPosition);
                    
                    tetraUpID = CalculateBlockID(currentPosition - perlinPosition + tetraederPositionOffset);
                    finalBlockIDs[i, j, k * 3 + 2] = tetraUpID;

                    octaID = CalculateBlockID(currentPosition - perlinPosition);
                    finalBlockIDs[i, j, k * 3 + 1] = octaID;

                    tetraDownID = CalculateBlockID(currentPosition - perlinPosition - tetraederPositionOffset);
                    finalBlockIDs[i, j, k * 3] = tetraDownID;

                    CreateBlock(currentPosition, perlinPosition, tetraUpID, octaID, tetraDownID);
                }
            }
        }

        return finalBlockIDs;

    }

    private void CreateMesh(ChunkGO chunkGO)
    {
        Vector3 currentPosition;

        for (int i = 0; i < chunkLength; i++)
        {
            for (int j = 0; j < chunkWidth; j++)
            {
                for (int k = 0; k < chunkHeight; k++)
                {
                    currentPosition =
                        chunkGO.position
                        + (i * blockOffsetLength)
                        + (j * blockOffsetWidth)
                        + (k * blockOffsetHeight);

                    int zBase = k * 3;

                    // Tetraeder up
                    if (CheckBlockActiveState(
                            BlockPart.TetraUp,
                            chunkGO,
                            i,
                            j,
                            zBase + 2))
                    {
                        GameObject tetraUp = Instantiate(
                            tetraeder,
                            currentPosition + tetraederPositionOffset,
                            tetraederUpQuaternion
                        );
                        tetraUp.transform.localScale = tetraederScale;
                        tetraUp.transform.parent = transform;
                    }

                    // Octaeder
                    if (CheckBlockActiveState(
                            BlockPart.Octa,
                            chunkGO,
                            i,
                            j,
                            zBase + 1))
                    {
                        GameObject octa = Instantiate(
                            octaeder,
                            currentPosition,
                            octaederQuaternion
                        );
                        octa.transform.localScale = octaederScale;
                        octa.transform.parent = transform;
                    }

                    // Tetraeder down
                    if (CheckBlockActiveState(
                            BlockPart.TetraDown,
                            chunkGO,
                            i,
                            j,
                            zBase))
                    {
                        GameObject tetraDown = Instantiate(
                            tetraeder,
                            currentPosition - tetraederPositionOffset,
                            tetraederDownQuaternion
                        );
                        tetraDown.transform.localScale = tetraederScale;
                        tetraDown.transform.parent = transform;
                    }
                }
            }
        }
    }



    private void CreateMeshOBSOLETE(ChunkGO chunkGO)
    {
        Vector3 currentPosition;

        for (int i = 0; i < chunkLength; i++)
        {
            for (int j = 0; j < chunkWidth; j++)
            {
                for (int k = 0; k < chunkHeight; k++)
                {
                    currentPosition = chunkGO.GetComponent<ChunkGO>().position + (i * blockOffsetLength) + (j * blockOffsetWidth) + (k * blockOffsetHeight);

                    if (CheckBlockActiveState("TETRAUP", chunkGO, i, j, k * 3 + 2))
                    {
                        GameObject tetraederUpGO = Instantiate(tetraeder, currentPosition + tetraederPositionOffset, tetraederUpQuaternion);
                        //tetraederUpGO.GetComponent<MeshRenderer>().material.mainTexture = blockIdTexture[tetraederUpID];
                        tetraederUpGO.transform.localScale = tetraederScale;
                        tetraederUpGO.transform.parent = transform;
                        //Combine(tetraederUpGO);
                    }
                }
            }
        }
    }

    private void SetBlockIDs(ChunkGO chunkGO, Vector3 position)
    {
        Vector3 currentPosition, perlinPosition;
        int tetraUpID, octaID, tetraDownID;

        for (int i = 0; i < chunkLength; i++)
        {
            for (int j = 0; j < chunkWidth; j++)
            {
                for (int k = 0; k < chunkHeight; k++)
                {
                    currentPosition = position + (i * blockOffsetLength) + (j * blockOffsetWidth) + (k * blockOffsetHeight);

                    perlinPosition = PerlinOfPosition(currentPosition);

                    tetraUpID = CalculateBlockID(currentPosition - perlinPosition + tetraederPositionOffset);
                    chunkGO.GetComponent<ChunkGO>().SetID(i, j, k * 3 + 2, tetraUpID);
                    
                    octaID = CalculateBlockID(currentPosition - perlinPosition);
                    chunkGO.GetComponent<ChunkGO>().SetID(i, j, k * 3 + 1, octaID);

                    tetraDownID = CalculateBlockID(currentPosition - perlinPosition - tetraederPositionOffset);
                    chunkGO.GetComponent<ChunkGO>().SetID(i, j, k * 3, tetraDownID);
                }
            }
        }
    }

    private bool CheckBlockActiveState(String blockPartType, ChunkGO chunk, int xInChunk, int yInChunk, int zInChunk)
    {
        bool isActive = false;

        try
        {
            switch (blockPartType)
            {
                case "TETRAUP":
                    if (chunk.blockID[xInChunk - 1, yInChunk - 1, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk - 1, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk - 1, yInChunk, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk, zInChunk - 1] == 0)
                    { isActive = true; }
                    break;

                case "TETRADOWN":
                    if (chunk.blockID[xInChunk + 1, yInChunk + 1, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk + 1, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk + 1, yInChunk, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk, zInChunk + 1] == 0)
                    { isActive = true; }
                    break;

                case "OCTA":
                    if (chunk.blockID[xInChunk - 1, yInChunk - 1, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk - 1, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk - 1, yInChunk, zInChunk + 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk, zInChunk - 1] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk + 1, yInChunk + 1, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk + 1, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk + 1, yInChunk, zInChunk - 2] == 0)
                    { isActive = true; }
                    else if (chunk.blockID[xInChunk, yInChunk, zInChunk + 1] == 0)
                    { isActive = true; }
                    break;
            }
        }
        catch (Exception)
        {

            throw;
        }

        return isActive;
    }


    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * CALCULATE BLOCK ID
     * 
     * The ID represents the blocks kind. 0 is Air (empty). The others will change the texture.
     * 
     * BLOCK ID LIST
     * 
     * 000: Air (empty)
     * 
     * 001: Stone (light grey)
     * 002: Stone (grey)
     * 003: Stone (dark grey)
     * ...  >PLACEHOLDER<
     * 010: Dirt (light brown)
     * 011: Dirt (brown)
     * 012: Dirt (dark brown)
     * ...  >PLACEHOLDER<
     * 020: Grass (yellow green)
     * 021: Grass (green)
     * 022: Grass (darker green)
     * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    private int CalculateBlockID(Vector3 position)
    {
        int finalID = 0;    // Air


        // HEIGHT BASED IDS
        //
        if(position.y < 30)
        {
            finalID = random.Next(1, 4);    // Stone
        }
        else if (position.y < 38)
        {
            finalID = random.Next(10, 13);    // Dirt
        }
        else if (position.y < 40)
        {
            finalID = random.Next(20, 23);    // Grass
        }

        // CAVE BASED IDS
        //
        //finalID = CaveCheck(position, finalID);

        return finalID;
    }

    int smallCaveHeight = 30;
    int mediumCaveHeight = 40;
    int largeCaveHeight = 50;
    
    private int CaveCheck(Vector3 position, int currentID)
    {
        int finalID = currentID;

        if (  60 + Mathf.PerlinNoise(position.x * 0.2f, position.z * 0.2f) * 20 > position.y * 5 - (position.x - 20) * Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f)
           && 30 + Mathf.PerlinNoise(position.x * 0.2f, position.z * 0.2f) * 20 < position.y * 5 - (position.x - 20) * Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f)
           && 60 + Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f) * 20 > position.z * 5 - (position.x - 20) * Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f)
           && 30 + Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f) * 20 < position.z * 5 - (position.x - 20) * Mathf.PerlinNoise(position.x * 0.2f, position.y * 0.2f))
        {
            finalID = 0;
        }

        /*
        if (60 + Mathf.Sin((position.x + position.z) * 0.2f) * 20 > position.y * 5
           && 30 + Mathf.Sin((position.x + position.z) * 0.2f) * 20 < position.y * 5
           && 60 + Mathf.Sin((position.x + position.y) * 0.2f) * 20 > position.z * 5
           && 30 + Mathf.Sin((position.x + position.y) * 0.2f) * 20 < position.z * 5)
        {
            finalID = 0;
        }
        */

        return finalID;
    }


    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * CREATE BLOCK
     * 
     * A "Block" is an octaeder with an tetraeder at its top (tetraederUp) and its bottom (tetraederDown).
     * All three parts have an specific integer ID representing its kind.
     * 
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
    void CreateBlock(Vector3 position, Vector3 perlinPosition, int tetraederUpID, int octaederID, int tetraederDownID)
    {
        //bool tetraUpIsActive = CheckBlockActiveState("TETRA_UP", position + tetraederPositionOffset);
        //bool octaIsActive = CheckBlockActiveState("OCTA", position);
        //bool tetraDownIsActive = CheckBlockActiveState("TETRA_DOWN", position - tetraederPositionOffset);
        
        if (true)
        {
            GameObject tetraederUpGO = Instantiate(tetraeder, position + tetraederPositionOffset, tetraederUpQuaternion);
            //tetraederUpGO.GetComponent<MeshRenderer>().material.mainTexture = blockIdTexture[tetraederUpID];
            tetraederUpGO.transform.localScale = tetraederScale;
            tetraederUpGO.transform.parent = transform;
            //Combine(tetraederUpGO);
        }
        if (true)
        {
            GameObject octaederGO = Instantiate(octaeder, position, octaederQuaternion);
            //octaederGO.GetComponent<MeshRenderer>().material.mainTexture = blockIdTexture[octaederID];
            octaederGO.transform.localScale = octaederScale;
            octaederGO.transform.parent = transform;
            //Combine(octaederGO);

        }
        if (true)
        {
            GameObject tetraederDownGO = Instantiate(tetraeder, position - tetraederPositionOffset, tetraederDownQuaternion);
            //tetraederDownGO.GetComponent<MeshRenderer>().material.mainTexture = blockIdTexture[tetraederDownID];
            tetraederDownGO.transform.localScale = tetraederScale;
            tetraederDownGO.transform.parent = transform;
            //Combine(tetraederDownGO);
        }
    }

    private void Combine(GameObject block)
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Destroy(gameObject.GetComponent<MeshCollider>());

        Vector2[] oldMeshUVs = transform.GetComponent<MeshFilter>().mesh.uv;
        
        int i = 0;

        while(i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);

        int numberOfVertices = transform.GetComponent<MeshFilter>().mesh.vertices.Length;
        Vector2[] newMeshUVs = new Vector2[oldMeshUVs.Length + numberOfVertices];

        for (int j = 0; j < oldMeshUVs.Length; j++)
        {
            newMeshUVs[i] = oldMeshUVs[i];
        }
        for (int j = 0; j < numberOfVertices; j++)
        {
            //newMeshUVs[oldMeshUVs.Length - j] = 
        }
        
        transform.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        transform.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        
        transform.gameObject.SetActive(true);

        Destroy(block);
    }


    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * CHECK BLOCK ACTIVE STATE
     * 
     * Only if a direct contacted neighbor is Air (ID = 0), set to active and show it ingame.
     * The tetraeders has 4 neighbors, the octaeder 8.
     * After checking the BLOCK active state, later will be checked, which MESH will be shown.
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    private bool CheckBlockActiveState(
        BlockPart blockPart,
        ChunkGO chunk,
        int x,
        int y,
        int z)
    {
        switch (blockPart)
        {
            case BlockPart.TetraUp:
                return
                    IsAir(chunk, x - 1, y - 1, z + 2) ||
                    IsAir(chunk, x, y - 1, z + 2) ||
                    IsAir(chunk, x - 1, y, z + 2) ||
                    IsAir(chunk, x, y, z - 1);

            case BlockPart.TetraDown:
                return
                    IsAir(chunk, x + 1, y + 1, z - 2) ||
                    IsAir(chunk, x, y + 1, z - 2) ||
                    IsAir(chunk, x + 1, y, z - 2) ||
                    IsAir(chunk, x, y, z + 1);

            case BlockPart.Octa:
                return
                    IsAir(chunk, x - 1, y - 1, z + 2) ||
                    IsAir(chunk, x, y - 1, z + 2) ||
                    IsAir(chunk, x - 1, y, z + 2) ||
                    IsAir(chunk, x, y, z - 1) ||
                    IsAir(chunk, x + 1, y + 1, z - 2) ||
                    IsAir(chunk, x, y + 1, z - 2) ||
                    IsAir(chunk, x + 1, y, z - 2) ||
                    IsAir(chunk, x, y, z + 1);
        }

        return false;
    }

    bool IsAir(ChunkGO chunk, int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
            return true;

        if (x >= chunkLength ||
            y >= chunkWidth ||
            z >= chunkHeight * 3)
            return true;

        return chunk.blockID[x, y, z] == 0;
    }


    private bool CheckBlockActiveStateOBSOLETE(string blockPart, Vector3 position)
    {
        bool isActive = false;
        Vector3 currentPosition;
        
        switch (blockPart)
        {
            case "TETRA_UP":
                currentPosition = position + neighborOfTetraUp1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;

            case "TETRA_DOWN":
                currentPosition = position + neighborOfTetraDown1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;

            case "OCTA":
                currentPosition = position + neighborOfTetraUp1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;
        }

        return isActive;
    }
    
    
    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * CHECK MESH ACTIVE STATE
     * 
     * Only if a direct contacted neighbor is Air (ID = 0), set to active and show it ingame.
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
    private bool CheckMeshActiveState(string blockPart, Vector3 position)
    {
        bool isActive = false;
        Vector3 currentPosition;

        switch (blockPart)
        {
            case "TETRA_UP":
                currentPosition = position + neighborOfTetraUp1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;

            case "TETRA_DOWN":
                currentPosition = position + neighborOfTetraDown1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;

            case "OCTA":
                currentPosition = position + neighborOfTetraUp1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraUp4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown1;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown2;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown3;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }

                currentPosition = position + neighborOfTetraDown4;
                currentPosition -= PerlinOfPosition(currentPosition);
                if (CalculateBlockID(currentPosition) == 0)
                { isActive = true; }
                break;
        }

        return isActive;
    }

    private Vector3 PerlinOfPosition(Vector3 position)
    {
        float perlinHeight = Mathf.PerlinNoise(position.x * 0.07f, position.z * 0.07f);
        Vector3 perlinedPosition = new Vector3(0, perlinHeight * 4, 0);

        return perlinedPosition;
    }
}
