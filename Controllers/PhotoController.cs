using Microsoft.AspNetCore.Mvc;
using BamukoyaPhotoApp.Models;
using BamukoyaPhotoApp.Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;

namespace BamukoyaPhotoApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhotoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Photo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PhotoModel>>> GetPhotosByUser()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var photos = await _context.Photos.Where(p => p.UserId == userId).ToListAsync();
            return Ok(photos);
        }

        // GET: api/Photo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoModel>> GetPhotoById(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            return Ok(photo);
        }

        // POST: api/Photo
        [HttpPost]
        public async Task<ActionResult<PhotoModel>> PostPhoto([FromForm] PhotoModel photo, [FromForm] IFormFile photoFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Handle file upload
            if (photoFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{photoFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }

                photo.PhotoUrl = $"/uploads/{uniqueFileName}";
            }

            photo.UserId = userId.Value;

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhotoById), new { id = photo.Id }, photo);
        }

        // PUT: api/Photo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoto(int id, PhotoModel photo)
        {
            if (id != photo.Id)
            {
                return BadRequest();
            }

            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Check if the authenticated user is the owner of the photo
            if (photo.UserId != userId)
            {
                return Unauthorized("You are not authorized to update this photo.");
            }

            _context.Entry(photo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Photos.Any(p => p.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Photo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Check if the authenticated user is the owner of the photo
            if (photo.UserId != userId)
            {
                return Unauthorized("You are not authorized to delete this photo.");
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to extract user ID from claims
        private long? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
