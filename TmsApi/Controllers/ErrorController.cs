using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error()
    {
        return Problem(
            title: "An unexpected error occurred.",
            statusCode: 500
        );
    }
}
