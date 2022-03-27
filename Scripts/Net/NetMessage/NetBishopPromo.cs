using Unity.Networking.Transport;
using UnityEngine;

public class NetBishopPromo : NetMessage
{	
	public NetBishopPromo()
	{
		Code = OpCode.BISHOP_PROMO;
	}
	
	public NetBishopPromo(DataStreamReader reader)
	{
		Code = OpCode.BISHOP_PROMO;
		Deserialize(reader);
	}
	
	public override void Serialize(ref DataStreamWriter writer)
	{
		writer.WriteByte((byte)Code);
	}
	
	public override void Deserialize(DataStreamReader reader)
	{
	}
	
	public override void ReceivedOnClient()
	{
		NetUtility.C_BISHOP_PROMO?.Invoke(this);
	}
	
	public override void ReceivedOnServer(NetworkConnection cnn)
	{
		NetUtility.S_BISHOP_PROMO?.Invoke(this, cnn);
	}

}