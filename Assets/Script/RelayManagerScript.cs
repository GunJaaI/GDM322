using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using QFSW.QC;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using Unity.VisualScripting;
//using Unity.Netcode.Editor;

public class RelayManagerScript : Singleton<RelayManagerScript> {
    private async void Start() {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) {
            AuthenticationService.Instance.SignedIn += () => {
                Debug.Log("Signed-in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public UnityTransport transport => NetworkManager.Singleton.GetComponent<UnityTransport>();
    public bool isRelayEnabled => 
        transport != null && transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    [Command]
    public async Task CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code = " + joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            //NetworkManager.Singleton.StartHost();
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    [Command]
    public async Task JoinRelay(string joinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            //NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }
}
