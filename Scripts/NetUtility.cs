using Unity.Networking.Transport;
using Unity.Collections;
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
	QUEEN_PROMO = 7,
	KNIGHT_PROMO = 8,
	BISHOP_PROMO = 9,
	ROOK_PROMO = 10
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
			case OpCode.RESIGN: msg = new NetRematch(stream); break;
			case OpCode.QUEEN_PROMO: msg = new NetQueenPromo(stream); break;
			case OpCode.KNIGHT_PROMO: msg = new NetKnightPromo(stream); break;
			case OpCode.BISHOP_PROMO: msg = new NetBishopPromo(stream); break;
			case OpCode.ROOK_PROMO: msg = new NetRookPromo(stream); break;
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
	public static Action<NetMessage> C_QUEEN_PROMO;
	public static Action<NetMessage> C_KNIGHT_PROMO;
	public static Action<NetMessage> C_BISHOP_PROMO;
	public static Action<NetMessage> C_ROOK_PROMO;
	public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
	public static Action<NetMessage, NetworkConnection> S_WELCOME;
	public static Action<NetMessage, NetworkConnection> S_START_GAME;
	public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
	public static Action<NetMessage, NetworkConnection> S_REMATCH;
	public static Action<NetMessage, NetworkConnection> S_RESIGN;
	public static Action<NetMessage, NetworkConnection> S_QUEEN_PROMO;
	public static Action<NetMessage, NetworkConnection> S_KNIGHT_PROMO;
	public static Action<NetMessage, NetworkConnection> S_BISHOP_PROMO;
	public static Action<NetMessage, NetworkConnection> S_ROOK_PROMO;
}