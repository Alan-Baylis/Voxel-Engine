using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    const int DEFAULT_POOL_SIZE = 3;

    /// <summary>
    /// The Pool class represents the pool for a particular prefab.
    /// </summary>
    class Pool
    {
        int nextId = 1;

        Stack<GameObject> inactive;

        GameObject prefab;
        Transform transform;

        public Pool( GameObject prefab, int initialQty, Transform transform )
        {
            this.prefab = prefab;
            this.transform = transform;

            inactive = new Stack<GameObject>( initialQty );
        }


        public GameObject Spawn( Vector3 pos, Quaternion rot, Transform tran )
        {
            GameObject obj;

            transform = tran;

            if ( inactive.Count == 0 )
            {
                obj = UnityEngine.Object.Instantiate( prefab, pos, rot, transform );
                obj.name = prefab.name + " (" + ( nextId++ ) + ")";

                obj.AddComponent<PoolMember>().myPool = this;
            }
            else
            {
                obj = inactive.Pop();

                if ( obj == null )
                {
                    return Spawn( pos, rot, tran );
                }
            }

            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.transform.parent = tran;
            obj.SetActive( true );
            return obj;
        }

        public void Despawn( GameObject obj )
        {
            obj.SetActive( false );

            inactive.Push( obj );
        }

    }


    /// <summary>
    /// Added to freshly instantiated objects, so we can link back
    /// to the correct pool on despawn.
    /// </summary>
    class PoolMember : MonoBehaviour
    {
        public Pool myPool;
    }

    static Dictionary<GameObject, Pool> pools;

    /// <summary>
    /// Initialize our dictionary.
    /// </summary>
    static void Init( GameObject prefab = null, int qty = DEFAULT_POOL_SIZE, Transform tran = null )
    {
        if ( pools == null )
        {
            pools = new Dictionary<GameObject, Pool>();
        }
        if ( prefab != null && pools.ContainsKey( prefab ) == false )
        {
            pools[prefab] = new Pool( prefab, qty, tran );
        }
    }

    /// <summary>
    /// If you want to preload a few copies of an object at the start
    /// of a scene, you can use this. Really not needed unless you're
    /// going to go from zero instances to 100+ very quickly.
    /// Could technically be optimized more, but in practice the
    /// Spawn/Despawn sequence is going to be pretty darn quick and
    /// this avoids code duplication.
    /// </summary>
    static public void Preload( GameObject prefab, int qty = 1, Transform tran = null )
    {
        Init( prefab, qty, tran );

        // Make an array to grab the objects we're about to pre-spawn.
        GameObject[] obs = new GameObject[qty];
        for ( int i = 0 ; i < qty ; i++ )
        {
            obs[i] = Spawn( prefab, Vector3.zero, Quaternion.identity, tran );
        }

        // Now despawn them all.
        for ( int i = 0 ; i < qty ; i++ )
        {
            Despawn( obs[i] );
        }
    }

    /// <summary>
    /// Spawns a copy of the specified prefab (instantiating one if required).
    /// NOTE: Remember that Awake() or Start() will only run on the very first
    /// spawn and that member variables won't get reset.  OnEnable will run
    /// after spawning -- but remember that toggling IsActive will also
    /// call that function.
    /// </summary>
    static public GameObject Spawn( GameObject prefab, Vector3 pos, Quaternion rot, Transform tran = null )
    {
        Init( prefab, 3, tran );

        return pools[prefab].Spawn( pos, rot, tran );
    }

    /// <summary>
    /// Despawn the specified gameobject back into its pool.
    /// </summary>
    static public void Despawn( GameObject obj )
    {
        PoolMember pm = obj.GetComponent<PoolMember>();
        if ( pm == null )
        {
            Debug.Log( "Object '" + obj.name + "' wasn't spawned from a pool. Destroying it instead." );
            GameObject.Destroy( obj );
        }
        else
        {
            pm.myPool.Despawn( obj );
        }
    }

}