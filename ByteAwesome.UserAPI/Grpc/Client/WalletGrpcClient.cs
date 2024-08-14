using ByteAwesome.WalletAPI.Grpc;
using Microsoft.AspNetCore.Authorization;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface IWalletGrpcClient
    {
        Task CreateWalletGroup(Guid userId, string userName, string emailAddress, string phoneNumber);
    }
    [AllowAnonymous]
    public class WalletGrpcClient : BaseController, IWalletGrpcClient
    {
        private readonly WalletGrpcService.WalletGrpcServiceClient client;

        public WalletGrpcClient(WalletGrpcService.WalletGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task CreateWalletGroup(Guid userId, string userName, string emailAddress, string phoneNumber)
        {
            try
            {
                var param = new CreateWalletGroupProtoDto()
                {
                    UserId = userId.ToString(),
                    UserName = userName,
                    EmailAddress = emailAddress,
                    PhoneNumber = phoneNumber
                };
                var response = await client.CreateWalletGroupAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage, fireAndForget: true);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
    }
}