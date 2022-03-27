using Unity.Networking.Transport;
using UnityEngine;

public class NetResign : NetMessage
{
	public int teamId;
	
	public NetResign()
	{
		Code = OpCode.RESIGN;
	}

	public NetResign(DataStreamReader reader)
	{
		Code = OpCode.RESIGN;
		Deserialize(reader);
	}

	public override void Serialize(ref DataStreamWriter writer)
	{
		writer.WriteByte((byte)Code);
		writer.WriteInt(teamId);
	}

	public override void Deserialize(DataStreamReader reader)
	{
		teamId = reader.ReadInt();
	}

	public override void ReceivedOnClient()
	{
		NetUtility.C_RESIGN?.Invoke(this);
	}

	public override void ReceivedOnServer(NetworkConnection cnn)
	{
		NetUtility.S_RESIGN?.Invoke(this, cnn);
	}

}