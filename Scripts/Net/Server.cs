using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;
using System;

public class Server : MonoSingleton<Server>
{
	public NetworkDriver driver;
	protected NativeList<NetworkConnection> connections;

	private bool isActive = false;
	private const float keepAliveTickRate = 20.0f;
	private float lastKeepAlive;

	public Action connectionDropped;

	// Methods
	public void Init(ushort port)
	{
		driver = NetworkDriver.Create();
		NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
		endPoint.Port = port;

		if (driver.Bind(endPoint) != 0)
		{
			Debug.Log("Unable to bind to the port " + endPoint.Port);
			return;
		}
		else
		{
			driver.Listen();
			Debug.Log("Currently Listening on port " + endPoint.Port);
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
		}
	}

	public void OnDestroy()
	{
		ShutDown();
	}

	public void Update()
	{
		if (!isActive)
		{
			return;
		}

		KeepAlive();

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
					connections[i] = default(NetworkConnection);
					connectionDropped?.Invoke();
					ShutDown();
					Debug.Log("Client disconnected from server");
				}

			}
		}
	}

	// Server Specefic
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
				Debug.Log($"Sending {msg.Code} to : {connections[i].InternalId}");
				SendToClient(connections[i], msg);
			}
		}
	}
}