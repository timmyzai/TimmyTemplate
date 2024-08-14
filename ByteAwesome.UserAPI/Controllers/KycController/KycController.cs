using Microsoft.AspNetCore.Mvc;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.GrpcClient;

namespace ByteAwesome.UserAPI.Controllers
{
    public class KycController : BaseController
    {
        private readonly ISumsubKycService sumsubKycService;
        private readonly IUserRepository userRepository;
        private readonly IKycRepository kycRepository;
        private readonly ITfaGrpcClient tfaGrpcClient;

        public KycController(
            ISumsubKycService sumsubKycService,
            IUserRepository userRepository,
            IKycRepository kycRepository,
            ITfaGrpcClient tfaGrpcClient
        )
        {
            this.sumsubKycService = sumsubKycService;
            this.userRepository = userRepository;
            this.kycRepository = kycRepository;
            this.tfaGrpcClient = tfaGrpcClient;
        }
        public async Task<ActionResult<ResponseDto<string>>> GetAccessToken()
        {
            var response = new ResponseDto<string>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var user = await userRepository.GetById(currentUserId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                var accessToken = await sumsubKycService.GetAccessTokenAsync(user.Id.ToString(), "basic-kyc-level");
                response.Result = accessToken.Token;
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
        public async Task<ActionResult<ResponseDto<KycDto>>> GetMyKyc(string TwoFactorPin)
        {
            var response = new ResponseDto<KycDto>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var kyc = await kycRepository.GetByUserId(currentUserId);
                if (kyc is null || kyc.Status == KycStatus.NotStarted)
                {
                    throw new AppException(ErrorCodes.General.EntityNameNotFound, args: typeof(Kyc).Name);
                }
                if (kyc.Status == KycStatus.Approved)
                {
                    await tfaGrpcClient.VerifyTwoFactorPin(currentUserId, TwoFactorPin);
                }
                response.Result = kyc;
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
    }
}