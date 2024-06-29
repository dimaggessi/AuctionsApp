using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

internal class Program
{
	private static void Main(string[] args)
	{
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

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();

		try
		{
			DbInitializer.DbInit(app);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		app.Run();
	}
}