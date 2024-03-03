using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class HPPlayerScript : NetworkBehaviour
{
    TMP_Text p1Text;
    TMP_Text p2Text;
    PlayerMovement playerMovement;
    private OwnerNetworkAnimation ownerNetworkAnimation;
    public NetworkVariable<int> hpP1 = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, 
                                                                   NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> hpP2 = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, 
                                                                   NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        p1Text = GameObject.Find("P1HPText (TMP)").GetComponent<TMP_Text>();
        p2Text = GameObject.Find("P2HPText (TMP)").GetComponent<TMP_Text>();
        playerMovement = GetComponent<PlayerMovement>();
        ownerNetworkAnimation = GetComponent<OwnerNetworkAnimation>();
    }

    private void UpdatePlayerNameAndScore()
    {
        if (IsOwnedByServer)
        {
            p1Text.text = $"{playerMovement.playerNameA.Value} : {hpP1.Value}";
        }
        else
        {
            p2Text.text = $"{playerMovement.playerNameB.Value} : {hpP2.Value}";
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerNameAndScore();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsLocalPlayer) return;

        // Script -HP
        if (collision.gameObject.tag == "DeathZone") {
            if (IsOwnedByServer) {
                hpP1.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            } else {
                hpP2.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            }
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        } else if (collision.gameObject.tag == "Bomb") {
            if (IsOwnedByServer) {
                hpP1.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            } else {
                hpP2.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            }
        } else if (collision.gameObject.tag == "Blink") {
            if (IsOwnedByServer) {
                hpP1.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            } else {
                hpP2.Value--;
                ownerNetworkAnimation.SetTrigger("TakeDamage");
            }
        }

        if (hpP1.Value <= 0 || hpP2.Value <= 0) {
            ownerNetworkAnimation.SetTrigger("Death");
        }
    }
}
