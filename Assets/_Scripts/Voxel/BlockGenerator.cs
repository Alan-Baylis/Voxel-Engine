using System.Collections;
using UnityEngine;

public class BlockGenerator : ThreadedBehaviour
{
    [Range( 0, 1 )]
    public float Scale = 0.32f;
    public int Seed;

    [Space]

    public Chunk chunk;
    public Neighbors ChunkNeighbors;


    System.Diagnostics.Stopwatch Datastopwatch;
    System.Diagnostics.Stopwatch Meshstopwatch;
    System.Diagnostics.Stopwatch Debugstopwatch;

    int Octaves = 3;

    float Persistence = .55f;

    const float FRACTION = .10537f;

    void Start()
    {
        Datastopwatch = new System.Diagnostics.Stopwatch();
        Meshstopwatch = new System.Diagnostics.Stopwatch();
        Debugstopwatch = new System.Diagnostics.Stopwatch();

        chunk = new Chunk();

        JobUtility.EnqueueJobOnMostFree( GenerateMap() );
    }

    IEnumerator GenerateMap()
    {
        yield return JobYields.SwitchToWorker;
        Datastopwatch.Start();

        for ( int X = 0 ; X < Chunk.Size ; X++ )
        {
            for ( int Z = 0 ; Z < Chunk.Size ; Z++ )
            {
                for ( int Y = 0 ; Y < Chunk.Size ; Y++ )
                {
                    Debugstopwatch.Start();
                    yield return JobYields.SwitchToMain;

                    float XPoint = ( ( transform.position.x + X ) * FRACTION * Scale ) + Seed;
                    float YPoint = ( ( transform.position.y + Y ) * FRACTION * Scale ) + Seed;
                    float ZPoint = ( ( transform.position.z + Z ) * FRACTION * Scale ) + Seed;

                    int Heigth = Mathf.RoundToInt( Perlin.PerlinOctave( XPoint, YPoint, ZPoint, Octaves, Persistence ) * Chunk.Size - transform.position.y );

                    yield return JobYields.SwitchToWorker;

                    if ( Heigth > Chunk.Size )
                    {
                        break;
                    }

                    if ( Y < Heigth )
                    {
                        chunk.SetBlock( X, Y, Z, ( byte ) Block.Stone );
                    }
                    else
                    {
                        chunk.SetBlock( X, Y, Z, ( byte ) Block.Air );
                    }
                    Debugstopwatch.Stop();
                }
            }
        }
        Datastopwatch.Stop();

        JobUtility.EnqueueJobOnMostFree( RenderChunk() );

        yield return null;
    }

    IEnumerator RenderChunk()
    {
        yield return JobYields.SwitchToWorker;

        Meshstopwatch.Start();

        for ( int X = 0 ; X < Chunk.Size ; X++ )
        {
            for ( int Z = 0 ; Z < Chunk.Size ; Z++ )
            {
                for ( int Y = 0 ; Y < Chunk.Size ; Y++ )
                {
                    if ( chunk.GetBlock( X, Y, Z ) == 1 )
                    {
                        bool Up = false, Down = false, Left = false, Right = false, Forward = false, Backward = false;

                        Vector3 VoxelPos = new Vector3( ( X - Chunk.Size / 2 ) + .5f, ( Y ) + .5f, ( Z - Chunk.Size / 2 ) + .5f );


                        if ( X < Chunk.Size - 1 )
                        {
                            if ( chunk.GetBlock( X + 1, Y, Z ) == 0 )
                            {
                                Right = true;
                            }
                        }

                        if ( X > 0 )
                        {
                            if ( chunk.GetBlock( X - 1, Y, Z ) == 0 )
                            {
                                Left = true;
                            }
                        }


                        if ( Y < Chunk.Size - 1 )
                        {
                            if ( chunk.GetBlock( X, Y + 1, Z ) == 0 )
                            {
                                Up = true;
                            }
                        }

                        if ( Y > 0 )
                        {
                            if ( chunk.GetBlock( X, Y - 1, Z ) == 0 )
                            {
                                Down = true;
                            }
                        }


                        if ( Z < Chunk.Size - 1 )
                        {
                            if ( chunk.GetBlock( X, Y, Z + 1 ) == 0 )
                            {
                                Forward = true;
                            }
                        }

                        if ( Z > 0 )
                        {
                            if ( chunk.GetBlock( X, Y, Z - 1 ) == 0 )
                            {
                                Backward = true;
                            }
                        }





                        yield return JobYields.SwitchToMain;

                        AddMeshFaces( VoxelPos, Up, Down, Left, Right, Forward, Backward );

                        yield return JobYields.SwitchToWorker;
                    }
                }
            }
        }

        Meshstopwatch.Stop();

        yield return JobYields.SwitchToMain;

        OnFinishGeneration();

        yield return null;
    }

    void OnFinishGeneration()
    {
        if ( chunk.Vertices.Count > 0 )
        {
            Mesh mesh = new Mesh()
            {
                vertices = chunk.Vertices.ToArray(),
                triangles = chunk.Triangles.ToArray()
            };
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;

        }


        float DataTime = Mathf.Round( ( float ) ( Datastopwatch.Elapsed.TotalSeconds * 1000 ) ) / 1000;
        float MeshTime = Mathf.Round( ( float ) ( Meshstopwatch.Elapsed.TotalSeconds * 1000 ) ) / 1000;
        float DebugTime = Mathf.Round( ( float ) ( Debugstopwatch.Elapsed.TotalSeconds * 1000 ) ) / 1000;

        float TotalTime = DataTime + MeshTime;

        Debug.Log( "Data: " + DataTime + ", Mesh: " + MeshTime + ", Total: " + TotalTime + ", Debug: " + DebugTime, gameObject );
    }

    void AddMeshFaces( Vector3 VoxelPos, bool Up, bool Down, bool Left, bool Right, bool Forward, bool Backward )
    {
        //Positive X Face
        if ( Right )
        {
            chunk.Vertices.AddRange( new Vector3[] {
                VoxelPos + new Vector3( 0.5f, 0.5f, 0.5f ) ,
                VoxelPos + new Vector3( 0.5f, -0.5f, 0.5f ),
                VoxelPos + new Vector3( 0.5f, 0.5f, -0.5f ),
                VoxelPos + new Vector3( 0.5f, -0.5f, -0.5f )} );


            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 0,
                chunk.Vertices.Count - 4 + 1,
                chunk.Vertices.Count - 4 + 2,
                chunk.Vertices.Count - 4 + 1,
                chunk.Vertices.Count - 4 + 3,
                chunk.Vertices.Count - 4 + 2  } );

        }

        //Negitive X face
        if ( Left )
        {
            chunk.Vertices.AddRange( new Vector3[] { VoxelPos + new Vector3( -0.5f, 0.5f, 0.5f ),
            VoxelPos + new Vector3( -0.5f, -0.5f, 0.5f ),
                VoxelPos + new Vector3( -0.5f, 0.5f, -0.5f ),
                VoxelPos + new Vector3( -0.5f, -0.5f, -0.5f )  } );


            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 0
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 3
            , chunk.Vertices.Count - 4 + 1 } );
        }

        //Positive Y Face
        if ( Up )
        {
            chunk.Vertices.AddRange( new Vector3[] { VoxelPos + new Vector3( -0.5f, 0.5f, 0.5f )
           , VoxelPos + new Vector3( 0.5f, 0.5f, -0.5f )
           ,  VoxelPos + new Vector3( -0.5f, 0.5f, -0.5f)
           , VoxelPos + new Vector3( 0.5f, 0.5f, 0.5f )} );

            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 0
            , chunk.Vertices.Count - 4 + 3
            , chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 0 } );
        }

        //Negitive Y face
        if ( Down )
        {
            chunk.Vertices.AddRange( new Vector3[] { VoxelPos + new Vector3( -0.5f, -0.5f, 0.5f )
            , VoxelPos + new Vector3( 0.5f, -0.5f, -0.5f )
            , VoxelPos + new Vector3( 0.5f, -0.5f, 0.5f )
            , VoxelPos + new Vector3( -0.5f, -0.5f, -0.5f )} );

            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 0
            , chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 3
            , chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 0} );
        }

        //Positive Z Face
        if ( Forward )
        {
            chunk.Vertices.AddRange( new Vector3[] { VoxelPos + new Vector3( 0.5f, 0.5f, 0.5f )
            , VoxelPos + new Vector3( -0.5f, 0.5f, 0.5f )
            , VoxelPos + new Vector3( -0.5f, -0.5f, 0.5f )
            , VoxelPos + new Vector3( 0.5f, -0.5f, 0.5f )} );

            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 0
            , chunk.Vertices.Count - 4 + 1
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 2
            , chunk.Vertices.Count - 4 + 3
            , chunk.Vertices.Count - 4 + 0 } );
        }

        //Negitive Z face
        if ( Backward )
        {
            chunk.Vertices.AddRange( new Vector3[] {
            VoxelPos + new Vector3( -0.5f, 0.5f, -0.5f )
           , VoxelPos + new Vector3( 0.5f, 0.5f, -0.5f )
           , VoxelPos + new Vector3( 0.5f, -0.5f, -0.5f )
           , VoxelPos + new Vector3( -0.5f, -0.5f, -0.5f ) } );

            chunk.Triangles.AddRange( new int[] { chunk.Vertices.Count - 4 + 0
            ,chunk.Vertices.Count - 4 + 1
            ,chunk.Vertices.Count - 4 + 2
            ,chunk.Vertices.Count - 4 + 2
            ,chunk.Vertices.Count - 4 + 3
            ,chunk.Vertices.Count - 4 + 0 } );
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube( new Vector3( transform.position.x, transform.position.y + Chunk.Size / 2, transform.position.z ),
            new Vector3( Chunk.Size, Chunk.Size, Chunk.Size ) );
    }
}

[System.Serializable]
public class Neighbors
{
    public Chunk Northl; //+Z
    public Chunk Southl; //-Z
    public Chunk East;   //+X
    public Chunk West;   //-X
    public Chunk Up;     //+Y
    public Chunk Down;   //-Y
}