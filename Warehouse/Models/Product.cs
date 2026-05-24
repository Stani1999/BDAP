using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Warehouse.Models.Types;

namespace Warehouse.Models
{
    /// <summary>
    /// Represents the main product aggregate in the catalog. 
    /// The Quantity property serves as a materialized view (cache) for fast UI rendering.
    /// </summary>
    public class Product
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ObjectId CategoryId { get; set; }
        public int Quantity { get; set; }
        public Price Price { get; set; } = new();
        public Unit Unit { get; set; }
        public List<ProductTag> Tags { get; set; } = new();
    }
}