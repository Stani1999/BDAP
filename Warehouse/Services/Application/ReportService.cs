using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Warehouse.Models;
using Warehouse.Services.Infrastructure;

namespace Warehouse.Services.Application
{
    /// <summary>
    /// Executes complex analytical queries utilizing the MongoDB Aggregation Framework.
    /// Offloads heavy data processing from the application layer to the database engine.
    /// </summary>
    public class ReportService
    {
        private readonly IMongoCollection<InventoryTransaction> _transactions;

        public ReportService(IMongoService mongoService)
        {
            _transactions = mongoService.GetCollection<InventoryTransaction>("InventoryTransaction");
        }

        public async Task<List<BsonDocument>> GetStockTrendsAsync(DateTime from, DateTime to)
        {
            var match = Builders<InventoryTransaction>.Filter.And(
                Builders<InventoryTransaction>.Filter.Gte(t => t.Timestamp, from),
                Builders<InventoryTransaction>.Filter.Lte(t => t.Timestamp, to)
            );

            return await _transactions.Aggregate()
                .Match(match)
                .Group(
                    new BsonDocument
                    {
                        { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$Timestamp" } }) },
                        { "DailyMovement", new BsonDocument("$sum", "$QuantityChanged") }
                    }
                )
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();
        }

        public async Task<List<BsonDocument>> GetTopMoversAsync(int limit)
        {
            return await _transactions.Aggregate()
                .Group(
                    new BsonDocument
                    {
                        { "_id", "$ProductId" },
                        { "TotalVolume", new BsonDocument("$sum", new BsonDocument("$abs", "$QuantityChanged")) }
                    }
                )
                .Sort(new BsonDocument("TotalVolume", -1))
                .Limit(limit)
                .Lookup<BsonDocument, BsonDocument>("Product", "_id", "_id", "ProductDetails")
                .Unwind<BsonDocument>("ProductDetails")
                .ToListAsync();
        }
    }
}