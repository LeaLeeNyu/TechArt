using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpawnRipple : MonoBehaviour
{
    public GameObject shieldRipples;

    private VisualEffect shieldRipplesVFX;

    private void OnCollisionEnter(Collision co)
    {
        
        if (co.gameObject.tag == "Bullet")
        {
            Debug.Log("Enter");
            var ripples = Instantiate(shieldRipples, transform) as GameObject;
            shieldRipplesVFX = ripples.GetComponent<VisualEffect>();
            shieldRipplesVFX.SetVector3("SphereCenter", co.contacts[0].point);

            Destroy(ripples, 1);
        }
    }
}
