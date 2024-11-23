using DEPI_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DEPI_Project.Controllers
{
	public class WishlistController : Controller
	{
		private readonly ApplicationDbContext _context;
		public WishlistController(ApplicationDbContext context)
		{
			_context = context;

		}
		[Authorize]
		public IActionResult Index()
		{

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" }); // Return null if user is not authenticated
            }

            var list = _context.Wishlist
                .Where(x => x.UserId == userId).Include(x => x.User)
                .Include(ci => ci.Product)
                .ToList();

            if (list == null)
            {
                return RedirectToAction("Home", "Index");
            }

            return View(list);
           
            


        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddWishes(int productId)
		{
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // الحصول على ID المستخدم
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" }); // Return null if user is not authenticated
            }
            // الحصول على السلة الحالية للمستخدم أو إنشاء واحدة جديدة

            if (productId != 0)
            {
                var proid = _context.Wishlist.Where(x=>x.ProductId == productId && x.UserId == userId).FirstOrDefault();
                if (proid==null)
                {
                    
                

                    var cart = new Wishlist
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        ProductId = productId
				    };
                    _context.Wishlist.Add(cart);
				}
                else
				{
					TempData["ErrorMessage"] = "You already added it to Wish List.";
					return RedirectToAction("Withlist", "Index");
				}
			}

			// البحث عن المنتج
			// حفظ التغييرات في قاعدة البيانات
			await _context.SaveChangesAsync();
			return Json(new { success = true });
        }
        [HttpPost]
        [Authorize]
        public IActionResult RemoveItem(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = Index();
            if (cart == null) return RedirectToAction("Home", "Index");

            var cartItem = _context.Wishlist.FirstOrDefault(item => item.Product.Id == productId && item.UserId == userId);
            if (cartItem != null)
            {
                _context.Wishlist.Remove(cartItem); // Remove item from cart
            }

            _context.SaveChanges(); // Save changes to the database
            return RedirectToAction("Index");
        }
    }
}
