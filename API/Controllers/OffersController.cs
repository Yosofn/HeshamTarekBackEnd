using API.DTOs;
using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.DTOs;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _env;

        public OffersController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("GetLatestOffers")]
        public async Task<ActionResult<IEnumerable<object>>> GetLatestOffers()
        {
            var offersResponse = _context.LatestOffers
                .Select(offer => new
                {
                    offerId = offer.OfferId,
                    title = offer.Title,
                    offerDescription = offer.OfferDescription,
                    offerUrl = offer.OfferUrl,
                    filePath = offer.FilePath != null
                        ? $"{Request.Scheme}://{Request.Host}/api/Offers/{offer.OfferId}"
                        : null
                });

            return await offersResponse.ToListAsync();
        }


        [HttpGet("SearchFreeBooks")]
        public async Task<IActionResult> SearchOffers([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var offers = await _context.LatestOffers

                    .ToListAsync();
                return Ok(offers);

            }
            else
            {
                var offers = await _context.LatestOffers
                    .Where(v => 
                                (v.OfferDescription != null && v.OfferDescription.ToLower().Contains(query.ToLower())))
                    .ToListAsync();

                if (!offers.Any())
                {
                    return NotFound("No Books found matching the search query.");
                }

                return Ok(offers);
            }
        }
        [Authorize(Policy = "UserType")]

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
                Title = null,

                OfferUrl = latestOfferDto.OfferUrl,
            };

            if (latestOfferDto.Image != null && latestOfferDto.Image.Length > 0)
            {
                latestOffer.FilePath = "true";
            }
            else
            {

                latestOffer.FilePath = null;
            }
            _context.LatestOffers.Add(latestOffer);
            await _context.SaveChangesAsync();


            if (latestOfferDto.Image != null && latestOfferDto.Image.Length > 0)
            {
                latestOffer.FilePath = "true";



                var bookPath = Path.Combine(_env.WebRootPath, "Offers", $"{latestOffer.OfferId}.jpg");
                Directory.CreateDirectory(Path.GetDirectoryName(bookPath)!);

                try
                {
                    using var stream = new FileStream(bookPath, FileMode.Create);
                    await latestOfferDto.Image.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while saving the Image: {ex.Message}");
                }

            }



            return Ok(latestOffer);
        }
        [Authorize(Policy = "UserType")]

        [HttpPut("UpdateLatestOffer/{id}")]
        public async Task<IActionResult> UpdateLatestOffer(int id, [FromForm] LatestOfferDto latestOfferDto)
        {


            var latestOffer = await _context.LatestOffers.FindAsync(id);
            if (latestOffer == null)
            {
                return NotFound("Offer not found.");
            }

            latestOffer.OfferDescription = latestOfferDto.OfferDescription;
            latestOffer.Title = null;
            latestOffer.OfferUrl = latestOfferDto.OfferUrl;

            if (latestOfferDto.Image != null && latestOfferDto.Image.Length > 0)
            {
                latestOffer.FilePath = "true";
            }

            _context.LatestOffers.Update(latestOffer);
            await _context.SaveChangesAsync();

            if (latestOfferDto.Image != null && latestOfferDto.Image.Length > 0)
            {
                var offerImagePath = Path.Combine(_env.WebRootPath, "Offers", $"{latestOffer.OfferId}.jpg");
                Directory.CreateDirectory(Path.GetDirectoryName(offerImagePath)!);

                try
                {
                    using var stream = new FileStream(offerImagePath, FileMode.Create);
                    await latestOfferDto.Image.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while updating the Image: {ex.Message}");
                }
            }

            return Ok(latestOffer);
        }

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteLatestOffer/{id}")]
        public async Task<IActionResult> DeleteLatestOffer(int id)
        {
            var latestOffer = await _context.LatestOffers.FindAsync(id);
            if (latestOffer == null)
            {
                return NotFound("Offer not found.");
            }

            var offerFilePath = Path.Combine(_env.WebRootPath, "Offers", $"{latestOffer.OfferId}.jpg");

            try
            {
                if (System.IO.File.Exists(offerFilePath))
                {
                    System.IO.File.Delete(offerFilePath);
                }

                _context.LatestOffers.Remove(latestOffer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Offer and associated file deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the offer: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetImage(int id)
        {
            var imagePath = Path.Combine(_env.WebRootPath, "Offers", $"{id}.jpg");
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image not found");
            }

            var imageFileStream = System.IO.File.OpenRead(imagePath);
            return File(imageFileStream, "image/jpeg");
        }

    }






}

