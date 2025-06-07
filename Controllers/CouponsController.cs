using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public CouponsController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Coupons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Coupon>>> GetCoupons()
        {
            return await _context.Coupons
                .Include(c => c.Schedules)
                .ToListAsync();
        }

        // GET: api/Coupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Coupon>> GetCoupon(int id)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return coupon;
        }

        // POST: api/Coupons
        [HttpPost]
        public async Task<ActionResult<Coupon>> CreateCoupon(Coupon coupon)
        {
            if (string.IsNullOrEmpty(coupon.Code))
            {
                return BadRequest("Coupon code is required");
            }

            // Check if coupon code already exists
            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
            {
                return BadRequest("Coupon code already exists");
            }

            // Validate dates for periodic coupons
            if (coupon.IsPeriodic)
            {
                if (coupon.StartDate == null || coupon.EndDate == null)
                {
                    return BadRequest("Start date and end date are required for periodic coupons");
                }

                if (coupon.StartDate >= coupon.EndDate)
                {
                    return BadRequest("Start date must be before end date");
                }
            }

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, coupon);
        }

        // PUT: api/Coupons/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoupon(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return BadRequest();
            }

            var existingCoupon = await _context.Coupons
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCoupon == null)
            {
                return NotFound();
            }

            // Check if code is being changed and if it already exists
            if (existingCoupon.Code != coupon.Code)
            {
                if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
                {
                    return BadRequest("Coupon code already exists");
                }
            }

            // Validate dates for periodic coupons
            if (coupon.IsPeriodic)
            {
                if (coupon.StartDate == null || coupon.EndDate == null)
                {
                    return BadRequest("Start date and end date are required for periodic coupons");
                }

                if (coupon.StartDate >= coupon.EndDate)
                {
                    return BadRequest("Start date must be before end date");
                }
            }

            _context.Entry(existingCoupon).CurrentValues.SetValues(coupon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CouponExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Coupons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Coupons/validate-schedule
        [HttpPost("validate-schedule")]
        public async Task<IActionResult> ValidateCouponSchedule([FromBody] CouponValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Coupon code is required");
            }

            var coupon = await _context.Coupons
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Code == request.Code);

            if (coupon == null)
            {
                return BadRequest("Coupon not found");
            }

            if (!coupon.Schedules.Any())
            {
                return BadRequest("Coupon schedule not found");
            }

            // 1. Check schedule validity
            var currentDay = DateTime.UtcNow.DayOfWeek;
            var currentTime = DateTime.UtcNow.TimeOfDay;
            var schedule = coupon.Schedules.First();

            bool isDayValid = currentDay switch
            {
                DayOfWeek.Monday => schedule.Monday,
                DayOfWeek.Tuesday => schedule.Tuesday,
                DayOfWeek.Wednesday => schedule.Wednesday,
                DayOfWeek.Thursday => schedule.Thursday,
                DayOfWeek.Friday => schedule.Friday,
                DayOfWeek.Saturday => schedule.Saturday,
                DayOfWeek.Sunday => schedule.Sunday,
                _ => false
            };

            if (!isDayValid)
            {
                return BadRequest("Coupon is not valid for today");
            }

            if (currentTime < schedule.BeginTime || currentTime > schedule.EndTime)
            {
                return BadRequest($"Coupon is only valid between {schedule.BeginTime:hh\\:mm} and {schedule.EndTime:hh\\:mm}");
            }

            // 2. Check general validity
            // (a) One-time coupon: check if used
            if (!coupon.IsPeriodic)
            {
                var isUsed = await _context.CouponHistory.AnyAsync(ch => ch.CouponId == coupon.Id);
                if (isUsed)
                {
                    return BadRequest("Coupon is used");
                }
            }
            // (b) Periodic: check date range
            if (coupon.IsPeriodic)
            {
                if (coupon.StartDate == null || coupon.EndDate == null)
                {
                    return BadRequest("Start date and end date are required for periodic coupons");
                }

                var currentDate = DateTime.UtcNow;
                if (currentDate < coupon.StartDate || currentDate > coupon.EndDate)
                {
                    return BadRequest("Coupon is not valid for current date");
                }
            }
            // (c) One-time: check email
            if (!coupon.IsPeriodic && !string.IsNullOrEmpty(coupon.Email))
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest("Email is required for this coupon");
                }
                if (coupon.Email.ToLower() != request.Email.ToLower())
                {
                    return BadRequest("Invalid email for this coupon");
                }
            }

            return Ok(new { 
                IsValid = true,
                DiscountRatio = coupon.DiscountRatio,
                Code = coupon.Code,
                Type = coupon.Type,
                Schedule = new {
                    BeginTime = schedule.BeginTime,
                    EndTime = schedule.EndTime,
                    ValidDays = new {
                        Monday = schedule.Monday,
                        Tuesday = schedule.Tuesday,
                        Wednesday = schedule.Wednesday,
                        Thursday = schedule.Thursday,
                        Friday = schedule.Friday,
                        Saturday = schedule.Saturday,
                        Sunday = schedule.Sunday
                    }
                }
            });
        }

        // POST: api/Coupons/use
        [HttpPost("use")]
        public async Task<IActionResult> UseCoupon([FromBody] CouponUseRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Coupon code is required");
            }

            var coupon = await _context.Coupons
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Code == request.Code);

            if (coupon == null)
            {
                return BadRequest("Coupon not found");
            }

            // Validate the coupon
            var validationResult = await ValidateCouponSchedule(new CouponValidationRequest 
            { 
                Code = request.Code,
                Email = request.Email
            });

            if (validationResult is BadRequestObjectResult)
            {
                return validationResult;
            }

            // Record coupon usage
            var history = new CouponHistory
            {
                CouponId = coupon.Id,
                Email = request.Email,
                UsedAt = DateTime.UtcNow
            };

            _context.CouponHistory.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Coupon used successfully" });
        }

        // GET: api/Coupons/5/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<CouponHistory>>> GetCouponHistory(int id)
        {
            if (!CouponExists(id))
            {
                return NotFound();
            }

            return await _context.CouponHistory
                .Where(ch => ch.CouponId == id)
                .OrderByDescending(ch => ch.UsedAt)
                .ToListAsync();
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }

    public class CouponValidationRequest
    {
        public string Code { get; set; }
        public string Email { get; set; }
    }

    public class CouponUseRequest
    {
        public string Code { get; set; }
        public string Email { get; set; }
    }
} 