using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    public GameObject powerUpPrefab;      // Drag your power-up prefab here
    public GameObject groundPrefab;      // Drag your ground prefab here
    public int[] spawnPointIndices = { 26, 27, 28 }; // Indices of the spawn points
    private float spawnInterval = 30f;    // Time between spawns in seconds

    private Transform[] spawnPoints;     // Array to store the selected spawn points
    private float timer;

    void Start()
    {
        // Fetch spawn points from the groundPrefab based on specified indices
        spawnPoints = new Transform[spawnPointIndices.Length];
        for (int i = 0; i < spawnPointIndices.Length; i++)
        {
            spawnPoints[i] = groundPrefab.transform.GetChild(spawnPointIndices[i]);
        }

        timer = spawnInterval; // Initialize timer
    }

    void Update()
    {
        // Update the timer
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnPowerUp();
            timer = spawnInterval; // Reset the timer
        }
    }

    void SpawnPowerUp()
    {
        if (spawnPoints.Length > 0)
        {
            // Choose a random spawn point
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform randomSpawnPoint = spawnPoints[randomIndex];

            // Instantiate power-up at the chosen spawn point
            Instantiate(powerUpPrefab, randomSpawnPoint.localPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No spawn points assigned for power-up spawner!");
        }
    }
}
