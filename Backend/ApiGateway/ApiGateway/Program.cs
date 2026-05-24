using ApiGateway;
using ApiGateway.Endpoints;
using ApiGateway.Observability;
using ApiGateway.Persistence;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiGateway(builder.Configuration, builder.Environment);
builder.Services.AddFitFlowObservability();

var app = builder.Build();

await app.Services.MigrateApiGatewayDatabaseAsync();

app.UseCors();
app.UseFitFlowObservability();
app.UseAuthentication();
app.UseAuthorization();

app.MapFitFlowApiGatewayEndpoints();

app.Run();
