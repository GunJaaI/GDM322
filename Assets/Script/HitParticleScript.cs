using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HitParticleScript : NetworkBehaviour
{
    public float damageAmount = 10f; // Adjust this based on your requirements

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Player"))
        {
            // Apply damage to the player
            //other.GetComponent<HPPlayerScript>().OnCollisionEnter(collision);

            // Destroy the particle when it hits the player
            NetworkObject.Destroy(gameObject);
        }
    }
}