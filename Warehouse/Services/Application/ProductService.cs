using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Warehouse.Models;
using Warehouse.Services.Infrastructure;

namespace Warehouse.Services.Application
{
    /// <summary>
    /// Application service responsible for Product Information Management (PIM).
    /// Handles CRUD operations, paginated retrieval, and fast barcode/name search capabilities.
    /// </summary>
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IMongoService mongoService)
        {
            _products = mongoService.GetCollection<Product>("Product");
        }

        public async Task<List<Product>> GetProductsPaginatedAsync(int skip, int limit)
        {
            return await _products.Find(_ => true)
                                  .Skip(skip)
                                  .Limit(limit)
                                  .ToListAsync();
        }

        public async Task<List<Product>> SearchByBarcodeOrNameAsync(string query)
        {
            if (ObjectId.TryParse(query, out var objectId))
            {
                var exactMatch = await _products.Find(p => p.Id == objectId).ToListAsync();
                if (exactMatch.Count > 0)
                {
                    return exactMatch;
                }
            }

            var filter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(query, "i"));
            return await _products.Find(filter).Limit(50).ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            await _products.InsertOneAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            await _products.ReplaceOneAsync(filter, product);
        }

        public async Task DeleteProductAsync(ObjectId id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            await _products.DeleteOneAsync(filter);
        }
    }
}