using TestAPI.Dtos.Wallet;
using TestAPI.Helper.Services;
using TestAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using AwesomeProject;
using TestAPI.Dtos.User;

namespace TestAPI.Controllers;

public class WalletController : CRUD_BaseController<WalletDto, CreateWalletDto, Guid, IWalletRepository>
{
    private readonly IWalletRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IExchangeRateService _exchangeRateService;

    public WalletController(
        IWalletRepository repository,
        IUserRepository userRepository,
        IExchangeRateService exchangeRateService
    ) : base(repository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ActionResult<ResponseDto<IEnumerable<WalletDto>>>> GetAll()
    {
        var response = new ResponseDto<IEnumerable<WalletDto>>();
        try
        {
            var result = await _repository.Get();
            response.Result = result;
        }
        catch (AppException ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex, response);
        }
        return Json(response);
    }

    public override async Task<ActionResult<ResponseDto<WalletDto>>> Add(CreateWalletDto input)
    {
        var response = new ResponseDto<WalletDto>();
        try
        {
            var currentUser = CurrentSession.GetUser();
            //check if user already has a wallet
            var existingWallet = await _repository.GetByUserId(currentUser.Id);
            if (existingWallet is not null)
            {
                throw new AppException(ErrorCodes.Wallet.UserAlreadyHasWallet);
            }
            var user = await _userRepository.GetById(currentUser.Id);
            if (user == null)
            {
                var newUser = new CreateUserDto
                {
                    Id = currentUser.Id,
                    Username = currentUser.UserName,
                    EmailAddress = currentUser.EmailAddress,
                    PhoneNumber = currentUser.PhoneNumber,
                    CountryName = input.CountryName
                };
                user = await _userRepository.Add(newUser);
            }
            input.UserId = currentUser.Id;
            response.Result = await repository.Add(input);
        }
        catch (AppException ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex, response);
        }
        return Json(response);
    }

    public async Task<ActionResult<ResponseDto<WalletDto>>> Deposit(DepositIntoWalletDto input)
    {
        var response = new ResponseDto<WalletDto>();
        try
        {
            var (existingWallet, amountUsd) = await GetWalletAndAmount(input.Amount);

            existingWallet.WalletAmount += amountUsd;

            response.Result = await repository.Update(existingWallet);
        }
        catch (AppException ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message);
        }
        return Json(response);
    }

    public async Task<ActionResult<ResponseDto<WalletDto>>> Withdraw(WithdrawFromWalletDto input)
    {
        var response = new ResponseDto<WalletDto>();
        try
        {
            var (existingWallet, amountUsd) = await GetWalletAndAmount(input.Amount);

            if (existingWallet.WalletAmount < amountUsd)
            {
                throw new AppException(ErrorCodes.Wallet.WalletNotEnoughFunds);
            }

            existingWallet.WalletAmount -= amountUsd;

            response.Result = await repository.Update(existingWallet);
        }
        catch (AppException ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex, response, ex.Message);
        }
        return Json(response);
    }

    private async Task<(WalletDto, decimal)> GetWalletAndAmount(decimal amount){
            var currentUser = CurrentSession.GetUser();

            var existingWallet = await _repository.GetByUserId(currentUser.Id);
            if (existingWallet == null)
            {
                throw new AppException(ErrorCodes.Wallet.WalletNotFound);
            }
  
            var countryName = await _userRepository.GetUserCountryByUserId(existingWallet.UserId);

            var amountUsd = await _exchangeRateService.GetConvertedAmount(amount, countryName);

            return (existingWallet, amountUsd);
    }

    [NonAction]
    public override Task<ActionResult<ResponseDto<WalletDto>>> Update(WalletDto input)
    {
        return default;
    }
}