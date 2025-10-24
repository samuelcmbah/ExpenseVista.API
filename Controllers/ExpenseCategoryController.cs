using AutoMapper;
using ExpenseVista.API.DTOs.ExpenseCategory;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/expenseCategory")]
    [ApiController]
    public class ExpenseCategoryController : ControllerBase
    {
        private readonly ExpenseCategoryService expenseCategoryService;
        private readonly IMapper mapper;

        public ExpenseCategoryController(ExpenseCategoryService expenseCategoryService, IMapper mapper)
        {
            this.expenseCategoryService = expenseCategoryService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseCategoryDTO>>> GetAll()
        {
            var expenseCategories = await expenseCategoryService.GetAllAsync();
            return Ok(mapper.Map<IEnumerable<ExpenseCategoryDTO>>(expenseCategories));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExpenseCategoryDTO>> GetById(int id)
        {
            var expenseCategory = await expenseCategoryService.GetByIdAsync(id);
            if (expenseCategory == null)
            {
                return NotFound();
            }

            var expenseCategoryDTO = mapper.Map<ExpenseCategoryDTO>(expenseCategory);
            return Ok(expenseCategoryDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]ExpenseCategoryCreateDTO expenseCategoryCreateDTO)
        {
            if(expenseCategoryCreateDTO == null)
            {
                return BadRequest();
            }
            var expenseCategory = mapper.Map<ExpenseCategory>(expenseCategoryCreateDTO);
            if (expenseCategory == null)
            {
                return BadRequest();
            }
            var created = await expenseCategoryService.CreateAsync(expenseCategory);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ExpenseCategoryUpdateDTO expenseCategoryUpdateDTO)
        {
            if (id != expenseCategoryUpdateDTO.Id || expenseCategoryUpdateDTO == null)
            {
                return BadRequest();
            }
            var expenseCategory = await expenseCategoryService.GetByIdAsync(id);
            if (expenseCategory == null)
            {
                return BadRequest();
            }
           
            await expenseCategoryService.UpdateAsync(mapper.Map(expenseCategoryUpdateDTO, expenseCategory)!);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var expenseCategory = await expenseCategoryService.GetByIdAsync(id);
            if(expenseCategory == null)
            {
                return BadRequest();
            }
            await expenseCategoryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
