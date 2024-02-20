using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSpawnerScript : NetworkBehaviour {
    public GameObject bombPrefab;
    private List<GameObject> spawnedBomb = new List<GameObject>();

    // Update is called once per frame
    void Update() {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.T)) {
            SpawnBombServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBombServerRpc() {
        Vector3 spawnPos = transform.position 
                        + (transform.forward * 1.5f) 
                        + (transform.up * 1.5f);
        Quaternion spawnRot = transform.rotation;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);
        
        spawnedBomb.Add(bomb);

        bomb.GetComponent<BombScript>().bombSpawner = this;
        bomb.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc (RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectID) {
        //when find obj
        GameObject obj = FindSpawnedBomb(networkObjectID); 
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();

        //when Despawn -> remove form list , destroy obj
        spawnedBomb.Remove(obj);
        Destroy(obj);   
    }

    private GameObject FindSpawnedBomb(ulong networkObjectID) {
        foreach (GameObject bomb in spawnedBomb) {
            ulong bombID = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            if (bombID == networkObjectID) {
                return bomb;
            }
        }
        return null;
    }
}
