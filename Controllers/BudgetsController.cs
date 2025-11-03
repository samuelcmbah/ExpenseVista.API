using ExpenseVista.API.DTOs.Budget;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExpenseVista.API.Controllers
{
    [Route("api/budgets")]
    [ApiController]
    [Authorize]
    public class BudgetsController : BaseController
    {
        private readonly IBudgetService budgetService;
        private readonly ILogger<BudgetsController> logger;

        public BudgetsController(IBudgetService budgetService, ILogger<BudgetsController> logger)
        {
            this.budgetService = budgetService;
            this.logger = logger;
        }

        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetStatusDTO>> GetBudgetStatus([FromQuery] DateTime month)
        {
            var userId = GetUserId();
            try
            {
                var status = await budgetService.GetBudgetStatusForMonthAsync(month, userId);
                return Ok(status);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
                
            }

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetAllBudgets()
        {
            var userId = GetUserId() ;
            try
            {
                var budgets = await budgetService.GetAllBudgetsAsync(userId);
                return Ok(budgets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error geting all Budgets");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BudgetDTO>> CreateBudget([FromBody] BudgetCreateDTO budgetCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            try
            {
                var newBudget = await budgetService.CreateAsync(budgetCreateDTO, userId);
                return CreatedAtAction(nameof(GetBudgetStatus),
                    new { month = newBudget.BudgetMonth.ToString("yyyy-MM-dd") }, newBudget);
            }
            catch(InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetUpdateDTO>> UpdateBudget(int id, [FromBody] BudgetUpdateDTO budgetUpdateDTO)
        {
            if (id != budgetUpdateDTO.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            try
            {
                await budgetService.UpdateAsync(id, budgetUpdateDTO, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var userId = GetUserId();
            try
            {
                await budgetService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Budget with ID {id} not found or unauthorized.");
            }
        }
    }
}
