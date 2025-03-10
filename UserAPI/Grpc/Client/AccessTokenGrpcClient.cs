using AwesomeProject;
using SecretAPI.Grpc;

namespace UserAPI.GrpcClient
{
    public interface IAccessTokenGrpcClient
    {
        Task<GenerateTokensAndPublicKeyProtoResult> GenerateTokensAndPublicKey(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames);
        Task<GenerateAccessTokenProtoResult> GenerateAccessToken(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames);
    }

    public class AccessTokenGrpcClient : IAccessTokenGrpcClient
    {
        private readonly AccessTokenGrpcService.AccessTokenGrpcServiceClient client;

        public AccessTokenGrpcClient(AccessTokenGrpcService.AccessTokenGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<GenerateTokensAndPublicKeyProtoResult> GenerateTokensAndPublicKey(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames)
        {
            var request = new AuthenticationClaimsProtoDto()
            {
                UserId = userId.ToString(),
                UserName = userName,
                EmailAddress = email,
                PhoneNumber = phoneNumber
            };
            var response = await client.GenerateTokensAndPublicKeyAsync(request);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            return response;
        }
        public async Task<GenerateAccessTokenProtoResult> GenerateAccessToken(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames)
        {
            var request = new AuthenticationClaimsProtoDto()
            {
                UserId = userId.ToString(),
                UserName = userName,
                EmailAddress = email,
                PhoneNumber = phoneNumber,
            };
            var response = await client.GenerateAccessTokenAsync(request);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            return response;
        }

    }
}
