using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class BaseController : Controller { }

    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize]
    public class BaseController_V2 : Controller { }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("webhooks/[controller]/[action]")]
    public class WebHook_BaseController : Controller { }

    [Route("api/[controller]/[action]")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminBaseController : Controller { }
}
