using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGO : MonoBehaviour
{
    public int[,,] blockID;
    public Vector3 position;

    void Awake()
    {
        blockID = new int[
            ChunkGenerator.chunkLength,
            ChunkGenerator.chunkWidth,
            ChunkGenerator.chunkHeight * 3
        ];
    }

    internal void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
    }

    internal void SetID(int indexX, int indexY, int indexZ, int newBlockID)
    {
        blockID[indexX, indexY, indexZ] = newBlockID;
    }

    internal void SetMesh(Mesh newMesh)
    {
        transform.GetComponent<MeshFilter>().mesh = newMesh;
    }
}
