using UnityEngine;

public class Move : MonoBehaviour
{


    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * 40;
    }
}