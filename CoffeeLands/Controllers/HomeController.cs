using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using PayPal.Api;

namespace CoffeeLands.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeLandsContext _coffeeContext;
        private readonly ILogger<HomeController> _logger;
		private IHttpContextAccessor _contextAccessor;
		IConfiguration _configuration;

		public HomeController(ILogger<HomeController> logger, CoffeeLandsContext coffeeContext, IHttpContextAccessor context, IConfiguration iconfiguration)
        {
            _logger = logger;
            _coffeeContext = coffeeContext;
			_contextAccessor = context;
			_configuration = iconfiguration;
		}

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
                ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Index.cshtml");
        }


     //   public ActionResult PaymentWithPaypal(string Cancel = null, string blogId = "", string PayerID = "", string guid = "")
     //   {

     //       var ClientID = _configuration.GetValue<string>("PayPal:Key");
     //       var ClientSecret = _configuration.GetValue<string>("PayPal:Secret");
     //       var mode = _configuration.GetValue<string>("PayPal:mode");
     //       APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);
     //       try
     //       {
     //           string payerId = PayerID;
     //           if (string.IsNullOrEmpty(payerId))
     //           {
     //               string baseUrl = this.Request.Scheme + "://" + this.Request.Host + "/Home/PaymentWithPayPal?";
     //               var guidd = Convert.ToString((new Random()).Next(100000));
     //               guid = guidd;
     //               var createdPayment = this.CreatePayment(apiContext, baseUrl + "guid=" + guid, blogId);
     //               var links = createdPayment.links.GetEnumerator();
     //               string paypalRedirectUrl = null;
     //               while (links.MoveNext())
     //               {
     //                   Links lnk = links.Current;
     //                   if (lnk.rel.ToLower().Trim().Equals("approval_url"))
     //                   {
     //                       paypalRedirectUrl = lnk.href;
     //                   }
     //               }
					//HttpContext.Session.SetString("payment", createdPayment.id);
     //               return Redirect(paypalRedirectUrl);
     //           }
     //           else
     //           {
     //               var paymentId = HttpContext.Session.GetString("payment");
     //               var executePayment = ExecutePayment(apiContext, payerId, paymentId as string);
     //               if (executePayment.state.ToLower() != "approved")
     //               {
     //                   return View("PaymentFailed");
     //               }
     //               var blogIds = executePayment.transactions[0].item_list.items[0].sku;
     //               return RedirectToAction("Index", "Home");

     //           }
     //       }
     //       catch (Exception ex)
     //       {
     //           return View("PaymentFailed");
     //       }
     //       return View("SuccessView");
     //   }
     //   private PayPal.Api.Payment payment;
     //   private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
     //   {
     //       var paymentExecution = new PaymentExecution()
     //       {
     //           payer_id = payerId
     //       };
     //       this.payment = new Payment()
     //       {
     //           id = paymentId
     //       };
     //       return this.payment.Execute(apiContext, paymentExecution);
     //   }
     //   private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId)
     //   {
     //       var itemList = new ItemList()
     //       {
     //           items = new List<Item>()
     //       };
     //       itemList.items.Add(new Item(){
     //           name = "Item Detail",
     //               currency = "USD",
     //               price = "1.00",
     //               quantity = "1",
     //               sku = "asd"
     //       });
     //       var payer = new Payer()
     //       {
     //           payment_method = "paypal"
     //       };
     //       var redirUrls = new RedirectUrls()
     //       {
     //           cancel_url = redirectUrl + "&Cancel=true",
     //           return_url = redirectUrl
     //       };
     //       var amount = new Amount()
     //       {
     //           currency = "USD",
     //           total = "1.00"
     //       };
     //       var transactionList = new List<Transaction>();
     //       transactionList.Add(new Transaction()
     //       {
     //           description = "Transaction description",
     //           invoice_number = Guid.NewGuid().ToString(),
     //           amount = amount,
     //           item_list = itemList
     //       });
     //       this.payment = new Payment()
     //       {
     //           intent = "sale",
     //           payer = payer,
     //           transactions = transactionList,
     //           redirect_urls = redirUrls
     //       };
     //       return this.payment.Create(apiContext);
     //   }
		

		// GET: Coffees
		public async Task<IActionResult> Menu()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            var products = await _coffeeContext.Product.Take(4).ToListAsync();
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Menu.cshtml", products);
        }

       

        public IActionResult Services()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Services.cshtml");
        }

        public IActionResult Blog()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Blog.cshtml");
        }

        public IActionResult About()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/About.cshtml");
        }

        
        public IActionResult Contact()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Contact.cshtml");
        }
        public IActionResult Privacy()
        {
            return View("~/Views/Home/Pages/Privacy.cshtml");
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
