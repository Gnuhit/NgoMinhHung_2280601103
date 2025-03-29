using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoMinhHung_2280601103.Models;

namespace NgoMinhHung_2280601103.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var order = new Order
            {
                TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity)
            };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            System.Diagnostics.Debug.WriteLine("Checkout POST called");
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            System.Diagnostics.Debug.WriteLine($"Cart: {(cart == null ? "null" : cart.Items.Count.ToString())} items");

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View("Index", order);
            }

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            try
            {
                _context.Orders.Add(order);
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Order saved with ID: {order.Id}");
                return RedirectToAction("OrderCompleted", new { id = order.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving your order.");
                return View("Index", order);
            }
        }

        public IActionResult OrderCompleted(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}