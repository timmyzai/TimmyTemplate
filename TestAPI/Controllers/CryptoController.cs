using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.ExternalApi;
using TestAPI.Models.Dtos;

namespace TestAPI.Controllers
{
    public class CryptoController : BaseController
    {
        private readonly ICoinGeckoService _coinGeckoService;

        public CryptoController(ICoinGeckoService coinGeckoService)
        {
            _coinGeckoService = coinGeckoService;
        }

        public async Task<ActionResult<ResponseDto<List<CoinsDto>>>> GetCoinList()
        {
            var response = new ResponseDto<List<CoinsDto>>();
            try
            {
                var result = await _coinGeckoService.GetCoinListAsync();
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

        public async Task<ActionResult<ResponseDto<CoinMarketDto>>> GetCoinData(string coinId)
        {
            var response = new ResponseDto<CoinMarketDto>();
            try
            {
                var result = await _coinGeckoService.GetCoinDataByIdAsync(coinId);
                response.Result = result;

                if (result is null) {
                    throw new AppException(ErrorCodes.CryptoOrder.CoinNotFound);
                }
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

    }
}