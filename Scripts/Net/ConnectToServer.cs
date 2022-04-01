using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
	[SerializeField] private TMP_InputField usernameInput;
	[SerializeField] private TMP_Text buttonText;

	public void OnClickConnect()
	{
		if (usernameInput.text.Length >= 1)
		{
			PhotonNetwork.NickName = usernameInput.text;
			buttonText.text = "Connecting...";
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		SceneManager.LoadScene("Lobby");
	}
}