using DataAccess.IRepositories;

namespace Product_Sale_API.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class PermissionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionHandlingMiddleware> _logger;
        private readonly Dictionary<string, Dictionary<string, List<string>>> _rolePermissions;
        private readonly Dictionary<string, List<string>> _excludedUris;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public PermissionHandlingMiddleware(RequestDelegate next, ILogger<PermissionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _excludedUris = new()
            {
                    { "POST", new List<string>() { "/api/auth/login"
                    , "/api/auth/register"
                    } },
            };
            _rolePermissions = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Admin", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "GET", new List<string>() { "/api/roles"
                        ,   "/api/users"
                        } },
                        { "POST", new List<string>() { "/api/auth/authenticated"
                        ,   "/api/users"
                        } },
                        { "PUT", new List<string>() { "/api/auth/authenticated"
                        ,   "/api/users"
                        } },
                        { "PATCH", new List<string>() { } },
                        { "DELETE", new List<string>() { "/api/auth/authenticated"
                        ,   "/api/users"
                        } },
                        { "CONTROLLER", new List<string>() { } }
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="unitOfWork"></param>
        //public async Task Invoke(HttpContext context, IUOW unitOfWork)
        //{
        //    _logger.LogDebug("Entered PermissionHandlingMiddleware.Invoke");
        //    if (HasPermission(context))
        //    {
        //        _logger.LogDebug("Permission granted, moving to next middleware");
        //        await _next(context);
        //    }
        //    else
        //    {
        //        _logger.LogDebug("Permission denied, returning forbidden");
        //        await Authentication.HandleForbiddenRequest(context);
        //    }
        //}

        private bool HasPermission(HttpContext context)
        {
            string requestUri = context.Request.Path.Value!;
            string requestMethod = context.Request.Method;
            _logger.LogDebug("Checking permission for request: {RequestMethod} {RequestUri}", requestMethod, requestUri);

            // Skip permission checks for non-API endpoints.
            if (!requestUri.StartsWith("/api/"))
            {
                _logger.LogDebug("Request is not for /api/, skipping permission check.");
                return true;
            }

            // Check if the request URI is excluded for the given method.
            if (_excludedUris.TryGetValue(requestMethod, out var allowedUris))
            {
                if (allowedUris.Any(uri => requestUri.StartsWith(uri, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogDebug("Request {RequestUri} is whitelisted for method {RequestMethod}.", requestUri, requestMethod);
                    return true;
                }
            }

            // Check controller-wide exclusions.
            if (_excludedUris.TryGetValue("CONTROLLER", out var controllerUris))
            {
                if (controllerUris.Any(controllerUri => requestUri.StartsWith(controllerUri, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogDebug("Request {RequestUri} is controller-wide whitelisted.", requestUri);
                    return true;
                }
            }

            try
            {
                // Retrieve the user role directly from HttpContext.User using ClaimTypes.Role
                var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    _logger.LogDebug("No role found in user claims.");
                    return false;
                }
                _logger.LogDebug("User role extracted from claims: {UserRole}", userRole);

                // If the user is admin, allow all endpoints.
                if (userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("User is admin. Allowing access.");
                    return true;
                }

                // Check role-based permissions.
                if (_rolePermissions.TryGetValue(userRole, out var methodPermissions))
                {
                    if (methodPermissions.TryGetValue(requestMethod, out var allowedUrisForRole))
                    {
                        _logger.LogDebug("Allowed URIs for role {UserRole} and method {RequestMethod}: {AllowedUris}",
                            userRole, requestMethod, string.Join(", ", allowedUrisForRole));

                        if (allowedUrisForRole.Any(uri => requestUri.StartsWith(uri, StringComparison.OrdinalIgnoreCase)))
                        {
                            _logger.LogDebug("Request {RequestUri} is allowed for role {UserRole} and method {RequestMethod}.", requestUri, userRole, requestMethod);
                            return true;
                        }
                        else
                        {
                            _logger.LogDebug("No matching allowed URI for request {RequestUri} under role {UserRole}.", requestUri, userRole);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No method permissions defined for method {RequestMethod} for role {UserRole}.", requestMethod, userRole);
                    }
                }
                else
                {
                    _logger.LogDebug("No permissions defined for role {UserRole}.", userRole);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking permissions for request {RequestMethod} {RequestUri}", requestMethod, requestUri);
                return false;
            }
        }
    }
}
