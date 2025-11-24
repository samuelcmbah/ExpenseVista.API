using AutoMapper;
using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExpenseVista.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }


        // GET: api/categories
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var userId = GetUserId();
            var categories = await categoryService.GetAllAsync(userId);

            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var userId = GetUserId();

            try
            {
                var category = await categoryService.GetByIdAsync(id, userId);
                
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                // Handles the exception thrown by the service if the category isn't found or doesn't belong to the user
                return NotFound();
            }
        }

        // POST: api/categories
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDTO>> PostCategory([FromBody] CreateCategoryDTO createCategoryDTO)
        {
            try
            {
                var userId = GetUserId();
                var newCategory = await categoryService.CreateAsync(createCategoryDTO, userId);

                // Returns HTTP 201 Created with a link to the new resource
                return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, newCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutCategory(int id, UpdateCategoryDTO updateCategryDTO)
        {
            var userId = GetUserId();

            try
            {
                if(id != updateCategryDTO.Id || updateCategryDTO == null)
                {
                    return BadRequest();
                }
                await categoryService.UpdateAsync(id, updateCategryDTO, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();

            try
            {
                await categoryService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); 
            }
            catch (InvalidOperationException ex) 
            {
                // Explicitly returns a 400 Bad Request with the reason.
                return BadRequest(ex.Message);
            }
        }
    }
}
