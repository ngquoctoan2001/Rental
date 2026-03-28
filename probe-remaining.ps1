$base = "http://localhost:5272"

function Invoke-Api($method, $path, $token = $null, $body = $null) {
    try {
        $headers = @{ "Content-Type" = "application/json" }
        if ($token) { $headers["Authorization"] = "Bearer $token" }
        $params = @{ Method = $method; Uri = "$base$path"; Headers = $headers; UseBasicParsing = $true; TimeoutSec = 20 }
        if ($body -ne $null) { $params.Body = ($body | ConvertTo-Json -Depth 10) }
        $resp = Invoke-WebRequest @params
        Write-Host "[OK $($resp.StatusCode)] $method $path"
        if ($resp.Content) { Write-Host $resp.Content }
        return ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue)
    } catch [System.Net.WebException] {
        $code = [int]$_.Exception.Response.StatusCode
        $raw = ""
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $raw = $reader.ReadToEnd()
        } catch {}
        Write-Host "[ERR $code] $method $path -> $raw"
        return $null
    } catch {
        Write-Host "[EXC] $method $path -> $_"
        return $null
    }
}

$ts = "probe$(Get-Random -Maximum 9999)"
$reg = Invoke-Api POST "/api/v1/auth/register" $null @{
    ownerName = "Probe Owner"
    ownerEmail = "owner@$ts.com"
    phone = "0912345678"
    tenantName = "Probe Company $ts"
    password = "Test@123456!"
}
if (-not $reg) { exit 1 }
$token = $reg.accessToken

$prop = Invoke-Api POST "/api/v1/properties" $token @{
    name = "Probe Property"
    address = "123 Probe St"
    province = "Ha Noi"
    district = "Ba Dinh"
    ward = "Phuc Xa"
    totalFloors = 3
}
$propId = $prop

$room = Invoke-Api POST "/api/v1/rooms" $token @{
    propertyId = $propId
    roomNumber = "201"
    floor = 2
    areaSqm = 20
    basePrice = 3000000
    electricityPrice = 3500
    waterPrice = 15000
    serviceFee = 50000
    amenities = @("wifi", "ac", "parking_motorbike")
}
$roomId = $room

$cust = Invoke-Api POST "/api/v1/customers" $token @{
    fullName = "Probe Customer"
    phone = "0901234567"
    idCardNumber = "001234567890"
}
$custId = $cust

if ($roomId) {
    Invoke-Api PATCH "/api/v1/rooms/$roomId/status" $token @{ id = $roomId; newStatus = "Maintenance"; maintenanceNote = "Test" } | Out-Null
    Invoke-Api PATCH "/api/v1/rooms/$roomId/status" $token @{ id = $roomId; newStatus = "Available" } | Out-Null
}

if ($roomId -and $custId) {
    Invoke-Api POST "/api/v1/contracts" $token @{
        roomId = $roomId
        customerId = $custId
        startDate = (Get-Date).ToString("yyyy-MM-dd")
        endDate = (Get-Date).AddMonths(6).ToString("yyyy-MM-dd")
        monthlyRent = 3000000
        depositMonths = 1
        billingDate = 5
        paymentDueDays = 10
    } | Out-Null
}

if ($roomId) {
    Invoke-Api POST "/api/v1/meterreadings" $token @{
        roomId = $roomId
        readingDate = (Get-Date).ToString("yyyy-MM-dd")
        electricityReading = 100
        waterReading = 20
        note = "Probe"
    } | Out-Null
}

# Use admin token for change-password probe
$adminLogin = Invoke-Api POST "/api/v1/auth/login" $null @{ email="admin@rentalos.vn"; password="Admin@123456!"; tenantSlug="admin" }
if ($adminLogin) {
    $adminToken = $adminLogin.accessToken
    Invoke-Api PUT "/api/v1/auth/change-password" $adminToken @{ currentPassword="Admin@123456!"; newPassword="Admin@123456!" } | Out-Null
}
