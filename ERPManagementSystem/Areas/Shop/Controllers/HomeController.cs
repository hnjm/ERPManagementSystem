﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPManagementSystem.Data;
using ERPManagementSystem.Extensions;
using ERPManagementSystem.Models;
using ERPManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPManagementSystem.Areas.Shop.Controllers
{
    [Area("Shop")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetAllProductByCategory(int pg,Guid categoryId)
        {
            var product = _context.StockProducts.Include(d => d.Product).Where(c=>c.Product.CategoryId==categoryId);
            ShopVm shopVm = new ShopVm();
            shopVm.StockProducts = _context.StockProducts.Include(d => d.Product).Where(c => c.Product.CategoryId == categoryId).ToList();
            shopVm.Banners = _context.Banners.Where(c => c.BannerStatus == "Enable").ToList();

            return View(shopVm);
        }
        //[HttpGet("/Home/Index")]
        public async Task<IActionResult> Index(int page)
        {
            ShopVm shopVm = new ShopVm();
            shopVm.StockProducts = _context.StockProducts.Include(d => d.Product).ToList();
            shopVm.Banners = _context.Banners.Where(c => c.BannerStatus == "Enable").ToList();
            const int pageSize = 2;
            
            if (page < 1)
            {
                page = 1;
            }
           // var resultProduct = _context.Products.Include(c => c.Category).Include(d => d.SubCategory).Include(e => e.Brand).Where(c => c.ProductStatus == "Enable").ToList();
            var resCount = shopVm.StockProducts.Count();
            ViewBag.TotalRecord = resCount;
            var pager = new Pager(resCount, page, pageSize);
            int resSkip = (page - 1) * pageSize;
            ViewBag.Pager = pager;
            var data = shopVm.StockProducts.Skip(resSkip).Take(pager.PageSize);
            shopVm.StockProducts = data.ToList();
            return View( shopVm);
        }
        [HttpPost("/Home/loaddataonscrol")]
        public async Task<IActionResult> loaddataonscrol(int page)
        {
            ShopVm shopVm = new ShopVm();
            shopVm.StockProducts = _context.StockProducts.Include(d => d.Product).ToList();
            shopVm.Banners = _context.Banners.Where(c => c.BannerStatus == "Enable").ToList();
            const int pageSize = 2;
           
            if (page < 1)
            {
                page = 1;
            }
            var resCount = shopVm.StockProducts.Count();
            ViewBag.TotalRecord = resCount;
            var pager = new Pager(resCount, page, pageSize);
            int resSkip = (page - 1) * pageSize;
            ViewBag.Pager = pager;
            var data = shopVm.StockProducts.Take(page);
            shopVm.StockProducts = data.ToList();
            return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_HomPartial", shopVm) });

        }
        [HttpGet]
        public async Task<IActionResult> ProductDetailInPopUp(Guid id)
        {
            //  StockProduct product;
            var product = await _context.StockProducts.Include(d => d.Product).Where(s => s.Id == id).FirstOrDefaultAsync();
            StockProductVm stockProductVm = new StockProductVm();
            stockProductVm.Id = product.Id;
            stockProductVm.ProductName = product.Product.Name;
            stockProductVm.PreviousPrice = product.PreviousPrice;
            stockProductVm.SalePrice = product.SalePrice;
            stockProductVm.ShortDescription = product.ShortDescription;
            stockProductVm.Description = product.Description;
            stockProductVm.Color = product.Color;
            stockProductVm.Size = product.Size;
            stockProductVm.Quantity = product.Quantity;
            stockProductVm.ImgPath = product.ImgPath;
            stockProductVm.CurrencyName = product.CurrencyName;
            ViewBag.gallery = _context.Galleries.Where(c => c.ProductId == product.ProductId);
          
            return View(stockProductVm);
        }
        [HttpGet]
        public async Task<IActionResult> ProductDetail(Guid id)
        {
            //  StockProduct product;
            var product = await _context.StockProducts.Include(d => d.Product).Where(s => s.Id == id).FirstOrDefaultAsync();
            StockProductVm stockProductVm = new StockProductVm();
            stockProductVm.Id = product.Id;
            stockProductVm.ProductName = product.Product.Name;
            stockProductVm.PreviousPrice = product.PreviousPrice;
            stockProductVm.SalePrice = product.SalePrice;
            stockProductVm.ShortDescription = product.ShortDescription;
            stockProductVm.Description = product.Description;
            stockProductVm.Color = product.Color;
            stockProductVm.Size = product.Size;
            stockProductVm.Quantity = product.Quantity;
            stockProductVm.CurrencyName = product.CurrencyName;
            ViewBag.gallery = _context.Galleries.Where(c => c.ProductId == product.ProductId);
            return View(stockProductVm);
        }
        [HttpPost]
        public async Task<IActionResult> ProductDetail(StockProductVm stockProductVm)
        {
            //  StockProduct product;
            List<StockProductVm> products;
            var stockProduct =await _context.StockProducts.Include(c=>c.Product).Where(d=>d.Id==stockProductVm.Id).FirstOrDefaultAsync();
            var shipping = _context.ShippingCharges.FirstOrDefault();
            StockProductVm productVm = new StockProductVm();
            productVm.Id = stockProduct.Id;
            
            productVm.ProductName = stockProduct.Product.Name;
            productVm.ImgPath = stockProduct.Product.ImagePath ;
            productVm.CartQuantity = stockProductVm.CartQuantity;
            productVm.Quantity = stockProduct.Quantity;
            productVm.SalePrice = stockProduct.SalePrice;
            productVm.StockProduct = stockProduct.Quantity;
            productVm.ProductSerial = stockProduct.Product.ProductSerial;
            productVm.ProductTotal = stockProductVm.CartQuantity * stockProduct.SalePrice;
            productVm.CurrencyName = stockProductVm.CurrencyName;
            //Start Session
            products = HttpContext.Session.Get<List<StockProductVm>>("products");
            if (products == null)
            {
                products = new List<StockProductVm>();
            }
            decimal subtotal = 0;
            int squantity = 0;
            foreach (var item in products)
            {
                subtotal = subtotal + item.ProductTotal;
                squantity = item.CartQuantity + squantity;
            }
            decimal finalCharge = 0;
            decimal scharge2 = 0;
            var shippingCharge = _context.ShippingCharges.FirstOrDefault();
            if (shippingCharge!=null)
            {
                 scharge2 = ((squantity - 1) * shippingCharge.IncreaeChargePerProduct) + shippingCharge.BaseCharge;
                 finalCharge = scharge2;
            }
         
          
            products.Add(productVm);
            HttpContext.Session.Set("products", products);
              ShippingTotal shippingTotal = new ShippingTotal();
            shippingTotal.ShippingCost = finalCharge;
            shippingTotal.SubTotal = subtotal;
            HttpContext.Session.Set("shippingTotal", shippingTotal);
            return RedirectToAction(nameof(Index));
        }
        //Remove fom cart
        [HttpPost]
        public ActionResult RemoveToCart(StockProductVm stockProductVm)
        {
            List<StockProductVm> products = products = HttpContext.Session.Get<List<StockProductVm>>("products");
            if (products != null)
            {
                var product = products.FirstOrDefault(c => c.Id == stockProductVm.Id);
                if (product != null)
                {
                    products.Remove(product);
                    HttpContext.Session.Set("products", products);
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult RemoveProductToCart(Guid id)
        {
            List<StockProductVm> products = products = HttpContext.Session.Get<List<StockProductVm>>("products");
            if (products != null)
            {
                var product = products.FirstOrDefault(c => c.Id == id);
                if (product != null)
                {
                    products.Remove(product);
                    HttpContext.Session.Set("products", products);
                }
            }
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> Cart(Guid id)
        {
            List<StockProductVm> products = HttpContext.Session.Get<List<StockProductVm>>("products");
            if (products == null)
            {
                products = new List<StockProductVm>();
                ViewBag.subTotal = 0.00;
            }
            decimal subtotal=0;
            foreach (var item in products)
            {
                subtotal = subtotal + item.ProductTotal;
            }
            ViewBag.subTotal = subtotal;
            return View(products);
           
        }
        [HttpPost]
        public async Task<IActionResult> Cart(List<StockProductVm>vm )
        {
            List<StockProductVm> products = HttpContext.Session.Get<List<StockProductVm>>("products");
            if (products == null)
            {
                products = new List<StockProductVm>();
                ViewBag.subTotal = 0.00;
            }

            foreach (var item in vm)
            {
                var updateCartItem = products.Where(c => c.Id == item.Id).FirstOrDefault();
                updateCartItem.CartQuantity = item.CartQuantity;
                updateCartItem.ProductTotal = item.ProductTotal;
                
            }
            HttpContext.Session.Set("products", products);
            decimal subtotal = 0;
            foreach (var item in products)
            {
                subtotal = subtotal + item.ProductTotal;
            }
            ViewBag.subTotal = subtotal;
            return View(products);

        }
    }
}
