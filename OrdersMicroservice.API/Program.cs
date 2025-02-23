using BusinessLogicLayer;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using System.Text.Json.Serialization;
using OrdersMicroservice.API.Middleware;
using BusinessLogicLayer.HttpClients;
using Polly;
using BusinessLogicLayer.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBusinessLogicLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(
    options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IProductMicroservicePolicies,ProductsMicrocservicePolicies>();
builder.Services.AddTransient<IPollyPolicies, PollyPolicies>();
builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
    //}).AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy())
    //.AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy())
    //.AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetTimeoutPolicy());
}).AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCombinedPolicy());

builder.Services.AddHttpClient<ProdcutsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
}).AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IProductMicroservicePolicies>().GetFallBackPolicy())
.AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IProductMicroservicePolicies>().GetBulkAheadIsolationPolicy());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandlingMiddleware();

app.UseRouting();
app.UseCors();
//Auth

app.UseAuthorization();
app.UseAuthentication();

//controllers

app.MapControllers();

app.Run();
