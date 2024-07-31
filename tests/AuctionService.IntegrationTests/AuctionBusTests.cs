using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _customWebAppFactory;
    private readonly HttpClient _httpClient;
    private readonly ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _customWebAppFactory = factory;
        _httpClient = factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        // arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

// dispose will be activated after every single test in this class
	public Task DisposeAsync()
	{
		using var scope = _customWebAppFactory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
		DbHelper.ResetDbForNewTests(db);

		return Task.CompletedTask;
	}

    private static CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}