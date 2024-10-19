using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float acceleration;
    public float maxStearengAngle;
    [SerializeField] internal Wheels[] frontWheels;
    [SerializeField] internal Wheels[] backWheels;

    [Range(-1, 1)] public float forward;
    [Range(-1, 1)] public float turn;

    public float speedKmh;
    public new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>(); 
    }

    private void LateUpdate()
    {
        speedKmh = rigidbody.velocity.magnitude * 3.2f;
    }

    private void FixedUpdate()
    {
        foreach (var wheels in backWheels)
        {
            wheels.collider.motorTorque = -forward * acceleration;
            wheels.collider.steerAngle = Mathf.Lerp(wheels.collider.steerAngle,turn * maxStearengAngle,0.5f );
        }
    }
}
