﻿using Corps.Server.Services;
using MegaCorps.Core.Model.GameUtils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Corps.Server.Controllers
{
    [Route("api/resource")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ImageService _imageService;

        public CardController(ImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet("card")]
        public async Task<IActionResult> GetCards()
        {
            return Ok(await _imageService.GetCardDTOs(DeckBuilder.GetDeckFromResources(_imageService.AttackInfos, _imageService.DefenceInfos, _imageService.DeveloperInfos, _imageService.Directions, _imageService.EventInfos).UnplayedCards));
        }

        [HttpGet("background")]
        public IActionResult GetBackground()
        {
            return Ok(_imageService.BackgroundImages);
        }

        [HttpGet("user")]
        public IActionResult GetUser()
        {
            return Ok(_imageService.UserImages);
        }

    }
}
