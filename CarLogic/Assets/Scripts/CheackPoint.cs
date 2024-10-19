using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheackPoint : MonoBehaviour
{
    public List<Car_Control_AI> cheacked;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Car_Control_AI ai))
        {
            if (!cheacked.Contains(ai))
            {
                cheacked.Add(ai);
                ai.checkPointCompleted = true;
                Debug.Log("CheackpointGet: " + ai.gameObject.name);
            }
        }
    }
}
