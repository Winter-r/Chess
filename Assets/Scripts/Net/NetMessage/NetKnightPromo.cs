using Unity.Networking.Transport;
using UnityEngine;

public class NetKnightPromo : NetMessage
{	
	public NetKnightPromo()
	{
		Code = OpCode.KNIGHT_PROMO;
	}
	
	public NetKnightPromo(DataStreamReader reader)
	{
		Code = OpCode.KNIGHT_PROMO;
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
		NetUtility.C_KNIGHT_PROMO?.Invoke(this);
	}
	
	public override void ReceivedOnServer(NetworkConnection cnn)
	{
		NetUtility.S_KNIGHT_PROMO?.Invoke(this, cnn);
	}

}