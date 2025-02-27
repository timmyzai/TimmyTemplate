using System.Text.Json;
using ByteAwesome.TestAPI.Data;
using ByteAwesome.TestAPI.Dtos.BaseCurrency;
using ByteAwesome.TestAPI.External.ExchangeRateAPI;
using ByteAwesome.TestAPI.Modules;
using ByteAwesome.TestAPI.Repositories;
using Microsoft.Extensions.Options;

namespace ByteAwesome.TestAPI.Helper.Services;

public interface IExchangeRateService
{
    Task<decimal> GetConvertedAmount(decimal amount, string userCountry);
    Task StoreExchangeRates();
}

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IBaseCurrencyRepository _baseCurrencyRepository;

    public ExchangeRateService(
        HttpClient httpClient, 
        IOptions<ApiSettings> apiSettings, 
        IExchangeRateRepository exchangeRateRepository, 
        IBaseCurrencyRepository baseCurrencyRepository)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
        
        _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
        
        _exchangeRateRepository = exchangeRateRepository;
        _baseCurrencyRepository = baseCurrencyRepository;
    }
    
    public async Task<decimal> GetConvertedAmount(decimal amount, string userCountry)
    {
        try
        {
            var countryCurrency = CountryCurrencyList.JsonList.FirstOrDefault(x =>
                x.CountryCode_2 == userCountry);
            if (countryCurrency is null)
            {
                throw new AppException("W1008");
            }
            
            var countryCurrencyCode = countryCurrency.CountryCode_2.ToUpper();

            var response =
                await _httpClient.GetAsync($"/latest?access_key={_apiSettings.AccessKey}&symbols={countryCurrencyCode},USD");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(json);

            var currencyRateUsd = jsonResponse.Rates.FirstOrDefault(r => r.TargetCurrency == "USD");
            var currencyRateUser = jsonResponse.Rates.FirstOrDefault(r => r.TargetCurrency == countryCurrencyCode);
            if (currencyRateUsd == null || currencyRateUser == null)
            {
                throw new AppException("W1006");
            }
            
            return 1 / currencyRateUser.Rate * amount * currencyRateUsd.Rate;
        }
        catch (Exception _)
        {
            throw new AppException("W1005");
        }
    }

    public async Task StoreExchangeRates()
    {
        try
        {
            var response =
                await _httpClient.GetAsync(
                    $"/latest?access_key={_apiSettings.AccessKey}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(json);

            var baseCurrency = await _baseCurrencyRepository.GetByCurrencyName(jsonResponse.BaseCurrency);
            if (baseCurrency is null)
            {
                var newBaseCurrencyDtoDto = new CreateBaseCurrencyDto() { Name = jsonResponse.BaseCurrency };
                await _baseCurrencyRepository.Add(newBaseCurrencyDtoDto);
            }
            
            foreach (var rate in jsonResponse.Rates)
            {
                rate.BaseCurrencyId = baseCurrency.Id;
            }

            await _exchangeRateRepository.UpsertRange(jsonResponse.Rates, 
                                e => e.TargetCurrency, 
                                                       e => e.BaseCurrencyId);
        }
        catch (Exception _)
        {
            throw new AppException("W1011");
        }
    }
}
