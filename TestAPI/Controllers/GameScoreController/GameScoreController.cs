using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.DbContexts;
using TestAPI.Models.Dtos;
using TestAPI.Repository;
using TestAPI.Services;
using UnitOfWork;

namespace TestAPI.Controllers.GameScoreController
{
    public class GameScoreController : BaseController<GameScoresDto, CreateGameScoresDto, Guid, GameScoreRepository>
    {


        private readonly IUnitOfWorkManager unitOfWorkManager;
        private readonly IMapper mapper;
        private readonly GameScoreServices gameScoreServices;
        private readonly GameRepository gameRepository;

        public GameScoreController(
            GameScoreRepository repository,
            IUnitOfWorkManager unitOfWorkManager,
            IMapper mapper,
            GameScoreServices gameScoreServices,
            GameRepository gameRepository
            ) : base(repository)
        {
            this.unitOfWorkManager = unitOfWorkManager;
            this.mapper = mapper;
            this.gameScoreServices = gameScoreServices;
            this.gameRepository = gameRepository;
        }

        public async Task<ActionResult<ResponseDto<GamesDto>>> SubmitWord([FromBody] SubmitWordDto input)
        {
            var response = new ResponseDto<GamesDto>();
            try
            {
                //check if game exists or ended
                var game = await gameRepository.GetById(input.GameId) ?? throw new AppException(ErrorCodes.Game.GameDoesNotExist);
                if (game.EndTime != null)
                    throw new AppException(ErrorCodes.Game.GameEnded);

                var result = new GamesDto();
                var displayMessage = "Submitting Word";
                using (var uow = unitOfWorkManager.Begin<ApplicationDbContext>())
                {
                    result = await gameScoreServices.SubmitWord(input.GameId, input.Word);
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
    }
}