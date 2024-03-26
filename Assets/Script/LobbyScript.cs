using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyScript : MonoBehaviour {
    public string lobbyCode;
    Lobby HostLobby;
    [Command]
    private async void CreateLobby() {
        try {
            string lobbyName = "My lobby";
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            HostLobby = lobby;
            StartCoroutine(HeartbeatLobbyCoroutine(HostLobby.Id, 15));
            Debug.Log("Create Lobby : " + lobbyName + "." + lobby.MaxPlayers);

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    } 

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds) {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    [Command]
    private async void JoinByLobbyCode(string lobbyCode) {
        //Cant use with PrivateLobby
        try {
            Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("Joined By Lobby code : " + lobbyCode);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    [Command]
    private async void QuickJoinLobby() {
        //Cant use with PrivateLobby
        try {
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            Debug.Log(lobby.Name + "." + lobby.AvailableSlots);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ListLobbies() {
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>() {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>() {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            Debug.Log("Lobby Found : " + lobbies.Results.Count);
            foreach (Lobby lobby in lobbies.Results) {
                Debug.Log(lobby.Name + "." + lobby.MaxPlayers);
            }
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinLobby() {
        try {
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            await Lobbies.Instance.JoinLobbyByIdAsync(lobbies.Results[0].Id);
            Debug.Log(lobbies.Results[0].Name + "." + lobbies.Results[0].AvailableSlots);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
}
