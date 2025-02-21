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
    public class BaseAdminController : BaseController { }

    public class BaseAdminController<TEntityDto, TCreateDto, TKey, TRepository> : BaseAdminController where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;

        public BaseAdminController(TRepository repository)
        {
            this.repository = repository;
        }
    }
}