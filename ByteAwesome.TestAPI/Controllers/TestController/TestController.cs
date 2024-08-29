using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.TestAPI.Controllers
{
    public partial class TestController : BaseController
    {

        public TestController()
        { }
        public ActionResult<ResponseDto<string>> GetHelloWorld()
        {
            var response = new ResponseDto<string>();
            try
            {
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