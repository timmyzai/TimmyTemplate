using ByteAwesome.SecretAPI.Grpc;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface IAccessTokenGrpcClient
    {
        Task<GenerateAccessTokenProtoResult> GenerateAccessToken(GenerateAccessTokenProtoDto request);
    }

    public class AccessTokenGrpcClient : IAccessTokenGrpcClient
    {
        private readonly AccessTokenGrpcService.AccessTokenGrpcServiceClient client;

        public AccessTokenGrpcClient(AccessTokenGrpcService.AccessTokenGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<GenerateAccessTokenProtoResult> GenerateAccessToken(GenerateAccessTokenProtoDto request)
        {
            try
            {
                var response = await client.GenerateAccessTokenAsync(request);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
    }
}
