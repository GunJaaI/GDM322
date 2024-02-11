using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    public void OnHostButtonClick() {
        NetworkManager.Singleton.StartHost();
    }

    public void OnServerButtonClick() {
        NetworkManager.Singleton.StartServer();
    }

    public void OnClientButtonClick() {
        NetworkManager.Singleton.StartClient();
    }
}
