using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealWorldUnitTest.Test
{
    public class ProductControllerTest
    {
        //Bu sayde artık startup ta ya da program.cs teki sql ile olan bağlantının kullanılması
        //yerine testler burada bizim istediğimiz vertabanını kullanacaklar.
        protected DbContextOptions<UnitTestContext> _dbContextOptions { get;private set; }
       public void setContextOptions(DbContextOptions<UnitTestContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }
        public void Seed()
        {
            using (UnitTestContext context = new UnitTestContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();//testler ayağa kalkınca varsa silinsin
                context.Database.EnsureCreated();//ardından sıfırdan oluşturulsun.
                //zaten category contextin web te kullanıldığı yerden inmemory db sine de eklenecek
                //orada product eklememiştik bu yüzden burada product ekliyoruz;
                context.SaveChanges();
                context.Products.Add(new Product { CategoryId = 1, Name = "kalem 10", Price = 100, Stock = 50,Color="Red" });
                context.Products.Add(new Product { CategoryId = 2, Name = "Defter 10", Price = 20, Stock = 10, Color = "Yellow" });
                context.SaveChanges();
            }

        }
    }
}
