using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

	public async Task InitializeAsync()
	{
		// inside docker this is going to start a running instance of a test container database server
		await _postgreSqlContainer.StartAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		// everything loaded from the Program.cs will be replaced inside here
		builder.ConfigureTestServices(services =>
		{
			services.RemoveDbContext<AuctionDbContext>();
			
			services.AddDbContext<AuctionDbContext>(options =>
			{
				options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
			});

			// MassTransit provides it
			// it's remove the configuration inside Program.cs and replace it with a Test Container
			services.AddMassTransitTestHarness();
			
			services.EnsureCreated<AuctionDbContext>();
			
			services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
				.AddFakeJwtBearer(opt =>
				
				{
					opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
				});
		});
	}

	Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}
