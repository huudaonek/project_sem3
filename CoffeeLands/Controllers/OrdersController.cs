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


namespace CoffeeLands.Controllers
{
	public class OrdersController : Controller
	{
		private readonly CoffeeLandsContext _context;
		private readonly PaypalClient _paypalClient;

		public OrdersController(CoffeeLandsContext context, PaypalClient paypalClient)
		{
			_context = context;
			_paypalClient = paypalClient;
		}

		// GET: Orders
		public async Task<IActionResult> Index(
	string sortOrder,
	string currentFilter,
	string searchString,
	int? pageNumber)
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

		// GET: Orders/Details/5
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

		// GET: Orders/Create
		public IActionResult Create()
		{
			ViewData["UserID"] = new SelectList(_context.User, "Id", "Email");
			return View();
		}

		#region Paypal payment
		[HttpPost("/Orders/create-paypal-order")]

		public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
		{
			// Thong tin don hang gui qua paypal
			var productListJson = HttpContext.Session.GetString("Cart");
			//ViewBag.Cart = HttpContext.Session.GetString("Cart");
			//if (!string.IsNullOrEmpty(productListJson))
			//{
			decimal subTotal = 0;
			var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
			ViewBag.Cart = productList;
			foreach (ProductCart productCart in productList)
			{
				subTotal += productCart.CartProduct.Price * productCart.Qty;
			}

			var grand_Total = subTotal * 1.1m;
			var tongtien = grand_Total.ToString("0.00");
			var donviTienTe = "USD";
			var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

			try
			{
				var response = await _paypalClient.CreateOrder(tongtien, donviTienTe, maDonHangThamChieu);
				return Ok(response);
			}
			catch (Exception ex)
			{
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
			//}

		}

		[HttpPost("/Orders/capture-paypal-order")]
		public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
		{
			try
			{
				var response = await _paypalClient.CaptureOrder(orderID);

				//luu db don hang cua minhf
				return Ok(response);
			}
			catch (Exception ex)
			{
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
		}
		#endregion


		//public async Task<IActionResult> CreateAndCapturePaypalOrder(int orderId)
		//{
		//	// Lấy thông tin đơn hàng từ cơ sở dữ liệu
		//	var order = await _context.OrderProduct
		//		.FirstOrDefaultAsync(m => m.Id == orderId);

		//	if (order == null)
		//	{
		//		return NotFound();
		//	}

		//	// Gọi hàm tạo và capture đơn hàng trong PaypalClient
		//	var paypalResponse = await _paypalClient.CreateAndCaptureOrder(order);

		//	// Xử lý kết quả từ PayPal (có thể cần kiểm tra trạng thái và xử lý các tình huống khác)

		//	// Chuyển hướng hoặc trả về thông tin thanh toán thành công
		//	return RedirectToAction("Success");
		//}




		public IActionResult Success()
		{
			return View();
		}

		

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
			//ViewBag.Cart = HttpContext.Session.GetString("Cart");
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

			ViewBag.PaypalClientId = _paypalClient.ClientId;

			return View();
		}

		// POST: Orders/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceOrder([Bind("Name,Email,Tel,Address,Status,Grand_total,Shipping_method,Payment_method,UserID")] OrderProduct order, decimal grandTotal, string shipping_method, string payment_method)
		{
			var checkUser = HttpContext.Session.GetString("UserSession");
			var user = await _context.User
				.Include(pc => pc.ProductCarts)
				.FirstOrDefaultAsync(m => m.Name == checkUser);

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


					// Thực hiện lưu đơn hàng vào database
					order.UserID = user.Id;
						_context.Add(order);
						await _context.SaveChangesAsync();

						var productListJson = HttpContext.Session.GetString("Cart");

						if (!string.IsNullOrEmpty(productListJson))
						{
							// Thực hiện lưu danh sách chi tiết đơn hàng vào database
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
                            _context.Add(orderDetail);
                            //orderDetails.Add(orderDetail);
                        }

						//_context.AddRange(orderDetails);
						
							await _context.SaveChangesAsync();

							// Lưu danh sách OrderProduct vào ViewBag
							ViewBag.OrderProductList = orderDetails;

							// Xóa tất cả sản phẩm khỏi giỏ hàng
							productList.Clear();

							// Cập nhật Session với giỏ hàng sau khi đã xóa
							string updatedProductListJson = JsonConvert.SerializeObject(productList);
							HttpContext.Session.SetString("Cart", updatedProductListJson);
							HttpContext.Session.SetString("CartNumber", productList.Count.ToString());

						// Chuyển hướng sang trang thanh toán PayPal với orderId
						//var orderId = order.Id;
						if(order.Payment_method == "Payment_On_Delivery")
						{
                            var userSession = HttpContext.Session.GetString("UserSession");
                            if (string.IsNullOrEmpty(userSession))
                            {
                                return RedirectToAction("Login", "Users");
                            }
                            else
                            {
                                ViewBag.MySession = userSession.ToString();
                            }
                            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");

                            var orderSuccess = await _context.OrderProduct
				.Include(o => o.User)
				.Include(od => od.OrderDetails)
				.ThenInclude(p => p.Product)
				.FirstOrDefaultAsync(m => m.Id == order.Id);
							return View("~/Views/Orders/Thankyou.cshtml", orderSuccess);

						}
						//		else
						//		{
						//			var orderUpdate = await _context.OrderProduct
						//.Include(o => o.User)
						//.Include(od => od.OrderDetails)
						//.ThenInclude(p => p.Product)
						//.FirstOrDefaultAsync(m => m.Id == order.Id);
						//			if (orderUpdate == null)
						//			{
						//				return NotFound();
						//			}
						//			orderUpdate.Is_paid = true;
						//			orderUpdate.Status = OrderStatus.CONFIRMED;
						//			_context.Update(orderUpdate);
						//			await _context.SaveChangesAsync();
						//			return View("~/Views/Orders/Thankyou.cshtml", orderUpdate);
						//		}
						return View("~/Views/Orders/Payment.cshtml", order);
						//return RedirectToAction("Payment", order.Id);
					}
					
				}
				catch (DbUpdateException)
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			ViewData["UserID"] = new SelectList(_context.User, "Id", "Email", order.UserID);
			return View("~/Views/Orders/Checkout.cshtml", order);
		}




		//public async Task<IActionResult> Payment(int id)
		//{
		//	if(id == null)
		//	{
		//		return View("~/Views/Orders/Checkout.cshtml");
		//	}
  //          var order = await _context.OrderProduct
  //              .Include(o => o.User)
  //              .FirstOrDefaultAsync(m => m.Id == id);

  //          if (order == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(order);
		//}



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
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

            return View("~/Views/Orders/Payment.cshtml");
        }

		//public async Task<IActionResult> PayLater(int? id)
		//{

		//	var orderUpdate = await _context.OrderProduct
		//		.Include(o => o.User)
		//		.Include(od => od.OrderDetails)
		//		.ThenInclude(p => p.Product)
		//		.FirstOrDefaultAsync(m => m.Id == id);

		//	if (orderUpdate == null)
		//	{
		//		return NotFound();
		//	}


		//	return View("~/Views/Orders/Thankyou.cshtml", orderUpdate);

		//}


		//public async Task<IActionResult> Thankyou(in? id)
		//{
			
		//	//var orderProduct = await _context.OrderProduct
		//	//   .Include(od => od.OrderDetails)
		//	//   .FirstOrDefaultAsync(m => m.Id == id);
		//	//if (orderProduct == null)
		//	//{
		//	//	return NotFound();
		//	//}

		//	return View(orderProduct);
		//}

		// GET: Orders/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.OrderProduct.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}
			ViewData["UserID"] = new SelectList(_context.User, "Id", "Email", order.UserID);
			return View(order);
		}

		// POST: Orders/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Tel,Address,Status,Grand_total,Shipping_method,Payment_method,UserID")] OrderProduct order)
		{
			if (id != order.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(order);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!OrderExists(order.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["UserID"] = new SelectList(_context.User, "Id", "Email", order.UserID);
			return View(order);
		}

		// GET: Orders/Delete/5
		public async Task<IActionResult> Delete(int? id)
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

		// POST: Orders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var order = await _context.OrderProduct.FindAsync(id);
			if (order != null)
			{
				_context.OrderProduct.Remove(order);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool OrderExists(int id)
		{
			return _context.OrderProduct.Any(e => e.Id == id);
		}
	}
}
