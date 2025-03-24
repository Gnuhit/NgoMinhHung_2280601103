using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NgoMinhHung_2280601103.Models;
using NgoMinhHung_2280601103.Repository;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace NgoMinhHung_2280601103.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.AddAsync(product);

                if (imageUrls != null && imageUrls.Count > 0)
                {
                    product.Images = new List<ProductImage>();
                    foreach (var file in imageUrls)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var image = new ProductImage
                            {
                                Url = await SaveImage(file),
                                ProductId = product.Id
                            };
                            product.Images.Add(image);
                        }
                    }
                    await _productRepository.UpdateAsync(product);
                }

                return RedirectToAction(nameof(Index), "Product", new { area = "Admin" });
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                if (imageUrl != null && imageUrl.Length > 0)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                if (imageUrls != null && imageUrls.Count > 0 && imageUrls.Any(f => f != null && f.Length > 0))
                {
                    existingProduct.Images = new List<ProductImage>();
                    foreach (var file in imageUrls)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var image = new ProductImage
                            {
                                Url = await SaveImage(file),
                                ProductId = existingProduct.Id
                            };
                            existingProduct.Images.Add(image);
                        }
                    }
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index), "Product", new { area = "Admin" });
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index), "Product", new { area = "Admin" });
        }

        // Thêm hành động Details
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            Console.WriteLine($"Images count: {(product.Images != null ? product.Images.Count : 0)}");
            return View(product);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }
    }
}