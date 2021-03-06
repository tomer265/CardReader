using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpCardReader.Models
{
    public class CardHolder
    {
        public ObjectId Id { get; set; } // MongoDb objectId
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PicUrl { get; set; }
        public string VocalFileUrl { get; set; }
        public string CardIdentifier { get; set; }
        public string GovernmentId { get; set; }
    }
}
