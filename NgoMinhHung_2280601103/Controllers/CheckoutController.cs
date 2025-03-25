using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoMinhHung_2280601103.Extensions;
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

        // GET: Checkout
        public IActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine("Checkout Index action called.");
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                System.Diagnostics.Debug.WriteLine("Cart is empty. Redirecting to ShoppingCart.");
                TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            System.Diagnostics.Debug.WriteLine($"Cart has {cart.Items.Count} items.");
            var order = new Order
            {
                TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity)
            };
            return View(order);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            System.Diagnostics.Debug.WriteLine("Checkout POST action called.");
            System.Diagnostics.Debug.WriteLine($"ShippingAddress: {order.ShippingAddress}");
            System.Diagnostics.Debug.WriteLine($"Notes: {order.Notes}");

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                System.Diagnostics.Debug.WriteLine("Cart is empty. Redirecting to ShoppingCart.");
                TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            System.Diagnostics.Debug.WriteLine($"Cart has {cart.Items.Count} items.");
            foreach (var item in cart.Items)
            {
                System.Diagnostics.Debug.WriteLine($"Cart Item: ProductId={item.ProductId}, Quantity={item.Quantity}, Price={item.Price}");
            }

            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View("Index", order);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("User is null. Redirecting to Login.");
                return RedirectToAction("Login", "Account");
            }
            System.Diagnostics.Debug.WriteLine($"User ID: {user.Id}");

            // Check if all products in the cart exist
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Product with ID {item.ProductId} not found.");
                    ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                    return View("Index", order);
                }
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
                System.Diagnostics.Debug.WriteLine("Saving order to database...");
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Order {order.Id} saved successfully.");
                HttpContext.Session.Remove("Cart");
                System.Diagnostics.Debug.WriteLine("Redirecting to OrderCompleted.");
                return RedirectToAction("OrderCompleted", new { id = order.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving order: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while placing your order. Please try again.");
                return View("Index", order);
            }
        }

        // GET: OrderCompleted
        public IActionResult OrderCompleted(int id)
        {
            System.Diagnostics.Debug.WriteLine($"OrderCompleted action called with ID: {id}");
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                System.Diagnostics.Debug.WriteLine($"Order with ID {id} not found.");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine("Rendering OrderCompleted view.");
            return View(order);
        }
    }
}