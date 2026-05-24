using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Warehouse.Models
{
    /// <summary>
    /// Represents a product category within a hierarchical structure. 
    /// Utilizes the Parent Reference pattern to allow infinite nesting within a single collection.
    /// </summary>
    public class Category
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public ObjectId? ParentCategoryId { get; set; }
    }
}