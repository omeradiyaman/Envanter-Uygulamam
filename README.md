# InventorySystem

Envanter yönetim uygulamasının ASP.NET Core ve Vue tabanlı yeni altyapısıdır.
Eski Flask uygulaması yalnızca iş alanı ve tasarım referansı olarak kullanılmıştır.

## Mimari

```text
backend/
  InventorySystem.Domain
  InventorySystem.Application
  InventorySystem.Infrastructure
  InventorySystem.API
frontend/
  Vue 3 + Vite + TypeScript
```

Backend; MediatR, CQRS, FluentValidation, Entity Framework Core ve PostgreSQL
kullanır. Frontend, REST API ile bağımsız olarak haberleşir.

## Gereksinimler

- .NET SDK 10.0.400
- Node.js 20.19 veya üzeri
- PostgreSQL

## Backend

Varsayılan geliştirme bağlantısı:

```text
Host=localhost;Port=5432;Database=inventory_system;Username=postgres;Password=postgres
```

Gerçek ortam değerini `ConnectionStrings__DefaultConnection` environment
değişkeniyle geçebilirsiniz.

```powershell
dotnet tool restore
dotnet restore InventorySystem.sln
dotnet tool run dotnet-ef database update `
  --project backend/InventorySystem.Infrastructure/InventorySystem.Infrastructure.csproj `
  --startup-project backend/InventorySystem.API/InventorySystem.API.csproj
dotnet run --project backend/InventorySystem.API/InventorySystem.API.csproj `
  --launch-profile https
```

API adresi `https://localhost:7001`, örnek endpoint ise
`GET /api/system/status?client=frontend` olur.

## Frontend

```powershell
cd frontend
npm install
npm run dev
```

Frontend `http://localhost:5173` adresinde açılır. Farklı bir backend adresi
için `.env.example` dosyasını `.env.local` olarak kopyalayıp
`VITE_API_BASE_URL` değerini değiştirin.

## Doğrulama

```powershell
dotnet build InventorySystem.sln --configuration Release
cd frontend
npm run build
```
