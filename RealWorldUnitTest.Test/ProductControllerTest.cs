using Microsoft.AspNetCore.Mvc;
using Moq;
using RealWorldUnitTest.Web.Controllers;
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
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _productController;
        private List<Product> _products;
        //Nereyi,Neyi,Ne Bekliyorum
        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _productController = new ProductsController(_mockRepo.Object);
            _products = new List<Product>() { 
                new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 1000, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Stock = 2000, Color = "Mavi" }
            };

        }
        [Fact]
        public async void test_index_actionExecutes_returnView()
        {
            var result = await _productController.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void test_index_actionExecutes_returnProductList()
        {
            //Bu mock setup sayesinde GetAll() methodu artık çağrıldığı zaman gereçek olan çalışmayacaktır
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _productController.Index();// index methodu çalıştırılıyor
            var viewResult = Assert.IsType<ViewResult>(result);// index methodunun dönüş değeri ViewResult mu ?
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model); //Dönüş değerinin modeli IEnumerable<Product> tipine dönüştürülebiliyor mu ?
            Assert.Equal<int>(2, productList.Count()); //Dönen değerde 2 tane değer var mı 
        }

        [Fact]
        public async void test_details_idIsNull_returnRedirectToIndexAction()
        {
            var result = await _productController.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result); //dönen RedirectToActionResult mi 
            Assert.Equal("Index", redirect.ActionName);// RedirectToActionResult ile gidilen yer Index mi
        }
        [Fact]
        public async void test_details_productNotFound_returnStatusCodeNotFound()
        {
            Product pr = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(pr);
            var result = await _productController.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        [Theory]
        [InlineData(1)]
        public async void test_details_successfull_returnAProduct(int productId)
        {
            var product = _products.FirstOrDefault(x=>x.Id==productId);
            Assert.NotNull(product);
            _mockRepo.Setup(x=>x.GetById(productId))!.ReturnsAsync(product);
            var result = await _productController.Details(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var productReturned = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.NotNull(productReturned);
            Assert.Equal(product!.Id, productReturned.Id);
        }

        [Fact]
       public  void test_create_actionExecutes_returnView()
        {
            var result =  _productController.Create();
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async void test_createPost_invalidModelState_returnSameProduct()
        {
            _productController.ModelState.AddModelError("Name","Name alanı gereklidir");
            var result = await _productController.Create(_products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedProduct =   Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(_products.First(), returnedProduct);
        }
        [Fact]
        public async void test_createPost_validModelState_returnIndexPage()
        {
            Product product = new Product();
            product.Id = 3;
            product.Name = "Bardak";
            product.Stock = 1;
            product.Price = 12;
            product.Color = "Red";
            //_mockRepo.Setup(x => x.Create(product)).Returns(product);
            var result =  await _productController.Create(product);
            var redirect = Assert.IsType<RedirectToActionResult>(result); //dönen RedirectToActionResult mi 
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void test_createPost_validModelState_repoCreateMethodExecutes()
        {
            Product newProduct = null;
            _mockRepo.Setup(repo=>repo.Create(It.IsAny<Product>()))
                .Callback<Product>(repo =>newProduct = repo);//repo create methoduna hangi
                                                             //product verildiyse callBack ile
                                                             //newProduct a o atanıyor.
         var result = await _productController.Create(_products.First());

            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);//repo create methodunun 1 kere çalışıp çalışmadığı doğrulanıyor
            Assert.Equal(_products.First().Id,newProduct.Id);
        }

        [Fact]
        public async void test_createPost_invalideModelState_repoCreateMethodNeverExecutes()
        {
            _productController.ModelState.AddModelError("Name","Name field required");
           var result = await _productController.Create(_products.First());
            _mockRepo.Verify(repo=>repo.Create(It.IsAny<Product>()),Times.Never);
        }

        [Fact]
        public async  void test_edit_idIsNull_redirectToActionIndex()
        {
          var result = await _productController.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        [Theory]
        [InlineData(3)]
        public async void test_edit_productNotExist_returnNotFound(int productId)
        {
            Product pr = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(pr);
            var result = await _productController.Edit(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, redirect.StatusCode);
        }
        [Theory]
        [InlineData(2)]
        public async void test_edit_productExist_returnFoundProduct(int productId)
        {
            Product pr = _products.First(x=>x.Id==productId);
            _mockRepo.Setup(repo=>repo.GetById(productId)).ReturnsAsync(pr);
            var result = await _productController.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(pr.Id, resultProduct.Id); 

        }

        [Theory]
        [InlineData(1)]
        public async void test_editPost_productNotMatch_returnNotFound(int productId)
        {
            var result = await _productController.Edit(2,_products.First(x=>x.Id == productId));
            var redirect = Assert.IsType<NotFoundResult>(result);
        }
        [Theory]
        [InlineData(1)]
        public async void test_editPost_modelStateInvalide_returnSameProduct(int productId)
        {
            _productController.ModelState.AddModelError("Name", "Name field required");
            var result = await _productController.Edit(productId, _products.First(x => x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(productId,returnedProduct.Id);
        }

       [Fact]
       public async void test_editPost_productModelStateValid_redirectToIndexPage()
        {
            _mockRepo.Setup(repo => repo.Update(_products.First()));
            var result = await _productController.Edit(_products.First().Id, _products.First());
            var redirect = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        [Theory]
        [InlineData(1)]
        public async void test_editPost_validModelState_updateMethodExecutes(int productId)
        {
            _mockRepo.Setup(repo => repo.Update(_products.First(x=>x.Id ==productId)));
            var result = await _productController.Edit(productId, _products.First(x => x.Id == productId));
            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }
        [Fact]
        public async void test_delete_whenIdIsNull_returnsNotFound()
        {
            var result = await _productController.Delete(null);
            var returnResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, returnResult.StatusCode);
        }
        [Theory]
        [InlineData(4)]
        public async void test_delete_whenIdNotMatchWithProductIds_returnsNotFound(int productId)
        {
            Product pr = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(pr);
            var result = await _productController.Delete(productId);
            var returnResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, returnResult.StatusCode);

        }
        [Theory]
        [InlineData(1)]
        public async void test_delete_whenProductFound_retunProductWithView(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(product.Id)).ReturnsAsync(product);
            var result = await _productController.Delete(productId);
            var returnView = Assert.IsType<ViewResult>(result);
            var returnModel = Assert.IsAssignableFrom<Product>(returnView.Model);
            Assert.Equal(returnModel.Id, productId);
        }

        [Theory]
        [InlineData(1)]
        public async void test_deleteConfirmed_whenProductFound_returnIndexAction(int productId)
        {
            Product product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.Delete(product));
            var result = await _productController.DeleteConfirmed(productId);
            var redirectView = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectView.ActionName);  
        }
        [Theory]
        [InlineData(1)]
        public async void test_deleteConfirmed_whenActionExecutes_deleteRepoMethodExecutes(int productId)
        {
            Product product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Delete(product));
             await _productController.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(It.IsAny<Product>()), Times.Once);
        }
    }
}
