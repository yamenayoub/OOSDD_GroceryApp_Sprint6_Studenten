using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Diagnostics;

namespace Grocery.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        public Product Add(Product item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item), "Product item cannot be null.");
            }

            var addedProduct = _productRepository.Add(item);
            return addedProduct;
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Get(int id)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }
    }
}
