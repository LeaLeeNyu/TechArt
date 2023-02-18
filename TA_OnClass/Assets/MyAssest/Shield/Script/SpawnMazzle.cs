using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMazzle : MonoBehaviour
{
    [SerializeField] private GameObject _MazzlePrefab;
    [SerializeField] private Transform VFXTransform;
    public void SpawnMazzleVFX()
    {
        GameObject vfxObject = Instantiate(_MazzlePrefab, VFXTransform);
        Destroy(vfxObject,2f);
    }
}
