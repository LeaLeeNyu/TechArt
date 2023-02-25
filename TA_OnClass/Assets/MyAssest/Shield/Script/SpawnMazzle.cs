using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMazzle : MonoBehaviour
{
    [SerializeField] private GameObject _MazzlePrefab;

    [Header("Projectile Parameter")]
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileParent;
    private ParticleSystem _ProjectileParticle;
    private GameObject _Projectile;

    //To count projectile velocity's direction
    [SerializeField] private Transform VFXTransform;
    [SerializeField] private Transform characterTrans;
    [SerializeField] private Transform shieldTrans;

    private void Update()
    {
        if (_Projectile != null)
        {
            Vector3 projectileDirect = new Vector3(shieldTrans.position.x - characterTrans.position.x,
                                                   shieldTrans.position.y - VFXTransform.position.y,
                                                   shieldTrans.position.z - characterTrans.position.z);
            //projectileObject.GetComponent<Rigidbody>().AddForce(projectileDirect.normalized * _projectileSpeed);
            _Projectile.transform.position += projectileDirect.normalized * _projectileSpeed;
        }
    }

    public void SpawnVFX()
    {
        //Spawn mazzle
        GameObject mazzleObject = Instantiate(_MazzlePrefab, VFXTransform);
        Destroy(mazzleObject,2f);

        //Spawn projectile
        _Projectile = Instantiate(_projectilePrefab, VFXTransform.position, VFXTransform.rotation, _projectileParent);
        //_ProjectileParticle = _Projectile.GetComponentsInChildren<ParticleSystem>()[0];
        //_ProjectileParticle.Play(true);

        //Destroy(_Projectile,1f);
    }


}
