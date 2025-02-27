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
    public TextMeshProUGUI RoundText; // UI Text "Kills: X/X"


    void Start()
    {
        PlayerPrefs.SetInt("rounds", currentRound);

        allZombies = new List<GameObject>();
        InitializeObjectPools();
        StartCoroutine(SpawnZombiesRoutine());
        UpdateKillText();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }


        if (Input.GetKeyDown(KeyCode.O))
        {
            float roll = Random.Range(0f, 1f); // Random number between 0 and 1

            print(roll);
            print(1 - ((currentRound - 5) * SpawnChanceChangeAmount));
            print(1 - ((currentRound - 3) * SpawnChanceChangeAmount));
            print(1 - ((currentRound - 1) * SpawnChanceChangeAmount));


            if (roll > 1 - ((currentRound - 4) * SpawnChanceChangeAmount))
                print("shielder");
            else if (roll > 1 - ((currentRound - 3) * SpawnChanceChangeAmount))
                print("big");
            else if (roll > 1 - ((currentRound - 2) * SpawnChanceChangeAmount))
                print("explode");
        }
        

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








    public void RegisterKill(GameObject zombie, int type)
    {
        
        switch (type)
        {
            case 0: currentKillCount++; break;
            case 1: currentKillCount++ ; break;
            case 2: currentKillCount += 5; break;
            case 3: currentKillCount += 3; break;
        }
        allZombies.Remove(zombie);

        //call zombie dies properly
        StartCoroutine(ZombieDies(zombie, type));

        UpdateKillText();

        if (currentKillCount >= zombiesToKillThisRound)
        {
            StartNewRound();
        }
    }

    public IEnumerator ZombieDies(GameObject zombie, int type)
    {
        yield return new WaitForSeconds(2f);

        zombie.SetActive(false);
        switch (type)
        {
            case 0: normalPool.Enqueue(zombie); break;
            case 1: explosivePool.Enqueue(zombie); break;
            case 2: bigPool.Enqueue(zombie); break;
            case 3: shieldPool.Enqueue(zombie); break;
        }


    }


    public int IncreaseKillsAMount;


    void StartNewRound()
    {

        currentKillCount = 0;


        // Create a temporary copy of the zombie list so we dont clear during iteration
        List<GameObject> zombiesToRemove = new List<GameObject>(allZombies);

        foreach (GameObject zombie in zombiesToRemove)
        {
            currentKillCount = 0;

            zombie.SetActive(false);
            int type = GetZombieTypeFromObject(zombie);
            RegisterKill(zombie, type);
        }
        allZombies.Clear();

 


        currentRound++;
        PlayerPrefs.SetInt("rounds", currentRound);
        currentKillCount = 0;
        zombiesToKillThisRound += IncreaseKillsAMount;



       // AdjustSpawnChances();
        AdjustSpawnRate();
        

        UpdateKillText();
        UpdateRoundText();
    }


    
    public void AdjustSpawnRate()
    {
        spawnBatchSize += 1;
        spawnRate -= 0.3f;
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



    GameObject GetRandomZombie()
    {
        float roll = Random.Range(0f, 1f); // Random number between 0 and 1

        if (roll > 1 - ((currentRound - 4) * SpawnChanceChangeAmount / 1.8))
            return shieldPool.Dequeue();
        else if (roll > 1 - ((currentRound - 3) * SpawnChanceChangeAmount / 1.2))
            return bigPool.Dequeue();
        else if (roll > 1 - ((currentRound - 2) * SpawnChanceChangeAmount))
            return explosivePool.Dequeue();
        else if (normalPool.Count > 0)
            return normalPool.Dequeue();

        return null; // Failsafe
    }


    public float SpawnChanceChangeAmount;

    //void AdjustSpawnChances()
    //{
    //    if (currentRound == 2)
    //    {
    //        zombieSpawnChances[1] = 1; // Explosive: 5%
    //        zombieSpawnChances[0] -= SpawnChanceChangeAmount; // Reduce normal chance
    //    }
    //    if (currentRound == 3)
    //    {
    //        zombieSpawnChances[2] = 1; // Big: 5%
    //        zombieSpawnChances[1] -= SpawnChanceChangeAmount; // Explosive: 5%
    //        zombieSpawnChances[0] -= SpawnChanceChangeAmount;
    //    }
    //    if (currentRound == 4)
    //    {
    //        zombieSpawnChances[3] = 1; // Big: 5%
    //        zombieSpawnChances[2] -= SpawnChanceChangeAmount;
    //        zombieSpawnChances[1] -= SpawnChanceChangeAmount; // Shield: 5%
    //        zombieSpawnChances[0] -= SpawnChanceChangeAmount;
    //    }
    //}






    void UpdateKillText()
    {
        if (killsText != null)
            killsText.text = $"Kills: {currentKillCount}/{zombiesToKillThisRound}";
    }


    void UpdateRoundText()
    {
        if (RoundText != null)
            RoundText.text = $"Round: {currentRound}";
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