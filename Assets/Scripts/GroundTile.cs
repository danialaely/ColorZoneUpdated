using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTile : MonoBehaviour
{
    GroundSpawner spawner;

    // Start is called before the first frame update
    void Start()
    {
        spawner = GameObject.FindObjectOfType<GroundSpawner>();
    }

    private void OnTriggerExit(Collider other)
    {
        spawner.SpawnTile();
        Destroy(this.gameObject,2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
