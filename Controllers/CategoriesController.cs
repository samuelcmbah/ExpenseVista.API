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
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        // Helper to get the authenticated user's ID
        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID claim not found in token.");

            return userId;
        }


        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var userId = GetUserId();
            var categories = await categoryService.GetAllAsync(userId);

            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
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
        public async Task<ActionResult<CategoryDTO>> PostCategory([FromBody] CreateCategoryDTO createCategoryDTO)
        {
            var userId = GetUserId();
            var newCategory = await categoryService.CreateAsync(createCategoryDTO, userId);

            // Returns HTTP 201 Created with a link to the new resource
            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, newCategory);
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
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
