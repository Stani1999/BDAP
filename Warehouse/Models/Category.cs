using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Warehouse.Models
{
    /// <summary>
    /// Represents a product category.
    /// Utilizes the grouping mechanism to categorize products into broader groups, allowing for better organization and retrieval of products based on their category.
    /// </summary>
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}