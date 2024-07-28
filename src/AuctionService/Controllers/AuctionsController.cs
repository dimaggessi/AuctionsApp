using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
	private readonly IMapper _mapper;
	private readonly IAuctionRepository _repository;
	public IPublishEndpoint _publishEndpoint { get;set; }
	public AuctionsController(IMapper mapper,
							  IPublishEndpoint publishEndpoint,
							  IAuctionRepository repository)
	{
		this._publishEndpoint = publishEndpoint;
		this._repository = repository;
		this._mapper = mapper;
	}
	
	[HttpGet]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
	{
		return await _repository.GetAuctionsAsync(date);
	}
	
	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		AuctionDto auction = await _repository.GetAuctionByIdAsync(id);
		
		if (auction is null) return NotFound();
		
		return auction;
	}
	
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
	{
		var auction = _mapper.Map<Auction>(auctionDto);
		
		auction.Seller = User.Identity.Name;
		
		_repository.AddAuction(auction);
		
		var newAuction = _mapper.Map<AuctionDto>(auction);
		
		// publish a message to all service consumers
		await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
		
		// all the code above will be part of the same transaction
		var result = await _repository.SaveChangesAsync();
		
		if(!result) return BadRequest("Could not save changes to the Database");
		
		return CreatedAtAction(nameof(GetAuctionById), 
			new {auction.Id}, newAuction);
	}
	
	[Authorize]
	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAction(Guid id, UpdateAuctionDto updateAuctionDto)
	
	{
		var auction = await _repository.GetAuctionEntityById(id);
		
		if (auction == null) return NotFound();
		
		if (auction.Seller != User.Identity.Name) return Forbid();
		
		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
		
		await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
		
		var result = await _repository.SaveChangesAsync();
		
		if(result) return Ok();
		
		return BadRequest("Problem saving changes");
	}
	
	[Authorize]
	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAuction(Guid id)
	{
		var auction = await _repository.GetAuctionEntityById(id);
		
		if (auction == null) return NotFound();
		
		if (auction.Seller != User.Identity.Name) return Forbid();
		
		_repository.RemoveAuction(auction);
		
		await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });
		
		var result = await _repository.SaveChangesAsync();
		
		if (!result) return BadRequest("Could not delete auction");
		
		return Ok();
	}
}