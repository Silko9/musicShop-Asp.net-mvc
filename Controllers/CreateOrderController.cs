﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using musicShop.Data;
using musicShop.Models;
using Microsoft.AspNetCore.Identity;
using musicShop.Areas.Identity.Data;
using System.Data;

namespace musicShop.Controllers
{
    [Authorize]
    public class CreateOrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly musicShopContext _contextUser;

        public CreateOrderController(AppDbContext context, musicShopContext contextUser)
        {
            _context = context;
            _contextUser = contextUser;
        }

        public async Task<IActionResult> Index()
        {
            var user = User.Identity.Name;
            Client client;
            if (_context.Clients.Where(p => p.Email == user).Count() >= 1)
            {
                client = await _context.Clients.Where(p => p.Email == user).FirstAsync();
                if (client.Email != null)
                    return await ChooseAddress(client.Id, false);
            }
              return View(await _context.Clients.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Index(int clientId)
        {
            Client client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                return NotFound();
            ViewBag.ClientId = clientId;
            ViewBag.Client = client;
            ViewBag.DateError = false;
            return View("ChooseAddress");
        }

        public async Task<IActionResult> ChooseAddress(int id, bool dateError)
        {
            Client client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound();
            ViewBag.ClientId = id;
            ViewBag.Client = client;
            ViewBag.DateError = dateError;
            return View("ChooseAddress");
        }

        [HttpPost]
        public async Task<IActionResult> ChooseAddress(int clientId, string address, string date)
        {
            DateTime selectedDate;
            if (!DateTime.TryParse(date, out selectedDate))
            {
                return RedirectToAction("ChooseAddress", new { id = clientId, dateError = true });
            }

            if (selectedDate.Date < DateTime.Today)
            {
                return RedirectToAction("ChooseAddress", new { id = clientId, dateError = true });
            }

            Client client = await _context.Clients.FindAsync(clientId);
            ViewBag.ClientId = clientId;
            ViewBag.Address = address;
            ViewBag.Date = date;
            ViewBag.Client = $"{client.Name} {client.Surname} {client.Patronymic}";
            var appDbContext = _context.Records.Include(p => p.Composition);
            return View("ChooseRecords", await appDbContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ChooseRecords(List<RecordSelection> records, int clientId, string address, string date)
        {
            List<RecordSelection> selectedRecords = records.Where(r => r.Count > 0).ToList();
            ICollection<Logging> loggings = new List<Logging>();

            DateTime dateDelivery = DateTime.ParseExact(date, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);

            Order order = new Order
            {
                Loggings = loggings,
                Client = await _context.Clients.FindAsync(clientId),
                ClientId = clientId,
                Address = address,
                DateDelivery = dateDelivery,
                DateCreate = DateTime.Today.Date
            };

            if (selectedRecords.Count == 0) return View("ViewOrder", order);

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var record in selectedRecords)
            {
                Logging logging = new Logging
                {
                    RecordId = record.Id,
                    Record = await _context.Records.FindAsync(record.Id),
                    Amount = record.Count,
                    TypeLoggingId = Const.ORDER_ID,
                    Operation = order.Id
                };
                logging.Record.Amount = logging.Record.Amount - record.Count;
                _context.Records.Update(logging.Record);
                _context.Loggings.Add(logging);
                loggings.Add(logging);
                _context.SaveChanges();
            }

            return View("ViewOrder", order);
        }


        public class RecordSelection
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public decimal RetailPrice { get; set; }
            public string CompositionName { get; set; }
            public int Count { get; set; }
        }
    }
}
