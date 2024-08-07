using AuctionService;
using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
	// MassTransit.EntityFrameworkCore package
	// Add an Outbox to avoid data inconsistency when Message Bus is down
	x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
	{
		// it looks the outbox every 10 seconds and try to deliver the message
		o.QueryDelay = TimeSpan.FromSeconds(10);

		// MassTransit doesn't have a SQL Server option
		o.UsePostgres();
		o.UseBusOutbox();
	});

	// Add Fault Consumer from Search Service
	x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

	// Set Exchange endpoint to "auction-auction-created-fault"
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		// Adds a polciy for new attempt when service is not available (ex: application starting)
		cfg.UseMessageRetry(r => 
		{
			r.Handle<RabbitMqConnectionException>();
			r.Interval(5, TimeSpan.FromSeconds(10));
		});
		
		cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
		{
			host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
			host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
		});

		cfg.ConfigureEndpoints(context);
	});
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.Authority = builder.Configuration["IdentityServiceUrl"];
		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters.ValidateAudience = false;
		options.TokenValidationParameters.NameClaimType = "username";
	});

builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

// Adds Polly Policy package for connection attempt
var retryPolicy = Policy
	.Handle<NpgsqlException>()
	.WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));
	
retryPolicy.ExecuteAndCapture(() => DbInitializer.DbInit(app));

app.Run();

public partial class Program { }