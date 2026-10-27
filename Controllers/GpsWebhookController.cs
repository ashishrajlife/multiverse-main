using ERPDemo.Data;
using ERPDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPDemo.Controllers
{
    [AllowAnonymous]   // Device auth use nahi karta — IMEI se identify hota hai
    [ApiController]
    [Route("api/gps")]
    public class GpsWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public GpsWebhookController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// GPS device ka ping — POST /api/gps/ping
        /// Payload: { "imei": "...", "lat": 28.4, "lng": 77.0, "speed": 60, "ignition": 1, "odometer": 1234, "ts": "..." }
        /// </summary>
        [HttpPost("ping")]
        public async Task<IActionResult> Ping([FromBody] JsonElement payload)
        {
            try
            {
                // 1. Extract common fields (multiple fallbacks for vendor compatibility)
                var imei     = GetString(payload, "imei", "IMEI", "device_id", "deviceId");
                var lat      = GetDecimal(payload, "lat", "latitude", "Lat");
                var lng      = GetDecimal(payload, "lng", "lon", "longitude", "Lng");
                var speed    = GetDecimal(payload, "speed", "speedKmph", "spd") ?? 0;
                var ignition = GetInt(payload, "ignition", "ign", "acc") ?? 0;
                var odometer = GetDecimal(payload, "odometer", "odo", "mileage");
                var ts       = GetDateTime(payload, "ts", "timestamp", "recorded_at") ?? DateTime.UtcNow;

                if (string.IsNullOrEmpty(imei) || !lat.HasValue || !lng.HasValue)
                    return BadRequest(new { error = "Missing required fields: imei, lat, lng" });

                // 2. Find device by IMEI
                var device = await _context.GpsDevices
                    .FirstOrDefaultAsync(d => d.Imei == imei && d.Status == "Active");

                if (device == null)
                    return NotFound(new { error = $"Device {imei} not registered or inactive" });

                // 3. Insert GpsLog
                var log = new GpsLog
                {
                    GpsDeviceId = device.GpsDeviceId,
                    VehicleId   = device.VehicleId,
                    TripId      = null,   // will be set if vehicle is on active trip (below)
                    Latitude    = lat.Value,
                    Longitude   = lng.Value,
                    SpeedKmph   = speed,
                    Ignition    = ignition,
                    OdometerKm  = odometer,
                    RecordedAt  = ts,
                    ReceivedAt  = DateTime.UtcNow,
                    Source      = "webhook"
                };

                // 4. Find active trip — if vehicle is on one, attach
                var activeTrip = await _context.Trips
                    .FirstOrDefaultAsync(t => t.VehicleId == device.VehicleId
                        && (t.Status == "InProgress" || t.Status == "Delayed"));
                if (activeTrip != null)
                {
                    log.TripId = activeTrip.TripId;
                }

                _context.GpsLogs.Add(log);
                device.LastPingAt = DateTime.UtcNow;

                // 5. Update VehicleLocation (live cache)
                var vLoc = await _context.VehicleLocations
                    .FirstOrDefaultAsync(vl => vl.VehicleId == device.VehicleId);
                if (vLoc == null)
                {
                    vLoc = new VehicleLocation { VehicleId = device.VehicleId };
                    _context.VehicleLocations.Add(vLoc);
                }
                vLoc.Latitude        = lat.Value;
                vLoc.Longitude       = lng.Value;
                vLoc.SpeedKmph       = speed;
                vLoc.Ignition        = ignition;
                vLoc.OdometerKm      = odometer;
                vLoc.LastUpdatedAt   = DateTime.UtcNow;
                vLoc.CurrentTripId   = activeTrip?.TripId;

                // 6. Update vehicle's current odometer (for trip start/end tracking)
                var vehicle = await _context.Vehicles.FindAsync(device.VehicleId);
                if (vehicle != null && odometer.HasValue)
                {
                    vehicle.CurrentOdometerKm = (int)odometer.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Ping accepted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error", detail = ex.Message });
            }
        }

        // ═══════════ Helpers ═══════════
        private static string? GetString(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (el.TryGetProperty(key, out var p))
                {
                    if (p.ValueKind == JsonValueKind.String) return p.GetString();
                    if (p.ValueKind == JsonValueKind.Number) return p.GetRawText();
                }
            }
            return null;
        }

        private static decimal? GetDecimal(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (el.TryGetProperty(key, out var p))
                {
                    if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
                    if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var d2)) return d2;
                }
            }
            return null;
        }

        private static int? GetInt(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (el.TryGetProperty(key, out var p))
                {
                    if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                    if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var i2)) return i2;
                    if (p.ValueKind == JsonValueKind.True) return 1;
                    if (p.ValueKind == JsonValueKind.False) return 0;
                }
            }
            return null;
        }

        private static DateTime? GetDateTime(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (el.TryGetProperty(key, out var p))
                {
                    if (p.ValueKind == JsonValueKind.String && DateTime.TryParse(p.GetString(), out var dt)) return dt;
                }
            }
            return null;
        }
    }
}