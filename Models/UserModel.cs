using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BamukoyaPhotoApp.Models
{
    public class UserModel : IdentityUser<long>
    {
        [Required]
        public  string Name { get; set; }

        // List of friends
        public ICollection<UserModel> Friends { get; set; } = new List<UserModel>();

        // List of friend requests sent by this user
        public ICollection<FriendRequest> FriendRequestsSent { get; set; } = new List<FriendRequest>();

        // List of friend requests received by this user
        public ICollection<FriendRequest> FriendRequestsReceived { get; set; } = new List<FriendRequest>();
    }

    public class FriendRequest
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public  UserModel Sender { get; set; }
        public long ReceiverId { get; set; }
        public  UserModel Receiver { get; set; }
        public bool IsAccepted { get; set; }
    }
}
