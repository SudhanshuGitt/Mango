using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
builder.AddAppAuthentication();

var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.UseOcelot().GetAwaiter().GetResult();
app.Run();


// we need to add authentication to oceleot 
// we need to add it from web project when we will calling the gateway
// it need to pass jwt tokens to indivigual web servers only theb API will work