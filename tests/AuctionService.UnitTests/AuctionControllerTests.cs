using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static System.Net.WebRequestMethods;

namespace AuctionService.UnitTests;

/* tests structure:
	[Fact]
	public void Method_Scenario_ExpectedResult() 
	{
		// arrange
		// act
		// assert
	}
*/

public class AuctionControllerTests 
{
	private readonly Mock<IAuctionRepository> _auctionRepositoryMock;
	private readonly Mock<IPublishEndpoint> _publishEndpointMock;
	private readonly Fixture _fixture;
	private readonly AuctionsController _controller;
	private readonly IMapper _mapper;
	
	// for each test, xUnit inicializes everything that is inside the constructor
	public AuctionControllerTests()
	{
		_fixture = new Fixture();
		_auctionRepositoryMock = new Mock<IAuctionRepository>();
		_publishEndpointMock = new Mock<IPublishEndpoint>();
		
		// mapping configuration from AuctionService
		var mockMapper = new MapperConfiguration(mc => 
		{
			mc.AddMaps(typeof(MappingProfiles).Assembly);
		}).CreateMapper().ConfigurationProvider;
		
		// it's not a mock
		_mapper = new Mapper(mockMapper);
		
		// instance of the controller to be tested
		_controller = new AuctionsController(
			_mapper, 
			_publishEndpointMock.Object, 
			_auctionRepositoryMock.Object)
			{
				ControllerContext = new ControllerContext
				{
					HttpContext = new DefaultHttpContext {User = Helpers.GetClaimsPrincipal()}
				}
			};
	}
	
	[Fact]
	public async Task GetAuctions_WithNoParams_Return10Auctions()
	{
		// arrange
		var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
		_auctionRepositoryMock.Setup(repository => repository.GetAuctionsAsync(null)).ReturnsAsync(auctions);
		
		// act
		var response = await _controller.GetAllAuctions(null);
		
		// assert
		Assert.Equal(10, response.Value.Count);
		Assert.IsType<ActionResult<List<AuctionDto>>>(response);
	}
	
	[Fact]
	public async Task GetAuctionsById_WithValidGuid_ReturnsAuction()
	{
		// arrange
		var auction = _fixture.Create<AuctionDto>();
		
		// it will not test the database, so Guid doesn't matter.
		_auctionRepositoryMock.Setup(repository => repository.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
		
		// act
		var response = await _controller.GetAuctionById(auction.Id);
		
		// assert
		Assert.Equal(auction.Make, response.Value.Make);
		Assert.IsType<ActionResult<AuctionDto>>(response);
	}
	
	[Fact]
	public async Task GetAuctionsById_WithInvalidGuid_ReturnsNotFound()
	{
		// arrange		
		// it will not test the database, so Guid doesn't matter.
		_auctionRepositoryMock.Setup(repository => repository.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
		
		// act
		var result = await _controller.GetAuctionById(Guid.NewGuid());
		
		// assert
		Assert.IsType<NotFoundResult>(result.Result);
	}
	
	[Fact]
	public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
	{
		// arrange		
		var auction = _fixture.Create<CreateAuctionDto>();
		_auctionRepositoryMock.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		// act
		var response = await _controller.CreateAuction(auction);
		var createdResult = response.Result as CreatedAtActionResult;
		
		// assert
		Assert.NotNull(createdResult);
		Assert.Equal("GetAuctionById", createdResult.ActionName);
		Assert.IsType<AuctionDto>(createdResult.Value);
	}
	
	[Fact]
	public async Task CreateAuction_FailedSave_Returns400BadRequest()
	{
		// arrange
		var auction = _fixture.Create<CreateAuctionDto>();
		_auctionRepositoryMock.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);
		
		// act
		var response = await _controller.CreateAuction(auction);
		
		// assert
		Assert.IsType<BadRequestObjectResult>(response.Result);
		
	}

	[Fact]
	public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
	{
		// arrange
		// create an Auction without an Item
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		
		// add Item
		auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		
		// set User.Identity.Name
		auction.Seller = "test";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		var auctionDto = _mapper.Map<UpdateAuctionDto>(auction);
		
		// act
		var response = await _controller.UpdateAction(auction.Id, auctionDto);
		
		// assert
		Assert.IsType<OkResult>(response);
				
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
	{
		// arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		
		auction.Seller = "invalidUser";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		var auctionDto = _mapper.Map<UpdateAuctionDto>(auction);
		
		// act
		var response = await _controller.UpdateAction(auction.Id, auctionDto);
		
		// assert
		Assert.IsType<ForbidResult>(response);
		
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
	{
		// arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		
		auction.Seller = "test";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);
		
		var auctionDto = _mapper.Map<UpdateAuctionDto>(auction);
		
		// act
		var response = await _controller.UpdateAction(auction.Id, auctionDto);
		
		// assert
		Assert.IsType<NotFoundResult>(response);
	}

	[Fact]
	public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
	{
		//arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Seller = "test";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepositoryMock.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		// act
		var response = await _controller.DeleteAuction(auction.Id);
		
		// assert
		Assert.IsType<OkResult>(response);
		
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
	{
		//arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Seller = "test";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);
		_auctionRepositoryMock.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		// act
		var response = await _controller.DeleteAuction(auction.Id);
		
		// assert
		Assert.IsType<NotFoundResult>(response);
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidUser_Returns403Response()
	{
		//arrange
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		auction.Seller = "invalidUser";
		
		_auctionRepositoryMock.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
		_auctionRepositoryMock.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
		
		// act
		var response = await _controller.DeleteAuction(auction.Id);
		
		// assert
		Assert.IsType<ForbidResult>(response);
	}
}