using AutoMapper;
using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.DTOs.ExpenseCategory;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService categoryService;
        private readonly IMapper mapper;

        public CategoriesController(CategoryService categoryService, IMapper mapper)
        {
            this.categoryService = categoryService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
        {
            var categories = await categoryService.GetAllAsync();
            return Ok(mapper.Map<IEnumerable<CategoryDTO>>(categories));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDTO>> GetById(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryDTO = mapper.Map<CategoryDTO>(category);
            return Ok(categoryDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateCategoryDTO createCategoryDTO)
        {
            if(createCategoryDTO == null)
            {
                return BadRequest();
            }
            var category = mapper.Map<Category>(createCategoryDTO);
            if (category == null)
            {
                return BadRequest();
            }
            var created = await categoryService.CreateAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDTO updateCategoryDTO)
        {
            if (id != updateCategoryDTO.Id || updateCategoryDTO == null)
            {
                return BadRequest();
            }
            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return BadRequest();
            }
           
            await categoryService.UpdateAsync(mapper.Map(updateCategoryDTO, category)!);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if(category == null)
            {
                return BadRequest();
            }
            await categoryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
