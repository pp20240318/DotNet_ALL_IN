using MediatR;
using WatchShop.Application.Features.Brands.Dtos;

namespace WatchShop.Application.Features.Brands.Commands;

public record CreateBrandCommand(BrandCreateRequest Request) : IRequest<long>;

public record UpdateBrandCommand(long Id, BrandUpdateRequest Request) : IRequest;

public record DeleteBrandCommand(long Id) : IRequest;
