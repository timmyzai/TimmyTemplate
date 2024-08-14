using ByteAwesome.WalletAPI.Grpc;

namespace ByteAwesome.TestAPI.GrpcClient
{
    public interface IWalletGrpcClient
    {
        Task<bool> ClaimReward(Guid userId, decimal rewardAmount, string symbol, string languageCode = "en");
    }
    public class WalletGrpcClient : IWalletGrpcClient
    {
        private readonly WalletGrpcService.WalletGrpcServiceClient client;

        public WalletGrpcClient(WalletGrpcService.WalletGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<bool> ClaimReward(Guid userId, decimal rewardAmount, string symbol, string languageCode = "en")
        {
            try
            {
                var param = new ClaimRewardProtoDto()
                {
                    UserId = userId.ToString(),
                    RewardAmount = (double)rewardAmount,
                    Symbol = symbol,
                    LanguageCode = languageCode
                };
                var response = await client.ClaimAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response.Success;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                return false;
            }
        }
    }
}