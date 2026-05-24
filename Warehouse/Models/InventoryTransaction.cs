using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Warehouse.Models
{
    /// <summary>
    /// Represents an immutable record of a stock movement. 
    /// Acts as the single source of truth for all inventory levels and historical aggregations.
    /// </summary>
    public class InventoryTransaction
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId ProductId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int QuantityChanged { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}