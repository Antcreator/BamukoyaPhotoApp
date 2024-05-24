using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BamukoyaPhotoApp.Db;
using BamukoyaPhotoApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BamukoyaPhotoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendRequestController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestSendModel model)
        {
            var senderId = GetUserIdFromClaims();
            if (senderId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var receiver = await _context.Users.FindAsync(model.ReceiverId);
            if (receiver == null)
            {
                return NotFound("Receiver not found.");
            }

            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == model.ReceiverId);
            if (existingRequest != null)
            {
                return BadRequest("Friend request already sent.");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = senderId.Value,
                ReceiverId = model.ReceiverId
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Ok("Friend request sent.");
        }

        [Authorize]
        [HttpPost("accept/{requestId}")]
        public async Task<IActionResult> AcceptFriendRequest(long requestId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var request = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == requestId
                    && (fr.ReceiverId == userId || fr.SenderId == userId));

            if (request == null)
            {
                return NotFound("Friend request not found.");
            }

            if (request.IsAccepted)
            {
                return BadRequest("Friend request already accepted.");
            }

            if (request.SenderId == userId)
            {
                return BadRequest("You cannot accept your own friend request.");
            }

            request.IsAccepted = true;
            request.Sender.Friends.Add(request.Receiver);
            request.Receiver.Friends.Add(request.Sender);

            await _context.SaveChangesAsync();

            return Ok("Friend request accepted.");
        }

        [Authorize]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var pendingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && !fr.IsAccepted)
                .Include(fr => fr.Sender)
                .Select(fr => new
                {
                    fr.Id,
                    SenderId = fr.SenderId,
                    SenderName = fr.Sender.Name,
                    fr.ReceiverId,
                    fr.IsAccepted
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }


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

    public class FriendRequestSendModel
    {
        public long ReceiverId { get; set; }
    }
}
