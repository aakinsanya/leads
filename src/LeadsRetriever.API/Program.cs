using API.Clients;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the leadsBaseUrl and leadsEndpoint from configuration
var leadsBaseUrl = builder.Configuration.GetValue<string>("LeadsApi:BaseUrl");
var leadsEndpoint = builder.Configuration.GetValue<string>("LeadsApi:Endpoint");

// Register ILeadsService and its implementation
builder.Services.AddScoped<ILeadsService, LeadsService>();

// Register the HttpClient and configure the BaseAddress
builder.Services.AddHttpClient<ILeadsApiClient, LeadsApiClient>(client =>
{
    client.BaseAddress = new Uri(leadsBaseUrl);
});

builder.Services.AddHostedService<LeadsRetrievalHostedService>();
builder.Services.AddDbContext<LeadsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapLeadEndpoints();

app.Run();