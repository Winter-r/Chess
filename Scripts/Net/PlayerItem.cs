using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;

public class PlayerItem : MonoBehaviourPunCallbacks
{
	public TMP_Text playerName;

	public Color highlightColor;
	public GameObject leftArrow;
	public GameObject rightArrow;
	Image backgroundImage;

	public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

	public Image playerAvatar;
	public Sprite[] avatars;

	Player player;

	private void Awake()
	{
		backgroundImage = GetComponent<Image>();
	}

	public void SetPlayerInfo(Player _player)
	{
		playerName.text = _player.NickName;
		player = _player;
		
		UpdatePlayerItem(player);
	}

	public void ApplyLocalChanges()
	{
		backgroundImage.color = highlightColor;
		leftArrow.SetActive(true);
		rightArrow.SetActive(true);
	}

	public void OnClickLeftArrow()
	{
		if ((int)playerProperties["playerAvatar"] == 0)
		{
			playerProperties["playerAvatar"] = avatars.Length - 1;
		}
		else
		{
			playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
		}

		PhotonNetwork.SetPlayerCustomProperties(playerProperties);
	}

	public void OnClickRightArrow()
	{
		if ((int)playerProperties["playerAvatar"] == avatars.Length - 1)
		{
			playerProperties["playerAvatar"] = 0;
		}
		else
		{
			playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
		}

		PhotonNetwork.SetPlayerCustomProperties(playerProperties);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
		if (player == targetPlayer)
		{
			UpdatePlayerItem(targetPlayer);
		}
	}

	void UpdatePlayerItem(Player player)
	{
		if (player.CustomProperties.ContainsKey("playerAvatar"))
		{
			playerAvatar.sprite = avatars[(int)player.CustomProperties["playerAvatar"]];
			playerProperties["playerAvatar"] = (int)player.CustomProperties["playerAvatar"];
		}
		else
		{
			playerProperties["playerAvatar"] = 0;
		}
	}
}