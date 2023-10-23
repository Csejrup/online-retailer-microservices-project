using RestSharp;
using SharedModels;
using System;
using System.Threading.Tasks;

namespace OrderApi.Infrastructure
{
    public class ProductServiceGateway : IServiceGateway<ProductDto>
    {
        private readonly Uri _productServiceBaseUrl;

        public ProductServiceGateway(Uri baseUrl)
        {
            _productServiceBaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ProductDto> GetAsync(int id)
        {
            var client = new RestClient(_productServiceBaseUrl);
            var request = new RestRequest(id.ToString());
            
            // Execute the request asynchronously
            var response = await client.ExecuteAsync<ProductDto>(request);
            return response.Data;
        }
    }
}