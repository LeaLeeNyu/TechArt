using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProceduralAnimation : MonoBehaviour
{
    public Transform[] legTargets;
    public float stepSize = 1f;
    public int smoothness = 1;
    public float stepHeight = 0.1f;
    public bool bodyOrientation = true;

    private float raycastRange = 20f;
    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;
    
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;

    private float velocityMultiplier = 15f;


    static Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up, - up);
        
        if (Physics.Raycast(ray, out hit, 2f * halfRange))
        {
            res[0] = hit.point;
            Debug.Log(res[0]);
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }
    
    void Start()
    {
        //Get the up direction of the spider
        lastBodyUp = transform.up;

        //Set up the original parameter
        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = new bool[nbLegs];

        for (int i = 0; i < nbLegs; ++i)
        {
            //defaultLegPosition: leg target local position
            defaultLegPositions[i] = legTargets[i].localPosition;
            //lastLegPosition: leg target world position
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }

        lastBodyPos = transform.position;
    }

    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for(int i = 1; i <= smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));
            legTargets[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        legMoving[0] = false;
    }


    void FixedUpdate()
    {
        //Get body velocity(Vector 3)
        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;
        
        Vector3[] desiredPositions = new Vector3[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;

        for (int i = 0; i < nbLegs; ++i)
        {
            //get legs world space position
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            //Get the legIK movement distance
            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up).magnitude;
            // if movement distance larger than step size, start move
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }

        // Update legIK position to move
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];

        // if spider have not move and need to move
        if (indexToMove != -1 && !legMoving[0])
        {
            Vector3 targetPoint = desiredPositions[indexToMove] 
                                  + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) 
                                  * (desiredPositions[indexToMove] - legTargets[indexToMove].position) 
                                  + velocity * velocityMultiplier;
            
            Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, -transform.up);
            legMoving[0] = true;

            //Only make one leg move at one time
            StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
        }

        lastBodyPos = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]), stepSize);
        }
    }
}
