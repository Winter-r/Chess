using Unity.Networking.Transport;
using UnityEngine;

public class NetRookPromo : NetMessage
{	
	public NetRookPromo()
	{
		Code = OpCode.ROOK_PROMO;
	}
	
	public NetRookPromo(DataStreamReader reader)
	{
		Code = OpCode.ROOK_PROMO;
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
		NetUtility.C_ROOK_PROMO?.Invoke(this);
	}
	
	public override void ReceivedOnServer(NetworkConnection cnn)
	{
		NetUtility.S_ROOK_PROMO?.Invoke(this, cnn);
	}

}