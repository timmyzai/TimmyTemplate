using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AwesomeProject;
using Microsoft.AspNetCore.Authorization;
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

            var repoInput = new CreateGamesDto
            {
                MaxRounds = input.MaxRounds
            };

            try
            {
                var displayMessage = "Creating Game";
                using (var uow = unitOfWorkManager.Begin<ApplicationDbContext>())
                {
                    #region Add Game
                    result = await gameService.CreateNewGame(repoInput);
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

        public async Task<ActionResult<ResponseDto<IEnumerable<GameHistoryDto>>>> GetLeaderboards()
        {
            var response = new ResponseDto<IEnumerable<GameHistoryDto>>();
            try
            {
                var result = await repository.GetByScoreDesc(isBoRequest: true);
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

        public async Task<ActionResult<ResponseDto<IEnumerable<GameHistoryDto>>>> GetMyGameHistory()
        {
            var response = new ResponseDto<IEnumerable<GameHistoryDto>>();
            try
            {
                var result = await repository.Get();
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