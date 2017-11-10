using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform Viewer;

    [Range( 1, 16 )]
    public int RenderDistance;

    [Space]

    public GameObject ChunkPrefab;

    Dictionary<Vector3, GameObject> Chunks = new Dictionary<Vector3, GameObject>();

    public bool Moved = false;

    private void OnEnable()
    {
        Moved = true;
    }

    private void Update()
    {
        if ( Viewer.transform.position.x % Chunk.Size == 1 )
        {
            Moved = true;
        }


        if ( Moved )
        {
            FindChunksToLoad();
            DeleteChunks();
            Moved = false;
        }
    }

    void FindChunksToLoad()
    {
        int PosX = ( int ) Viewer.transform.position.x;
        int PosY = ( int ) Viewer.transform.position.y;
        int PosZ = ( int ) Viewer.transform.position.z;

        for ( int X = PosX - ( RenderDistance * Chunk.Size ) ; X < PosX + ( RenderDistance * Chunk.Size ) ; X += Chunk.Size )
        {
            for ( int Y = PosY - ( RenderDistance * Chunk.Size ) ; Y < PosY + ( ( RenderDistance ) * Chunk.Size ) ; Y += Chunk.Size )
            {
                for ( int Z = PosZ - ( RenderDistance * Chunk.Size ) ; Z < PosZ + ( RenderDistance * Chunk.Size ) ; Z += Chunk.Size )
                {
                    float Distance = Vector3.Distance( Viewer.transform.position, new Vector3( X, Y, Z ) );

                    if ( Mathf.Abs( Distance ) < RenderDistance * Chunk.Size )
                    {
                        MakeChunkAt( X, Y, Z );
                    }
                }
            }
        }
    }

    void MakeChunkAt( int X, int Y, int Z )
    {
        X = Mathf.FloorToInt( X / ( float ) Chunk.Size ) * Chunk.Size;
        Y = Mathf.FloorToInt( Y / ( float ) Chunk.Size ) * Chunk.Size;
        Z = Mathf.FloorToInt( Z / ( float ) Chunk.Size ) * Chunk.Size;

        if ( Chunks.ContainsKey( new Vector3( X, Y, Z ) ) == false )
        {
            GameObject go = SimplePool.Spawn( ChunkPrefab, new Vector3( X, Y, Z ), Quaternion.identity, transform );
            go.name = "Chunk : " + ( X / Chunk.Size ) + "," + ( Y / Chunk.Size ) + "," + ( Z / Chunk.Size );
            Chunks.Add( new Vector3( X, Y, Z ), go );
        }
    }

    void DeleteChunks()
    {
        List<GameObject> DeleteChunks = new List<GameObject>( Chunks.Values );
        Queue<GameObject> DeleteQueue = new Queue<GameObject>();

        for ( int i = 0 ; i < DeleteChunks.Count ; i++ )
        {
            float Distance = Vector3.Distance( Viewer.transform.position
                , DeleteChunks[i].transform.position );

            if ( Distance > ( RenderDistance ) * Chunk.Size )
            {
                DeleteQueue.Enqueue( DeleteChunks[i] );
            }
        }

        while ( DeleteQueue.Count > 0 )
        {
            GameObject chunk = DeleteQueue.Dequeue();
            Chunks.Remove( chunk.transform.position );
            Destroy( chunk );
        }
    }
}