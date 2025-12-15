using TMPro;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats - Base")]
    public int baseDamage = 50;
    public float baseFireRate = 0.3f;
    public int baseMaxAmmo = 15;
    public float reloadTime = 2f;
    public float range = 100f;
    public float bulletSpread = 0.1f;

    [Header("Weapon Upgrade System")]
    public float upgradeInterval = 30f; // Cada 30 segundos mejora el arma
    public int damagePerUpgrade = 10;
    public float fireRateDecrease = 0.03f; // Reduce el intervalo = más rápido
    public int ammoPerUpgrade = 5;
    public int maxWeaponLevel = 10;

    [Header("Current Stats")]
    public int damage;
    public float fireRate;
    public int maxAmmo;
    public int currentAmmo;

    [Header("Precision Settings")]
    [Tooltip("Si está activado, el disparo usará el centro exacto de la pantalla")]
    public bool useScreenCenter = true;
    [Tooltip("Capa(s) que el raycast puede golpear")]
    public LayerMask shootableLayers = -1;
    [Tooltip("Radio del SphereCast para mejor detección (0 = raycast normal)")]
    public float sphereCastRadius = 0f;

    [Header("References")]
    public Camera fpsCam;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    [Header("Effects")]
    public GameObject impactEffect;
    public LineRenderer bulletTrail;
    public float bulletTrailTime = 0.05f;

    [Header("UI")]
    public TMP_Text ammoText;
    public TMP_Text weaponLevelText;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private AudioSource audioSource;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private int currentWeaponLevel = 1;
    private float nextUpgradeTime = 0f;

    void Start()
    {
        // Inicializar stats con valores base
        damage = baseDamage;
        fireRate = baseFireRate;
        maxAmmo = baseMaxAmmo;
        currentAmmo = maxAmmo;
        nextUpgradeTime = upgradeInterval;

        if (fpsCam == null)
            fpsCam = Camera.main;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateUI();
    }

    void Update()
    {
        // Sistema de mejora automática por tiempo
        CheckWeaponUpgrade();

        if (isReloading) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }

        UpdateUI();
    }

    void CheckWeaponUpgrade()
    {
        if (GameManager.Instance == null || !GameManager.Instance.gameObject.activeInHierarchy) return;
        
        // Obtener el tiempo de juego del GameManager
        float gameTime = Time.timeSinceLevelLoad;
        
        if (gameTime >= nextUpgradeTime && currentWeaponLevel < maxWeaponLevel)
        {
            UpgradeWeapon();
            nextUpgradeTime += upgradeInterval;
        }
    }

    void UpgradeWeapon()
    {
        currentWeaponLevel++;
        
        // Mejorar stats
        damage += damagePerUpgrade;
        fireRate = Mathf.Max(0.1f, fireRate - fireRateDecrease); // Mínimo 0.1s entre disparos
        maxAmmo += ammoPerUpgrade;
        
        // Recargar automáticamente al mejorar
        currentAmmo = maxAmmo;
        
        Debug.Log($"⬆️ WEAPON UPGRADE! Level {currentWeaponLevel} | Damage: {damage} | Fire Rate: {fireRate:F2}s | Ammo: {maxAmmo}");
    }

    void Shoot()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Vector3 shootOrigin;
        Vector3 shootDirection;

        if (useScreenCenter)
        {
            Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            shootOrigin = ray.origin;
            shootDirection = ray.direction;
        }
        else
        {
            shootOrigin = fpsCam.transform.position;
            shootDirection = fpsCam.transform.forward;
        }

        if (bulletSpread > 0)
        {
            shootDirection.x += Random.Range(-bulletSpread, bulletSpread);
            shootDirection.y += Random.Range(-bulletSpread, bulletSpread);
            shootDirection = shootDirection.normalized;
        }

        RaycastHit hit;
        bool didHit;

        if (sphereCastRadius > 0)
        {
            didHit = Physics.SphereCast(shootOrigin, sphereCastRadius, shootDirection, out hit, range, shootableLayers);
        }
        else
        {
            didHit = Physics.Raycast(shootOrigin, shootDirection, out hit, range, shootableLayers);
        }

        Vector3 hitPoint;

        if (didHit)
        {
            hitPoint = hit.point;

            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if (enemy == null)
            {
                enemy = hit.transform.GetComponentInParent<EnemyHealth>();
            }

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 1f);
            }
        }
        else
        {
            hitPoint = shootOrigin + shootDirection * range;
        }

        Vector3 trailStart = firePoint != null ? firePoint.position : shootOrigin;
        if (bulletTrail != null)
        {
            StartCoroutine(ShowBulletTrail(trailStart, hitPoint));
        }

        #if UNITY_EDITOR
        Debug.DrawRay(shootOrigin, shootDirection * (didHit ? hit.distance : range), didHit ? Color.red : Color.green, 1f);
        #endif
    }

    System.Collections.IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        if (bulletTrail == null) yield break;

        LineRenderer trail = Instantiate(bulletTrail);
        trail.SetPosition(0, start);
        trail.SetPosition(1, end);

        yield return new WaitForSeconds(bulletTrailTime);

        if (trail != null)
            Destroy(trail.gameObject);
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void UpdateUI()
    {
        if (ammoText != null)
        {
            if (isReloading)
            {
                ammoText.text = "RELOADING...";
            }
            else
            {
                ammoText.text = currentAmmo + " / " + maxAmmo;
            }
        }

        if (weaponLevelText != null)
        {
            weaponLevelText.text = "WEAPON LV." + currentWeaponLevel;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (fpsCam == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range);

        if (sphereCastRadius > 0)
        {
            Gizmos.DrawWireSphere(fpsCam.transform.position + fpsCam.transform.forward * range, sphereCastRadius);
        }
    }
}