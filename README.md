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

Container içindeki `5432` portu, bilgisayarda `5433` portuna açılır.
`.env.example` dosyasını `.env` olarak kopyalayın ve güçlü PostgreSQL ile ilk
Admin parolalarını yalnız bu dosyada tanımlayın. `.env` Git tarafından izlenmez. Production bağlantısı
`ConnectionStrings__DefaultConnection` environment değişkeni veya secret
provider ile verilmelidir. Docker kullanıcı bilgileri değiştirilirse backend
connection string'i de aynı bilgilerle environment üzerinden verilmelidir.

Veritabanını durdurmak için:

```powershell
docker compose down
```

Named volume kullanıldığı için `docker compose down` sonrasında veriler korunur.

## Windows'ta tek tıkla başlatma

Proje kökündeki `UygulamayiBaslat.bat` dosyasına çift tıklayın. Başlatıcı:

1. Docker Desktop'ın çalıştığını kontrol eder.
2. `docker compose up -d` ile PostgreSQL, backend ve frontend servislerini başlatır.
3. PostgreSQL health durumu ile frontend/API hazır olana kadar bekler.
4. Varsayılan tarayıcıda uygulamayı `http://localhost:8081` adresinde açar.
5. İkinci sekmede doğrudan backend API sağlık adresini açar:
   `http://localhost:8080/api/system/status?client=windows-launcher-browser`.

Bu yöntem Docker Desktop'taki mevcut Compose Start/Stop davranışını ve named
volume'ları değiştirmez. Production ortamında Swagger kapalı tutulur; bu nedenle
ikinci sekmede Swagger yerine gerçek API sağlık cevabı gösterilir. Hazır olma
kontrolü hem Nginx proxy hem de doğrudan backend üzerinden yapılır.

### Docker Desktop Start düğmesiyle tarayıcı açma

Docker Compose, güvenlik nedeniyle Windows host üzerinde kendi başına tarayıcı
başlatamaz. Bu davranışı bir kez etkinleştirmek için proje kökündeki
`DockerStartOtomatikAcmaKur.bat` dosyasına çift tıklayın. Kurulan kullanıcı
oturumu yardımcısı yalnızca `inventory-system-frontend` container'ının start
olayını izler. Daha sonraki kullanımlarda Docker Desktop'ta
`Envanter-Uygulamam` grubuna **Start** demek uygulama ve API sağlık sayfasını
varsayılan tarayıcıda açar.

Otomasyonu kaldırmak için `DockerStartOtomatikAcmaKaldir.bat` dosyasını
çalıştırın. Bu işlem container'lara veya PostgreSQL volume'larına dokunmaz.

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
