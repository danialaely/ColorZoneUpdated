using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject[] groundTiles; // Array to hold multiple ground tile prefabs
    public Vector3 initialSpawnPoint; // Define the initial spawn position
    private Vector3 nextSpawnPoint;

    public void SpawnTile()
    {
        // Randomly select a tile from the array
        GameObject selectedTile = groundTiles[Random.Range(0, groundTiles.Length)];

        // Instantiate the selected tile at the next spawn point
        GameObject temp = Instantiate(selectedTile, nextSpawnPoint, Quaternion.identity);

        // Update the next spawn point using the child transform (assuming it exists at index 24)
        nextSpawnPoint = temp.transform.GetChild(3).transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial spawn point
        nextSpawnPoint = initialSpawnPoint;

        // Spawn the first few tiles
        SpawnTile();
        SpawnTile();
        SpawnTile();
    }

}
