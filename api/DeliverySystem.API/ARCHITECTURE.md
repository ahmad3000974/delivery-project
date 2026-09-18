# Architecture

One ASP.NET Core app. One SQL Server database.

```
Request/Response DTO  ->  Controller (HTTP only)  ->  Service (rules + transaction)  ->  EF DbContext
```

## Current modules

- Merchants: `GET/POST api/Merchants`
- Couriers: `GET/POST api/Couriers`
- Orders: `GET/POST api/DeliveryOrders`
- Assignments: `GET/POST api/OrderAssignments`

Do not return EF entities from controllers. Map to response DTOs.

## Add a feature later

1. Add request and response classes in `Models`.
2. Add a thin controller under `Controllers` if it is a new resource.
3. Put rules and `SaveChanges` in a service under `Services`.
4. Use a database transaction when more than one write must succeed together.
5. Throw `NotFoundException`, `ConflictException`, or `BusinessRuleException` so the API returns `404` / `409` / `400`.

## Re-scaffold EF

Scaffold into a temp folder, then diff. Keep manual relationship fixes in `DeliveryDbContext`. Do not copy `OnConfiguring` connection strings back into source.
