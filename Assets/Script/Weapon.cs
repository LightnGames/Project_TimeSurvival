using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponScriptableObject _weaponScriptableObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _muzzleFlashAncher;
    [SerializeField] private Transform _triggerTransform;
    [SerializeField] private Transform _emptyShellAncherTransform;
    [SerializeField] private Light _flashLight;
    [SerializeField] private Transform _onCatchedEventTransform = null;
    [SerializeField] private MeshRenderer _remainingBulletTextMeshRenderer;
    private readonly int ShotHash = Animator.StringToHash("Shot");
    private readonly int TrailLengthId = Shader.PropertyToID("_TrailLength");
    private readonly int TrailStartTimeId = Shader.PropertyToID("_TrailStartTime");
    private readonly int FresnelEffectId = Shader.PropertyToID("_FresnelEffect");
    private readonly int DisplayNumberId = Shader.PropertyToID("_DisplayNumber");
    private readonly int OutlineEffectStartTimeId = Shader.PropertyToID("_OutlineEffectStartTime");
    private float _triggerPitchAngleEuler = 0.0f;
    private float _shotRecoilTimer = 0.0f;
    private int _ammo = 0;
    private Material _remainingBulletNumberMaterial;
    private AudioSource _audioSource;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private List<Material> _materials = new List<Material>();
    private event CatchableItem.VibrateEvent _mainGripVibrationEvent = null;
    private event CatchableItem.XrHandAnimationTransformEvent _mainGripAnimationTransformEvent = null;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _audioSource = GetComponent<AudioSource>();
        _ammo = _weaponScriptableObject.MaxAmmo;
        _remainingBulletNumberMaterial = _remainingBulletTextMeshRenderer.material;
;
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            foreach(Material material in renderer.materials)
            {
                _materials.Add(material);
            }
        }

        SwitchFresnelEffect(true);
    }

    protected void SwitchFresnelEffect(bool visibility)
    {
        foreach (Material material in _materials)
        {
            material.SetFloat(FresnelEffectId, visibility ? 1.0f : 0.0f);
        }
    }

    protected virtual void Update()
    {
    }

    protected virtual void LateUpdate()
    {
    }

    private void UpdateRemainingBulletText()
    {
        float currentDisplayNumber = _remainingBulletNumberMaterial.GetFloat(DisplayNumberId);
        float displayDiff = Mathf.Abs(_ammo - currentDisplayNumber);
        if (displayDiff < 0.01f)
        {
            return;
        }

        float newDisplayNumber = currentDisplayNumber + (_ammo - currentDisplayNumber) / 50.0f;
        if (displayDiff < 0.01f)
        {
            newDisplayNumber = _ammo;
            PlayRemainingBulletTextOutlineEffect();
        }
        _remainingBulletNumberMaterial.SetFloat(DisplayNumberId, newDisplayNumber);
    }

    private void PlayRemainingBulletTextOutlineEffect()
    {
        _remainingBulletNumberMaterial.SetFloat(OutlineEffectStartTimeId, Time.time);
    }

    private void UpdateTriggerRotation()
    {
        _triggerTransform.localRotation = Quaternion.Euler(_triggerPitchAngleEuler, 0, 0);
    }

    public virtual void MainGripCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform mainGripTransform)
    {
        const float MaxTriggerAngleEuler = 45.0f;
        _triggerPitchAngleEuler = input._indexTrigger * MaxTriggerAngleEuler;
        _shotRecoilTimer += Time.deltaTime;

        UpdateRemainingBulletText();
        UpdateTriggerRotation();
    }

    public virtual void MainGripCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        _mainGripVibrationEvent = vibrateEvent;
        _mainGripAnimationTransformEvent = transformEvent;
        CatchedWeapon();

        if (_onCatchedEventTransform != null)
        {
            _onCatchedEventTransform.GetComponent<IEventTrigger>().OnEventTriggered();
        }

        _remainingBulletNumberMaterial.SetFloat(DisplayNumberId, 0);
    }

    public virtual void MainGripReleased()
    {
        _mainGripVibrationEvent = null;
        _mainGripAnimationTransformEvent = null;
    }

    protected void CatchedWeapon()
    {
        int randomIndex = Random.Range(0, _weaponScriptableObject.EquipAudioClips.Length);
        _audioSource.PlayOneShot(_weaponScriptableObject.EquipAudioClips[randomIndex]);
        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        _flashLight.enabled = true;
        SwitchFresnelEffect(false);
    }

    protected void ReleasedWeapon()
    {
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        _flashLight.enabled = false;

        if (!IsEmptyAmmo())
        {
            SwitchFresnelEffect(true);
        }
    }

    public bool IsMainGripCatched()
    {
        return _mainGripAnimationTransformEvent != null;
    }

    public bool IsEmptyAmmo()
    {
        return _ammo == 0;
    }

    public void OnMainGripIndexTriggered()
    {
        if (IsEmptyAmmo())
        {
            DryFire();
            return;
        }

        if (!IsReadyToShot())
        {
            return;
        }

        MuzzleFlashEffect();
        DischargeEmptyShell();
        Shot();
        StartCoroutine(PlayShotRecoilAnimation());
    }

    protected bool IsReadyToShotTimer()
    {
        return _shotRecoilTimer > _weaponScriptableObject.RecoilTimeInSec;
    }

    protected virtual bool IsReadyToShot()
    {
        return IsReadyToShotTimer();
    }

    private void MuzzleFlashEffect()
    {
        _animator.SetTrigger(ShotHash);
        ParticleSystem muzzleFlashParticle = Instantiate(_weaponScriptableObject.MuzzleFlashParticlePrefab, _muzzleFlashAncher.transform.position, _muzzleFlashAncher.transform.rotation);
        const float MuzzleFlashParticleDestroyTimeInSec = 3.0f;
        Destroy(muzzleFlashParticle.gameObject, MuzzleFlashParticleDestroyTimeInSec);

        float vibrationScale = _weaponScriptableObject.ShotVibrationScale;
        _mainGripVibrationEvent(0, vibrationScale, vibrationScale * 0.15f / Time.timeScale);

        AudioClip[] audioClips = _weaponScriptableObject.ShotAudioClips;
        int randomIndex = Random.Range(0, audioClips.Length);
        _muzzleFlashAncher.PlayOneShot(audioClips[randomIndex]);
    }

    private void DischargeEmptyShell()
    {
        Rigidbody emptyShell = Instantiate(_weaponScriptableObject.EmptyShellPrefab, _emptyShellAncherTransform.position, _emptyShellAncherTransform.rotation);
        emptyShell.AddForce(_emptyShellAncherTransform.forward * 50.0f);
        emptyShell.AddTorque(_emptyShellAncherTransform.right * 1.0f);
        const float DestroyTimeInSec = 5.0f;
        Destroy(emptyShell.gameObject, DestroyTimeInSec);
    }

    private void DryFire()
    {
        AudioClip[] audioClips = _weaponScriptableObject.DryFireAudioClips;
        int randomIndex = Random.Range(0, audioClips.Length);
        _muzzleFlashAncher.PlayOneShot(audioClips[randomIndex]);

        // 残弾UIにアウトライン強調を表示させる
        PlayRemainingBulletTextOutlineEffect();
    }

    protected virtual void Shot()
    {
        RaycastHit hit;
        const float MaxRayLength = 50.0f;
        const float RayRadius = 0.05f;
        Vector3 muzzleFlashPosition = _muzzleFlashAncher.transform.position;
        if (Physics.SphereCast(muzzleFlashPosition, RayRadius, _muzzleFlashAncher.transform.forward * MaxRayLength, out hit))
        {
            const float BulletTrailDestroyTimeInSec = 5.0f;
            const float HitDistanceMergin = 0.05f;
            MeshRenderer meshRenderer = Instantiate(_weaponScriptableObject.BulletTrailCilinderMeshRendererPrefab, muzzleFlashPosition, _muzzleFlashAncher.transform.rotation);
            Material dynamicMaterial = new Material(meshRenderer.sharedMaterial);
            dynamicMaterial.SetFloat(TrailStartTimeId, Time.time);
            dynamicMaterial.SetFloat(TrailLengthId, hit.distance + HitDistanceMergin);
            meshRenderer.sharedMaterial = dynamicMaterial;
            Destroy(meshRenderer.gameObject, BulletTrailDestroyTimeInSec);

            ParticleSystem impactParticlePrefab = _weaponScriptableObject.ImpactParticlePrefab;
            DamageableCollider damageableCollider = hit.collider.GetComponent<DamageableCollider>();
            if (damageableCollider != null)
            {
                float baseDamage = _weaponScriptableObject.BaseDamage;
                float damageScale = damageableCollider.PartType == DamageableCollider.DamagePartType.Critical ? 2.0f : 1.0f;
                damageableCollider.Damage((int)(baseDamage * damageScale), transform);
                impactParticlePrefab = _weaponScriptableObject.BloodImpactParticlePrefab;
            }

            const float ImpactParticleDestroyTimeInSec = 10.0f;
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            ParticleSystem impactParticle = Instantiate(impactParticlePrefab, hit.point, rotation);
            impactParticle.transform.parent = hit.transform;
            Destroy(impactParticle.gameObject, ImpactParticleDestroyTimeInSec);
        }

        _shotRecoilTimer = 0.0f;
        _ammo--;
    }

    private IEnumerator PlayShotRecoilAnimation()
    {
        float animationLength = _weaponScriptableObject.RecoilTimeInSec;
        float animationTime = 0.0f;
        while (animationTime < 1.0f)
        {
            Vector3 position = Vector3.forward * _weaponScriptableObject.RecoilTranslationZ.Evaluate(animationTime);
            Quaternion rotation = Quaternion.Euler(Vector3.up * _weaponScriptableObject.RecoilPitchEuler.Evaluate(animationTime));
            _mainGripAnimationTransformEvent(position, rotation);
            yield return null;
            animationTime += Time.deltaTime / animationLength;
        }
        _mainGripAnimationTransformEvent(Vector3.zero, Quaternion.identity);
    }
}
