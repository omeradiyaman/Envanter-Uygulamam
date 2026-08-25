param(
    [Parameter(Mandatory = $true)]
    [string] $Password,
    [string] $ApiBaseUrl = 'http://localhost:8080',
    [string] $Username = 'admin'
)

$ErrorActionPreference = 'Stop'

function Invoke-InventoryApi {
    param(
        [Parameter(Mandatory = $true)] [string] $Path,
        [ValidateSet('Get', 'Post')] [string] $Method = 'Get',
        [object] $Body
    )

    $parameters = @{
        Uri        = "$ApiBaseUrl$Path"
        Method     = $Method
        WebSession = $script:Session
    }

    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = $Body | ConvertTo-Json -Depth 6 -Compress
    }

    Invoke-RestMethod @parameters
}

$script:Session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
Invoke-InventoryApi -Path '/api/auth/login' -Method Post -Body @{
    username = $Username
    password = $Password
} | Out-Null

$personnelSeed = @(
    @{ sicilNo = 'DEMO-1001'; ad = 'Ahmet';  soyad = 'Yılmaz'; departman = 'Bilgi İşlem';       pozisyon = 'Sistem Uzmanı';             zimmetNo = 'DEMO-ZM-001'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1002'; ad = 'Elif';   soyad = 'Kaya';   departman = 'Muhasebe';          pozisyon = 'Muhasebe Uzmanı';           zimmetNo = 'DEMO-ZM-002'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1003'; ad = 'Mehmet'; soyad = 'Demir';  departman = 'Üretim';            pozisyon = 'Üretim Planlama Uzmanı';    zimmetNo = 'DEMO-ZM-003'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1004'; ad = 'Zeynep'; soyad = 'Şahin';  departman = 'İnsan Kaynakları';  pozisyon = 'İK Uzmanı';                 zimmetNo = 'DEMO-ZM-004'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1005'; ad = 'Can';    soyad = 'Aydın';  departman = 'Satış';              pozisyon = 'Satış Temsilcisi';          zimmetNo = 'DEMO-ZM-005'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1006'; ad = 'Selin';  soyad = 'Arslan'; departman = 'Satın Alma';         pozisyon = 'Satın Alma Uzmanı';         zimmetNo = 'DEMO-ZM-006'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1007'; ad = 'Burak';  soyad = 'Koç';    departman = 'Lojistik';           pozisyon = 'Lojistik Sorumlusu';        zimmetNo = 'DEMO-ZM-007'; aktifMi = $true },
    @{ sicilNo = 'DEMO-1008'; ad = 'Derya';  soyad = 'Çelik';  departman = 'Bilgi İşlem';        pozisyon = 'Destek Uzmanı';             zimmetNo = 'DEMO-ZM-008'; aktifMi = $false }
)

$categoryIds = @{
    Laptop   = '11111111-1111-1111-1111-111111111111'
    Monitor  = '22222222-2222-2222-2222-222222222222'
    Desktop  = '33333333-3333-3333-3333-333333333333'
    Printer  = '44444444-4444-4444-4444-444444444444'
    Tablet   = '55555555-5555-5555-5555-555555555555'
    Phone    = '66666666-6666-6666-6666-666666666666'
    Zbox     = '77777777-7777-7777-7777-777777777777'
}

function New-DeviceSeed {
    param(
        [string] $Code, [string] $Name, [string] $Brand, [string] $Model,
        [string] $CategoryId, [int] $Status = 1,
        [string] $WarrantyStartDate = '2025-01-15',
        [AllowNull()] [string] $WarrantyEndDate = '2028-01-15',
        [AllowNull()] [string] $WarrantyProvider = 'Yetkili Servis'
    )

    @{
        cihazAdi = $Name; seriNo = "DEMO-SN-$Code"; envanterNo = "DEMO-ENV-$Code"
        barkodNo = $null; marka = $Brand; model = $Model; categoryId = $CategoryId
        status = $Status; personelId = $null; warrantyStartDate = if ($WarrantyEndDate) { $WarrantyStartDate } else { $null }
        warrantyEndDate = if ($WarrantyEndDate) { $WarrantyEndDate } else { $null }
        warrantyProvider = if ($WarrantyProvider) { $WarrantyProvider } else { $null }
        warrantyNote = if ($WarrantyEndDate) { 'Referans envanter dağılımından uyarlanmış demo kayıt' } else { $null }
    }
}

$deviceSeed = @(
    (New-DeviceSeed '001' 'Kurumsal Laptop 01' 'Dell' 'Latitude 5450' $categoryIds.Laptop),
    (New-DeviceSeed '002' 'Kurumsal Laptop 02' 'Dell' 'Latitude 5450' $categoryIds.Laptop -WarrantyEndDate '2026-10-20'),
    (New-DeviceSeed '003' 'Kurumsal Laptop 03' 'Dell' 'Latitude 5410' $categoryIds.Laptop -WarrantyEndDate '2026-06-01'),
    (New-DeviceSeed '004' 'Dönüşebilir Laptop' 'Dell' 'Latitude 7350 2-in-1' $categoryIds.Laptop -Status 3 -WarrantyEndDate '2027-08-10'),
    (New-DeviceSeed '005' 'Ofis Bilgisayarı 01' 'Dell' 'OptiPlex 7010' $categoryIds.Desktop -WarrantyEndDate '2026-11-05'),
    (New-DeviceSeed '006' 'Ofis Bilgisayarı 02' 'Lenovo' 'ThinkCentre M900' $categoryIds.Desktop -Status 4 -WarrantyStartDate '2021-03-12' -WarrantyEndDate '2024-03-12'),
    (New-DeviceSeed '007' 'Ofis Monitörü 01' 'Iiyama' 'ProLite XUB2793HS-B5' $categoryIds.Monitor),
    (New-DeviceSeed '008' 'Ofis Monitörü 02' 'Iiyama' 'ProLite XUB2793HS-B5' $categoryIds.Monitor -WarrantyEndDate '2026-09-18'),
    (New-DeviceSeed '009' 'Ofis Monitörü 03' 'Dell' 'E2425HM' $categoryIds.Monitor -WarrantyEndDate '2027-04-11'),
    (New-DeviceSeed '010' 'Toplantı Monitörü' 'Dell' 'P2422H' $categoryIds.Monitor -WarrantyEndDate $null -WarrantyProvider $null),
    (New-DeviceSeed '011' 'Departman Yazıcısı' 'Lexmark' 'MS621dn' $categoryIds.Printer -WarrantyEndDate '2026-03-30'),
    (New-DeviceSeed '012' 'Renkli Çok Fonksiyonlu Yazıcı' 'Lexmark' 'CX922DE' $categoryIds.Printer -WarrantyEndDate '2027-12-12'),
    (New-DeviceSeed '013' 'Etiket Yazıcısı' 'Zebra' 'ZT411' $categoryIds.Printer -Status 6 -WarrantyEndDate $null -WarrantyProvider $null),
    (New-DeviceSeed '014' 'Saha Tableti' 'Samsung' 'Galaxy Tab Active4 Pro' $categoryIds.Tablet -WarrantyEndDate '2027-02-24'),
    (New-DeviceSeed '015' 'Kurumsal Telefon 01' 'Samsung' 'Galaxy A55' $categoryIds.Phone -WarrantyEndDate '2026-10-02'),
    (New-DeviceSeed '016' 'Kurumsal Telefon 02' 'Samsung' 'Galaxy A55' $categoryIds.Phone -Status 5 -WarrantyEndDate '2026-06-10'),
    (New-DeviceSeed '017' 'İnce İstemci 01' 'Zotac' 'ZBOX CI337 nano' $categoryIds.Zbox -WarrantyEndDate '2028-05-17'),
    (New-DeviceSeed '018' 'İnce İstemci 02' 'Zotac' 'ZBOX CI337 nano' $categoryIds.Zbox -WarrantyEndDate $null -WarrantyProvider $null)
)

$people = @((Invoke-InventoryApi -Path '/api/personnel'))
foreach ($person in $personnelSeed) {
    if (-not ($people | Where-Object { $_.sicilNo -eq $person.sicilNo })) {
        Invoke-InventoryApi -Path '/api/personnel' -Method Post -Body $person | Out-Null
    }
}

$devices = @((Invoke-InventoryApi -Path '/api/devices'))
foreach ($device in $deviceSeed) {
    if (-not ($devices | Where-Object { $_.envanterNo -eq $device.envanterNo })) {
        Invoke-InventoryApi -Path '/api/devices' -Method Post -Body $device | Out-Null
    }
}

$people = @((Invoke-InventoryApi -Path '/api/personnel'))
$devices = @((Invoke-InventoryApi -Path '/api/devices'))
$assignments = @(
    @{ inventory = 'DEMO-ENV-001'; personnel = 'DEMO-1001' },
    @{ inventory = 'DEMO-ENV-002'; personnel = 'DEMO-1002' },
    @{ inventory = 'DEMO-ENV-005'; personnel = 'DEMO-1003' },
    @{ inventory = 'DEMO-ENV-007'; personnel = 'DEMO-1001' },
    @{ inventory = 'DEMO-ENV-008'; personnel = 'DEMO-1004' },
    @{ inventory = 'DEMO-ENV-014'; personnel = 'DEMO-1005' },
    @{ inventory = 'DEMO-ENV-015'; personnel = 'DEMO-1006' },
    @{ inventory = 'DEMO-ENV-017'; personnel = 'DEMO-1007' }
)

foreach ($assignment in $assignments) {
    $device = $devices | Where-Object { $_.envanterNo -eq $assignment.inventory } | Select-Object -First 1
    $person = $people | Where-Object { $_.sicilNo -eq $assignment.personnel } | Select-Object -First 1

    if ($device -and $person -and $device.status -eq 1) {
        Invoke-InventoryApi -Path '/api/assignments/assign' -Method Post -Body @{
            deviceId = $device.id
            personnelId = $person.id
            note = 'Referans projeden uyarlanmış demo zimmet kaydı'
        } | Out-Null
    }
}

$finalPeople = @((Invoke-InventoryApi -Path '/api/personnel') | Where-Object { $_.sicilNo -like 'DEMO-*' })
$finalDevices = @((Invoke-InventoryApi -Path '/api/devices') | Where-Object { $_.envanterNo -like 'DEMO-*' })

[pscustomobject]@{
    DemoPersonnel = $finalPeople.Count
    DemoDevices = $finalDevices.Count
    AssignedDemoDevices = @($finalDevices | Where-Object { $_.status -eq 2 }).Count
} | ConvertTo-Json
