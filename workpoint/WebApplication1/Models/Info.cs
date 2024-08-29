namespace WebApplication1.Models
{
    public class Info
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Image { get; set; } // ใช้เก็บ URL หรือชื่อไฟล์ของรูปภาพ
        public string Detail { get; set; }
    }
}
