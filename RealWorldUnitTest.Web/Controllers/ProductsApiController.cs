#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repository;

namespace RealWorldUnitTest.Web.Controllers
{
    [Route("api/[controller]")]
   // [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly IRepository<Product> _repository;

        public ProductsApiController(IRepository<Product> repository)
        {
            _repository = repository;
        }

        // GET: api/ProductsApi
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            var result = await _repository.GetAll();
            return Ok(result);
        }

        // GET: api/ProductsApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _repository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/ProductsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

          await  _repository.Update(product);

            return NoContent();
        }

        // POST: api/ProductsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostProduct(Product product)
        {
          await _repository.Create(product);
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/ProductsApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            await _repository.Delete(product);
            return NoContent();
        }

        private async Task<bool> ProductExists(int id)
        {
            Product product = await _repository.GetById(id);
            if (product == null)
                return false;
            return true;
        }
    }
}
