using Microsoft.AspNetCore.Mvc;
using Moq;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Helpers;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RealWorldUnitTest.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _productApiController;
        private List<Product> _products;
        //Business codes test example;
        private readonly Helper _helper;
        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _products = new List<Product>() {
                new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 1000, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Stock = 2000, Color = "Mavi" }
            };
            _productApiController = new ProductsApiController(_mockRepo.Object);
            _helper = new Helper();
        }
        [Fact]
        public async void test_getProduct_actionExecutes_returnOkResultWithProducts()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);
            var result = await _productApiController.GetProduct();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
            Assert.Equal<int>(_products.Count, returnValue.ToList().Count());
        }
        [Theory]
        [InlineData(3)]
        public async void test_getProductWithId_whenProductNotFound_returnsNotFound(int productId)
        {
            Product pr = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(pr);
            var result = await _productApiController.GetProduct(productId);
            // geriye status code ile birlikte bir model dönecek olsaydı ozamam NotFoundObjectResult
            // kullanılacaktı.
            var returnStatus = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, returnStatus.StatusCode);
        }
        [Theory]
        [InlineData(1)]
        public async void test_getProductWithId_whenProductFound_returnsOkWithProduct(int productId)
        {
            Product pr = _products.First(x=>x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(pr.Id)).ReturnsAsync(pr);
            var result = await _productApiController.GetProduct(pr.Id);
            var returnStatus = Assert.IsType<OkObjectResult>(result);
            var returnProduct = Assert.IsAssignableFrom<Product>(returnStatus.Value);
            Assert.Equal(pr.Id, returnProduct.Id);

        }
        [Theory]
        [InlineData(1)]
        public async void test_putProduct_whenIdsNotMatch_returnBadRequest(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            var result = await _productApiController.PutProduct(2, product);
          Assert.IsType<BadRequestResult>(result);
        }
        [Theory]
        [InlineData(1)]
        public async void test_putProduct_whenIdsMatch_returnNoContent(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));
            var result = await _productApiController.PutProduct(productId, product);
            _mockRepo.Verify(repo => repo.Update(product),Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void test_postProduct_actionExecute_returnCreatedAtAction()
        {
            var product = _products.First();
            _mockRepo.Setup(repo => repo.Create(product)).Returns(Task.CompletedTask);
            var result = await _productApiController.PostProduct(product);
            var createAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetProduct", createAtActionResult.ActionName);
            _mockRepo.Verify(x => x.Create(product), Times.Once);

        }
        [Theory]
        [InlineData(0)]
        public async void test_delete_whenIdNotMatchWithAnyProduct_returnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _productApiController.DeleteProduct(productId);
            var returnStatus = Assert.IsType<NotFoundResult>(result.Result);
        }
        [Theory]
        [InlineData(1)]
        public async void test_delete_whenIdMatchsWithAnyProduct_returnNoContent(int productId)
        {
            Product product = _products.First(x=>x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.Delete(product));
            var result = await _productApiController.DeleteProduct(productId);
            var returnStatus = Assert.IsType<NoContentResult>(result.Result);
            _mockRepo.Verify(repo=>repo.Delete(product),Times.Once);
        }
        [Theory]
        [InlineData(4,5,9)]
        public void test_add_sampleData_returnTotal(int a, int b,int total)
        {
            var result = _helper.add(a, b);
            Assert.Equal(total, result);
        }
    }
}
