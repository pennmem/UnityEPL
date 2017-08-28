using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using AsyncIO;

public class Publisher : MonoBehaviour
{

	private PublisherSocket m_publisher;


	// Use this for initialization
	void Start()
	{
		ForceDotNet.Force();

		//	m_context = NetMQContext.Create();
		m_publisher=new PublisherSocket();
		m_publisher.Bind ("tcp://localhost:8889");
		//		m_publisher.SendFrame ("A");


		Debug.Log("Connected");
	}

	// Update is called once per frame
	void Update()
	{
		m_publisher.SendFrame ("CONNECTED");
	}

	void OnApplicationQuit()
	{
		m_publisher.Dispose ();
		//		m_pull.Dispose();
		Debug.Log("Exit");

	}
}