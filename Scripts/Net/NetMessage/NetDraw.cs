using Unity.Networking.Transport;

public class NetDraw : NetMessage
{
    public int teamId;
    public byte offerDraw;

    public NetDraw()
    {
        Code = OpCode.DRAW;
    }

    public NetDraw(DataStreamReader reader)
    {
        Code = OpCode.DRAW;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteByte(offerDraw);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        offerDraw = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_DRAW?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_DRAW?.Invoke(this, cnn);
    }

}