using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIWithRSL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public ProductsController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_dataContext.Products.ToList());
        }

        [HttpGet("{productId}", Name = "ProductGet")]
        public IActionResult GetById(Guid productId)
        {
            var product = _dataContext.Products.Where(p => p.ProductId == productId).FirstOrDefault();
            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public IActionResult Post([FromBody]Product product)
        {
            product.ProductId = Guid.NewGuid();
            product.TenantId = _dataContext.GetTenantId();

            _dataContext.Products.Add(product);
            _dataContext.SaveChanges();

            var url = Url.Link("ProductGet", new { productId = product.ProductId });
            return Created(url, product);
        }

        [HttpPut("{productId}")]
        public IActionResult Put(Guid productId, [FromBody]Product product)
        {
            var existingProduct = _dataContext.Products.Where(p => p.ProductId == productId).FirstOrDefault();
            if (existingProduct == null) return NotFound();

            existingProduct.ProductName = product.ProductName;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.UnitsInStock = product.UnitsInStock;
            existingProduct.UnitsOnOrder = product.UnitsOnOrder;
            existingProduct.ReorderLevel = product.ReorderLevel;
            existingProduct.Discontinued = product.Discontinued;

            _dataContext.SaveChanges();

            return Ok(existingProduct);
        }

        [HttpDelete("{productId}")]
        public IActionResult Delete(Guid productId, [FromBody]Product product)
        {
            var existingProduct = _dataContext.Products.Where(p => p.ProductId == productId).FirstOrDefault();
            if (existingProduct == null) return NotFound();

            _dataContext.Products.Remove(existingProduct);

            _dataContext.SaveChanges();

            return NoContent();
        }
    }
}