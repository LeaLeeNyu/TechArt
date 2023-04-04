using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SharkCollision : MonoBehaviour
{

    private SmallFishManager smallFishManager;
    private void OnEnable()
    {
        smallFishManager = GameObject.Find("SmallFishFlockingManager").GetComponent<SmallFishManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SmallFish")
        {
            smallFishManager.agents.Remove(other.gameObject.GetComponent<FlockingAgent>());
            Destroy(other.gameObject);
            Debug.Log("Eat");
        }

        Debug.Log("Collide");
    }
}
