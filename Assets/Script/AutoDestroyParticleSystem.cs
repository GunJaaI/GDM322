using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AutoDestroyParticleSystem : NetworkBehaviour
{
    public float delayBeforeDestroy = 2f;
    private ParticleSystem ps;
    public void Start() {
        ps = GetComponent<ParticleSystem>();
    }

    public void Update() {
        if (!IsOwner) return;
        if (ps && !ps.IsAlive()) {
            DestroyObject();
        }
    }

    void DestroyObject() {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject, delayBeforeDestroy);
    }
}
