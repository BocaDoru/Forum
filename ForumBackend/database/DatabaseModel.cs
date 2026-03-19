using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#nullable disable

namespace database;

public abstract record DatabaseModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]

    public string Id { get; set; }
}
