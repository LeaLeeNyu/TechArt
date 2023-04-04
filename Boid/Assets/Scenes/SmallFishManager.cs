using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallFishManager : MonoBehaviour
{
    public FlockingSettings settings;

    public GameObject[] agentPrefabs;
    public GameObject agentParent;

    public GameObject targetPrefab;
    public GameObject targetParent;
    public GameObject seekTarget;

    public int numAgents = 20;
    public Vector3 bounds = new Vector3(5, 5, 5);

    //private FlockingAgent[] agents;

    [HideInInspector]public List<FlockingAgent> agents;

    void Start()
    {
        if (agentPrefabs != null)
        {
            agents = new List<FlockingAgent>();

            seekTarget = targetParent.transform.GetChild(0).gameObject;
            for (int i = 0; i < numAgents; i++)
            {
                Vector3 pos = this.transform.position + new Vector3(
                    Random.Range(-bounds.x, +bounds.x),
                    Random.Range(-bounds.y, +bounds.y),
                    Random.Range(-bounds.z, +bounds.z));

                int agentIndex = Random.Range(0, agentPrefabs.Length);

                GameObject agentObj = Instantiate(agentPrefabs[agentIndex], pos, Quaternion.identity, agentParent.transform);

                agents.Add(agentObj.gameObject.GetComponent<FlockingAgent>());
                //agents[i] = agentObj.GetComponent<FlockingAgent>();
                agentObj.gameObject.GetComponent<FlockingAgent>().Initialize(settings, seekTarget);
            }
        }
    }

    void Update()
    {
        foreach (var agent in agents)
        {
            List<FlockingAgent> neighbors = new List<FlockingAgent>();
            foreach (var neighbor in agents)
            {
                if (neighbor != agent)
                {
                    Vector3 neighborOffset = neighbor.transform.position - agent.transform.position;
                    if (neighborOffset.sqrMagnitude < settings.perceptionRadius * settings.perceptionRadius)
                    {
                        neighbors.Add(neighbor);
                    }
                }               
            }

            agent.UpdateSeekTarget(seekTarget);
            agent.UpdateAgent(neighbors);
        }

        //Click mouse to instantiate new target
        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
            Debug.Log("Mouse Clicking");
        }

    }

    void MouseClick()
    {
        RaycastHit hitInfo;

        Vector2 mousePosition = Input.mousePosition;

        Ray rayOrigin = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(rayOrigin, out hitInfo))
        {
            Debug.Log("Raycast hit object " + hitInfo.transform.name + " at the position of " + hitInfo.transform.position);

            Destroy(seekTarget);
            seekTarget = Instantiate(seekTarget, hitInfo.point, Quaternion.identity, targetParent.transform);            
        }
    }
}
