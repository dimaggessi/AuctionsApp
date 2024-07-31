
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using MassTransit.SagaStateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

// It's share the same instance of CustomWebAppFactory
// therefore, same instance of Postgres and MassTransitTestHarness
// among every test in this class

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
	private readonly CustomWebAppFactory _customWebAppFactory;
	private readonly HttpClient _httpClient;

	public AuctionControllerTests(CustomWebAppFactory customWebAppFactory)
	{
		this._customWebAppFactory = customWebAppFactory;
		this._httpClient = customWebAppFactory.CreateClient();
	}

	private CreateAuctionDto GetAuctionForCreate()
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

	// because it's not a fixture, 
	// this will be initialized before every test in this class
	// in this case, it's not needed
	public Task InitializeAsync() => Task.CompletedTask;


	// dispose will be activated after every single test in this class
	public Task DisposeAsync()
	{
		using var scope = _customWebAppFactory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
		DbHelper.ResetDbForNewTests(db);

		return Task.CompletedTask;
	}

	[Fact]
	public async Task GetAuctions_ShouldReturn3Auctions()
	{
		// arrange (inside WebAppFactory)

		// act
		var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

		// assert
		Assert.Equal(3, response.Count);
	}

	[Fact]
	public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
	{
		// arrange
		var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

		// act
		var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{auctionId}");

		// assert
		Assert.Equal("GT", response.Model);
	}

	[Fact]
	public async Task GetAuctionById_WithInvalidId_ShouldReturn404()
	{
		// arrange

		// act
		var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

		// assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400()
	{
		// arrange

		// act
		var response = await _httpClient.GetAsync($"api/auctions/itsnotaguid");

		// assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task CreateAuction_WithAuth_ShouldReturn201()
	{
		// arrange
		var auction = GetAuctionForCreate();
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

		// assert
		response.EnsureSuccessStatusCode();
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
		Assert.Equal("bob", createdAuction.Seller);
	}

	[Fact]
	public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
	{
		// arrange
		var auction = GetAuctionForCreate();
		auction.Make = null;
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

		// assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
	{
		// arrange
		var updateAuction = new UpdateAuctionDto { Make = "Updated" };
		var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

		// act
		var response = await _httpClient.PutAsJsonAsync($"api/auctions/{auctionId}", updateAuction);

		// assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
	{
		// arrange
		var updateAuction = new UpdateAuctionDto { Make = "Updated" };
		var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
		_httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("notbob"));

		// act
		var response = await _httpClient.PutAsJsonAsync($"api/auctions/{auctionId}", updateAuction);

		// assert
		Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
	}
}