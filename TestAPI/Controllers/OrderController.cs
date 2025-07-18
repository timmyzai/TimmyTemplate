using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.ExternalApi;
using TestAPI.Models;
using TestAPI.Models.Dtos;
using TestAPI.Repository;
using TestAPI.Service;

namespace TestAPI.Controllers
{

    public class OrderController : BaseController<CryptoOrderDto, CreateCryptoOrderDto, Guid, ICryptoOrderRepository>
    {
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly IOrderService _orderService;


        public OrderController(
            ICoinGeckoService coinGeckoService,
            IOrderService orderService,
            ICryptoOrderRepository repository) : base(repository)
        {
            _coinGeckoService = coinGeckoService;
            _orderService = orderService;
        }

        public async Task<ActionResult<ResponseDto<CryptoOrderDto>>> AddOrder([FromBody] CreateCryptoOrderDto input)
        {
            var response = new ResponseDto<CryptoOrderDto>();
            try
            {
                var userId = CurrentSession.GetUserId().Value;
                var coinData = await _coinGeckoService.GetCoinDataByIdAsync(input.CoinId);

                if (coinData is null)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.CoinNotFound);
                }

                var priceUsd = coinData.PriceUsd;
                if (priceUsd <= 0)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.InvalidLivePrice);
                }
                double coinAmount = input.UsdAmount / (double)priceUsd;
                if (coinAmount <= 0)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.InvalidCoinAmount);
                }

                if (input.UsdAmount <= 0)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.InvalidUsdAmount);
                }

                input.UserId = userId;
                input.CoinAmount = coinAmount;
                input.PriceUsd = (double)priceUsd;


                var result = await _orderService.PlaceOrderAsync(input);

                response.Result = result;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.CryptoOrder.CreateMyCoinOrder);
            }
            return Json(response);
        }

        public async Task<ActionResult<ResponseDto<List<CryptoOrderDto>>>> GetMyOrders()
        {
            var response = new ResponseDto<List<CryptoOrderDto>>();
            try
            {
                var result = await _orderService.GetMyOrdersAsync();
                response.Result = result;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.CryptoOrder.DataNotFound);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<CryptoOrderDto>>> GetOrderById(Guid id)
        {
            var response = new ResponseDto<CryptoOrderDto>();
            try
            {
                var result = await _orderService.GetOrderByIdAsync(id);
                response.Result = result;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.CryptoOrder.DataNotFound);
            }
            return Json(response);
        }

        public async Task<ActionResult<ResponseDto<CryptoOrderDto>>> UpdateOrder([FromBody] UpdateCryptoOrderDto input)
        {
            var response = new ResponseDto<CryptoOrderDto>();

            try
            {
                if (input.UsdAmount <= 0)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.InvalidUsdAmount);
                }

                var result = await _orderService.UpdateOrderAsync(input);
                response.Result = result;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.CryptoOrder.UpdateMyCoinOrder);
            }
            return Json(response);
        }

        public async Task<ActionResult<ResponseDto<bool?>>> DeleteOrder(Guid id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var existingUser = await repository.GetById(id);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.CryptoOrder.OrderNotFound);
                }
                await _orderService.DeleteOrderAsync(id);
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.CryptoOrder.DeleteMyCoinOrder);
            }
            return Json(response);
        }
    }
}

