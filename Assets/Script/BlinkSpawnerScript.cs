using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BlinkSpawnerScript : NetworkBehaviour {
    public GameObject blinkEffectPrefab;
    private List<GameObject> spawnedBlink = new List<GameObject>();

    // Update is called once per frame
    void Update() {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Y)) {
            SpawnBlinkServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBlinkServerRpc() {
        Vector3 spawnPos = transform.position 
                        + (transform.forward * 3.2f) 
                        + (transform.up * 1.35f)
                        + (transform.right * 0.615f);
        Quaternion spawnRot = Quaternion.Euler(90f, 
                                               transform.rotation.eulerAngles.y, 
                                               transform.rotation.eulerAngles.z);
        GameObject blink = Instantiate(blinkEffectPrefab, spawnPos, spawnRot);
    
        spawnedBlink.Add(blink);

        blink.GetComponent<BlinkScript>().blinkSpawner = this;
        blink.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc (RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectID) {
        //when find obj
        GameObject obj = FindSpawnedBlink(networkObjectID); 
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();

        //when Despawn -> remove form list , destroy obj
        spawnedBlink.Remove(obj);
        Destroy(obj);   
    }

    private GameObject FindSpawnedBlink(ulong networkObjectID) {
        foreach (GameObject blink in spawnedBlink) {
            ulong blinkID = blink.GetComponent<NetworkObject>().NetworkObjectId;
            if (blinkID == networkObjectID) {
                return blink;
            }
        }
        return null;
    }
}
