using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	[Header("Lobby Panel")]
	[SerializeField] private TMP_InputField roomInput;
	[SerializeField] private GameObject lobbyPanel;

	[Header("Room Panel")]
	[SerializeField] private GameObject roomPanel;
	[SerializeField] private TMP_Text roomName;
	[SerializeField] private RoomItem roomItemPrefab;
	[SerializeField] private Transform contentObject;
	[SerializeField] private List<PlayerItem> playerItemsList = new List<PlayerItem>();
	[SerializeField] private PlayerItem playerItemPrefab;
	[SerializeField] private Transform playerItemParent;
	[SerializeField] private Button playButton;
	List<RoomItem> roomItemsList = new List<RoomItem>();

	[Header("Scene Management")]
	[SerializeField] private SceneTransition sceneTransition;
	
	[Header("Bug Fixes")]
	[SerializeField] private float timeBetweenUpdates = 1.5f;
	float nextUpdateTime;

	private void Start()
	{
		PhotonNetwork.JoinLobby();
	}

	private void Update()
	{

		if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
		{
			playButton.gameObject.SetActive(true);
		}
		else
		{
			playButton.gameObject.SetActive(false);
		}
	}

	public void OnClickCreate()
	{
		if (roomInput.text.Length >= 1)
		{
			PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions() { MaxPlayers = 2, BroadcastPropsChangeToAll = true });
		}
	}

	public override void OnJoinedRoom()
	{
		lobbyPanel.SetActive(false);
		roomPanel.SetActive(true);
		roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
		UpdatePlayerList();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		if (Time.time >= nextUpdateTime)
		{
			UpdateRoomList(roomList);
			nextUpdateTime = Time.time + timeBetweenUpdates;
		}
	}

	void UpdateRoomList(List<RoomInfo> list)
	{
		foreach (RoomItem item in roomItemsList)
		{
			Destroy(item.gameObject);
		}

		roomItemsList.Clear();

		foreach (RoomInfo room in list)
		{
			RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
			newRoom.SetRoomName(room.Name);
			roomItemsList.Add(newRoom);

			if (room.PlayerCount == 0)
			{
				PhotonNetwork.Destroy(newRoom.gameObject);
				roomItemsList.Remove(newRoom);
			}
		}
	}

	public void JoinRoom(string roomName)
	{
		PhotonNetwork.JoinRoom(roomName);
	}

	public void OnClickLeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom()
	{
		roomPanel.SetActive(false);
		lobbyPanel.SetActive(true);
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
	}

	void UpdatePlayerList()
	{
		foreach (PlayerItem item in playerItemsList)
		{
			Destroy(item.gameObject);
		}

		playerItemsList.Clear();

		if (PhotonNetwork.CurrentRoom == null)
		{
			return;
		}

		foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
		{
			PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
			newPlayerItem.SetPlayerInfo(player.Value);

			if (player.Value == PhotonNetwork.LocalPlayer)
			{
				newPlayerItem.ApplyLocalChanges();
			}

			playerItemsList.Add(newPlayerItem);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdatePlayerList();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		UpdatePlayerList();
	}

	public void OnClickPlayButton()
	{
		sceneTransition.LoadNextScene("OnlineChess");
		PhotonNetwork.LoadLevel("OnlineChess");
	}
}