using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Cinema.Services.Interface;
using Cinema.Domain.DomainModels;
using Cinema.Domain.DTO;
using Cinema.Domain;

namespace Cinema.Web.Controllers
{
    public class TicketsController : Controller
    {
        private readonly  ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: Tickets
        public IActionResult Index(string FromDate, string ToDate)

        {
            //var allTickets = this._ticketService.GetAllTickets();
            //return View(allTickets);
            var result = FilterData(FromDate, ToDate);
            return View(result);
        }
        //add tickets to card
        public  IActionResult AddTicketToCard(Guid? id)
        {

            var model = this._ticketService.GetShoppingCartInfo(id);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTicketToCard([Bind("TicketId","Quantity")] AddToShoppingCardDto item)
        { 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = this._ticketService.AddToShoppingCart(item, userId);

            if(result)
            {
                return RedirectToAction("Index", "Tickets");
            }
         
            return View(item);
        }


        // GET: Tickets/Details/5 
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = this._ticketService.GetDetailsForTickets(id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View();
        }



        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Title,Date,TicketPrice")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                this._ticketService.CreateNewTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = this._ticketService.GetDetailsForTickets(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,Title,Date,TicketPrice")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this._ticketService.UpdeteExistingTicket(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = this._ticketService.GetDetailsForTickets(id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            this._ticketService.DeleteTicket(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(Guid id)
        {
            return this._ticketService.GetDetailsForTickets(id) != null;
        }
        public SpecialCollection FilterData(string FromDate, string ToDate)
        {
            SpecialCollection result = new SpecialCollection();
            //get All type
            result.TypeOptions = new SelectList(
                this._ticketService.GetAllTickets()
               // .GroupBy(c => c.Genre)
                //.Select(c =>
                //    new SelectListItem()
                //    {
                //        Value = c.Key,
                //        Text = c.Key
                //    }).ToList(), "Value", "Text"
                );

            var allTickets = this._ticketService.GetAllTickets(); 


            //if (!string.IsNullOrEmpty(SearchGenre) && SearchGenre != "All")
            //    allTickets = allTickets.Where(c => c.Genre == SearchGenre).ToList();


            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                DateTime from;
                DateTime to;

                if (DateTime.TryParse(FromDate, out from) && DateTime.TryParse(ToDate, out to))
                {
                    allTickets = allTickets.Where(c => c.Date > from && c.Date < to).ToList();
                }
            }
            result.SearchResults = allTickets;
            return result;
        }

        // GET: SpecialCollections/
        public IActionResult IndexFilter(string FromDate, string ToDate)
        {
            var result = FilterData(FromDate, ToDate);
            return PartialView("_IndexFilter", result.SearchResults);
        }
    }
}
