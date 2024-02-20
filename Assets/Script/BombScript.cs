using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BombScript : NetworkBehaviour {
    //Monitoring hitblock and destroy
    public BombSpawnerScript bombSpawner;
    public GameObject effectPrefab;
    private void OnCollisionEnter(Collision collision) {
        if (!IsOwner) return;
        if (collision.gameObject.tag == "Player") {
            ulong networkObjectID = GetComponent<NetworkObject>().NetworkObjectId;
            SpawnEffect();
            bombSpawner.DestroyServerRpc(networkObjectID);
        }
    }

    private void SpawnEffect() {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}
