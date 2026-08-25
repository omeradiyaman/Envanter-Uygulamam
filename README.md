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
- Docker Desktop

## PostgreSQL

PostgreSQL geliştirme veritabanı Docker Compose ile çalışır:

```powershell
docker compose up -d postgres
docker compose ps
```

Container içindeki `5432` portu, bilgisayarda `5433` portuna açılır. Varsayılan
development bağlantısı:

```text
Host=localhost;Port=5433;Database=inventory_system;Username=inventory_app;Password=inventory_dev_password
```

Development değerlerini değiştirmek için `.env.example` dosyasını `.env`
olarak kopyalayın. `.env` Git tarafından izlenmez. Production bağlantısı
`ConnectionStrings__DefaultConnection` environment değişkeni veya secret
provider ile verilmelidir. Docker kullanıcı bilgileri değiştirilirse backend
connection string'i de aynı bilgilerle environment üzerinden verilmelidir.

Veritabanını durdurmak için:

```powershell
docker compose down
```

Named volume kullanıldığı için `docker compose down` sonrasında veriler korunur.

## Backend

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
