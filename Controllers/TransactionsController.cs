using ExpenseVista.API.DTOs.Pagination;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

       

        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<TransactionDTO>>> GetAllTransactions([FromQuery] FilterPagedTransactionDTO filterDTO)
        {
            var userId = GetUserId();
            var transactions = await transactionService.GetAllAsync(userId, filterDTO);
            return Ok(transactions);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransactionDTO>> GetTransactionById(int id)
        {
            var userId = GetUserId();
            try
            {
                var transaction = await transactionService.GetByIdAsync(id, userId);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                // KeyNotFoundException is thrown by the service if the transaction is not found or doesn't belong to the user
                return NotFound($"Transaction with ID {id} not found or unauthorized.");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransactionDTO>> CreateTransaction([FromBody] TransactionCreateDTO transactionCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var newTransaction = await transactionService.CreateAsync(transactionCreateDTO, userId);

            // Returns 201 Created with the location of the new resource
            return CreatedAtAction(nameof(GetTransactionById), new { id = newTransaction.Id }, newTransaction);
        }

     
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] TransactionUpdateDTO transactionUpdateDTO)
        {
            if (id != transactionUpdateDTO.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            try
            {
                await transactionService.UpdateAsync(id, transactionUpdateDTO, userId);
                return NoContent(); // Successful update returns 204 No Content
            }
            catch (Exception ex)
            {
                return NotFound($"Transaction with ID {id} not found or unauthorized. message: {ex.Message}");
            }
        }

       
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = GetUserId();
            try
            {
                await transactionService.DeleteAsync(id, userId);
                return NoContent(); // Successful deletion returns 204 No Content
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Transaction with ID {id} not found or unauthorized.");
            }
        }
    }
}