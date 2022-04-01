using UnityEngine;
using TMPro;

public class RoomItem : MonoBehaviour
{
	[SerializeField] private TMP_Text roomName;
	[SerializeField] private LobbyManager manager;
	
	private void Start()
	{
		manager = FindObjectOfType<LobbyManager>();
	}
	
	public void SetRoomName(string _roomName)
	{
		roomName.text = _roomName;
	}
	
	public void OnClickItem()
	{
		manager.JoinRoom(roomName.text);
	}
}
