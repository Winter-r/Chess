using System;
using System.Threading.Tasks;
using StreamChat.Core;
using StreamChat.Core.Auth;
using StreamChat.Core.Events;
using StreamChat.Core.Exceptions;
using StreamChat.Core.Models;
using StreamChat.Core.Requests;
using StreamChat.Libs.Utils;
using UnityEngine;

public class StreamChatBehaviour : MonoBehaviour
{
	public static StreamChatBehaviour instance;
	
	IStreamChatClient client;

	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(this);
	}
	
	public void GetOrCreateClient(string userName)
	{
		string userId = StreamChatClient.SanitizeUserId(userName);
		AuthCredentials credentials = new AuthCredentials("xg4h594jgy57", userId, StreamChatClient.CreateDeveloperAuthToken(userId));
		client = StreamChatClient.CreateDefaultClient(credentials);
		client.Connect();
		client.Connected += OnClientConnected;
	}
	
	void OnClientConnected()
	{
		Debug.Log("Stream client connected!");
	}
	
	private void Update()
	{
		if (client != null)
		{
			client.Update(Time.deltaTime);
		}
	}
}
