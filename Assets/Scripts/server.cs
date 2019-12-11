using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;

public class server : MonoBehaviour
{


    private const int MAX_USER = 10;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    private byte reliableChannel;
    private int hostID;
    private int WebHostID;

    private bool isStarted = false;
    private byte error;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }
    private void Update()
    {
        UpdateMessagePump();
    }
    public void UpdateMessagePump()
    {
        if (!isStarted)
            return;

        int recHostID;      //Is this from Web? Or standalone
        int connectionID;   //Which user is sending my this?
        int channelID;      //Which lane is he senting that message from

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostID, out connectionID, out channelID, recBuffer, BYTE_SIZE, out dataSize, out error);
        switch (type)
        {

            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected through host {1}", connectionID, hostID));
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected", connectionID));
                break;

            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);

                OnData(connectionID, channelID, recHostID, msg);
                break;

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;

        }
    }
    public void Init()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, MAX_USER);

        //Server only code
        hostID = NetworkTransport.AddHost(topo, PORT, null);
        WebHostID = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

        Debug.Log(string.Format("Opening connection on port {0} and webport {1}", PORT, WEB_PORT));
        isStarted = true;
    }
    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    #region OnData
    public void OnData(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.None:
                break;
            case NetOP.SendScore:
                SendScore(connectionID, channelID, recHostID, (Net_SendScore)msg);
                break;
        }
    }
    #endregion
    public void SendScore(int connectionID, int channelID, int recHostID, Net_SendScore netScore)
    {
        Debug.Log(netScore.score);
    }
    #region Send
    public void SendClient(int recHost, int ConnectionID, NetMsg msg)
    {
        //This is where we hold out data
        byte[] buffer = new byte[BYTE_SIZE];

        //this is where you crush your data into a byte[]
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (recHost == 0)
        {
            NetworkTransport.Send(hostID, ConnectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        else
        {
            NetworkTransport.Send(WebHostID, ConnectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
    }
    #endregion
}
