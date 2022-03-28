using Unity.Networking.Transport;
using UnityEngine;
using System;

public enum OpCode
{
	KEEP_ALIVE = 1,
	WELCOME = 2,
	START_GAME = 3,
	MAKE_MOVE = 4,
	REMATCH = 5,
	RESIGN = 6,
	DRAW = 7
}

public static class NetUtility
{
	public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null)
	{
		NetMessage msg = null;
		var opCode = (OpCode)stream.ReadByte();
		switch (opCode)
		{
			case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(stream); break;
			case OpCode.WELCOME: msg = new NetWelcome(stream); break;
			case OpCode.START_GAME: msg = new NetStartGame(stream); break;
			case OpCode.MAKE_MOVE: msg = new NetMakeMove(stream); break;
			case OpCode.REMATCH: msg = new NetRematch(stream); break;
			case OpCode.RESIGN: msg = new NetResign(stream); break;
			case OpCode.DRAW: msg = new NetDraw(stream); break;
			default:
				Debug.LogError("Message recieved had no OpCode");
				break;
		}
		
		if (server != null)
		{
			msg.ReceivedOnServer(cnn);
		}
		else
		{
			msg.ReceivedOnClient();
		}
	}
	
	// Net Messages
	public static Action<NetMessage> C_KEEP_ALIVE;
	public static Action<NetMessage> C_WELCOME;
	public static Action<NetMessage> C_START_GAME;
	public static Action<NetMessage> C_MAKE_MOVE;
	public static Action<NetMessage> C_REMATCH;
	public static Action<NetMessage> C_RESIGN;
	public static Action<NetMessage> C_DRAW;
	public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
	public static Action<NetMessage, NetworkConnection> S_WELCOME;
	public static Action<NetMessage, NetworkConnection> S_START_GAME;
	public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
	public static Action<NetMessage, NetworkConnection> S_REMATCH;
	public static Action<NetMessage, NetworkConnection> S_RESIGN;
	public static Action<NetMessage, NetworkConnection> S_DRAW;
}