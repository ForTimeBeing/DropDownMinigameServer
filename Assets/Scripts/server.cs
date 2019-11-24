using UnityEngine.Networking;
using UnityEngine;

public class server : MonoBehaviour {
	

	private const int MAX_USER = 10;
	private const int PORT = 26000;
	private const int WEB_PORT = 26001;

	private byte reliableChannel;
	private int hostID;
	private int WebHostID;

	private bool isStarted = false;

	private void Start(){
		DontDestroyOnLoad(gameObject);
		Init();
	}
	public void Init(){
		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();
		cc.AddChannel(QosType.Reliable);

		HostTopology topo = new HostTopology(cc, MAX_USER);

		//Server only code
		hostID = NetworkTransport.AddHost(topo,PORT,null);
		WebHostID = NetworkTransport.AddWebsocketHost(topo, WEB_PORT,null);

		Debug.Log(string.Format("Opening connection on port {0} and webport {1}", PORT, WEB_PORT));
		isStarted = true;
	}
	public void Shutdown(){
		isStarted = false;
		NetworkTransport.Shutdown();
	}
}
