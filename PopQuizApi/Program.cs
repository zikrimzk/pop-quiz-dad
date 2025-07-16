using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PopQuizApi.Controllers;
using PopQuizApi.Models;
using PopQuizApi.Services;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://*:7268", "http://*:7267");
// Add services to the container.
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();

var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    ReceiveBufferSize = 4 * 1024,
};

app.UseWebSockets(webSocketOptions);



app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        
        var sessionId = context.Request.Query["sessionId"].ToString();
        var uniqueId = context.Request.Query["uniqueId"].ToString();


        var socket = await context.WebSockets.AcceptWebSocketAsync();

       
        MyDbContext ent = new MyDbContext();

        if (!string.IsNullOrWhiteSpace(uniqueId))
        {
            try
            {
                var game = ent.Games.FirstOrDefault(x => x.UniqueId == uniqueId);

                if (!SocketService.GameSockets.ContainsKey(game.UniqueId))
                    SocketService.GameSockets[game.UniqueId] = new List<WebSocket>();

                SocketService.GameSockets[game.UniqueId].Add(socket);
                await GameSocketController.ProcessWebSocket(socket, game.UniqueId, SocketService.GameSockets);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }


        try
        {
             var participant = ent.Participants.Include(x=>x.Game).FirstOrDefault(x => x.SessionId == sessionId);
           
            if (!SocketService.GameSockets.ContainsKey(participant.Game.UniqueId))
                SocketService.GameSockets[participant.Game.UniqueId] = new List<WebSocket>();

            SocketService.GameSockets[participant.Game.UniqueId].Add(socket);
                await GameSocketController.ProcessWebSocket(socket, participant.Game.UniqueId, SocketService.GameSockets);

        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
       
       

    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<AuthenticationMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


