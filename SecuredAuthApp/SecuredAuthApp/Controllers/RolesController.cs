using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SecuredAuthApp.DTOs;
using SecuredAuthApp.Models;

namespace SecuredAuthApp.Controllers
{
    [Authorize(Roles="admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;    
        }
        //implement create role endpoint
        [HttpPost("CreateRole")]
        public async Task<ActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (string.IsNullOrEmpty(createRoleDto.RoleName))
            {
                return BadRequest("Role name is required");
            }
            var roleExist= await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
            if (roleExist) {
                return BadRequest("Role Already Exits");
            }
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));
            if (roleResult.Succeeded)
            {
                return Ok(new { Message = "Role Created successfully"});

            }
            return BadRequest("Role Creation Failed");
        }
        [HttpGet("GetRoles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDtos>>> GetRoles()
        {
            //lists of users with total user count
            var roles = await _roleManager.Roles.Select( r => new RoleResponseDtos
            {
                Id=r.Id,
                Name=r.Name,
                TotalUsers=_userManager.GetUsersInRoleAsync(r.Name!).Result.Count
            }).ToListAsync();

            return Ok(roles);
        }
        //implement delete role endpoint
        [HttpDelete("{id}")]
        public async Task<IActionResult>DeleteRole(string id)
        {
            var role= await _roleManager.FindByIdAsync(id);
            if(role is null)
            {
                return NotFound("Role not Found");
            }
            var result= await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role Deleted Successfully" });
            }
            return BadRequest("Role Deletion Failed");
        }
        [HttpPost("Assign")]
        public async Task<IActionResult> AssignRole([FromBody]RoleAssignDto roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(roleAssignDto.UserId);
            if (user is null)
            {
                return NotFound("User not found");
            }
            var role = await _roleManager.FindByIdAsync(roleAssignDto.RoleId);
            if (role is null)
            {
                return NotFound("Role not found");
            }
            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (result.Succeeded)
            {
                return Ok(new { message = "Role Assigned Successfully" });
            }
            var errors = result.Errors.FirstOrDefault();
            return BadRequest(errors!.Description);

        }
    }  
}
