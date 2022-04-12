using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RealWorldUnitTest.Test
{
    public class ProductControllerTestWithSqlServerExpressLocalDB : ProductControllerTest
    {
        //Not: SQL Server Object Explorer panelindeki local db leri kullanabiliriz
        public ProductControllerTestWithSqlServerExpressLocalDB()
        {
            var sqlConn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=testDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            setContextOptions(new DbContextOptionsBuilder<UnitTestContext>().UseSqlServer(sqlConn).Options);
            Seed();

        }
        //testimizi yazalım;
        [Fact]
        public async Task test_create_modelValidProduct_returnRedirectToActionIndex() 
        {
            var newProduct = new Product { Name = "Kalem 30", Price = 200, Stock = 300, Color = "Blue" };

            using (var context = new UnitTestContext(_dbContextOptions))
            {
                var category = context.Categories.First();
                newProduct.CategoryId = category.Id;
                var controller = new ProductsController(context);
                var result = await controller.Create(newProduct);
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirect.ActionName);
            }
            using (var context = new UnitTestContext(_dbContextOptions))
            {
                var product = context.Products.FirstOrDefault(x => x.Name == newProduct.Name);
                Assert.Equal(newProduct.Name, product?.Name);
            }
        }
        [Theory]
        [InlineData(1)]
        public async Task test_deleteCatgegory_existCategoryId_deleteAllProducts(int categoryId)
        {
            using (var context = new UnitTestContext(_dbContextOptions))
            {
                var category = await context.Categories.FindAsync(categoryId);
                context.Categories.Remove(category);
                context.SaveChanges();
            }
            using (var context = new UnitTestContext(_dbContextOptions))
            {
                var product = await context.Products.Where(x=>x.CategoryId == categoryId).ToListAsync();
                Assert.Empty(product);//ilişkisel veritabanı olduğu için category silinince ona bağlı tüm productlar silindi.
            }
        }
    }
}
