using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
	// other consumers in the same namespace will automatically be registered by MassTransit
	x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

	// add a prefix in front of queue name: search-auction-created
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.UseMessageRetry(r =>
		{
			r.Handle<RabbitMqConnectionException>();
			r.Interval(5, TimeSpan.FromSeconds(10));
		});

		// RabbitMQ for Docker configuration
		cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
				{
					host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
					host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
				});

		// retry for receive message when MongoDB is inacessible
		cfg.ReceiveEndpoint("search-auction-created", e =>
		{
			e.UseMessageRetry(r => r.Interval(5, 5));

			e.ConfigureConsumer<AuctionCreatedConsumer>(context);
		});

		cfg.ConfigureEndpoints(context);
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
	// Adds Polly Policy package for connection attempt
	await Policy.Handle<TimeoutException>()
		.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
		.ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
});

app.Run();

// Policy for retry Auction Service HttpRequest
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
	=> HttpPolicyExtensions.HandleTransientHttpError()
		.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
		.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));