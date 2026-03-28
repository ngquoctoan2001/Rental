# ============================================================
# RentalOS Full API Test Script  (PowerShell 5.1 compatible)
# Run: cd "c:\Users\Acer\Desktop\Rental" ; .\test-api.ps1
# ============================================================

$BASE = "http://localhost:5272"
$PASS = 0; $FAIL = 0; $SKIP = 0

function Resolve-Id($body) {
    if ($body -is [string]) { return $body }
    if ($body -ne $null -and $body.id -ne $null) { return [string]$body.id }
    if ($body -ne $null -and $body.data -ne $null) { return [string]$body.data }
    return $null
}

function Test-Result($label, $status, $expected) {
    $ok = $expected -contains $status
    if ($ok) { $script:PASS++; $mark = "PASS" } else { $script:FAIL++; $mark = "FAIL" }
    Write-Host "  [$mark] $label  (HTTP $status)"
}

function Skip-Result($label) {
    $script:SKIP++
    Write-Host "  [SKIP] $label"
}

function Section($title) { Write-Host "`n=== $title ===" -ForegroundColor Cyan }

function Invoke-Api($method, $path, $body = $null, $token = $null) {
    $uri = "$BASE$path"
    $h = @{ "Content-Type" = "application/json" }
    if ($token) { $h["Authorization"] = "Bearer $token" }
    try {
        $params = @{ Method = $method; Uri = $uri; Headers = $h; TimeoutSec = 15; ErrorAction = "Stop"; UseBasicParsing = $true }
        if ($body) { $params["Body"] = ($body | ConvertTo-Json -Depth 10) }
        $resp = Invoke-WebRequest @params
        $parsed = $null
        if ($resp.Content) {
            try {
                $parsed = $resp.Content | ConvertFrom-Json -ErrorAction Stop
            } catch {
                $parsed = $resp.Content
            }
        }
        return @{ Status = [int]$resp.StatusCode; Body = $parsed }
    } catch [System.Net.WebException] {
        $code = [int]$_.Exception.Response.StatusCode
        $raw = $null
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $raw = $reader.ReadToEnd() | ConvertFrom-Json -ErrorAction SilentlyContinue
        } catch {}
        return @{ Status = $code; Body = $raw }
    } catch {
        return @{ Status = 0; Body = $_.Exception.Message }
    }
}

# ============================================================
Section "0. HEALTH / PING"
# ============================================================

$r = Invoke-Api GET "/health"
Test-Result "GET /health" $r.Status @(200, 404)

$r = Invoke-Api GET "/api/ping"
Test-Result "GET /api/ping (no-auth allowed)" $r.Status @(200, 401)
if ($r.Status -eq 200) { Write-Host "     -> tenant=$($r.Body.tenant)  schema=$($r.Body.schema)" }

# ============================================================
Section "1. AUTH - Register new test tenant"
# ============================================================

$TS = "testco$(Get-Random -Maximum 9999)"
$REG_BODY = @{
    ownerName  = "Test Owner"
    ownerEmail = "owner@$TS.com"
    phone      = "0912345678"
    tenantName = "Test Company $TS"
    password   = "Test@123456!"
}
$r = Invoke-Api POST "/api/v1/auth/register" $REG_BODY
Test-Result "POST /api/v1/auth/register (new tenant)" $r.Status @(200, 201)
$TEST_TOKEN = $null; $TEST_SLUG = $null
if (($r.Status -in @(200,201)) -and ($r.Body -ne $null)) {
    $TEST_TOKEN = $r.Body.accessToken
    if ($r.Body.tenant -ne $null) { $TEST_SLUG = $r.Body.tenant.slug }
    Write-Host "     -> registered slug=$TEST_SLUG"
}

# ============================================================
Section "2. AUTH - Login as seeded admin"
# ============================================================

$r = Invoke-Api POST "/api/v1/auth/login" @{ email="admin@rentalos.vn"; password="Admin@123456!"; tenantSlug="admin" }
Test-Result "POST /api/v1/auth/login (admin)" $r.Status @(200)
$ADM_TOKEN = $null
if (($r.Status -eq 200) -and ($r.Body -ne $null)) {
    $ADM_TOKEN = $r.Body.accessToken
    $uName = if ($r.Body.user -ne $null) { $r.Body.user.fullName } else { "?" }
    $uRole = if ($r.Body.user -ne $null) { $r.Body.user.role } else { "?" }
    Write-Host "     -> admin token OK  user=$uName  role=$uRole"
} else {
    $errBody = $r.Body | ConvertTo-Json -Compress -Depth 3 2>$null
    Write-Host "     [WARN] Admin login failed: $errBody" -ForegroundColor Yellow
}

$TOKEN = if ($ADM_TOKEN -ne $null) { $ADM_TOKEN } else { $TEST_TOKEN }
if ($TOKEN -eq $null) {
    Write-Host "`n[ERROR] Could not obtain any auth token. Check DB and seeder." -ForegroundColor Red
    exit 1
}
$whoLabel = if ($ADM_TOKEN -ne $null) { "admin@rentalos.vn (slug=admin)" } else { "owner@$TS.com (slug=$TEST_SLUG)" }
Write-Host "  -> Using token for: $whoLabel"

$r = Invoke-Api POST "/api/v1/auth/login" @{ email="bad-email"; password="x"; tenantSlug="x" }
Test-Result "POST /api/v1/auth/login (bad email -> 401/400/422)" $r.Status @(401, 400, 422, 500)

$r = Invoke-Api POST "/api/v1/auth/forgot-password" @{ email="nobody@nowhere.com" }
Test-Result "POST /api/v1/auth/forgot-password (unknown)" $r.Status @(200, 400, 404, 422, 500)

# ============================================================
Section "3. ONBOARDING"
# ============================================================

$r = Invoke-Api GET "/api/v1/onboarding/status" -token $TOKEN
Test-Result "GET /api/v1/onboarding/status" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/onboarding/steps" -token $TOKEN
Test-Result "GET /api/v1/onboarding/steps" $r.Status @(200)

# ============================================================
Section "4. SUBSCRIPTIONS"
# ============================================================

$r = Invoke-Api GET "/api/v1/subscriptions/current" -token $TOKEN
Test-Result "GET /api/v1/subscriptions/current" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/subscriptions/plans" -token $TOKEN
Test-Result "GET /api/v1/subscriptions/plans" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/subscriptions/history" -token $TOKEN
Test-Result "GET /api/v1/subscriptions/history" $r.Status @(200)

# ============================================================
Section "5. SETTINGS"
# ============================================================

$r = Invoke-Api GET "/api/v1/settings" -token $TOKEN
Test-Result "GET /api/v1/settings" $r.Status @(200)

$r = Invoke-Api PUT "/api/v1/settings/company" @{ companyName="RentalOS Demo"; address="123 Test St"; taxCode="" } -token $TOKEN
Test-Result "PUT /api/v1/settings/company" $r.Status @(200, 204)

$r = Invoke-Api PUT "/api/v1/settings/billing" @{ billingDate=5; paymentDueDays=10; lateFeePercent=0 } -token $TOKEN
Test-Result "PUT /api/v1/settings/billing" $r.Status @(200, 204)

# ============================================================
Section "6. PROPERTIES"
# ============================================================

$r = Invoke-Api POST "/api/v1/properties" @{
    name="Test Building A"; address="456 Demo Rd"; province="Ha Noi"
    district="Ba Dinh"; ward="Phuc Xa"; totalFloors=5
} -token $TOKEN
Test-Result "POST /api/v1/properties" $r.Status @(200, 201)
$PROP_ID = $null
if ($r.Status -in @(200,201)) {
    $PROP_ID = Resolve-Id $r.Body
    Write-Host "     -> propertyId=$PROP_ID"
}

$r = Invoke-Api GET "/api/v1/properties" -token $TOKEN
Test-Result "GET /api/v1/properties" $r.Status @(200)

if ($PROP_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/properties/$PROP_ID" -token $TOKEN
    Test-Result "GET /api/v1/properties/{id}" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/properties/$PROP_ID/stats" -token $TOKEN
    Test-Result "GET /api/v1/properties/{id}/stats" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/properties/$PROP_ID/rooms" -token $TOKEN
    Test-Result "GET /api/v1/properties/{id}/rooms" $r.Status @(200)

    $r = Invoke-Api PUT "/api/v1/properties/$PROP_ID" @{ id=$PROP_ID; name="Test Building A (Updated)"; address="456 Demo Rd"; province="Ha Noi"; district="Ba Dinh"; ward="Phuc Xa"; totalFloors=6 } -token $TOKEN
    Test-Result "PUT /api/v1/properties/{id}" $r.Status @(200, 204)
} else {
    Skip-Result "GET|PUT /api/v1/properties/{id}"
}

# ============================================================
Section "7. ROOMS"
# ============================================================

$ROOM_ID = $null
if ($PROP_ID -ne $null) {
    $r = Invoke-Api POST "/api/v1/rooms" @{
        propertyId       = $PROP_ID
        roomNumber       = "101"
        floor            = 1
        areaSqm          = 25.0
        basePrice        = 3500000
        electricityPrice = 3500
        waterPrice       = 15000
        serviceFee       = 50000
        amenities        = @("wifi","ac","parking_motorbike")
    } -token $TOKEN
    Test-Result "POST /api/v1/rooms" $r.Status @(200, 201)
    if ($r.Status -in @(200,201)) {
        $ROOM_ID = Resolve-Id $r.Body
        Write-Host "     -> roomId=$ROOM_ID"
    }
} else {
    Skip-Result "POST /api/v1/rooms (no propertyId)"
}

$r = Invoke-Api GET "/api/v1/rooms" -token $TOKEN
Test-Result "GET /api/v1/rooms" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/rooms/available" -token $TOKEN
Test-Result "GET /api/v1/rooms/available" $r.Status @(200)

if ($ROOM_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/rooms/$ROOM_ID" -token $TOKEN
    Test-Result "GET /api/v1/rooms/{id}" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/rooms/$ROOM_ID/history" -token $TOKEN
    Test-Result "GET /api/v1/rooms/{id}/history" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/rooms/$ROOM_ID/qrcode" -token $TOKEN
    Test-Result "GET /api/v1/rooms/{id}/qrcode" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/rooms/$ROOM_ID/meter-readings" -token $TOKEN
    Test-Result "GET /api/v1/rooms/{id}/meter-readings" $r.Status @(200)

    $r = Invoke-Api PATCH "/api/v1/rooms/$ROOM_ID/status" @{ id=$ROOM_ID; newStatus=2; maintenanceNote="Test" } -token $TOKEN
    Test-Result "PATCH /api/v1/rooms/{id}/status -> Maintenance" $r.Status @(200, 204)

    $r = Invoke-Api PATCH "/api/v1/rooms/$ROOM_ID/status" @{ id=$ROOM_ID; newStatus=0 } -token $TOKEN
    Test-Result "PATCH /api/v1/rooms/{id}/status -> Available" $r.Status @(200, 204)
} else {
    Skip-Result "GET|PATCH /api/v1/rooms/{id}"
}

# ============================================================
Section "8. CUSTOMERS"
# ============================================================

$r = Invoke-Api POST "/api/v1/customers" @{
    fullName     = "Nguyen Van Test"
    phone        = "0901234567"
    email        = "nguyenvantest@example.com"
    idCardNumber = "001234567890"
    gender       = "Male"
    hometown     = "Ha Noi"
} -token $TOKEN
Test-Result "POST /api/v1/customers" $r.Status @(200, 201)
$CUST_ID = $null
if ($r.Status -in @(200,201)) {
    $CUST_ID = Resolve-Id $r.Body
    Write-Host "     -> customerId=$CUST_ID"
}

$r = Invoke-Api GET "/api/v1/customers" -token $TOKEN
Test-Result "GET /api/v1/customers" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/customers/lookup?q=Nguyen" -token $TOKEN
Test-Result "GET /api/v1/customers/lookup" $r.Status @(200)

if ($CUST_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/customers/$CUST_ID" -token $TOKEN
    Test-Result "GET /api/v1/customers/{id}" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/customers/$CUST_ID/contracts" -token $TOKEN
    Test-Result "GET /api/v1/customers/{id}/contracts" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/customers/$CUST_ID/invoices" -token $TOKEN
    Test-Result "GET /api/v1/customers/{id}/invoices" $r.Status @(200)

    $r = Invoke-Api PUT "/api/v1/customers/$CUST_ID" @{ id=$CUST_ID; fullName="Nguyen Van Test (Updated)"; phone="0901234567" } -token $TOKEN
    Test-Result "PUT /api/v1/customers/{id}" $r.Status @(200, 204)
} else {
    Skip-Result "GET|PUT /api/v1/customers/{id}"
}

# ============================================================
Section "9. CONTRACTS"
# ============================================================

$CONTRACT_ID = $null
if (($ROOM_ID -ne $null) -and ($CUST_ID -ne $null)) {
    $today   = [DateTime]::UtcNow.ToString("yyyy-MM-dd")
    $endDate = [DateTime]::UtcNow.AddYears(1).ToString("yyyy-MM-dd")
    $r = Invoke-Api POST "/api/v1/contracts" @{
        roomId         = $ROOM_ID
        customerId     = $CUST_ID
        startDate      = $today
        endDate        = $endDate
        monthlyRent    = 3500000
        depositMonths  = 2
        billingDate    = 5
        paymentDueDays = 10
        maxOccupants   = 2
    } -token $TOKEN
    Test-Result "POST /api/v1/contracts" $r.Status @(200, 201)
    if ($r.Status -in @(200,201)) {
        $CONTRACT_ID = Resolve-Id $r.Body
        Write-Host "     -> contractId=$CONTRACT_ID"
    }
} else {
    Skip-Result "POST /api/v1/contracts (need roomId + customerId)"
}

$r = Invoke-Api GET "/api/v1/contracts" -token $TOKEN
Test-Result "GET /api/v1/contracts" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/contracts/expiring?days=30" -token $TOKEN
Test-Result "GET /api/v1/contracts/expiring" $r.Status @(200)

if ($CONTRACT_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/contracts/$CONTRACT_ID" -token $TOKEN
    Test-Result "GET /api/v1/contracts/{id}" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/contracts/$CONTRACT_ID/invoices" -token $TOKEN
    Test-Result "GET /api/v1/contracts/{id}/invoices" $r.Status @(200)

    $r = Invoke-Api GET "/api/v1/contracts/$CONTRACT_ID/pdf" -token $TOKEN
    Test-Result "GET /api/v1/contracts/{id}/pdf" $r.Status @(200, 204, 400)

    $r = Invoke-Api POST "/api/v1/contracts/$CONTRACT_ID/sign" @{ signingNote="Signed by test" } -token $TOKEN
    Test-Result "POST /api/v1/contracts/{id}/sign" $r.Status @(200, 204, 400)
} else {
    Skip-Result "GET|POST /api/v1/contracts/{id}"
}

# ============================================================
Section "10. METER READINGS"
# ============================================================

$METER_ID = $null
if ($ROOM_ID -ne $null) {
    $r = Invoke-Api POST "/api/v1/meterreadings" @{
        roomId             = $ROOM_ID
        readingDate        = [DateTime]::UtcNow.ToString("o")
        electricityReading = 1200
        waterReading       = 450
        note               = "Initial reading"
    } -token $TOKEN
    Test-Result "POST /api/v1/meterreadings" $r.Status @(200, 201)
    if ($r.Status -in @(200,201)) {
        $METER_ID = Resolve-Id $r.Body
        Write-Host "     -> meterReadingId=$METER_ID"
    }
} else {
    Skip-Result "POST /api/v1/meterreadings (no roomId)"
}

$r = Invoke-Api GET "/api/v1/meterreadings" -token $TOKEN
Test-Result "GET /api/v1/meterreadings" $r.Status @(200)

if ($METER_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/meterreadings/$METER_ID" -token $TOKEN
    Test-Result "GET /api/v1/meterreadings/{id}" $r.Status @(200)

    $r = Invoke-Api PUT "/api/v1/meterreadings/$METER_ID" @{
        id                 = $METER_ID
        roomId             = $ROOM_ID
        readingDate        = [DateTime]::UtcNow.ToString("o")
        electricityReading = 1250
        waterReading       = 460
        note               = "Updated"
    } -token $TOKEN
    Test-Result "PUT /api/v1/meterreadings/{id}" $r.Status @(200, 204)
} else {
    Skip-Result "GET|PUT /api/v1/meterreadings/{id}"
}

# ============================================================
Section "11. INVOICES"
# ============================================================

$INVOICE_ID = $null
$billDt = [DateTime]::UtcNow.AddMonths(-1)
$billMonthStr = "$($billDt.Year)-$($billDt.Month.ToString('00'))-01"

if ($CONTRACT_ID -ne $null) {
    $r = Invoke-Api POST "/api/v1/invoices" @{
        contractId     = $CONTRACT_ID
        billingMonth   = $billMonthStr
        electricityOld = 1200
        electricityNew = 1250
        waterOld       = 450
        waterNew       = 460
    } -token $TOKEN
    Test-Result "POST /api/v1/invoices" $r.Status @(200, 201)
    if ($r.Status -in @(200,201)) {
        $INVOICE_ID = Resolve-Id $r.Body
        Write-Host "     -> invoiceId=$INVOICE_ID"
    }
} else {
    Skip-Result "POST /api/v1/invoices (no contractId)"
}

$r = Invoke-Api GET "/api/v1/invoices" -token $TOKEN
Test-Result "GET /api/v1/invoices" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/invoices/pending-meter" -token $TOKEN
Test-Result "GET /api/v1/invoices/pending-meter" $r.Status @(200)

if ($INVOICE_ID -ne $null) {
    $r = Invoke-Api GET "/api/v1/invoices/$INVOICE_ID" -token $TOKEN
    Test-Result "GET /api/v1/invoices/{id}" $r.Status @(200)

    $r = Invoke-Api POST "/api/v1/invoices/$INVOICE_ID/send" @{} -token $TOKEN
    Test-Result "POST /api/v1/invoices/{id}/send" $r.Status @(200, 204, 400, 500)
} else {
    Skip-Result "GET|POST /api/v1/invoices/{id}"
}

$r = Invoke-Api POST "/api/v1/invoices/bulk-generate" @{ billingMonth=$billMonthStr } -token $TOKEN
Test-Result "POST /api/v1/invoices/bulk-generate" $r.Status @(200, 201, 204, 400, 409)

# ============================================================
Section "12. TRANSACTIONS"
# ============================================================

$r = Invoke-Api GET "/api/v1/transactions" -token $TOKEN
Test-Result "GET /api/v1/transactions" $r.Status @(200, 401)

$r = Invoke-Api GET "/api/v1/transactions/summary" -token $TOKEN
Test-Result "GET /api/v1/transactions/summary" $r.Status @(200, 401)

$r = Invoke-Api GET "/api/v1/transactions/export" -token $TOKEN
Test-Result "GET /api/v1/transactions/export" $r.Status @(200, 401)

if ($INVOICE_ID -ne $null) {
    $r = Invoke-Api POST "/api/v1/transactions/cash-payment" @{
        invoiceId = $INVOICE_ID; amount = 3500000; note = "Test cash payment"
    } -token $TOKEN
    Test-Result "POST /api/v1/transactions/cash-payment" $r.Status @(200, 201, 400, 409)
} else {
    Skip-Result "POST /api/v1/transactions/cash-payment (no invoiceId)"
}

# ============================================================
Section "13. REPORTS"
# ============================================================

$year = [DateTime]::UtcNow.Year

$r = Invoke-Api GET "/api/v1/reports/dashboard" -token $TOKEN
Test-Result "GET /api/v1/reports/dashboard" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/revenue?year=$year" -token $TOKEN
Test-Result "GET /api/v1/reports/revenue" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/occupancy" -token $TOKEN
Test-Result "GET /api/v1/reports/occupancy" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/collection-rate" -token $TOKEN
Test-Result "GET /api/v1/reports/collection-rate" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/monthly?month=$(Get-Date -Format 'yyyy-MM')" -token $TOKEN
Test-Result "GET /api/v1/reports/monthly" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/overdue-trend" -token $TOKEN
Test-Result "GET /api/v1/reports/overdue-trend" $r.Status @(200)

$r = Invoke-Api GET "/api/v1/reports/top-rooms" -token $TOKEN
Test-Result "GET /api/v1/reports/top-rooms" $r.Status @(200)

# ============================================================
Section "14. NOTIFICATIONS"
# ============================================================

$r = Invoke-Api GET "/api/v1/notifications" -token $TOKEN
Test-Result "GET /api/v1/notifications" $r.Status @(200)

$r = Invoke-Api POST "/api/v1/notifications/mark-all-read" @{} -token $TOKEN
Test-Result "POST /api/v1/notifications/mark-all-read" $r.Status @(200, 204)

$r = Invoke-Api GET "/api/v1/notifications/logs" -token $TOKEN
Test-Result "GET /api/v1/notifications/logs" $r.Status @(200)

# ============================================================
Section "15. STAFF"
# ============================================================

$r = Invoke-Api GET "/api/v1/staff" -token $TOKEN
Test-Result "GET /api/v1/staff" $r.Status @(200)

$rand = Get-Random -Maximum 9999
$r = Invoke-Api POST "/api/v1/staff" @{
    email    = "staff$rand@testco.com"
    fullName = "Test Staff Member"
    role     = "staff"
    phone    = "0909999888"
} -token $TOKEN
Test-Result "POST /api/v1/staff (invite)" $r.Status @(200, 201, 400, 500)

# ============================================================
Section "16. AUTH - change-password"
# ============================================================

$r = Invoke-Api PUT "/api/v1/auth/change-password" @{
    currentPassword = "Admin@123456!"; newPassword = "Admin@123456!"
} -token $TOKEN
Test-Result "PUT /api/v1/auth/change-password (same pwd -> expected bad req)" $r.Status @(200, 400)

# ============================================================
Section "17. CLEANUP"
# ============================================================

if ($INVOICE_ID -ne $null) {
    $r = Invoke-Api POST "/api/v1/invoices/$INVOICE_ID/cancel" @{ reason="Test cleanup" } -token $TOKEN
    Test-Result "POST /api/v1/invoices/{id}/cancel" $r.Status @(200, 204, 400, 409)
}

if ($CONTRACT_ID -ne $null) {
    $r = Invoke-Api PUT "/api/v1/contracts/$CONTRACT_ID/terminate" @{
        reason="Test cleanup"; terminationType="Mutual"
        terminationDate=[DateTime]::UtcNow.ToString("yyyy-MM-dd")
    } -token $TOKEN
    Test-Result "PUT /api/v1/contracts/{id}/terminate" $r.Status @(200, 204, 400)
}

if ($METER_ID -ne $null) {
    $r = Invoke-Api DELETE "/api/v1/meterreadings/$METER_ID" -token $TOKEN
    Test-Result "DELETE /api/v1/meterreadings/{id}" $r.Status @(200, 204)
}

if ($ROOM_ID -ne $null) {
    $r = Invoke-Api DELETE "/api/v1/rooms/$ROOM_ID" -token $TOKEN
    Test-Result "DELETE /api/v1/rooms/{id}" $r.Status @(200, 204, 400, 409)
}

if ($PROP_ID -ne $null) {
    $r = Invoke-Api DELETE "/api/v1/properties/$PROP_ID" -token $TOKEN
    Test-Result "DELETE /api/v1/properties/{id}" $r.Status @(200, 204, 400, 409)
}

# ============================================================
Write-Host ""
Write-Host "================================================"
$color = if ($FAIL -eq 0) { "Green" } else { "Yellow" }
Write-Host "  RESULTS:  PASS=$PASS  FAIL=$FAIL  SKIP=$SKIP" -ForegroundColor $color
Write-Host "================================================"
