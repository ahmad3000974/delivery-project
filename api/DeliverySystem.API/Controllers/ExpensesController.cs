using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(ExpenseService expenses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await expenses.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        return await expenses.CreateAsync(request, cancellationToken);
    }
}
