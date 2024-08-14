using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ByteAwesome.Services;

namespace ByteAwesome.TestAPI.Controllers
{
    public partial class TestController : BaseController
    {

        public TestController()
        { }
        public ActionResult<ResponseDto<string>> GetCampaignById()
        {
            var response = new ResponseDto<string>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                response.Result = "Hello World";
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