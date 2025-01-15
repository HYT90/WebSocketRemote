// See https://aka.ms/new-console-template for more information
using ClientConsoleRemote;

Client client = new([127,0,0,1],8080);
await client.StartAsync();