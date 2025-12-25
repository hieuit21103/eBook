using Application.Interfaces;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.DTOs;

namespace Documents.Test.Controllers.Documents;

public abstract class DocumentControllerBase
{
    protected readonly IDocumentService _documentService;
    protected readonly DocumentController _documentController;

    protected DocumentControllerBase()
    {
        _documentService = Substitute.For<IDocumentService>();
        _documentController = new DocumentController(_documentService);
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        }));

        _documentController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}

