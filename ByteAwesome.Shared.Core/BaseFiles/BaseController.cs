using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class BaseController : Controller { }

    public class BaseController<TEntityDto, TCreateDto, TKey, TRepository> : BaseController where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;

        public BaseController(TRepository repository)
        {
            this.repository = repository;
        }
    }
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class BaseController_Version : BaseController { }

    public class BaseController_Version<TEntityDto, TCreateDto, TKey, TRepository> : BaseController_Version where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;

        public BaseController_Version(TRepository repository)
        {
            this.repository = repository;
        }
    }
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminBaseController : BaseController { }

    public class AdminBaseController<TEntityDto, TCreateDto, TKey, TRepository> : AdminBaseController where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;

        public AdminBaseController(TRepository repository)
        {
            this.repository = repository;
        }
    }
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("webhooks/[controller]/[action]")]
    public class WebHook_BaseController : BaseController { }

}