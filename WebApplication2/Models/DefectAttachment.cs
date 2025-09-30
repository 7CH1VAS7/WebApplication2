using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class DefectAttachment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя файла обязательно")]
        [Display(Name = "Имя файла")]
        [StringLength(255, ErrorMessage = "Имя файла не может превышать 255 символов")]
        public string FileName { get; set; }

        [Display(Name = "Оригинальное имя файла")]
        [StringLength(255, ErrorMessage = "Оригинальное имя файла не может превышать 255 символов")]
        public string OriginalFileName { get; set; }

        [Display(Name = "Путь к файлу")]
        [StringLength(500, ErrorMessage = "Путь к файлу не может превышать 500 символов")]
        public string FilePath { get; set; }

        [Display(Name = "Тип файла (MIME)")]
        [StringLength(100, ErrorMessage = "Тип файла не может превышать 100 символов")]
        public string ContentType { get; set; }

        [Display(Name = "Размер файла (байт)")]
        public long FileSize { get; set; }

        [Display(Name = "Описание")]
        [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов")]
        public string? Description { get; set; }

        [Display(Name = "Дата загрузки")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Связь с дефектом
        [Display(Name = "Дефект")]
        public int DefectId { get; set; }
        public Defect? Defect { get; set; }

        // Связь с пользователем, загрузившим файл
        [Display(Name = "Загрузил")]
        public string UploadedById { get; set; }

        // Метод для форматирования размера файла
        [NotMapped]
        public string FormattedFileSize
        {
            get
            {
                string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
                double len = FileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
