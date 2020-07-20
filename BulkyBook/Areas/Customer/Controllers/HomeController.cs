using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            //var admin = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.UserName == "admin@gmail.com") as IdentityUser;
            //var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            //var result = await _userManager.ResetPasswordAsync(admin, token, "Bulky123$");

            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                   .GetAll(c => c.ApplicationUserId == claim.Value)
                   .ToList().Count();
            }

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productfromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category,CoverType");
            var cartObj = new ShoppingCart()
            {
                Product = productfromDb,
                ProductId = productfromDb.Id
            }; 
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObject)
        {
            cartObject.Id = 0;
            if(ModelState.IsValid)
            {
                // then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart
                    .GetFirstOrDefault(
                    sc => sc.ApplicationUserId == cartObject.ApplicationUserId
                    && sc.ProductId == cartObject.ProductId,
                    includeProperties: "Product"
                    );

                if(cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(cartObject);
                } 
                else
                {
                    cartFromDb.Count += cartObject.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }

                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == cartObject.ApplicationUserId)
                    .ToList().Count();

                //HttpContext.Session.SetObject(SD.ssShoppingCart, cartObject);
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
                


                return RedirectToAction(nameof(Index));
            } 
            else
            {
                var productfromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == cartObject.Id, includeProperties: "Category,CoverType");
                var cartObj = new ShoppingCart()
                {
                    Product = productfromDb,
                    ProductId = productfromDb.Id
                };
                return View(cartObj);
            }


           
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
