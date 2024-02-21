using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BlinkScript : NetworkBehaviour {
    //Monitoring hitblock and destroy
    public BlinkSpawnerScript blinkSpawner;
    public GameObject effectPrefab;
    public float selfDestructDelay = 0.5f;

    void Update() {
        StartCoroutine(SelfDestruct());
    }

    private void OnCollisionEnter(Collision collision) {
        if (!IsOwner) return;
        if (collision.gameObject.tag == "Player") {

            SpawnEffect(collision);

            ulong networkObjectID = GetComponent<NetworkObject>().NetworkObjectId;
            blinkSpawner.DestroyServerRpc(networkObjectID);
        }
    }

    private void SpawnEffect(Collision collision) {
        // Use the collision's contact point to determine the position of the hit particle
        Vector3 spawnPosition = collision.contacts[0].point;
        
        // Get the normal vector of the collision to determine the rotation of the hit particle
        Quaternion spawnRotation = Quaternion.LookRotation(collision.contacts[0].normal);

        GameObject effect = Instantiate(effectPrefab, spawnPosition, spawnRotation);
        effect.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator SelfDestruct() {
        yield return new WaitForSeconds(selfDestructDelay);

        if (IsServer) {
            // Destroy the collider on the server
            blinkSpawner.DestroyServerRpc(GetComponent<NetworkObject>().NetworkObjectId);
        }
    }
}
