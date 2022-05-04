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
}
