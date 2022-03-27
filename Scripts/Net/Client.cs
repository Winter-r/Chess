using Unity.Networking.Transport;
using UnityEngine;
using System;

public class Client : MonoBehaviour
{
	#region Singleton Implementation

	public static Client Instance { set; get; }
	private void Awake()
	{
		Instance = this;
	}

	#endregion

	public NetworkDriver driver;
	private NetworkConnection connection;

	private bool isActive = false;

	public Action connectionDropped;

	#region Methods

	public void Init(string ip, ushort port)
	{
		driver = NetworkDriver.Create();
		NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);

		connection = driver.Connect(endpoint);

		Debug.Log("Attempting to connect to Server on " + endpoint.Address);

		isActive = true;

		RegisterToEvent();
	}

	public void ShutDown()
	{
		if (isActive)
		{
			UnregisterToEvent();
			driver.Dispose();
			isActive = false;
			connection = default(NetworkConnection);
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

		driver.ScheduleUpdate().Complete();
		CheckAlive();

		UpdateMessagePump();
	}

	private void CheckAlive()
	{
		if (!connection.IsCreated && isActive)
		{
			Debug.Log("Something went wrong, lost connection to server");
			connectionDropped?.Invoke();
			ShutDown();
		}
	}

	private void UpdateMessagePump()
	{
		DataStreamReader stream;
		NetworkEvent.Type cmd;
		while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
		{
			if (cmd == NetworkEvent.Type.Connect)
			{
				SendToServer(new NetWelcome());
			}
			else if (cmd == NetworkEvent.Type.Data)
			{
				NetUtility.OnData(stream, default(NetworkConnection));
			}
			else if (cmd == NetworkEvent.Type.Disconnect)
			{
				Debug.Log("Client got disconnected from server");
				connection = default(NetworkConnection);
				connectionDropped?.Invoke();
				ShutDown();
			}
		}
	}

	public void SendToServer(NetMessage msg)
	{
		DataStreamWriter writer;
		driver.BeginSend(connection, out writer);
		msg.Serialize(ref writer);
		driver.EndSend(writer);
	}

	#endregion
	
	#region Event Parsing
	
	private void RegisterToEvent()
	{
		NetUtility.C_KEEP_ALIVE += OnKeepAlive;
	}
	
	private void UnregisterToEvent()
	{
		NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
	}
	
	private void OnKeepAlive(NetMessage nm)
	{
		// Send it back to keep both alive
		SendToServer(nm);
	}

	#endregion
}