using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] GameObject statusObject;
    [SerializeField] GameObject statusWand;
    public float speed = 5.0f;
    public float rotateSpeed = 10.0f;
    Rigidbody rb;
    private LoginManagerScript loginManager;
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    private bool timerIsRunning = false;

    private NetworkVariable<int> posX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, 
                                                                    NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isOfflineStatus = new NetworkVariable<bool>( false, 
                                                                    NetworkVariableReadPermission.Everyone, 
                                                                    NetworkVariableWritePermission.Owner);
                                
    private NetworkVariable<bool> isOfflineWand = new NetworkVariable<bool>( false, 
                                                                    NetworkVariableReadPermission.Everyone, 
                                                                    NetworkVariableWritePermission.Owner);
    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(new NetworkString { info = "Player" }, 
                                                             NetworkVariableReadPermission.Everyone, 
                                                             NetworkVariableWritePermission.Owner);
    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(new NetworkString { info = "Player" }, 
                                                             NetworkVariableReadPermission.Everyone, 
                                                             NetworkVariableWritePermission.Owner);
    public struct NetworkString : INetworkSerializable {
        public FixedString32Bytes info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref info);
        }
        public override string ToString() {
            return info.ToString();
        }
        public static implicit operator NetworkString(string v) => new NetworkString() { 
            info = new FixedString32Bytes(v) 
        };
    }
    public override void OnNetworkSpawn() {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as TMP_Text;
        nameLabel.transform.SetParent(canvas.transform);

        // Instaniate Head Wand to not use GetComponentInChildren
        statusWand = Instantiate(statusWand, transform.position + new Vector3(0.615f, 1.343f, 0.727f), Quaternion.identity);
        statusWand.transform.SetParent(transform);

        posX.OnValueChanged += (int previousValue, int newValue) => {
            Debug.Log("Owner ID = " + OwnerClientId + ": Pos X = " + posX.Value);
        };

        if (IsOwner) {
            loginManager = GameObject.FindObjectOfType<LoginManagerScript>();
            if (loginManager != null) {
                string name = loginManager.userNameInputField.text;
                if (IsOwnedByServer) { playerNameA.Value = name; }
                else { playerNameB.Value = name; }
            }
        }
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        loginManager = GameObject.FindAnyObjectByType<LoginManagerScript>();
    }

    private void Update() {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;

        if (IsOwner) {
            if (Input.GetKeyDown(KeyCode.R)) {
                isOfflineStatus.Value = !isOfflineStatus.Value;

            }

            if (Input.GetKeyDown(KeyCode.Y) && !timerIsRunning) {
                isOfflineWand.Value = !isOfflineWand.Value;
                StartCoroutine(WaitForColorChangeBack());

            }

            posX.Value = (int)System.Math.Ceiling(transform.position.x);
            if (Input.GetKeyDown(KeyCode.X)) {
                TestServerRpc("Hello ", new ServerRpcParams());
            }
            if (Input.GetKeyDown(KeyCode.C)) {
                ClientRpcSendParams clientRpcSendParams = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } }; 
                ClientRpcParams clientRpcParams = new ClientRpcParams { Send = clientRpcSendParams }; 
                TestClientRpc("Hello ", clientRpcParams);
            }
        }

        UpdatePlayerInfo();
        ChangeEyeColor();
        ChangeWandColor();
    }

    [ServerRpc]
    private void TestServerRpc(string msg, ServerRpcParams serverRpcParams) {
        Debug.Log("Test server RPC form Client " + OwnerClientId);
    }

    [ClientRpc]
    private void TestClientRpc(string msg, ClientRpcParams clientRpcParams) {
        Debug.Log("Test client RPC form Server " + msg);
    }

    void UpdatePlayerInfo() {
        if (IsOwnedByServer) {
            nameLabel.text = playerNameA.Value.ToString();
        } else { 
            nameLabel.text = playerNameB.Value.ToString(); 
        }
    }

    void ChangeEyeColor() {
        if (IsOwnedByServer && OwnerClientId == 0) {
            if (isOfflineStatus.Value) { 
                gameObject.GetComponentInChildren<Renderer>().material = loginManager.statusObjectColor[1]; 
            } else { 
                statusObject.GetComponent<Renderer>().material = loginManager.statusObjectColor[0]; 
            }
        } else {
            if (isOfflineStatus.Value) { 
                gameObject.GetComponentInChildren<Renderer>().material = loginManager.statusObjectColor[1]; 
            } else { 
                statusObject.GetComponent<Renderer>().material = loginManager.statusObjectColor[0]; 
            }
        }
    }

    void ChangeWandColor() {
        // if (IsOwnedByServer && OwnerClientId == 0) {
        //     if (isOfflineWand.Value) { 
        //         gameObject.GetComponentInParent<Renderer>().material = loginManager.statusWandColor[1]; 
        //     } else { 
        //         statusObject.GetComponent<Renderer>().material = loginManager.statusWandColor[0]; 
        //     }
        // } else {
        //     if (isOfflineWand.Value) { 
        //         gameObject.GetComponentInParent<Renderer>().material = loginManager.statusWandColor[1]; 
        //     } else { 
        //         statusObject.GetComponent<Renderer>().material = loginManager.statusWandColor[0]; 
        //     }
        // }
        if (IsOwnedByServer && OwnerClientId == 0) {
            if (isOfflineWand.Value) {
                // Use the wandObject reference here
                statusWand.GetComponent<Renderer>().material = loginManager.statusWandColor[1];
            } else {
                // Use the wandObject reference here
                statusWand.GetComponent<Renderer>().material = loginManager.statusWandColor[0];
            }
        } else {
            if (isOfflineWand.Value) {
                // Use the wandObject reference here
                statusWand.GetComponent<Renderer>().material = loginManager.statusWandColor[1];
            } else {
                // Use the wandObject reference here
                statusWand.GetComponent<Renderer>().material = loginManager.statusWandColor[0];
            }
        }
    }

    private IEnumerator WaitForColorChangeBack() {
        timerIsRunning = true;
        yield return new WaitForSeconds(0.5f);

        // Change the color back after waiting for 0.5 seconds
        isOfflineWand.Value = !isOfflineWand.Value;

        // Reset the flag
        timerIsRunning = false;
    }

    private void FixedUpdate() {
        if (IsOwner) {
            float translation = Input.GetAxis("Vertical") * speed ;
            translation *= Time.deltaTime;
            rb.MovePosition(rb.position + this.transform.forward * translation);

            float rotate = Input.GetAxis("Horizontal") * rotateSpeed ;
            if (rotate != 0) {
                rotate *= rotateSpeed;
                Quaternion turn = Quaternion.Euler(0f, rotate, 0f);
                rb.MoveRotation(rb.rotation * turn);
            } else {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    // public override void OnDestroy() {
    //     if (nameLabel != null)
    //         Destroy(nameLabel.gameObject);

    //     base.OnDestroy();
    // }

    private void OnEnable() {
        if (nameLabel != null) {
            nameLabel.enabled = true;
        }
    }

    private void OnDisable() {
        if (nameLabel != null) {
            nameLabel.enabled = false;
        }
    }
}
