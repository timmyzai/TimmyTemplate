using ByteAwesome;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Helper;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Repository;
using ByteAwesome.WalletAPI.ExternalApis.AwsKmsServices;
using Grpc.Core;

namespace grpcServer.Services;

public class KmsKeysGrpcServer : KmsKeysGrpcService.KmsKeysGrpcServiceBase
{
    private readonly IKmsKeysRepository kmsKeysRepository;
    private readonly IAwsKmsServices awsKmsService;

    public KmsKeysGrpcServer(
        IKmsKeysRepository kmsKeysRepository,
        IAwsKmsServices awsKmsService
    )
    {
        this.kmsKeysRepository = kmsKeysRepository;
        this.awsKmsService = awsKmsService;
    }
    public override async Task<CreateKmsKeyResultDto> CreateOrUpdateKmsKeys(CreateKmsKeyProtoDto request, ServerCallContext context)
    {
        var responseDto = new CreateKmsKeyResultDto();

        try
        {
            _ = Guid.TryParse(request.UserId, out var userId);
            if (userId == Guid.Empty)
            {
                throw new Exception("Invalid UserId");
            }
            KmsKeysDto key = null;
            var existingKeys = await kmsKeysRepository.GetByUserId(userId);
            if (existingKeys is null)
            {
                var asymmetricKeys = RsaEncoder.GenerateRsaKeys();
                var encryptedPrivateKey = await awsKmsService.EncryptAsync(asymmetricKeys.PrivateKey);
                var newKeys = new KmsKeysDto
                {
                    UserId = userId,
                    PublicKey = asymmetricKeys.PublicKey,
                    EncryptedPrivateKey = encryptedPrivateKey
                };
                key = await kmsKeysRepository.Add(newKeys);
            }
            else
            {
                key = existingKeys;
            }
            responseDto.PublicKey = key.PublicKey;
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public override async Task<GetPublicKeyResultDto> GetPublicKey(GetPublicKeyProtoDto request, ServerCallContext context)
    {
        var responseDto = new GetPublicKeyResultDto();
        try
        {
            _ = Guid.TryParse(request.UserId, out var userId);
            if (userId == Guid.Empty)
            {
                throw new Exception("Invalid UserId");
            }
            var keys = await kmsKeysRepository.GetByUserId(userId);
            if (keys is not null)
            {
                responseDto.PublicKey = keys.PublicKey;
            }

        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public override async Task<VerifySignAndDecryptResultDto> VerifyDataAndDecrypt(VerifySignAndDecryptProtoDto request, ServerCallContext context)
    {
        var responseDto = new VerifySignAndDecryptResultDto();
        try
        {
            _ = Guid.TryParse(request.UserId, out var userId);
            if (userId == Guid.Empty)
            {
                throw new Exception("Invalid UserId");
            }
            var keys = await kmsKeysRepository.GetByUserId(userId);
            if (keys is null)
            {
                throw new Exception(ErrorCodes.Secret.KmsKeysNotFound);
            }
            var decryptedPrivateKey = await awsKmsService.DecryptAsync(keys.EncryptedPrivateKey);
            var decryptedData = RsaEncoder.Decrypt_Chunk(request.EncryptedData, decryptedPrivateKey);
            responseDto.DecryptedData = decryptedData;
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}
