using Corps.Server.Services;
using MegaCorps.Core.Model.GameUtils;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult GetCards()
        {
            var cards = _imageService.GetCardDTOs(DeckBuilder.GetDeck().UnplayedCards);
            return Ok(cards);
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
