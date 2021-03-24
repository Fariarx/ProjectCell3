using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAfter : MonoBehaviour
{
    public float deleteAfterSec = 0; 
    private void Start()
    {  
        if (deleteAfterSec != 0)
        {
            StartCoroutine(TimerDelete(deleteAfterSec));
        }
    }
    private IEnumerator TimerDelete(float sec)
    {
        yield return new WaitForSeconds(sec);
        GameObject.DestroyImmediate(this.gameObject);
    }
}
