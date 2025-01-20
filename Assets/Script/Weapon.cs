using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class Weapon : GrabableItem
{
    [SerializeField] private WeaponScriptableObject _weaponScriptableObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzleFlashAncher;
    [SerializeField] private Transform _triggerTransform;
    [SerializeField] private Transform _emptyShellAncherTransform;
    private readonly int ShotHash = Animator.StringToHash("Shot");
    private readonly int TrailLengthId = Shader.PropertyToID("_TrailLength");
    private readonly int TrailStartTimeId = Shader.PropertyToID("_TrailStartTime");
    private float _triggerPitchAngleEuler = 0.0f;
    private float _shotRecoilTimer = 0.0f;
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
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
        _shotRecoilTimer += Time.deltaTime;
    }

    public override void Catched(VibrateEvent vibrateEvent, XrHandTransformEvent transformEvent)
    {
        base.Catched(vibrateEvent, transformEvent);
        int randomIndex = Random.Range(0, _weaponScriptableObject.EquipAudioClips.Length);
        _audioSource.PlayOneShot(_weaponScriptableObject.EquipAudioClips[randomIndex]);
    }

    public override void OnIndexTriggered() 
    {
        if (_shotRecoilTimer < _weaponScriptableObject.RecoilTimeInSec)
        {
            return;
        }

        MuzzleFlashEffect();
        DischargeEmptyShell();
        Shot();
        StartCoroutine(PlayShotRecoilAnimation());
    }

    private void MuzzleFlashEffect()
    {
        bool canFire = true;
        _animator.SetTrigger(ShotHash);
        ParticleSystem muzzleFlashParticle = Instantiate(_weaponScriptableObject.MuzzleFlashParticlePrefab, _muzzleFlashAncher.transform.position, _muzzleFlashAncher.transform.rotation);
        const float MuzzleFlashParticleDestroyTimeInSec = 3.0f;
        Destroy(muzzleFlashParticle.gameObject, MuzzleFlashParticleDestroyTimeInSec);
        VibrateXrHand(0, 1, 0.1f);

        AudioSource audioSource = muzzleFlashParticle.GetComponent<AudioSource>();
        AudioClip[] audioClips = canFire ? _weaponScriptableObject.ShotAudioClips : _weaponScriptableObject.DryDireAudioClips;
        int randomIndex = Random.Range(0, audioClips.Length);
        audioSource.PlayOneShot(audioClips[randomIndex]);
    }

    private void DischargeEmptyShell()
    {
        Rigidbody emptyShell = Instantiate(_weaponScriptableObject.EmptyShellPrefab, _emptyShellAncherTransform.transform.position, _emptyShellAncherTransform.transform.rotation);
        emptyShell.AddForce(Vector3.forward * 3.0f);
        emptyShell.AddTorque(Vector3.right * 1.0f);
        const float DestroyTimeInSec = 5.0f;
        Destroy(emptyShell.gameObject, DestroyTimeInSec);
    }

    private void Shot()
    {
        RaycastHit hit;
        const float MaxRayLength = 30.0f;
        const float RayRadius = 0.05f;
        Vector3 muzzleFlashPosition = _muzzleFlashAncher.position;
        if (Physics.SphereCast(muzzleFlashPosition, RayRadius, _muzzleFlashAncher.forward * MaxRayLength, out hit))
        {
            const float BulletTrailDestroyTimeInSec = 5.0f;
            const float HitDistanceMergin = 0.05f;
            MeshRenderer meshRenderer = Instantiate(_weaponScriptableObject.BulletTrailCilinderMeshRendererPrefab, muzzleFlashPosition, _muzzleFlashAncher.rotation);
            Material dynamicMaterial = new Material(meshRenderer.sharedMaterial);
            dynamicMaterial.SetFloat(TrailStartTimeId, Time.time);
            dynamicMaterial.SetFloat(TrailLengthId, hit.distance + HitDistanceMergin);
            meshRenderer.sharedMaterial = dynamicMaterial;
            Destroy(meshRenderer.gameObject, BulletTrailDestroyTimeInSec);

            ParticleSystem impactParticlePrefab = _weaponScriptableObject.ImpactParticlePrefab;
            DamageableCollider damageableCollider = hit.collider.GetComponent<DamageableCollider>();
            if (damageableCollider != null)
            {
                damageableCollider.Damage(1);
                impactParticlePrefab = _weaponScriptableObject.BloodImpactParticlePrefab;
            }

            const float ImpactParticleDestroyTimeInSec = 10.0f;
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            ParticleSystem impactParticle = Instantiate(impactParticlePrefab, hit.point, rotation);
            impactParticle.transform.parent = hit.transform;
            Destroy(impactParticle.gameObject, ImpactParticleDestroyTimeInSec);
        }

        _shotRecoilTimer = 0.0f;
    }

    private IEnumerator PlayShotRecoilAnimation()
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
