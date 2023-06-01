// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using UnityEngine;
// using WebSocketSharp;

// public class SocketIOComponent : MonoBehaviour
// {
//     public string url = "http://localhost:3000";
//     public bool debug = false;

//     private WebSocket socket;
//     private Dictionary<string, List<System.Action<List<object>>>> eventHandlers = new Dictionary<string, List<System.Action<List<object>>>>();

//     private void Start()
//     {
//         socket = new WebSocket(url);
//         socket.OnOpen += OnOpen;
//         socket.OnMessage += OnMessage;
//         socket.OnError += OnError;
//         socket.OnClose += OnClose;
//         socket.Connect();
//     }

//     private void OnOpen(object sender, System.EventArgs e)
//     {
//         if (debug) Debug.Log("SocketIOComponent connected to " + url);
//     }

//     private void OnMessage(object sender, MessageEventArgs e)
//     {
//         if (debug) Debug.Log("SocketIOComponent received message: " + e.Data);

//         SocketIOPacket packet = SocketIOPacket.Parse(e.Data);
//         if (packet != null)
//         {
//             if (packet.Type == SocketIOPacketType.Connect)
//             {
//                 Emit("connect", null);
//             }
//             else if (packet.Type == SocketIOPacketType.Disconnect)
//             {
//                 Emit("disconnect", null);
//             }
//             else if (packet.Type == SocketIOPacketType.Event)
//             {
//                 string eventName = packet.Args[0].ToString();
//                 List<object> args = new List<object>();
//                 for (int i = 1; i < packet.Args.Count; i++)
//                 {
//                     args.Add(packet.Args[i]);
//                 }
//                 Emit(eventName, args);
//             }
//         }
//     }

//     private void OnError(object sender, ErrorEventArgs e)
//     {
//         if (debug) Debug.LogError("SocketIOComponent error: " + e.Message);
//     }

//     private void OnClose(object sender, CloseEventArgs e)
//     {
//         if (debug) Debug.Log("SocketIOComponent disconnected from " + url);
//     }

//     public void Emit(string eventName, List<object> args)
//     {
//         if (eventHandlers.ContainsKey(eventName))
//         {
//             foreach (System.Action<List<object>> handler in eventHandlers[eventName])
//             {
//                 handler(args);
//             }
//         }
//     }

//     public void On(string eventName, System.Action<List<object>> handler)
//     {
//         if (!eventHandlers.ContainsKey(eventName))
//         {
//             eventHandlers[eventName] = new List<System.Action<List<object>>>();
//         }
//         eventHandlers[eventName].Add(handler);
//     }

//     public void Off(string eventName, System.Action<List<object>> handler)
//     {
//         if (eventHandlers.ContainsKey(eventName))
//         {
//             eventHandlers[eventName].Remove(handler);
//         }
//     }

//     public void Send(string eventName, List<object> args)
//     {
//         SocketIOPacket packet = new SocketIOPacket(SocketIOPacketType.Event, SocketIOEngine.PacketId++, "/", new List<object>(new object[] { eventName }).Concat(args).ToList());
//         string json = packet.ToJson();
//         if (debug) Debug.Log("SocketIOComponent sending message: " + json);
//         socket.Send(json);
//     }
// }

// public class SocketIOPacket
// {
//     public SocketIOPacketType Type;
//     public int Id;
//     public string Namespace;
//     public List<object> Args;

//     public string ToJson()
//     {
//         string data = "";
//         if (Type == SocketIOPacketType.Connect)
//         {
//             data = "0";
//         }
//         else if (Type == SocketIOPacketType.Disconnect)
//         {
//             data = "1";
//         }
//         else if (Type == SocketIOPacketType.Event)
//         {
//             data = "2";
//         }
//         string json = string.Format("{{\"type\":{0},\"nsp\":\"{1}\",\"id\":{2},\"data\":{3}}}", data, Namespace, Id, JsonUtility.ToJson(Args));
//         return json;
//     }

//     public static SocketIOPacket Parse(string json)
//     {
//         SocketIOPacket packet = null;
//         Match match = Regex.Match(json, @"^{""type"":(\d),""nsp"":""([^""]*)"",""id"":(\d+),""data"":(.*)}$");
//         if (match.Success)
//         {
//             int type = int.Parse(match.Groups[1].Value);
//             string ns = match.Groups[2].Value;
//             int id = int.Parse(match.Groups[3].Value);
//             string data = match.Groups[4].Value;
//             List<object> args = JsonUtility.FromJson<List<object>>(data);
//             if (type == 0)
//             {
//                 packet = new SocketIOPacket(SocketIOPacketType.Connect, id, ns, args);
//             }
//             else if (type == 1)
//             {
//                 packet = new SocketIOPacket(SocketIOPacketType.Disconnect, id, ns, args);
//             }
//             else if (type == 2)
//             {
//                 packet = new SocketIOPacket(SocketIOPacketType.Event, id, ns, args);
//             }
//         }
//         return packet;
//     }

//     public SocketIOPacket(SocketIOPacketType type, int id, string ns, List<object> args)
//     {
//         Type = type;
//         Id = id;
//         Namespace = ns;
//         Args = args;
//     }
// }

// public enum SocketIOPacketType
// {
//     Connect,
//     Disconnect,
//     Event
// }