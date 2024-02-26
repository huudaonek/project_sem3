using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Data;
using CoffeeLands.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using PayPal.Api;
using Microsoft.Extensions.Configuration;
using CoffeeLands.Services;
using Azure;
using CoffeeLands.ViewModels.Paypal;
using CoffeeLands.ViewModels.Mail;
using CoffeeLands.ViewModels.VNPay;
using CoffeeLands.ViewModels.Momo;
using CoffeeLands.Helpers;
using CoffeeLands.ViewModels;
using Microsoft.AspNetCore.Authorization;



namespace CoffeeLands.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CoffeeLandsContext _context;
        private IHttpContextAccessor _contextAccessor;
        IConfiguration _configuration;
        private IVNPayService _vnPayService;
        private readonly IMailService mailService;
        private IMomoService _momoService;
        public CheckoutController(CoffeeLandsContext context, IHttpContextAccessor contextAccessor, IConfiguration iconfiguration, IVNPayService vnPayService, IMailService mailService, IMomoService momoService)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _configuration = iconfiguration;
            _vnPayService = vnPayService;
            this.mailService = mailService;
            _momoService = momoService;
        }
        public List<CartItem> MyCart => HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var cart = MyCart;
            if (cart != null)
            {
                decimal total = 0;
                ViewBag.Cart = MyCart;
                foreach (CartItem cartItem in cart)
                {
                    total += cartItem.SubTotal;
                }
                ViewBag.Total = total;
                ViewBag.GrandTotal = Math.Round(total * 1.1m, 2);
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([Bind("Name,Email,Tel,Address,Status,Grand_total,Shipping_method,Payment_method,UserID")] OrderProduct order)
        {
            var checkUser = User.Identity.Name;
            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Name == checkUser);

            if (true)
            {
                try
                {
                    //order.Grand_total = grandTotal;
                    //order.Shipping_method = shipping_method;
                    //order.Payment_method = payment_method;
                    order.UserID = user.Id;
                    _context.Add(order);
                    await _context.SaveChangesAsync();

                    var cart = MyCart;
                    if (cart != null)
                    {
                        var orderDetails = new List<OrderDetail>();

                        foreach (CartItem cartItem in cart)
                        {
                            OrderDetail orderDetail = new OrderDetail
                            {
                                OrderProductID = order.Id,
                                ProductID = cartItem.ProductID,
                                Qty = cartItem.Qty,
                                Price = cartItem.SubTotal
                            };
                            orderDetails.Add(orderDetail);
                        }
                        _context.AddRange(orderDetails);
                        await _context.SaveChangesAsync();

                        var orderID = order.Id;
                        HttpContext.Session.SetInt32("OrderID", orderID);
                        ViewBag.OrderProductList = orderDetails;

                        //Paypal
                        if (order.Payment_method == "Paypal")
                        {
                            return RedirectToAction("PaymentWithPaypal");
                        }
                        //Remove product
                        cart.Clear();
                        HttpContext.Session.Set("Cart", cart);
                        //VNPay
                        if (order.Payment_method == "VnPay")
                        {
                            var vnPayModel = new VnPaymentRequestModel
                            {
                                Amount = (double)order.Grand_total,
                                CreatedDate = DateTime.Now,
                                Description = $"{order.Name} {order.Tel}",
                                FullName = order.Name,
                                OrderId = new Random().Next(1000, 10000)
                            };
                            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                        }
                        //Momo
                        if (order.Payment_method == "Momo")
                        {
                            var momoModel = new OrderInfoModel
                            {
                                Amount = (double)order.Grand_total,
                                OrderInfo = order.Address,
                                FullName = order.Name
                            };
                            var response = await _momoService.CreatePaymentAsync(momoModel);
                            return Redirect(response.PayUrl);
                        }
                        //COD
                        if (order.Payment_method == "COD")
                        {
                            //send email
                            var data = new SendMailRequest
                            {
                                ToEmail = order.Email,
                                UserName = order.Name,
                                Url = "ThankYou"
                            };
                            await mailService.SendBodyEmailAsync(data);
                            return Redirect("ThankYou");
                        }
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View("~/Views/Checkout/Index.cshtml");
        }

        #region paypal
        public async Task<IActionResult> PaymentWithPaypal(string Cancel = null, string blogId = "", string PayerID = "", string guid = "")
        {
            var orderID = HttpContext.Session.GetInt32("OrderID");
            var orderUpdate = await _context.OrderProduct
                .Include(o => o.User)
                .Include(od => od.OrderDetails)
                .ThenInclude(p => p.Product)
                 .FirstOrDefaultAsync(m => m.Id == orderID);

            if (orderUpdate != null)
            {
                var ClientID = _configuration.GetValue<string>("PayPal:Key");
                var ClientSecret = _configuration.GetValue<string>("PayPal:Secret");
                var mode = _configuration.GetValue<string>("PayPal:mode");
                APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);
                try
                {
                    string payerId = PayerID;
                    if (string.IsNullOrEmpty(payerId))
                    {
                        string baseUrl = this.Request.Scheme + "://" + this.Request.Host + "/Checkout/PaymentWithPayPal?";
                        var guidd = Convert.ToString((new Random()).Next(100000));
                        guid = guidd;
                        var createdPayment = this.CreatePayment(apiContext, baseUrl + "guid=" + guid, blogId);
                        var links = createdPayment.links.GetEnumerator();
                        string paypalRedirectUrl = null;
                        while (links.MoveNext())
                        {
                            Links lnk = links.Current;
                            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        HttpContext.Session.SetString("payment", createdPayment.id);
                        return Redirect(paypalRedirectUrl);
                    }
                    else
                    {
                        HttpContext.Session.Remove("Cart");
                        var paymentId = HttpContext.Session.GetString("payment");
                        var executePayment = ExecutePayment(apiContext, payerId, paymentId as string);
                        if (executePayment.state.ToLower() != "approved")
                        {
                            return Redirect("Fail");
                        }
                        var blogIds = executePayment.transactions[0].item_list.items[0].sku;

                        //Update Order
                        orderUpdate.Is_paid = true;
                        orderUpdate.Status = OrderStatus.CONFIRMED;
                        _context.Update(orderUpdate);
                        await _context.SaveChangesAsync();
                        ViewBag.OrderId = orderID;
                        //send email
                        var data = new SendMailRequest
                        {
                            ToEmail = orderUpdate.Email,
                            UserName = orderUpdate.Name,
                            Url = "ThankYou"
                        };
                        await mailService.SendBodyEmailAsync(data);
                        return Redirect("ThankYou");
                    }
                }
                catch (Exception ex)
                {
                    return View("~/Views/Checkout/Fail.cshtml");
                }
            }
            return View("~/Views/Checkout/Fail.cshtml");
        }

        private PayPal.Api.Payment payment;
        private IHttpContextAccessor? contextAccessor;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId)
        {
            var cart = MyCart;
            decimal GrandTotal = 0;

            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            foreach (var item in cart)
            {
                var subTotal = Math.Round(item.SubTotal * 1.1m, 2);
                GrandTotal += subTotal;
                itemList.items.Add(new Item()
                {
                    name = item.Name,
                    currency = "USD",
                    price = subTotal.ToString(),
                    quantity = "1",
                    sku = "asd"
                });
            }

            var payer = new PayPal.Api.Payer()
            {
                payment_method = "paypal"
            };
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var amount = new PayPal.Api.Amount()
            {
                currency = "USD",
                total = GrandTotal.ToString()
            };
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(),
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            return this.payment.Create(apiContext);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Fail()
        {
            return View();
        }
        #endregion

        #region Payment VNPay
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExcute(Request.Query);
            var orderID = HttpContext.Session.GetInt32("OrderID");
            var orderUpdate = await _context.OrderProduct
                .Include(o => o.User)
                .Include(od => od.OrderDetails)
                .ThenInclude(p => p.Product)
                 .FirstOrDefaultAsync(m => m.Id == orderID);

            if (orderUpdate != null)
            {
                //HttpContext.Session.Remove("OrderID");
                if (response == null || response.VnPayResponseCode != "00")
                {
                    TempData["Message"] = $"Lỗi thanh toán vnpay: {response.VnPayResponseCode}";
                    return Redirect("Fail");
                }
                TempData["Message"] = $"Thanh toán VNPay thành công";
                //update order
                orderUpdate.Is_paid = true;
                orderUpdate.Status = OrderStatus.CONFIRMED;
                _context.Update(orderUpdate);
                await _context.SaveChangesAsync();
                //send email
                var data = new SendMailRequest
                {
                    ToEmail = orderUpdate.Email,
                    UserName = orderUpdate.Name,
                    Url = "ThankYou"
                };
                await mailService.SendBodyEmailAsync(data);
                return Redirect("ThankYou");
            }
            return View("~/Views/Checkout/Index.cshtml");
        }
        #endregion

        #region Momo Payment
        [HttpGet]
        public IActionResult PaymentCallBackMomo()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View("~/Views/Checkout/Thankyou.cshtml");
        }
        #endregion

        [Authorize]
        public async Task<IActionResult> ThankYou()
        {
            var orderID = HttpContext.Session.GetInt32("OrderID");
            var order = await _context.OrderProduct
    .Include(o => o.User)
    .Include(od => od.OrderDetails)
    .ThenInclude(p => p.Product)
     .FirstOrDefaultAsync(m => m.Id == orderID);
            HttpContext.Session.Remove("OrderID");
            return View(order);
        }
    }
}
