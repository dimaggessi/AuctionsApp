using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
			_auctionRepositoryMock.Object);
	}
	
	[Fact]
	public async Task GetAuctions_WithNoParams_Return10Auctions()
	{
		// arrange
		var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
		_auctionRepositoryMock.Setup(repository => repository.GetAuctionsAsync(null)).ReturnsAsync(auctions);
		
		// act
		var result = await _controller.GetAllAuctions(null);
		
		// assert
		Assert.Equal(10, result.Value.Count);
		Assert.IsType<ActionResult<List<AuctionDto>>>(result);
	}
	
	[Fact]
	public async Task GetAuctionsById_WithValidGuid_ReturnsAuction()
	{
		// arrange
		var auction = _fixture.Create<AuctionDto>();
		
		// it will not test the database, so Guid doesn't matter.
		_auctionRepositoryMock.Setup(repository => repository.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
		
		// act
		var result = await _controller.GetAuctionById(auction.Id);
		
		// assert
		Assert.Equal(auction.Make, result.Value.Make);
		Assert.IsType<ActionResult<AuctionDto>>(result);
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
}