using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chunk
{
    public static int Size = 32;

    public List<int> Triangles;
    public List<Vector3> Vertices;

    byte[,,] Data;

    public Chunk()
    {
        Triangles = new List<int>();
        Vertices = new List<Vector3>();
        Data = new byte[Size, Size, Size];
    }

    public byte GetBlock( int X, int Y, int Z )
    {
        return Data[X, Y, Z];
    }

    public void SetBlock( int X, int Y, int Z, int Value )
    {
        Data[X, Y, Z] = ( byte ) Value;
    }

}