using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public IEnumerator Timer(System.Action action, float delay)
    { 
        yield return new WaitForSeconds(delay);
        action();
    }
    public IEnumerator Wait(System.Func<bool> action, float delay)
    {
        while (action())
        {
            yield return new WaitForSeconds(delay);
        }
    }
}
