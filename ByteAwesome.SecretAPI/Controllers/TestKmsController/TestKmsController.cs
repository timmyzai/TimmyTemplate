using ByteAwesome.SecretAPI.Helper;
using ByteAwesome.SecretAPI.Repository;
using ByteAwesome.WalletAPI.ExternalApis.AwsKmsServices;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.SecretAPI.Controllers
{
    public class TestKmsController : BaseController
    {
        private readonly IKmsKeysRepository kmsKeysRepository;
        private readonly IAwsKmsServices awsKmsService;

        public TestKmsController(
            IKmsKeysRepository kmsKeysRepository,
            IAwsKmsServices awsKmsService
        )
        {
            this.kmsKeysRepository = kmsKeysRepository;
            this.awsKmsService = awsKmsService;
        }
        public class TransferDto : OptionalTwoFactorPin
        {
            public string Symbol { get; set; }
            public decimal? Amount { get; set; }
            public string Remark { get; set; }
            public Guid? OwnerWalletGroupsId { get; set; }
            public string ReceiverWalletAddress { get; set; }
            public string Network { get; set; }
        }
        public async Task<IActionResult> Encrypt([FromBody] dynamic input)
        {
            try
            {
                var UserId = CurrentSession.GetUserId();
                var keys = await kmsKeysRepository.GetByUserId(UserId);
                if (keys is null)
                {
                    throw new Exception(ErrorCodes.Secret.KmsKeysNotFound);
                }
                var jsonString = input.ToString();
                var encryptedData = RsaEncoder.Encrypt(jsonString, keys.PublicKey);
                return Ok(encryptedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> Decrypt(string encryptedData)
        {
            try
            {
                var UserId = CurrentSession.GetUserId();
                var keys = await kmsKeysRepository.GetByUserId(UserId);
                if (keys is null)
                {
                    throw new Exception(ErrorCodes.Secret.KmsKeysNotFound);
                }
                var decryptedPrivateKey = await awsKmsService.DecryptAsync(keys.EncryptedPrivateKey);
                var decryptedData = RsaEncoder.Decrypt(encryptedData, decryptedPrivateKey);
                return Ok(decryptedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> Encrypt_Chunk([FromBody] dynamic input)
        {
            try
            {
                var UserId = CurrentSession.GetUserId();
                var keys = await kmsKeysRepository.GetByUserId(UserId);
                if (keys is null)
                {
                    throw new Exception(ErrorCodes.Secret.KmsKeysNotFound);
                }
                var jsonString = input.ToString();
                var encryptedData = RsaEncoder.Encrypt_Chunk(jsonString, keys.PublicKey);
                return Ok(encryptedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> Decrypt_Chunk(string encryptedData)
        {
            try
            {
                var UserId = CurrentSession.GetUserId();
                var keys = await kmsKeysRepository.GetByUserId(UserId);
                if (keys is null)
                {
                    throw new Exception(ErrorCodes.Secret.KmsKeysNotFound);
                }
                var decryptedPrivateKey = await awsKmsService.DecryptAsync(keys.EncryptedPrivateKey);
                var decryptedData = RsaEncoder.Decrypt_Chunk(encryptedData, decryptedPrivateKey);
                return Ok(decryptedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
