using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDeleteAnimation : MonoBehaviour
{
    public float deleteTime = 3f;
    private float time = 0f;
    private Vector3 toPosition;

    void Start()
    {
        toPosition = transform.position;
        toPosition.y = -10;
    }
     
    void Update()
    {
        time += Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, toPosition, time / deleteTime);

        if(time/deleteTime > 1)
        {
            Destroy(gameObject);
        }
    }
}
