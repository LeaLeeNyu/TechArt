using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkManager : MonoBehaviour
{
    public FlockingSettings settings;
    public GameObject agentPrefab;
    public GameObject smallFishParent;
    public GameObject seekTarget;
    public int numAgents = 20;
    public Vector3 bounds = new Vector3(5, 5, 5);

    private FlockingAgent[] agents;

    void Start()
    {
        //seekTarget = smallFishParent.transform.GetChild(0).gameObject;

        if (agentPrefab != null)
        {
            agents = new FlockingAgent[numAgents];
            for (int i = 0; i < numAgents; i++)
            {
                Vector3 pos = this.transform.position + new Vector3(
                    Random.Range(-bounds.x, +bounds.x),
                    Random.Range(-bounds.y, +bounds.y),
                    Random.Range(-bounds.z, +bounds.z));

                GameObject agentObj = Instantiate(agentPrefab, pos, Quaternion.identity);

                agents[i] = agentObj.GetComponent<FlockingAgent>();
                agents[i].Initialize(settings, seekTarget);
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

            seekTarget = smallFishParent.transform.GetChild(0).gameObject;
            agent.UpdateSeekTarget(seekTarget);
            agent.UpdateAgent(neighbors);
        }
    }

}
