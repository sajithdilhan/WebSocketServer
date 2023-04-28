using Microsoft.VisualBasic;
using System.Net;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();
app.MapGet("/ws",
    (RequestDelegate)(async context =>
    {
        await Init(context);
    })
    );
Console.Title = "Server";
app.Run("http://localhost:5000");

static async Task Init(HttpContext context)
{

    if (context.WebSockets.IsWebSocketRequest)
    {
        using var websocket = await context.WebSockets.AcceptWebSocketAsync();
        await Console.Out.WriteLineAsync("Connection established!");
        byte[] data = Encoding.ASCII.GetBytes("Connection established!");
        await websocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        byte[] buf = new byte[1056];
        var result = await websocket.ReceiveAsync(buf, CancellationToken.None);

        while (result.MessageType != WebSocketMessageType.Close)
        {
            string msg = Encoding.ASCII.GetString(buf, 0, result.Count);
            await Console.Out.WriteLineAsync($"received message from client: {msg}");
            data = Encoding.ASCII.GetBytes($"Server received message: {msg} at {DateAndTime.Now.ToString()}");
            await websocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            result = await websocket.ReceiveAsync(buf, CancellationToken.None);
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
}