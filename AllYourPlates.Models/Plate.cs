using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AllYourPlates.WebMVC.Models
{
    public class Plate
    {
        [Key]
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public required IdentityUser User { get; set; } 
    }
}
