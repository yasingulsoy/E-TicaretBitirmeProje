﻿using System.Threading.Tasks;
using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using ETicaret.WebUI.ExtensionMethods;
using ETicaret.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _service;
        private readonly IService<Adress> _adressservice;
        private readonly IService<AppUser> _appuserservice;
        private readonly IService<Order> _orderservice;
        private readonly IConfiguration _configuration; //appsettings içerisinde ki ödeme apisini burada çektik
        public CartController(IService<Product> service, IService<Adress> adressservice, IService<AppUser> appuserservice, IService<Order> orderservice, IConfiguration configuration)
        {
            _service = service;
            _adressservice = adressservice;
            _appuserservice = appuserservice;
            _orderservice = orderservice;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            var model = new CartViewModel()
            {
                CartLines = cart.CartLines,
                TotalPrice = cart.TotalPrice()
            };
            return View(model);
        }
        public IActionResult Add(int ProductId, int quantity = 1)
        {
            var product = _service.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
                return Redirect(Request.Headers["Referer"].ToString()); //Kullanıcının add metoduna gelmeden önceki sayfaya yönlendirir.
            }
            return RedirectToAction("Index");
        }
        public IActionResult Update(int ProductId, int quantity = 1)
        {
            var product = _service.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.UpdateProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int ProductId)
        {
            var product = _service.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.RemoveProduct(product);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        [Authorize]
        public async Task<IActionResult> CheckOut()
        {

            var cart = GetCart();
            var appUser = await _appuserservice.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            var addresses = await _adressservice.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckOutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Adresses = addresses


            };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CheckOut(string CardNumber, string CardNameSurname, string CardMonth, string CardYear, string CVV, string DeliveryAdress, string BillingAdress)
        {

            var cart = GetCart();
            var appUser = await _appuserservice.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            var addresses = await _adressservice.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckOutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Adresses = addresses


            }; //Bilgilerin Boş Gelip Gelmediğini kontrol ettik
            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardMonth) || string.IsNullOrWhiteSpace(CardMonth)
                || string.IsNullOrWhiteSpace(CardYear) || string.IsNullOrWhiteSpace(CVV) || string.IsNullOrWhiteSpace(DeliveryAdress) || string.IsNullOrWhiteSpace(BillingAdress))
            {
                return View(model); //Değerler boşsa geri dönecek
            } //Boş değilse adres bilgileri kontrol edilecek
            var faturaAdresi = addresses.FirstOrDefault(x => x.AdressGuid.ToString() == BillingAdress);
            var teslimatAdresi = addresses.FirstOrDefault(x => x.AdressGuid.ToString() == DeliveryAdress);


            //Ödeme Alma İşlemi
            var siparis = new Order
            {
                AppUserId = appUser.Id,
                BillingAddress = $"{faturaAdresi.OpenAdress} {faturaAdresi.District} {faturaAdresi.City}",//BillingAdress
                DeliveryAddress = $"{faturaAdresi.OpenAdress} {faturaAdresi.District} {faturaAdresi.City}", //DeliveryAddress
                CustomerId = appUser.UserGuid.ToString(),
                OrderDate = DateTime.Now,
                TotalPrice = cart.TotalPrice(),
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = 0,
                OrderLines = []
            };

            //foreach (var item in cart.CartLines)
            //{
            //    siparis.OrderLines.Add(new OrderLine
            //    {
            //        ProductId = item.Product.Id,
            //        OrderId = siparis.Id,
            //        Quantity = item.Quantity,
            //        UnitPrice = item.Product.Price,

            //    });
            //}
            #region Ödeme İşlemi
            Options options = new Options();
            options.ApiKey = "sandbox-UqtsYUPugfoIpoaDr2KrqMxeV6BU8BHf";
            options.SecretKey = "sandbox-ANN9RPBQahoPrOwqIulyNqFTGKBUdyMM";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = HttpContext.Session.Id;
            request.Price = siparis.TotalPrice.ToString().Replace(",", ".");
            request.PaidPrice = siparis.TotalPrice.ToString().Replace(",", ".");
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B" + HttpContext.Session.Id;
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = CardNameSurname;                  //"John Doe";
            paymentCard.CardNumber = CardNumber;                           //"5528790000000008";
            paymentCard.ExpireMonth = CardMonth;                           //"12";
            paymentCard.ExpireYear = CardYear;                             // "2030";
            paymentCard.Cvc = CVV;                                          //"123";
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = "BY" + appUser.Id;
            buyer.Name = appUser.Name;
            buyer.Surname = appUser.Surname;
            buyer.GsmNumber = appUser.Phone;
            buyer.Email = appUser.Email;
            buyer.IdentityNumber = "11111111111";
            buyer.LastLoginDate = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss");            //"2015-10-05 12:43:35";
            buyer.RegistrationDate = appUser.CreateDate.ToString("yyyy-mm-dd hh:mm:ss");                                 //"2013-04-21 15:12:09";
            buyer.RegistrationAddress = siparis.DeliveryAddress.ToString();                                //"Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = HttpContext.Connection.RemoteIpAddress?.ToString();                   //"85.34.78.112";
            buyer.City = teslimatAdresi.City;                                                        // "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            var shippingAddress = new Address();
            shippingAddress.ContactName = appUser.Name + " " + appUser.Surname;
            shippingAddress.City = teslimatAdresi.City;
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = teslimatAdresi.OpenAdress;                      //"Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            var billingAddress = new Address();
            billingAddress.ContactName = appUser.Name + " " + appUser.Surname;
            billingAddress.City = faturaAdresi.City;
            billingAddress.Country = "Turkey";
            billingAddress.Description = faturaAdresi.OpenAdress;
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            //BasketItem firstBasketItem = new BasketItem();
            //firstBasketItem.Id = "BI101";
            //firstBasketItem.Name = "Binocular";
            //firstBasketItem.Category1 = "Collectibles";
            //firstBasketItem.Category2 = "Accessories";
            //firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            //firstBasketItem.Price = "0.3";
            //basketItems.Add(firstBasketItem);

            foreach (var item in cart.CartLines)
            {
                siparis.OrderLines.Add(new OrderLine
                {
                    ProductId = item.Product.Id,
                    OrderId = siparis.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,

                });
                basketItems.Add(new BasketItem
                {
                    Id=item.Product.Id.ToString(),
                    Name = item.Product.Name,
                    Category1 ="Collectibles",
                    ItemType= BasketItemType.PHYSICAL.ToString(),
                    Price =(item.Product.Price * item.Quantity).ToString().Replace(",",".")



                });
            }

            request.BasketItems = basketItems;

            Payment payment = await Payment.Create(request, options);

            #endregion
            try
            {
                if (payment.Status == "success")
                {
                    //Sipariş Oluşturma
                    await _orderservice.AddAsync(siparis);
                    var sonuc = await _orderservice.SaveChangesAsync();
                    if (sonuc > 0)
                    {
                        HttpContext.Session.Remove("Cart");
                        return RedirectToAction("Thanks");
                    }
                }
                else
                {
                    TempData["Message"] = $"<div class='alert alert-danger'>Ödeme İşlemi Başarısız!</div>({payment.ErrorMessage})";
                }
            }
            catch (Exception)
            {

                TempData["Message"] = "<div class='alert alert-danger'>Hata Oluştu!</div>";
            }

            return View(model);
        }
        public IActionResult Thanks()
        {


            return View();
        }
        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService(); //Session üzerinden verileri çekmeye çalışacak eğer null ise yani sepette ürün yoksa yeni bir cartservice oluştur.

        }

    }
}
