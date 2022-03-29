using UnityEngine;
using TMPro;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
	[SerializeField] private TMP_InputField createInput;
	[SerializeField] private TMP_InputField joinInput;
	
	public void CreateRoom()
	{
		PhotonNetwork.CreateRoom(createInput.text);
	}
	
	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom(joinInput.text);
	}

	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Chess");
	}
}