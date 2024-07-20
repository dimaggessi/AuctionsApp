using AutoMapper;
using BiddingService;
using Contracts;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Bid, BidDto>();
		CreateMap<Bid, BidPlaced>();
	}
}
