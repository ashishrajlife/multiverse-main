using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ERPDemo.Services
{
    public class GstLookupResult
    {
        public bool Success { get; set; }
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? Status { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Error { get; set; }
    }

    public class GstLookupService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public GstLookupService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<GstLookupResult> LookupAsync(string gstin)
        {
            gstin = gstin.Trim().ToUpper();

            // 1️⃣ Offline checksum validation (API credit bachao)
            if (!GstinValidator.IsValid(gstin))
            {
                return new GstLookupResult
                {
                    Success = false,
                    Error = "Invalid GSTIN — format ya checksum match nahi kar raha."
                };
            }

            try
            {
                // 2️⃣ gstinapi.in real API call
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://www.gstinapi.in/v1/gstin/{gstin}"),
                    Headers =
                    {
                        { "x-api-key", _config["GstApi:GstinApiKey"] }
                    }
                };

                using var response = await _http.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                // 3️⃣ Error handling
                if (!response.IsSuccessStatusCode)
                {
                    return new GstLookupResult
                    {
                        Success = false,
                        Error = response.StatusCode switch
                        {
                            System.Net.HttpStatusCode.Unauthorized => "API Key invalid hai. Check karo.",
                            System.Net.HttpStatusCode.PaymentRequired => "Free credits khatam. Recharge karo.",
                            System.Net.HttpStatusCode.NotFound => "Ye GSTIN GST database mein nahi mila.",
                            System.Net.HttpStatusCode.TooManyRequests => "Bahut requests. Thodi der baad try karo.",
                            _ => $"Query fail hui ({(int)response.StatusCode})."
                        }
                    };
                }

                // 4️⃣ JSON parse
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Data root mein ya data object mein ho sakta hai
                JsonElement data;
                if (root.TryGetProperty("data", out data) == false)
                {
                    // Fallback: root ke andar hi fields hain
                    data = root;
                }

                // 5️⃣ Address fields extract (multiple fallback keys)
                var fullAddress = GetString(data, "address", "pradr", "principal_place_address");
                var city = GetString(data, "city", "dst", "district");
                var state = GetString(data, "state", "stcd");
                var pincode = GetString(data, "pincode", "pncd");

                // Address se pincode / city / state nikalne ki koshish
                if (string.IsNullOrEmpty(pincode) && !string.IsNullOrEmpty(fullAddress))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(fullAddress, @"\b\d{6}\b");
                    if (match.Success) pincode = match.Value;
                }

                if (string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(fullAddress))
                {
                    var states = new[] { "Chhattisgarh", "Maharashtra", "Delhi", "Karnataka",
                        "Tamil Nadu", "Gujarat", "Rajasthan", "Uttar Pradesh",
                        "Madhya Pradesh", "West Bengal", "Punjab", "Haryana",
                        "Kerala", "Telangana", "Andhra Pradesh", "Bihar", "Odisha", "Jharkhand" };
                    foreach (var s in states)
                    {
                        if (fullAddress.Contains(s, StringComparison.OrdinalIgnoreCase))
                        {
                            state = s;
                            break;
                        }
                    }
                }

                return new GstLookupResult
                {
                    Success = true,
                    LegalName = GetString(data, "legal_name", "lgnm"),
                    TradeName = GetString(data, "trade_name", "tradeNam"),
                    Status = GetString(data, "status", "sts"),
                    Address = fullAddress,
                    City = city,
                    State = state,
                    Pincode = pincode
                };
            }
            catch (JsonException)
            {
                return new GstLookupResult
                {
                    Success = false,
                    Error = "Server ne valid JSON return nahi kiya."
                };
            }
            catch (Exception ex)
            {
                return new GstLookupResult
                {
                    Success = false,
                    Error = "Verification fail hui: " + ex.Message
                };
            }
        }

        // Helper: pehla available key return karo
        private static string? GetString(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (el.TryGetProperty(key, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String)
                        return prop.GetString();
                    if (prop.ValueKind == JsonValueKind.Object)
                    {
                        // Nested address object handle karo
                        if (prop.TryGetProperty("adr", out var adr))
                            return adr.GetString();
                        if (prop.TryGetProperty("addr", out var addr))
                            return addr.GetString();
                    }
                }
            }
            return null;
        }
    }
}