using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FlockingSettings : ScriptableObject
{
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;

    [Header("Behaviors")]
    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float separateWeight = 1;
    public float seekWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = 0.27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDist = 5;
}