using Microsoft.AspNetCore.Mvc;
using DeliverySystem.API.Models ;
using DeliverySystem.API.Services;
namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly CollectionService _collectionService;

    public CollectionsController(CollectionService collectionService)
    {
        _collectionService = collectionService;
    }
    [HttpGet]
 public async Task<ActionResult<List<CollectionResponse>>> GetAll(CancellationToken cancellationToken)
{
    return await _collectionService.ListAsync(cancellationToken);
}
   [HttpPost]
public async Task<ActionResult<CollectionResponse>> Create(
    CreateCollectionRequest request,
    CancellationToken cancellationToken)
{
    return await _collectionService.CreateAsync(request, cancellationToken);
}

    
}
