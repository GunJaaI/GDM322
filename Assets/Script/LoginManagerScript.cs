using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using QFSW.QC;
using TMPro;
using UnityEditor;
using Unity.Netcode.Transports.UTP;

public class LoginManagerScript : MonoBehaviour {
    
    public string ip_address = "127.0.0.1";
    public TMP_InputField userNameInputField;
    public TMP_InputField passCodeInputField;

    public TMP_InputField ipInputField;
    UnityTransport transport;

    public TMP_Dropdown skinSelector;
    public List<Material> statusObjectColor;
    public List<Material> statusWandColor;
    private bool isApproveConnection = false;

    public GameObject loginPanel;
    public GameObject leaveButton;
    public GameObject scorePanel;

    public List<GameObject> spawnPoint;
    public List<uint> AlternativePlayerPrefabs;

    private void Start() {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        SetUIVisible(false);
    }

    private void OnDestroy() {
        if (NetworkManager.Singleton == null) {
            return;
        }

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleServerStarted() {
        Debug.Log("HandleServerStarted");

    }

    private void HandleClientConnected(ulong clientId) {
        Debug.Log("Connected clientID = " + clientId);

        if (clientId == NetworkManager.Singleton.LocalClientId) {
            SetUIVisible(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId) {
        //Debug.Log("Disconnect clientID = " + clientId);
        if (NetworkManager.Singleton.IsHost) {
            Debug.Log("Host Disconnect clientID = " + clientId);
        } else if (NetworkManager.Singleton.IsClient) {
            Debug.Log("Client Disconnect clientID = " + clientId);
            Leave();
        }
    }

    public void Leave() {
        if (NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        } else if (NetworkManager.Singleton.IsClient) {
            NetworkManager.Singleton.Shutdown();
        }
        SetUIVisible(false);
    }

    public void SetUIVisible(bool isUserLogin) {
        if (isUserLogin) {
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
            scorePanel.SetActive(true);
        } else {
            loginPanel.SetActive(true);
            leaveButton.SetActive(false);
            scorePanel.SetActive(false);
        }
    }

    [Command("set-approve")]

    private string passCodeID;
    public bool SetIsApproveConnection()
    {
        isApproveConnection = !isApproveConnection;
        return isApproveConnection;
    }

    private void setIpAddress() {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        ip_address = ipInputField.GetComponent<TMP_InputField>().text;
        transport.ConnectionData.Address = ip_address;
    }

    public void Host()
    {
        setIpAddress();
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        passCodeID = passCodeInputField.GetComponent<TMP_InputField>().text;
        Debug.Log("PassCode : " + passCodeID);
        Debug.Log("Start host");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        int byteLength = connectionData.Length;
        //bool characterPrefabIndex = 0; ////++
        bool isApproved = false;

        bool isUserNameApproved = false; 
        bool isPassCodeApproved = false;

        if (byteLength > 0) {
            string combinedString = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            string[] allData = combinedString.Split("?");
            string hostData = userNameInputField.GetComponent<TMP_InputField>().text;
            string clientUserNameData = allData[0];
            string clientPassCodeData = allData[1];
            int playerSkinPrefabs = int.Parse(allData[2]);
            Debug.Log(playerSkinPrefabs);
            
            isUserNameApproved = ApproveConnection(clientUserNameData, hostData);
            isPassCodeApproved = ApproveConnectionPassCode(clientPassCodeData);
            response.PlayerPrefabHash = AlternativePlayerPrefabs[playerSkinPrefabs];
        } else {
            if (NetworkManager.Singleton.IsHost) {
                response.PlayerPrefabHash = AlternativePlayerPrefabs[skinSelected()];
            }
        }

        if ((isUserNameApproved == true) && (isPassCodeApproved == true)) {
            isApproved = true;
        } else if ((isUserNameApproved == false) && (isPassCodeApproved == false)) {
            isApproved = false;
            Debug.Log("Username already used and PassCode not exist.");
        } else if ((isUserNameApproved == false) || (isPassCodeApproved == false)) {
            if (isUserNameApproved == false) {
                isApproved = false;
                Debug.Log("Username already used.");
            } else {
                isApproved = false;
                Debug.Log("PassCode not exist.");
            }
        }

        // bool isUserNameApproved = false; 
        // bool isPassCodeApproved = false;

        // if (byteLength > 0)
        // {
        //     string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
        //     string[] allData = clientData.Split("?");

        //     string clientUserNameData = allData[0];
        //     string clientPassCodeData = allData[1];
        //     Debug.Log("SplitData = Complete --> clientUserNameData = " + clientUserNameData);
        //     Debug.Log("SplitData = Complete --> clientPassCodeData = " + clientPassCodeData);

        //     string hostData = userNameInputField.GetComponent<TMP_InputField>().text;

        //     isUserNameApproved = ApproveConnection(clientUserNameData, hostData);
        //     isPassCodeApproved = ApproveConnectionPassCode(clientPassCodeData);

        // }

        // bool isApproved = false;
        // if ((isUserNameApproved == true) && (isPassCodeApproved == true)) {
        //     isApproved = true;
        // } else if ((isUserNameApproved == false) && (isPassCodeApproved == false)) {
        //     isApproved = false;
        //     Debug.Log("Username already used and PassCode not exist.");
        // } else if ((isUserNameApproved == false) || (isPassCodeApproved == false)) {
        //     if (isUserNameApproved == false) {
        //         isApproved = false;
        //         Debug.Log("Username already used.");
        //     } else {
        //         isApproved = false;
        //         Debug.Log("PassCode not exist.");
        //     }
        // }

        // Your approval logic determines the following values
        response.Approved = isApproved;
        response.CreatePlayerObject = true;

        //// The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        //response.PlayerPrefabHash = 694872113; //blue
        //response.PlayerPrefabHash = 666998363; //green

        //    // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        //    // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;
        setSpawnLocation(clientId, response);
        NetworkLog.LogInfoServer("Spawn Pos of " + clientId + " is " + response.Position.ToString());

        //    // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        //    // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";

        //    // If additional approval steps are needed, set this to true until the additional steps are complete
        //    // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }

    public void Client()
    {
        setIpAddress();
        string userName = userNameInputField.GetComponent<TMP_InputField>().text;
        string passCodeID = passCodeInputField.GetComponent<TMP_InputField>().text; ////++
        int playerSkinSelected = skinSelected();
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(userName + "?" + passCodeID + "?" + playerSkinSelected);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Start Client");
    }

    public bool ApproveConnection(string clientUserNameData, string hostData)
    {
        bool isApprove = System.String.Equals(clientUserNameData.Trim(), hostData.Trim()) ? false : true;
        Debug.Log("isUserNameApprove = " + isApprove);
        return isApprove;
    }

    public bool ApproveConnectionPassCode(string clientPassCodeData)
    {
        bool isPassCodeApprove;
        if (clientPassCodeData == passCodeID) {
            isPassCodeApprove = true;
        } else {
            isPassCodeApprove = false;
        }
        Debug.Log("isPassCodeApprove = " + isPassCodeApprove);
        return isPassCodeApprove;
    }
    
    private void setSpawnLocation(ulong clientId,NetworkManager.ConnectionApprovalResponse response) {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            GameObject selectSpawn = spawnPoint[UnityEngine.Random.Range(0, spawnPoint.Count)];
            spawnPosition = selectSpawn.transform.position;
            spawnRotation = selectSpawn.transform.rotation;
            Debug.Log("qwer");
        } else {
            GameObject selectSpawn = spawnPoint[UnityEngine.Random.Range(0, spawnPoint.Count)];
            spawnPosition = selectSpawn.transform.position;
            spawnRotation = selectSpawn.transform.rotation;
            Debug.Log("rewq");
        }
        response.Position = spawnPosition;
        response.Rotation = spawnRotation;
    }

    // private void setSpawnLocation(ulong clientId,NetworkManager.ConnectionApprovalResponse response) {
    //     Vector3 spawnPosition = Vector3.zero;
    //     Quaternion spawnRotation = Quaternion.identity;
    //     if (clientId == NetworkManager.Singleton.LocalClientId) {
    //         spawnPosition = new Vector3(-2f, 0f, 0f);
    //         spawnRotation = Quaternion.Euler(0f, 135f, 0f);
    //         Debug.Log("qwer");
    //     } else {
    //         switch (NetworkManager.Singleton.ConnectedClients.Count) {
    //             case 1:
    //                 spawnPosition = new Vector3(0f, 0f, 0f);
    //                 spawnRotation = Quaternion.Euler(0f, 180f, 0f);
    //                 break;
    //             case 2:
    //                 spawnPosition = new Vector3(2f, 0f, 0f);
    //                 spawnRotation = Quaternion.Euler(0f, 225f, 0f);
    //                 break;
    //         }
    //     }
    //     response.Position = spawnPosition;
    //     response.Rotation = spawnRotation;
    // }

    public int skinSelected() {
    if (skinSelector.GetComponent<TMP_Dropdown>().value == 0) {
      return 0;
    } else if (skinSelector.GetComponent<TMP_Dropdown>().value == 1) {
      return 1;
    } else if (skinSelector.GetComponent<TMP_Dropdown>().value == 2) {
      return 2;
    } 
    
    // else if (skinSelector.GetComponent<TMP_Dropdown>().value == 2) {
    //   return 2;
    // }
    return 0;
    }
}
