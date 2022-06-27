using UnityEngine;
using TMPro;

public class Message : MonoBehaviour
{
	public TMP_Text messageText;
	public TMP_Text usernameText;

	[HideInInspector] public string messageId;
	[HideInInspector] public string userId;

	public GameObject deleteButton;

	public GameObject updateButton;
	public TMP_InputField updateInputField;
	bool updateFieldOpen;
	
	public GameObject likeButton;
	public GameObject reactionObject;
	public TMP_Text numberOfLikes;
	bool buttonLikes = true;

	private void Start()
	{
		if (StreamChatBehaviour.instance.client.UserId != userId)
		{
			deleteButton.SetActive(false);
			updateButton.SetActive(false);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			StreamChatBehaviour.instance.UpdateMessage(messageId, updateInputField.text);
			updateInputField.gameObject.SetActive(false);
			deleteButton.SetActive(true);
			updateFieldOpen = false;
		}
	}

	public void OnClickDeleteButton()
	{
		StreamChatBehaviour.instance.DeleteMessage(messageId);
		deleteButton.SetActive(false);
		updateButton.SetActive(false);
	}

	public void OnClickUpdateButton()
	{
		if (!updateInputField.enabled)
		{
			updateInputField.gameObject.SetActive(true);
		}

		if (updateInputField.enabled)
		{
			updateInputField.gameObject.SetActive(false);
		}

		updateInputField.text = messageText.text;
		updateFieldOpen = true;
	}
	
	public void OnClickLikeButton()
	{
		if (buttonLikes)
		{
			StreamChatBehaviour.instance.LikeMessage(messageId);
			buttonLikes = false;
		}
		else
		{
			StreamChatBehaviour.instance.UnlikeMessage(messageId);
			buttonLikes = true;
		}
	}
}