using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.Models.Dtos;
using TestAPI.Repository;
using TestAPI.Repository.ScoreRuleRepository;

namespace TestAPI.Controllers.ScoreRuleController
{
    public class ScoreRuleController : CRUD_BaseController<ScoreRuleDto, CreateScoreRuleDto, Guid, ScoreRuleRepository>
    {
        public ScoreRuleController(ScoreRuleRepository repository) : base(repository)
        {
        }

        public async Task<ActionResult<ResponseDto<IEnumerable<ScoreRuleDto>>>> Get()
        {
            var response = new ResponseDto<IEnumerable<ScoreRuleDto>>();
            try
            {
                var result = await repository.Get();
                response.Result = result;
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