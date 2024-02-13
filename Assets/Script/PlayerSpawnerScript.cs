using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnerScript : MonoBehaviour {
    //PlayerMovement playerMovement;
    public Behaviour[] scripts;
    public Renderer[] renderers;
    // Start is called before the first frame update
    void Start() {
        //playerMovement = gameObject.GetComponent<PlayerMovement>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void SetPlayerStatus(bool state) {
        foreach (var script in scripts) {script.enabled = state;}
        foreach (var renderer in renderers) {renderer.enabled = state;}
    }

    // Update is called once per frame
    private Vector3 GetRandomPos() {
        Vector3 randPos = new Vector3(Random.Range(-1f, 3f), 1f, Random.Range(-3f, 3f));
        return randPos;
    }

    public void Respawn() {
        RespawnServerRpc();
    }

    [ServerRpc]
    void RespawnServerRpc() {
        Vector3 pos = GetRandomPos();
        RespawnClientRpc(pos);
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 spawnPos) {
        StartCoroutine(RespawnCoroutine(spawnPos));
    }
    IEnumerator RespawnCoroutine(Vector3 spawnPos) {
        SetPlayerStatus(false);
        transform.position = spawnPos;
        yield return new WaitForSeconds(3f);
        SetPlayerStatus(true);
        // playerMovement.enabled = false;
        // //might have delay time
        // transform.position = spawnPos;
        // playerMovement.enabled = true;
    }
}
