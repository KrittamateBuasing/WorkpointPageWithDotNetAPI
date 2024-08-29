using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imageFolderPath;

        // Constructor สำหรับรับ ApplicationDbContext และกำหนดโฟลเดอร์สำหรับเก็บรูปภาพ
        public InfoApiController(ApplicationDbContext context)
        {
            _context = context;
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }
        }
        //
        // GET: api/InfoApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Info>>> GetInfos()
        {
            return await _context.Infos.ToListAsync();
        }

        // GET: api/InfoApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Info>> GetInfo(int id)
        {
            var info = await _context.Infos.FindAsync(id);

            if (info == null)
            {
                return NotFound();
            }

            return info;
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(_imageFolderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Ok(new { FilePath = filePath });
        }

        // POST: api/InfoApi
        [HttpPost]
        public async Task<ActionResult<Info>> Create([FromForm] Info info, [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ตรวจสอบการอัปโหลดภาพ
            if (image != null && image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(_imageFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                info.Image = $"/images/{fileName}";
            }
            else
            {
                info.Image = null; // หรือไม่ตั้งค่าเลยก็ได้
            }

            // เพิ่มข้อมูลลงในฐานข้อมูล
            _context.Infos.Add(info);
            await _context.SaveChangesAsync();

            // คืนค่าการตอบสนองพร้อมกับข้อมูลที่สร้างใหม่
            return CreatedAtAction(nameof(GetInfo), new { id = info.Id }, info);
        }

      
   
        // PUT: api/InfoApi/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] int id, [FromForm] string title, [FromForm] string detail, [FromForm] IFormFile image)
        {
            var existingInfo = await _context.Infos.FindAsync(id);
            if (existingInfo == null)
            {
                return NotFound();
            }

            existingInfo.Title = title;
            existingInfo.Detail = detail;

            if (image != null && image.Length > 0)
            {
                // ลบภาพเดิมหากมี
                if (!string.IsNullOrEmpty(existingInfo.Image))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingInfo.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // อัปโหลดภาพใหม่
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(_imageFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                existingInfo.Image = $"/images/{fileName}";
            }

            _context.Entry(existingInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Infos.Any(e => e.Id == id))
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





        // DELETE: api/Info/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfo(int id)
        {
            var info = await _context.Infos.FindAsync(id);
            if (info == null)
            {
                return NotFound();
            }

            // ลบภาพที่เกี่ยวข้องด้วย
            if (!string.IsNullOrEmpty(info.Image))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", info.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Infos.Remove(info);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InfoExists(int id)
        {
            return _context.Infos.Any(e => e.Id == id);
        }
    }
}
