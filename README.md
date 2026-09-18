# Delivery Operations API

Backend REST API for a delivery company: orders, courier assignment, cash collection, courier remittances, commissions, merchant settlements, payouts, expenses, and cash movements.

Built with C#, ASP.NET Core 8, Entity Framework Core, and SQL Server.

## Architecture

```
Request DTO -> Controller (HTTP only) -> Service (business rules + transaction) -> EF Core -> SQL Server
```

- Controllers stay thin: no business rules, no database access.
- Services own the rules and wrap multi-table writes in a database transaction.
- Entities are never returned to clients; every response is a DTO.
- Domain errors are thrown as typed exceptions and translated to `400`, `404`, and `409` with ProblemDetails.

## Modules

| Area | Endpoints |
| --- | --- |
| Merchants, merchant phones | `api/Merchants`, `api/MerchantPhones` |
| Couriers | `api/Couriers` |
| Orders, status transitions | `api/DeliveryOrders` |
| Assignments, delivery attempts | `api/OrderAssignments`, `api/DeliveryAttempts` |
| Cash collection | `api/Collections` |
| Courier remittances, commission payments | `api/CourierRemittances`, `api/CourierCommissionPayments` |
| Merchant settlements and payouts | `api/MerchantSettlements`, `api/MerchantPayouts` |
| Expenses, adjustments | `api/Expenses`, `api/Adjustments` |
| Company settings | `api/CompanySettings` |

## Business rules enforced

- An order moves through a fixed status flow; invalid transitions are rejected.
- A delivery attempt is only allowed while the order is out for delivery and the assignment is open.
- Collecting cash on a delivered order also creates the courier commission and the merchant accrual, in one transaction.
- A remittance allocation cannot exceed the remaining amount of a collection, and the courier must match.
- A commission payment cannot exceed the remaining commission.
- A settlement starts as a draft; accruals must belong to the merchant and cannot already sit on an open settlement.
- A merchant payout requires an approved settlement and cannot exceed the remaining settlement total.
- Every cash payment records exactly one cash movement with the correct direction.
- Idempotency keys and payment references are unique, so a retried request cannot double-pay.

## Running locally

1. Create the SQL Server database and set the connection string `DeliveryDB` in `appsettings.json`.
2. Run the API:

```bash
cd api/DeliverySystem.API
dotnet run
```

3. Open Swagger at `http://localhost:5274/swagger`.

## Status

Backend feature work is complete and manually tested end to end through the API. Authentication and the web client are the next milestones.
