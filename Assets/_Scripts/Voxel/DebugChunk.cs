using UnityEngine;

public class DebugChunk : MonoBehaviour
{
    Chunk chunk;

    private void OnDrawGizmosSelected()
    {
        chunk = GetComponent<BlockGenerator>().chunk;


        for ( int X = 0 ; X < Chunk.Size ; X++ )
        {
            for ( int Y = 0 ; Y < Chunk.Size ; Y++ )
            {
                for ( int Z = 0 ; Z < Chunk.Size ; Z++ )
                {
                    if ( chunk.GetBlock( X, Y, Z ) == 1 )
                    {
                        Gizmos.color = Color.gray;
                    }

                    if ( chunk.GetBlock( X, Y, Z ) == 0 )
                    {
                        Gizmos.color = new Color( 0, 0, 0, .5f );
                    }

                    Gizmos.DrawCube( new Vector3( X - Chunk.Size / 2 + 0.5f, Y + 0.5f, Z - Chunk.Size / 2 + 0.5f ), new Vector3( .1f, .1f, .1f ) );
                }
            }
        }
    }
}
