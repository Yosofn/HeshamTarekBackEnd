using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.DTOs;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OffersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Offers
        [HttpGet("GetLatestOffers")]
        public async Task<ActionResult<IEnumerable<LatestOffer>>> GetLatestOffers()
        {
            return await _context.LatestOffers.ToListAsync();
        }

        // POST: api/Offers
        [HttpPost("AddLatestOffer")]
        public async Task<ActionResult<LatestOffer>> AddLatestOffer(LatestOfferDto latestOfferDto)
        {
            if (string.IsNullOrWhiteSpace(latestOfferDto.OfferDescription))
            {
                return BadRequest("Please enter the offer description.");
            }

            var latestOffer = new LatestOffer
            {
                OfferDescription = latestOfferDto.OfferDescription,
                Title = latestOfferDto.OfferTitle,

                OfferUrl = latestOfferDto.OfferUrl,
                FilePath = latestOfferDto.FilePath
            };

            _context.LatestOffers.Add(latestOffer);
            await _context.SaveChangesAsync();

            return Ok( latestOffer);
        }
    }




}

