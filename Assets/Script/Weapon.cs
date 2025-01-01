using System.Collections;
using UnityEngine;

public class Weapon : GrabableItem
{
    [SerializeField] private WeaponScriptableObject _weaponScriptableObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzleFlashAncher;
    [SerializeField] private Transform _triggerTransform;
    private readonly int ShotHash = Animator.StringToHash("Shot");
    private readonly int TrailLengthId = Shader.PropertyToID("_TrailLength");
    private readonly int TrailStartTimeId = Shader.PropertyToID("_TrailStartTime");
    private float _triggerPitchAngleEuler = 0.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void LateUpdate()
    {
        _triggerTransform.localRotation = Quaternion.Euler(_triggerPitchAngleEuler, 0, 0);
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);

        const float MaxTriggerAngleEuler = 45.0f;
        _triggerPitchAngleEuler = input._indexTrigger * MaxTriggerAngleEuler;
    }

    public override void OnIndexTriggered() 
    {
        MuzzleFlashEffect();
        Shot();
        StartCoroutine(ShotRecoilAnimation());
    }

    private void MuzzleFlashEffect()
    {
        _animator.SetTrigger(ShotHash);
        ParticleSystem muzzleFlashParticle = Instantiate(_weaponScriptableObject.MuzzleFlashParticlePrefab, _muzzleFlashAncher.transform.position, _muzzleFlashAncher.transform.rotation);
        const float MuzzleFlashParticleDestroyTimeInSec = 3.0f;
        Destroy(muzzleFlashParticle.gameObject, MuzzleFlashParticleDestroyTimeInSec);
        VibrateXrHand(0, 1, 0.1f);
    }

    private void Shot()
    {
        RaycastHit hit;
        const float MaxRayLength = 30.0f;
        const float RayRadius = 0.05f;
        Vector3 muzzleFlashPosition = _muzzleFlashAncher.position;
        if (Physics.SphereCast(muzzleFlashPosition, RayRadius, _muzzleFlashAncher.forward * MaxRayLength, out hit))
        {
            const float ImpactParticleDestroyTimeInSec = 10.0f;
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            ParticleSystem impactParticle = Instantiate(_weaponScriptableObject.ImpactParticlePrefab, hit.point, rotation);
            impactParticle.transform.parent = hit.transform;
            Destroy(impactParticle.gameObject, ImpactParticleDestroyTimeInSec);

            const float BulletTrailDestroyTimeInSec = 5.0f;
            const float HitDistanceMergin = 0.05f;
            MeshRenderer meshRenderer = Instantiate(_weaponScriptableObject.BulletTrailCilinderMeshRendererPrefab, muzzleFlashPosition, _muzzleFlashAncher.rotation);
            Material dynamicMaterial = new Material(meshRenderer.sharedMaterial);
            dynamicMaterial.SetFloat(TrailStartTimeId, Time.time);
            dynamicMaterial.SetFloat(TrailLengthId, hit.distance + HitDistanceMergin);
            meshRenderer.sharedMaterial = dynamicMaterial;
            Destroy(meshRenderer.gameObject, BulletTrailDestroyTimeInSec);
        }
    }

    private IEnumerator ShotRecoilAnimation()
    {
        float animationLength = _weaponScriptableObject.RecoilTimeInSec;
        float animationTime = 0.0f;
        while (animationTime < 1.0f)
        {
            Vector3 position = Vector3.forward * _weaponScriptableObject.RecoilTranslationZ.Evaluate(animationTime);
            Quaternion rotation = Quaternion.Euler(Vector3.up * _weaponScriptableObject.RecoilPitchEuler.Evaluate(animationTime));
            AnimateXrHandTransform(position, rotation);
            yield return null;
            animationTime += Time.deltaTime / animationLength;
        }
        AnimateXrHandTransform(Vector3.zero, Quaternion.identity);
    }
}
