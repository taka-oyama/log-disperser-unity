using UnityEngine;
using System.Collections;
using System;

public class Example : MonoBehaviour {
	int counter = 0;
	System.Random rnd;
	LogDisperser logger;

	void Start () {
		string roomId = "test";
		rnd = new System.Random();
		logger = new LogDisperser(new Uri("http://localhost:3030/socket.io/"), roomId).Enable();
		Application.logMessageReceivedThreaded += (condition, stackTrace, type) => {
			logger.Send(type, condition);
		};
	}

	void Update () {
		var message = "Counter: " + counter + "\n" + Guid.NewGuid().ToString();
		switch (rnd.Next (0, 3)) {
		case 0:
			Debug.Log(message);
			break;
		case 1:
			Debug.LogWarning(message);
			break;
		case 2:
			Debug.LogError(message);
			break;
		}
		counter++;
	}
}
