using AutoMapper;
using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.DbContexts;
using TestAPI.Models.Dtos;
using TestAPI.Repository;
using TestAPI.Services;
using UnitOfWork;

namespace TestAPI.Controllers.GameController
{
    public class GameController : BaseController<GamesDto, CreateGamesDto, Guid, GameRepository>

    {

        private readonly GameRepository gameRepository;
        private readonly IUnitOfWorkManager unitOfWorkManager;
        private readonly IMapper mapper;
        private readonly GameService gameService;

        public GameController(
            GameRepository gameRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IMapper mapper,
            GameService gameService
            ) : base(gameRepository)
        {
            this.gameRepository = gameRepository;
            this.unitOfWorkManager = unitOfWorkManager;
            this.mapper = mapper;
            this.gameService = gameService;
        }

        public async Task<ActionResult<ResponseDto<GamesDto>>> CreateGame([FromBody] UserCreateGamesDto input)
        {
            var response = new ResponseDto<GamesDto>();
            var result = new GamesDto();

            try
            {
                var displayMessage = "Creating Game";
                using (var uow = unitOfWorkManager.Begin<ApplicationDbContext>())
                {
                    #region Add Game
                    result = await gameService.CreateNewGame(input);
                    #endregion

                    await uow.SaveChangesAsync();
                }
                response.Result = result;
                response.DisplayMessage = displayMessage;
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

        public async Task<ActionResult<ResponseDto<GamesDto>>> GetById(Guid id)
        {
            var response = new ResponseDto<GamesDto>();
            try
            {
                var result = await gameRepository.GetById(id);
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

        public async Task<ActionResult<ResponseDto<IEnumerable<GameHistoryDto>>>> GetLeaderboards(FilterPaginationRequestDto pagingFilter)
        {
            var response = new ResponseDto<IEnumerable<GameHistoryDto>>();
            try
            {
                var result = await repository.GetByScoreDesc(pagingFilter, true);
                response.Result = mapper.Map<IEnumerable<GameHistoryDto>>(result);
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

        public async Task<ActionResult<ResponseDto<IEnumerable<GameHistoryDto>>>> GetMyGameHistory(FilterPaginationRequestDto pagingFilter)
        {
            var response = new ResponseDto<IEnumerable<GameHistoryDto>>();
            try
            {
                var result = await repository.Get(pagingFilter);
                response.Result = mapper.Map<IEnumerable<GameHistoryDto>>(result);
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