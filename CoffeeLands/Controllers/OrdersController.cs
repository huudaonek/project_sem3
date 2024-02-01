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


namespace CoffeeLands.Controllers
{
    public class OrdersController : Controller
    {
        private readonly CoffeeLandsContext _context;
        private IHttpContextAccessor _contextAccessor;
        IConfiguration _configuration;
        public OrdersController(CoffeeLandsContext context, IHttpContextAccessor contextAccessor, IConfiguration iconfiguration)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _configuration = iconfiguration;
        }

        // Index Orders
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var orders = from o in _context.OrderProduct
                           .Include(u => u.User)
                         select o;
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    orders = orders.OrderByDescending(s => s.Name);
                    break;
                default:
                    orders = orders.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<OrderProduct>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        #region Checkout
        public async Task<IActionResult> Checkout()
        {
            var checkUser = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(checkUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                ViewBag.MySession = checkUser.ToString();
            }

            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            var productListJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(productListJson))
            {
                decimal totalProduct = 0;
                decimal subtotal = 0;
                var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                ViewBag.Cart = productList;
                foreach (ProductCart productCart in productList)
                {
                    totalProduct = productCart.Qty * productCart.CartProduct.Price;
                    subtotal += totalProduct;
                }
                ViewBag.TotalProduct = totalProduct;
                ViewBag.Subtotal = subtotal;
                ViewBag.GrandTotal = subtotal * 1.1m;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([Bind("Name,Email,Tel,Address,Status,Grand_total,Shipping_method,Payment_method,UserID")] OrderProduct order, decimal grandTotal, string shipping_method, string payment_method)
        {
            var checkUser = HttpContext.Session.GetString("UserSession");
            var user = await _context.User
                .Include(pc => pc.ProductCarts)
                .FirstOrDefaultAsync(m => m.Name == checkUser);

            if (true)
            {
                if (user != null)
                {
                    try
                    {
                        order.Grand_total = grandTotal;

                        if (shipping_method == "express")
                        {
                            order.Shipping_method = "Express";
                        }
                        else if (shipping_method == "free_shipping")
                        {
                            order.Shipping_method = "Free Ship";
                        }

                        if (payment_method == "bank")
                        {
                            order.Payment_method = "Banking";
                        }
                        else if (payment_method == "paypal")
                        {
                            order.Payment_method = "Paypal";
                        }
                        else if (payment_method == "momo")
                        {
                            order.Payment_method = "Momo";
                        }
                        else if (payment_method == "paymentOnDelivery")
                        {
                            order.Payment_method = "Payment_On_Delivery";
                        }

                        order.UserID = user.Id;
                        _context.Add(order);
                        await _context.SaveChangesAsync();

                        var productListJson = HttpContext.Session.GetString("Cart");

                        if (!string.IsNullOrEmpty(productListJson))
                        {
                            var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                            var orderDetails = new List<OrderDetail>();

                            foreach (ProductCart cart in productList)
                            {
                                OrderDetail orderDetail = new OrderDetail
                                {
                                    OrderProductID = order.Id,
                                    ProductID = cart.CartProduct.Id,
                                    Qty = cart.Qty,
                                    Price = cart.CartProduct.Price * cart.Qty
                                };
                                //_context.Add(orderDetail);
                                orderDetails.Add(orderDetail);
                            }
                            _context.AddRange(orderDetails);
                            await _context.SaveChangesAsync();

                            ViewBag.OrderProductList = orderDetails;

                            if (order.Payment_method == "Paypal")
                            {
                                var orderID = order.Id;
                                HttpContext.Session.SetInt32("OrderID", orderID);

                                return RedirectToAction("PaymentWithPaypal");
                            }
                            productList.Clear();
                            string updatedProductListJson = JsonConvert.SerializeObject(productList);
                            HttpContext.Session.SetString("Cart", updatedProductListJson);
                            HttpContext.Session.SetString("CartNumber", productList.Count.ToString());

                            if (order.Payment_method == "Payment_On_Delivery")
                            {
                                ViewBag.MySession = checkUser.ToString();
                                ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
                                var orderSuccess = await _context.OrderProduct
                                    .Include(o => o.User)
                                    .Include(od => od.OrderDetails)
                                    .ThenInclude(p => p.Product)
                                    .FirstOrDefaultAsync(m => m.Id == order.Id);
                                return View("~/Views/Orders/Thankyou.cshtml", orderSuccess);

                            }
                            return View("~/Views/Orders/Payment.cshtml", order);
                        }
                    }
                    catch (DbUpdateException)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View("~/Views/Orders/Checkout.cshtml");
        }

        public async Task<IActionResult> Pay(int? id)
        {
            var checkUser = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(checkUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                ViewBag.MySession = checkUser.ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");

            var orderUpdate = await _context.OrderProduct
                .Include(o => o.User)
                .Include(od => od.OrderDetails)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (orderUpdate == null)
            {
                return NotFound();
            }
            if (true)
            {
                try
                {
                    orderUpdate.Is_paid = true;
                    orderUpdate.Status = OrderStatus.CONFIRMED;
                    _context.Update(orderUpdate);
                    await _context.SaveChangesAsync();
                    return View("~/Views/Orders/Thankyou.cshtml", orderUpdate);
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View("~/Views/Orders/Payment.cshtml");
        }
        #endregion

        #region paypal
        public async Task<IActionResult> PaymentWithPaypal(string Cancel = null, string blogId = "", string PayerID = "", string guid = "")
        {
            var orderID = HttpContext.Session.GetInt32("OrderID");
            //ViewBag.OrderId = orderID;
            var checkUser = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(checkUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                ViewBag.MySession = checkUser.ToString();
            }
            var productListJson = HttpContext.Session.GetString("Cart");

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
                        string baseUrl = this.Request.Scheme + "://" + this.Request.Host + "/Orders/PaymentWithPayPal?";
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
                        var paymentId = HttpContext.Session.GetString("payment");
                        var executePayment = ExecutePayment(apiContext, payerId, paymentId as string);
                        if (executePayment.state.ToLower() != "approved")
                        {
                            if (productListJson != null)
                            {
                                var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                                productList.Clear();
                                string updatedProductListJson = JsonConvert.SerializeObject(productList);
                                HttpContext.Session.SetString("Cart", updatedProductListJson);
                                HttpContext.Session.SetString("CartNumber", productList.Count.ToString());
                                HttpContext.Session.Remove("OrderID");
                                ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
                            }
                            return View("~/Views/Orders/Thankyou.cshtml", orderUpdate);
                        }
                        var blogIds = executePayment.transactions[0].item_list.items[0].sku;
                        orderUpdate.Is_paid = true;
                        orderUpdate.Status = OrderStatus.CONFIRMED;
                        _context.Update(orderUpdate);
                        await _context.SaveChangesAsync();

                        if (productListJson != null)
                        {
                            var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                            productList.Clear();
                            string updatedProductListJson = JsonConvert.SerializeObject(productList);
                            HttpContext.Session.SetString("Cart", updatedProductListJson);
                            HttpContext.Session.SetString("CartNumber", productList.Count.ToString());
                            HttpContext.Session.Remove("OrderID");
                            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
                        }
                        return View("~/Views/Orders/Thankyou.cshtml", orderUpdate);
                    }
                }
                catch (Exception ex)
                {
                    return View("~/Views/Orders/Fail.cshtml");
                }
            }
            return View("~/Views/Orders/Fail.cshtml");
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
            var cart = HttpContext.Session.GetString("Cart");
            var orderPaypal = JsonConvert.DeserializeObject<List<ProductCart>>(cart);
            decimal grandTotal = 0;
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            foreach (var item in orderPaypal)
            {
                var price = item.CartProduct.Price * item.Qty;
                grandTotal += price;
                itemList.items.Add(new Item()
                {
                    name = item.CartProduct.Name,
                    currency = "USD",
                    price = price.ToString(),
                    quantity = item.Qty.ToString(),
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

            //var details = new Details()
            //{
            //    tax = "1",
            //    shipping = "1",
            //    subtotal = "1"
            //};

            var amount = new PayPal.Api.Amount()
            {
                currency = "USD",
                total = grandTotal.ToString("0.00")
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.OrderProduct
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private bool OrderExists(int id)
        {
            return _context.OrderProduct.Any(e => e.Id == id);
        }
    }
}
