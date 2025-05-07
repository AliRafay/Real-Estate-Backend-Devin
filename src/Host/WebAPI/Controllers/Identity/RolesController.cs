using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Identity.Roles;
using Demo.WebApi.Infrastructure.Common.Extensions;

namespace Demo.WebApi.Host.Controllers.Identity;

public class RolesController : VersionNeutralApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService) => _roleService = roleService;

    [HttpGet]
    [MustHavePermission(AppAction.View, AppResource.Roles)]
    [OpenApiOperation("Get a list of all roles.", "")]
    public async Task<HttpResponseDto<List<RoleDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        return (await _roleService.GetListAsync(cancellationToken)).ToInformationResponse();
    }

    [HttpGet("{id}")]
    [MustHavePermission(AppAction.View, AppResource.Roles)]
    [OpenApiOperation("Get role details.", "")]
    public async Task<HttpResponseDto<RoleDto>> GetByIdAsync(string id)
    {
        return (await _roleService.GetByIdAsync(id)).ToInformationResponse();
    }

    [HttpGet("{id}/permissions")]
    [MustHavePermission(AppAction.View, AppResource.RoleClaims)]
    [OpenApiOperation("Get role details with its permissions.", "")]
    public async Task<HttpResponseDto<RoleDto>> GetByIdWithPermissionsAsync(string id, CancellationToken cancellationToken)
    {
        return (await _roleService.GetByIdWithPermissionsAsync(id, cancellationToken)).ToInformationResponse();
    }

    [HttpPut("{id}/permissions")]
    [MustHavePermission(AppAction.Update, AppResource.RoleClaims)]
    [OpenApiOperation("Update a role's permissions.", "")]
    public async Task<HttpResponseDto<string>> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RoleId)
            throw new BadRequestException("Invalid Request");

        var msg = await _roleService.UpdatePermissionsAsync(request, cancellationToken);
        return HttpResponseExtension.InformationResponse(msg);
    }

    [HttpPost]
    [MustHavePermission(AppAction.Create, AppResource.Roles)]
    [OpenApiOperation("Create or update a role.", "")]
    public async Task<HttpResponseDto<string>> RegisterRoleAsync(CreateOrUpdateRoleRequest request)
    {
        var msg = await _roleService.CreateOrUpdateAsync(request);
        return HttpResponseExtension.InformationResponse(msg);
    }

    [HttpDelete("{id}")]
    [MustHavePermission(AppAction.Delete, AppResource.Roles)]
    [OpenApiOperation("Delete a role.", "")]
    public async Task<HttpResponseDto<string>> DeleteAsync(string id)
    {
        var msg = await _roleService.DeleteAsync(id);
        return HttpResponseExtension.InformationResponse(msg);
    }
}