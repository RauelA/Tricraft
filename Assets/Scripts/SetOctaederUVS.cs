using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOctaederUVS : MonoBehaviour
{
    public float pixelSize = 1;
    public float tileX = 1;
    public float tileY = 1;

    // Use this for initialization
    void Start ()
    {
        float tilePerc = 1 / pixelSize;

        float uMin = tilePerc * tileX;
        float uMax = tilePerc * (tileX + 1);
        float vMin = tilePerc * tileY;
        float vMax = tilePerc * (tileY + 1);

        Vector2[] blockUVs = new Vector2[24];

        blockUVs[0] = new Vector2(uMin, vMin);
        blockUVs[1] = new Vector2(uMax, vMin);
        blockUVs[2] = new Vector2(uMin, vMax);
        blockUVs[3] = new Vector2(uMax, vMax);
        blockUVs[4] = new Vector2(uMin, vMax);
        blockUVs[5] = new Vector2(uMax, vMax);
        blockUVs[6] = new Vector2(uMin, vMax);
        blockUVs[7] = new Vector2(uMax, vMax);
        blockUVs[8] = new Vector2(uMin, vMin);
        blockUVs[9] = new Vector2(uMax, vMin);
        blockUVs[10] = new Vector2(uMin, vMin);
        blockUVs[11] = new Vector2(uMax, vMax);
        blockUVs[12] = new Vector2(uMin, vMin);
        blockUVs[13] = new Vector2(uMin, vMax);
        blockUVs[14] = new Vector2(uMax, vMax);
        blockUVs[15] = new Vector2(uMax, vMin);
        blockUVs[16] = new Vector2(uMin, vMin);
        blockUVs[17] = new Vector2(uMin, vMax);
        blockUVs[18] = new Vector2(uMax, vMax);
        blockUVs[19] = new Vector2(uMax, vMin);
        blockUVs[20] = new Vector2(uMin, vMin);
        blockUVs[21] = new Vector2(uMin, vMax);
        blockUVs[22] = new Vector2(uMax, vMax);
        blockUVs[23] = new Vector2(uMax, vMax);

        GetComponent<MeshFilter>().mesh.uv = blockUVs;
    }
}
