using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;
using System;

public class Server : MonoBehaviour
{
	#region Singleton Implementation

	public static Server Instance { set; get; }
	private void Awake()
	{
		Instance = this;
	}

	#endregion

	public Chessboard chessBoard;
	public NetworkDriver driver;
	public NativeList<NetworkConnection> connections;

	private bool isActive = false;
	private const float keepAliveTickRate = 20.0f;
	private float lastKeepAlive;

	public Action connectionDropped;

	#region Methods

	public void Init(ushort port)
	{
		driver = NetworkDriver.Create();
		NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
		endpoint.Port = port;

		if (driver.Bind(endpoint) != 0 || !endpoint.IsValid)
		{
			Debug.Log("Unable to bind on port " + endpoint.Port);
			ShutDown();
			return;
		}
		else
		{
			driver.Listen();
			Debug.Log("Currently listening on port " + endpoint.Port);
		}

		connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
		isActive = true;
	}

	public void ShutDown()
	{
		if (isActive)
		{
			driver.Dispose();
			connections.Dispose();
			isActive = false;
			chessBoard.GameReset();
			GameUI.Instance.ChangeCamera(CameraAngle.menu);
			GameUI.Instance.menuAnimator.SetTrigger("StartMenu");
			chessBoard.drawButton.gameObject.SetActive(false);
			chessBoard.drawIndicator.gameObject.SetActive(false);
			chessBoard.drawIndicator.transform.GetChild(0).gameObject.SetActive(false);
			chessBoard.drawIndicator.transform.GetChild(1).gameObject.SetActive(false);

		}
	}

	public void OnDestroy()
	{
		ShutDown();
	}

	public void OnApplicationQuit()
	{
		ShutDown();
	}

	public void Update()
	{
		if (!isActive)
			return;

		KeepAlive();
		
		if (connections.Length == 2)
		{
			chessBoard.drawButton.interactable = true;
			chessBoard.drawButton.gameObject.SetActive(true);
			chessBoard.drawIndicator.gameObject.SetActive(true);
		}

		driver.ScheduleUpdate().Complete();

		CleanupConnections();
		AcceptNewConnections();
		UpdateMessagePump();
	}

	private void KeepAlive()
	{
		if (Time.time - lastKeepAlive > keepAliveTickRate)
		{
			lastKeepAlive = Time.time;
			Broadcast(new NetKeepAlive());
		}
	}

	private void CleanupConnections()
	{
		for (int i = 0; i < connections.Length; i++)
		{
			if (!connections[i].IsCreated)
			{
				connections.RemoveAtSwapBack(i);
				--i;
			}
		}
	}

	private void AcceptNewConnections()
	{
		// Accept new Connections
		NetworkConnection c;
		while ((c = driver.Accept()) != default(NetworkConnection))
		{
			connections.Add(c);
		}
	}

	private void UpdateMessagePump()
	{
		DataStreamReader stream;
		for (int i = 0; i < connections.Length; i++)
		{
			NetworkEvent.Type cmd;
			while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
			{
				if (cmd == NetworkEvent.Type.Data)
				{
					NetUtility.OnData(stream, connections[i], this);
				}
				else if (cmd == NetworkEvent.Type.Disconnect)
				{
					Debug.Log("Client disconnected from server");
					connections[i] = default(NetworkConnection);
					connectionDropped?.Invoke();
					ShutDown(); // This is only happening because there are only 2 players.
				}
			}
		}
	}

	#endregion

	#region Server Specific

	public void SendToClient(NetworkConnection connection, NetMessage msg)
	{
		DataStreamWriter writer;
		driver.BeginSend(connection, out writer);
		msg.Serialize(ref writer);
		driver.EndSend(writer);
	}

	public void Broadcast(NetMessage msg)
	{
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].IsCreated)
			{
				Debug.Log($"Sending {msg.Code} to: {connections[i].InternalId}");
				SendToClient(connections[i], msg);
			}
		}
	}

	#endregion
}