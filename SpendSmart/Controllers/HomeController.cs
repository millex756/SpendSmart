using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace SpendSmart.Controllers
{
    public class HomeController : Controller
    {
        private readonly SpendSmartDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        } 

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Expenses()
        {
            // Use AsNoTracking for read-only queries to reduce change-tracking overhead and memory usage
            var allExpenses = await _context.Expenses
                .AsNoTracking()
                .ToListAsync();

            // Let EF perform the aggregation server-side to avoid loading extra data into memory
            var totalExpenses = await _context.Expenses
                .AsNoTracking()
                .SumAsync(expense => expense.Value);

            ViewBag.Expenses = totalExpenses;

            return View(allExpenses);
        }

        public async Task<IActionResult> CreateEditExpense(int? id)
        {
            if (id != null)
            {
                // Read-only fetch - AsNoTracking
                var expenseInDb = await _context.Expenses
                    .AsNoTracking()
                    .SingleOrDefaultAsync(expense => expense.Id == id);
                return View(expenseInDb);
            }

            return View();
        }

        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expenseInDb = await _context.Expenses.FindAsync(id);

            if (expenseInDb == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expenseInDb);
            await _context.SaveChangesAsync();

            return RedirectToAction("Expenses");
        }

        public async Task<IActionResult> SubmitExpenseForm(Expense expense)
        {
            if (expense.Id == 0) {
                _context.Expenses.Add(expense);
            }
            else
            {
                // Attach and mark modified to avoid extra database roundtrips if the entity is detached
                _context.Expenses.Update(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Expenses");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
