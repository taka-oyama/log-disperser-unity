using BestHTTP.SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogDisperser {
	public string roomId;
	public string uuid;
	public SocketManager manager;
	public bool isReady = false;

	public LogDisperser(Uri host, string roomId, string uuid = null) {
		this.roomId = roomId;
		this.uuid = uuid ?? SystemInfo.deviceName;
		SocketOptions options = new SocketOptions();
		options.AutoConnect = false;
		manager = new SocketManager(new Uri (host, "socket.io/"), options);
		manager.Socket.On("connect", onConnect);
		manager.Socket.On("disconnect", onDisconnect);
		manager.Socket.On("error", onError);
		manager.Socket.AutoDecodePayload = false;
	}

	public LogDisperser Enable() {
		manager.Open();
		return this;
	}

	public LogDisperser Disable() {
		manager.Close();
		return this;
	}

	public void onConnect(Socket socket, Packet packet, object[] json) {
		Debug.Log ("[LogDisperser] Connected");
		JoinRoom();
	}
	
	public void onDisconnect(Socket socket, Packet packet, object[] json) {
		isReady = false;
		Debug.Log ("[LogDisperser] Disconnected");
	}
	
	public void onError(Socket socket, Packet packet, object[] json) {
		var error = json [0] as Error;
		Debug.Log ("[LogDisperser] " + error.Code + " Error\nMessage: " + error.Message);
	}

	public void JoinRoom() {
		var param = new Dictionary<string, string>(){ { "room", this.roomId } };
		manager.Socket.Emit("create", (s, r, o) => {
			isReady = true;
			Debug.Log ("[LogDisperser] Joined Room: " + this.roomId + " with uuid: " + this.uuid);
		}, param);
	}

	public void Send(LogType type, string message) {
		if (!isReady) {
			return;
		}

		var typeStr = "Unknown";
		switch(type) {
		case LogType.Log:       typeStr = "log"; break;
		case LogType.Warning:   typeStr = "warn"; break;
		case LogType.Error:     typeStr = "error"; break;
		case LogType.Exception: typeStr = "exception"; break;
		case LogType.Assert:    typeStr = "Assert"; break;
		};
		
		var data = new Dictionary<string, object>();
		data.Add("room", this.roomId);
		data.Add("uuid", this.uuid);
		data.Add("time", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
		data.Add("type", typeStr);
		data.Add("message", message);
		manager.Socket.Emit("log", data);
	}
}
