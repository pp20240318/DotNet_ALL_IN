using Microsoft.Extensions.Options;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Features.Store.Dtos;
using WatchShop.Application.Options;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Services;

public class StoreAuthService : IStoreAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenService _jwtTokenService;
    private readonly StoreJwtOptions _storeJwtOptions;

    public StoreAuthService(
        IUnitOfWork unitOfWork,
        JwtTokenService jwtTokenService,
        IOptions<StoreJwtOptions> storeJwtOptions)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _storeJwtOptions = storeJwtOptions.Value;
    }

    public async Task<CustomerLoginResponse> RegisterAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Repository<Customer>()
            .GetListAsync(x => x.Username == request.Username, cancellationToken);

        if (existing.Count > 0)
        {
            throw new BusinessException("用户名已存在");
        }

        var customer = new Customer
        {
            Username = request.Username,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Nickname = request.Nickname ?? request.Username,
            Phone = request.Phone,
            IsEnabled = true
        };
        var id = await _unitOfWork.Repository<Customer>().InsertAsync(customer, cancellationToken);
        customer.Id = id;
        return BuildLoginResponse(customer);
    }

    public async Task<CustomerLoginResponse> LoginAsync(CustomerLoginRequest request, CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Repository<Customer>()
            .GetListAsync(x => x.Username == request.Username, cancellationToken);
        var customer = customers.FirstOrDefault()
            ?? throw new BusinessException("用户名或密码错误");

        if (!customer.IsEnabled || !PasswordHasher.Verify(request.Password, customer.PasswordHash))
        {
            throw new BusinessException("用户名或密码错误");
        }

        return BuildLoginResponse(customer);
    }

    public async Task<CustomerProfileResponse> GetProfileAsync(long customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId, cancellationToken)
            ?? throw new BusinessException("用户不存在");

        return MapProfile(customer);
    }

    private CustomerLoginResponse BuildLoginResponse(Customer customer)
    {
        var (token, expiresAt) = _jwtTokenService.CreateCustomerToken(customer, _storeJwtOptions);
        return new CustomerLoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Profile = MapProfile(customer)
        };
    }

    private static CustomerProfileResponse MapProfile(Customer customer) => new()
    {
        Id = customer.Id,
        Username = customer.Username,
        Nickname = customer.Nickname,
        Phone = customer.Phone
    };
}
