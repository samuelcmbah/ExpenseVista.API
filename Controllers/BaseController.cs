using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseVista.API.Controllers
{
    public class BaseController : ControllerBase
    {

        public BaseController()
        {
            
        }

        /// <summary>
        /// Helper method to safely extract the authenticated User ID from the JWT claims.
        /// </summary>
        protected string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                // This should not happen on an [Authorize] endpoint, but serves as a safety check.
                throw new UnauthorizedAccessException("User ID claim not found in token.");
            }
            return userId;
        }
    }
}
