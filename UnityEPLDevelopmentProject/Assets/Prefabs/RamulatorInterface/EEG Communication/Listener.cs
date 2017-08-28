using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using AsyncIO;

public class Listener : MonoBehaviour
{
	//private NetMQContext m_context;
	private SubscriberSocket m_subscriber;
	private TextMesh txtMesh;

	// Use this for initialization
	void Start()
	{
		ForceDotNet.Force();


		m_subscriber = new SubscriberSocket();
		m_subscriber.Options.Linger = System.TimeSpan.Zero;
		m_subscriber.Subscribe("");
		m_subscriber.Connect("tcp://localhost:8889");

		txtMesh = GetComponent<TextMesh>();
		//		txtMesh.transform.Translate(-6f, 4f, 0f);
		txtMesh.text = "0";
		Debug.Log("Connected");
	}

	// Update is called once per frame
	void Update()
	{
		string message = "";
		string rcvd;

		//	Debug.Log("Update");

		while (m_subscriber.TryReceiveFrameString(out rcvd))
		{
			message = message + " - " + rcvd;
			UnityEngine.Debug.Log(rcvd);
		}

		//		if (!string.IsNullOrEmpty(message)) message = message + "\n";
		//
		//		while (m_pull.TryReceiveFrameString(out rcvd))
		//		{
		//			message = message + " - " + rcvd;
		//			UnityEngine.Debug.Log(rcvd);
		//		}
		//
		if (!string.IsNullOrEmpty(message)) txtMesh.text = message;
	}

	void OnApplicationQuit()
	{
		m_subscriber.Dispose();
		//		m_pull.Dispose();
		Debug.Log("Exit");

	}
}