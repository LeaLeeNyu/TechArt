using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingAgent : MonoBehaviour
{
    Vector3 velocity;

    private GameObject seekTarget;
    private FlockingSettings settings;

    public void Initialize(FlockingSettings settings, GameObject seekTarget)
    {
        this.settings = settings;
        this.seekTarget = seekTarget;
    }

    public void UpdateAgent(List<FlockingAgent> neighbors)
    {
        Vector3 acceleration = Vector3.zero;

        //Move toward target
        if (seekTarget != null)
        {
            Vector3 directionToTarget = seekTarget.transform.position - transform.position;
            acceleration += SteerTowards(directionToTarget) * settings.seekWeight;
        }

        if (neighbors.Count > 0)
        {
            Vector3 centerOfNeighbors = Vector3.zero;
            Vector3 avgNeighborHeading = Vector3.zero;
            Vector3 avgAvoidanceHeading = Vector3.zero;

            foreach (FlockingAgent neighbor in neighbors)
            {
                centerOfNeighbors += neighbor.transform.position;
                avgNeighborHeading += neighbor.transform.forward;

                Vector3 directionToNeighbor = neighbor.transform.position - this.transform.position;
                float sqrDist = directionToNeighbor.sqrMagnitude;

                //Why compare again? not the same parameter
                if (sqrDist < settings.avoidanceRadius * settings.avoidanceRadius)
                {
                    // Move away from normalized neighbor offset
                    avgAvoidanceHeading -= directionToNeighbor / sqrDist;
                }
            }

            centerOfNeighbors /= neighbors.Count;
            avgNeighborHeading /= neighbors.Count;
            avgAvoidanceHeading /= neighbors.Count;

            Vector3 offsetToCenter = centerOfNeighbors - this.transform.position;

            acceleration += SteerTowards(avgNeighborHeading) * settings.alignWeight;
            acceleration += SteerTowards(offsetToCenter) * settings.cohesionWeight;
            acceleration += SteerTowards(avgAvoidanceHeading) * settings.separateWeight;
        }

        //Detect collision 
        if (IsHeadingForCollision())
        {
            acceleration += SteerTowards(GetObstacleAvoidDir()) * settings.avoidCollisionWeight;
        }

        velocity += acceleration * Time.deltaTime;

        // Clamp our velocity's magnitude between a min and max speed.
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        this.transform.position += velocity * Time.deltaTime;
        this.transform.rotation = Quaternion.LookRotation(dir); 
    }

    private Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = (vector.normalized * settings.maxSpeed) - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

    private bool IsHeadingForCollision()
    {
        RaycastHit hit;
        return Physics.SphereCast(
            this.transform.position, 
            settings.boundsRadius, 
            this.transform.forward, 
            out hit, 
            settings.collisionAvoidDist, 
            settings.obstacleMask);
    }

    private Vector3 GetObstacleAvoidDir()
    {
        Vector3[] rayDirections = AgentCollisionHelpers.directions;
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = this.transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(this.transform.position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDist, settings.obstacleMask))
            {
                return dir;
            }
        }

        return this.transform.forward;
    }

    //void OnDrawGizmos()
    //{
    //    if (settings != null)
    //    {
    //        Vector3[] rayDirections = AgentCollisionHelpers.directions;
    //        for (int i = 0; i < rayDirections.Length; i++)
    //        {
    //            Vector3 dir = this.transform.TransformDirection(rayDirections[i]);
    //            Gizmos.DrawLine(this.transform.position, this.transform.position + dir * settings.collisionAvoidDist);
    //        }
    //    }
    //}

    // Draw a spherical rays from one point
    private static class AgentCollisionHelpers
    {
        const int numViewDirections = 300;
        public static readonly Vector3[] directions;

        static AgentCollisionHelpers()
        {
            directions = new Vector3[numViewDirections];

            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < numViewDirections; i++)
            {
                float t = (float)i / numViewDirections;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = angleIncrement * i;

                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);
                directions[i] = new Vector3(x, y, z);
            }
        }
    }
}
