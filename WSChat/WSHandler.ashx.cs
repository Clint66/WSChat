using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using WebApplication1.Utility;
using Newtonsoft.Json;

namespace WebApplication1 {
	public class WSHandler : IHttpHandler {

		private static ConcurrentDictionary<int, User> mUserSessions = new ConcurrentDictionary<int, User>();
		private static List<User> mUsers = new List<User> {
			new User { Id = 1, Name = "Clint" },
			new User { Id = 2, Name = "Dave" },
			new User { Id = 3, Name = "Doug" },
			new User { Id = 4, Name = "Peter" }
		};

		private static int mMessageId = 0;

		public void ProcessRequest(HttpContext context) {

			if (context.IsWebSocketRequest) {
				context.AcceptWebSocketRequest(ProcessWSChat);
			}

		}

		public bool IsReusable { get { return false; } }

		private async Task ProcessWSChat(AspNetWebSocketContext context) {

			var webSocket = context.WebSocket;
			var buffer = new ArraySegment<byte>(new byte[1024]);

			while (true) {

				buffer.DefaultIfEmpty<byte>();

				var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

				var userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count).FromJson<UserMessage>(true);

				if (webSocket.State == WebSocketState.Open) {

					switch (userMessage.MessageId) {
						case "init":

							userMessage.Users = mUsers;

							SendMessage(webSocket, userMessage);

							break;

						case "login": {

								var user = mUsers.FirstOrDefault(item => item.Id == userMessage.UserId);

								user.AddConnection(webSocket);

								//user.IsSelected = true;

								UpdateUserState();

								break;

							}
						case "select": {

								var user = mUsers.FirstOrDefault(item => item.Id == userMessage.UserId);
								var selecteduser = mUsers.FirstOrDefault(item => item.Id == userMessage.SelectedUserId);

								selecteduser.IsSelected = userMessage.IsSelected;

								UpdateUserState();

								break;
							}
						case "send": {

								var user = mUsers.FirstOrDefault(item => item.Id == userMessage.UserId);
								var id = Interlocked.Increment(ref mMessageId);
								var conversationId = String.Join("|", mUsers.Where(item => item.IsSelected).Select(item => item.Id)).GetHashCode();

								user.Messages.Add(new Message { Id = id, Text = userMessage.Message, UserId = userMessage.UserId, ConversationId = conversationId });

								userMessage.FromUserId = userMessage.UserId;

								Parallel.ForEach(mUsers.Where(item => item.IsSelected).SelectMany(item => item.Connections).ToArray(), item => {

									SendMessage(item.WebSocket, userMessage);

								});

								break;

							}

					}

				} else {

					var user = mUsers.FirstOrDefault(item => item.AllConnections.Any(subItem => subItem.WebSocket == webSocket));

					if (user != null) {

						user.Flush();

						UpdateUserState();

					}

					break;
				}
			}
		}

		private void UpdateUserState() {

			var conversationId = String.Join("|", mUsers.Where(item => item.IsSelected).Select(item => item.Id)).GetHashCode();

			var messages = mUsers.Where(item => item.IsSelected).SelectMany(item => item.Messages).Where(item=> item.ConversationId == conversationId).OrderBy(item => item.Id).ToList();



			// Broadcast to all user sockets.

			Parallel.ForEach(mUsers, user => {

				var userMessage = new UserMessage {
					MessageId = "userState",
					Users = mUsers,
					History = user.IsSelected ? messages : new List<Message>()
				};

				foreach (var connection in user.Connections) {

					SendMessage(connection.WebSocket, userMessage);

				}

			});

		}

		private static async Task<ArraySegment<byte>> SendMessageAsync(WebSocket socket, UserMessage userMessage) {

			var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMessage.ToJson(true)));

			await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

			return buffer;
		}

		private static void SendMessage(WebSocket socket, UserMessage userMessage) {

			var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMessage.ToJson(true)));

			socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);


		}
	}

	public class UserMessage {

		public int UserId { get; set; }

		public int FromUserId { get; set; }

		public int SelectedUserId { get; set; }

		public bool IsSelected { get; set; }

		public List<User> Users { get; set; } = new List<User>();

		public string MessageId { get; set; }

		public string Message { get; set; }

		public List<Message> History { get; set; } = new List<Message>();

		public override string ToString() => $"{this.UserId} {this.MessageId} {this.Message}";

	}

	public class User {

		private ConcurrentDictionary<WebSocket, Connection> mConnections = new ConcurrentDictionary<WebSocket, Connection>();


		public int Id { get; set; }

		public string Name { get; set; }

		public bool IsOnline {
			get => this.Connections.Any() && this.Connections.All(item => item.State == WebSocketState.Open);
		}

		public bool IsSelected { get; set; }

		public Connection AddConnection(WebSocket pWebSocket) {

			return mConnections.AddOrUpdate(pWebSocket, new Connection { WebSocket = pWebSocket }, (ws, con) => con);

		}

		[JsonIgnore]
		public IReadOnlyCollection<Connection> Connections {
			get => mConnections.Values.Where(item => item.State == WebSocketState.Open).ToList();
		}

		[JsonIgnore]
		public IReadOnlyCollection<Connection> AllConnections {
			get => mConnections.Values.ToList();
		}

		public void Flush() {

			foreach (var key in mConnections.Keys) {
				var connection = null as Connection;
				mConnections.TryRemove(key, out connection);
			}

			if (!this.IsOnline) {
				this.IsSelected = false;
			}
		}

		public List<Message> Messages { get; set; } = new List<Message>();

		public override string ToString() => $"{this.Id} {this.Name} Cons:{this.Connections.Count} Msgs:{this.Messages.Count}";



	}

	public class Message {

		public string Text { get; set; }

		public int UserId { get; set; }

		public int ConversationId { get; set; }

		public int Id { get; set; }

		public override string ToString() => $"{this.Id} {this.Text}";

	}

	public class Connection {

		//context.Headers["User-Agent"]

		public WebSocketState State {
			get {

				if (this.WebSocket == null) {

					return WebSocketState.None;

				}

				return (WebSocketState)this.WebSocket.GetType().GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this.WebSocket);

			}
		}

		public WebSocket WebSocket { get; set; }

		public override string ToString() {
			return $"{this.State}";
		}

		//public 

	}
}