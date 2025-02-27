using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("Spawning Settings")]
    public Transform[] spawnPoints; // Spawn locations
    public GameObject[] zombiePrefabs; // 0 = Normal, 1 = Explosive, 2 = Big, 3 = Shield
    public float[] zombieSpawnChances = { 1f, 0f, 0f, 0f }; // Probability of each zombie type (Normal starts at 1)
    List<GameObject> allZombies;
    public Vector3 hordeCentroid;

    public float spawnRate = 5f; // Time between spawns
    public int spawnBatchSize = 3; // Zombies per spawn wave

    [Header("Round Settings")]
    public int currentRound = 1; // Tracks the current round
    public int zombiesToKillThisRound = 10; // Kills needed to advance
    private int currentKillCount = 0; // Zombies killed this round

    [Header("Object Pools")]
    private Queue<GameObject> normalPool = new Queue<GameObject>();
    private Queue<GameObject> explosivePool = new Queue<GameObject>();
    private Queue<GameObject> bigPool = new Queue<GameObject>();
    private Queue<GameObject> shieldPool = new Queue<GameObject>();

    [Header("UI Elements")]
    public TextMeshProUGUI killsText; // UI Text "Kills: X/X"

    void Start()
    {
        allZombies = new List<GameObject>();
        InitializeObjectPools();
        StartCoroutine(SpawnZombiesRoutine());
        UpdateKillText();
    }

    private void Update()
    {
        float recalculationTimer = 0;
        
        recalculationTimer -= Time.deltaTime;
        if (recalculationTimer < 0)
        {
            recalculationTimer = 2;
            hordeCentroid = GetHordeCentroid();
        }
    }

    // *** Updated: Separated object pools for each zombie type ***
    void InitializeObjectPools()
    {
        for (int i = 0; i < 1000; i++) // Large pool for normal zombies
        {
            GameObject zombie = Instantiate(zombiePrefabs[0]);
            zombie.SetActive(false);
            normalPool.Enqueue(zombie);
        }



        for (int i = 0; i < 100; i++) // Smaller pools for special zombies
        {
            GameObject explosive = Instantiate(zombiePrefabs[1]);
            explosive.SetActive(false);
            explosivePool.Enqueue(explosive);

            GameObject big = Instantiate(zombiePrefabs[2]);
            big.SetActive(false);
            bigPool.Enqueue(big);

            GameObject shield = Instantiate(zombiePrefabs[3]);
            shield.SetActive(false);
            shieldPool.Enqueue(shield);
        }
    }




    IEnumerator SpawnZombiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);
            SpawnZombies();
        }
    }






    void SpawnZombies()
    {
        for (int i = 0; i < spawnBatchSize; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject zombie = GetRandomZombie();
            if (zombie != null)
            {
                allZombies.Add(zombie);
                zombie.transform.position = spawnPoint.position;
                zombie.transform.rotation = Quaternion.identity;
                zombie.SetActive(true);
            }
        }
    }





    GameObject GetRandomZombie()
    {
        float roll = Random.Range(0f, 1f); // Random number between 0 and 1

        if (roll < zombieSpawnChances[0] && normalPool.Count > 0)
            return normalPool.Dequeue();
        else if (roll < zombieSpawnChances[0] + zombieSpawnChances[1] && explosivePool.Count > 0)
            return explosivePool.Dequeue();
        else if (roll < zombieSpawnChances[0] + zombieSpawnChances[1] + zombieSpawnChances[2] && bigPool.Count > 0)
            return bigPool.Dequeue();
        else if (shieldPool.Count > 0)
            return shieldPool.Dequeue();

        return null; // Failsafe
    }




    public void RegisterKill(GameObject zombie, int type)
    {
        currentKillCount++;
        allZombies.Remove(zombie);
        // *** Updated: Return zombie to correct pool based on type ***
        zombie.SetActive(false);
        switch (type)
        {
            case 0: normalPool.Enqueue(zombie); break;
            case 1: explosivePool.Enqueue(zombie); break;
            case 2: bigPool.Enqueue(zombie); break;
            case 3: shieldPool.Enqueue(zombie); break;
        }

        UpdateKillText();

        if (currentKillCount >= zombiesToKillThisRound)
        {
            StartNewRound();
        }
    }


    public int IncreaseKillsAMount;


    void StartNewRound()
    {
        // Create a temporary copy of the zombie list so we dont clear during iteration
        List<GameObject> zombiesToRemove = new List<GameObject>(allZombies);

        foreach (GameObject zombie in zombiesToRemove)
        {
            zombie.SetActive(false);
            int type = GetZombieTypeFromObject(zombie);
            RegisterKill(zombie, type);
        }
        allZombies.Clear();

 


        currentRound++;
        currentKillCount = 0;
        zombiesToKillThisRound += IncreaseKillsAMount;
        AdjustSpawnChances();



        

        UpdateKillText();
    }

    public Vector3 GetHordeCentroid()
    {
        Vector3 vectorSum = Vector3.zero;
        foreach(GameObject zombie in allZombies)
        {
            vectorSum += zombie.transform.position;
        }
        return vectorSum / allZombies.Count;
    }


    public float SpawnChanceChangeAmount;

    void AdjustSpawnChances()
    {
        if (currentRound == 2)
        {
            zombieSpawnChances[1] = SpawnChanceChangeAmount; // Explosive: 5%
            zombieSpawnChances[0] -= SpawnChanceChangeAmount; // Reduce normal chance
        }
        if (currentRound == 3)
        {
            zombieSpawnChances[2] = SpawnChanceChangeAmount; // Big: 5%
            zombieSpawnChances[0] -= SpawnChanceChangeAmount;
        }
        if (currentRound == 4)
        {
            zombieSpawnChances[3] = SpawnChanceChangeAmount; // Shield: 5%
            zombieSpawnChances[0] -= SpawnChanceChangeAmount;
        }
    }






    void UpdateKillText()
    {
        if (killsText != null)
            killsText.text = $"Kills: {currentKillCount}/{zombiesToKillThisRound}";
    }

    //// *** Updated: Determines zombie type based on prefab reference ***
    //int GetZombieTypeFromObject(GameObject zombie)
    //{
    //    if (zombiePrefabs[1] == zombie) return 1; // Explosive
    //    if (zombiePrefabs[2] == zombie) return 2; // Big
    //    if (zombiePrefabs[3] == zombie) return 3; // Shield
    //    return 0; // Normal
    //}

    int GetZombieTypeFromObject(GameObject zombie)
    {
        ZombieScript zombieScript = zombie.GetComponent<ZombieScript>();
        if (zombieScript != null)
        {
            return zombieScript.zombCode;
        }
        return 0; // Default to normal type
    }



}