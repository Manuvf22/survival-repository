using System.Collections;
using UnityEngine;

public class PowerUpSystem : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject maxAmmoPrefab;
    public GameObject instaKillPrefab;
    public GameObject doublePointsPrefab;
    public GameObject nukePrefab;
    
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnInterval = 45f;
    public float powerUpLifetime = 20f;
    
    void Start()
    {
        StartCoroutine(SpawnLoop());
    }
    
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (spawnPoints.Length > 0)
            {
                SpawnRandomPowerUp();
            }
        }
    }
    
    void SpawnRandomPowerUp()
    {
        GameObject prefab = GetRandomPrefab();
        if (prefab == null) return;
        
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject powerUp = Instantiate(prefab, spawn.position, Quaternion.identity);
        
        Destroy(powerUp, powerUpLifetime);
    }
    
    GameObject GetRandomPrefab()
    {
        int random = Random.Range(0, 4);
        
        switch (random)
        {
            case 0: return maxAmmoPrefab;
            case 1: return instaKillPrefab;
            case 2: return doublePointsPrefab;
            case 3: return nukePrefab;
            default: return maxAmmoPrefab;
        }
    }
}

public class PowerUpPickup : MonoBehaviour
{
    public enum Type { MaxAmmo, InstaKill, DoublePoints, Nuke }
    public Type powerUpType;
    public float duration = 30f;
    
    void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (powerUpType == Type.MaxAmmo) MaxAmmo(other.gameObject);
        if (powerUpType == Type.InstaKill) StartCoroutine(InstaKill(other.gameObject));
        if (powerUpType == Type.DoublePoints) StartCoroutine(DoublePoints());
        if (powerUpType == Type.Nuke) Nuke();
        
        Destroy(gameObject);
    }
    
    void MaxAmmo(GameObject player)
    {
        WeaponController weapon = player.GetComponentInChildren<WeaponController>();
        if (weapon) weapon.currentAmmo = weapon.maxAmmo;
    }
    
    IEnumerator InstaKill(GameObject player)
    {
        WeaponController weapon = player.GetComponentInChildren<WeaponController>();
        if (!weapon) yield break;
        
        int original = weapon.damage;
        weapon.damage = 999999;
        yield return new WaitForSeconds(duration);
        weapon.damage = original;
    }
    
    IEnumerator DoublePoints()
    {
        if (!GameManager.Instance) yield break;
        
        GameManager.Instance.pointsPerHit *= 2;
        GameManager.Instance.pointsPerKill *= 2;
        yield return new WaitForSeconds(duration);
        GameManager.Instance.pointsPerHit /= 2;
        GameManager.Instance.pointsPerKill /= 2;
    }
    
    void Nuke()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy) enemy.GetComponent<EnemyHealth>()?.InstantKill();
        }
        if (GameManager.Instance) GameManager.Instance.AddScore(400);
    }
}
