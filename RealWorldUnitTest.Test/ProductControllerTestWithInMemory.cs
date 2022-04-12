using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RealWorldUnitTest.Test
{
    public class ProductControllerTestWithInMemory:ProductControllerTest
    {
        //Not: EFCore Inmemory i indiriyoruz manage nugget package den bu katmana.
        public ProductControllerTestWithInMemory()
        {
            //Bu kısım bildiğin startup.cs ya da program.cs teki ksımım gibi;
            setContextOptions(new DbContextOptionsBuilder<UnitTestContext>().UseInMemoryDatabase("UnitTest2InMemoryDB").Options);
            // setContextOptions(new DbContextOptionsBuilder<UnitTestContext>().UseSqlServer())
            //setContextOptions(new DbContextOptionsBuilder<UnitTestContext>().s()) sql lite bende gelmedi.
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
                Assert.NotEmpty(product);//ilişkisel veritabanı olsaydı category silinince ona bağlı tüm productlar silinirdi.
            }
        }
    }
}
