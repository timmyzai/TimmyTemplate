using ByteAwesome.TestAPI.Dtos.Wallet;
using ByteAwesome.TestAPI.Helper.Services;
using ByteAwesome.TestAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.TestAPI.Controllers;

[AllowAnonymous]
public class WalletController : CRUD_BaseController<WalletDto, CreateWalletDto, Guid, IWalletRepository>
{
    private readonly IWalletRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IExchangeRateService _exchangeRateService;
    
    public WalletController(IWalletRepository repository, IUserRepository userRepository, IExchangeRateService exchangeRateService) : base(repository)
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
            //check if user ID exists
            if (await _userRepository.GetById(input.UserId) == null)
            {
                throw new AppException("W1003");
            }
            
            //check if user already has a wallet
            var existingWalletList = await _repository.Get();
            if (existingWalletList.FirstOrDefault(wallet => wallet.UserId == input.UserId) != null)
            {
                throw new AppException("W1009");
            }

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
    
    [HttpPost]
    public async Task<ActionResult<ResponseDto<WalletDto>>> Deposit(DepositIntoWalletDto input)
    {
        var response = new ResponseDto<WalletDto>();
        try
        {
            var existingWallet = (await _repository.GetById(input.Id));
            if (existingWallet == null)
            {
                throw new AppException("W1001");
            }
            
            //check if user exists
            var existingUser = await _userRepository.GetById(existingWallet.UserId);
            if (existingUser == null)
            {
                throw new AppException("W1003");
            }
            
            var amountUsd = await _exchangeRateService.GetConvertedAmount(input.Amount, existingUser.CountryName);

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
    
        
    [HttpPost]
    public async Task<ActionResult<ResponseDto<WalletDto>>> Withdraw(WithdrawFromWalletDto input)
    {
        var response = new ResponseDto<WalletDto>();
        try
        {
            var existingWallet = (await _repository.GetById(input.Id));
            if (existingWallet == null)
            {
                throw new AppException("W1001");
            }
            
            //check if user exists
            var existingUser = await _userRepository.GetById(existingWallet.UserId);
            if (existingUser == null)
            {
                throw new AppException("W1003");
            }
            
            var amountUsd = await _exchangeRateService.GetConvertedAmount(input.Amount, existingUser.CountryName);
            
            if (existingWallet.WalletAmount < amountUsd)
            {
                throw new AppException("W1002");
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
    
    
}