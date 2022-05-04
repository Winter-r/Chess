using UnityEngine;
using Photon.Pun;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
	[SerializeField] private TMP_InputField usernameInput;
	[SerializeField] private TMP_Text buttonText;
	[SerializeField] private SceneTransition sceneTransition;

	public void OnClickConnect()
	{
		if (usernameInput.text.Length >= 1)
		{
			PhotonNetwork.NickName = usernameInput.text;
			buttonText.text = "Connecting...";
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		StreamChatBehaviour.instance.GetOrCreateClient(usernameInput.text);
		sceneTransition.LoadNextScene("Lobby");
	}
}