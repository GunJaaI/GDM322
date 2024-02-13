using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] GameObject statusObject;
    public float speed = 5.0f;
    public float rotateSpeed = 10.0f;
    Rigidbody rb;
    private LoginManagerScript loginManager;
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    private NetworkVariable<int> posX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, 
                                                                    NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isOfflineStatus = new NetworkVariable<bool>( false, 
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

        // playerNameA.OnValueChanged += (NetworkString previousValue, NetworkString newValue) => {
        //     Debug.Log("OwnerID = " + OwnerClientId + ": old name = " + previousValue.info + ": new name = " + newValue.info);
        // };
        // playerNameB.OnValueChanged += (NetworkString previousValue, NetworkString newValue) => {
        //     Debug.Log("OwnerID = " + OwnerClientId + ": old name = " + previousValue.info + ": new name = " + newValue.info);
        // };

        // if (IsServer) {
        //     playerNameA.Value = new NetworkString() { info = new FixedString32Bytes("Player1") };
        //     playerNameB.Value = new NetworkString() { info = new FixedString32Bytes("Player2") };
        // }
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
            if (Input.GetKeyDown(KeyCode.F)) {
                isOfflineStatus.Value = !isOfflineStatus.Value;
            }
            posX.Value = (int)System.Math.Ceiling(transform.position.x);
        }

        UpdatePlayerInfo();
        ChangeEyeColor();
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
            if (isOfflineStatus.Value) { gameObject.GetComponentInChildren<Renderer>().material = loginManager.statusObjectColor[1]; }
            else { statusObject.GetComponent<Renderer>().material = loginManager.statusObjectColor[0]; }
        } else {
            if (isOfflineStatus.Value) { gameObject.GetComponentInChildren<Renderer>().material = loginManager.statusObjectColor[1]; }
            else { statusObject.GetComponent<Renderer>().material = loginManager.statusObjectColor[0]; }
        }
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

    public override void OnDestroy() {
        if (nameLabel != null)
            Destroy(nameLabel.gameObject);

        base.OnDestroy();
    }
}
