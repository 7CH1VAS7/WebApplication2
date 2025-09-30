using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class DefectComment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Текст комментария обязателен")]
        [Display(Name = "Текст комментария")]
        [StringLength(1000, ErrorMessage = "Комментарий не может превышать 1000 символов")]
        public string Text { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата изменения")]
        public DateTime? UpdatedAt { get; set; }

        // Связь с дефектом
        [Display(Name = "Дефект")]
        public int DefectId { get; set; }
        public Defect? Defect { get; set; }

        // Связь с автором комментария
        [Display(Name = "Автор")]
        public string AuthorId { get; set; }
        public IdentityUser? Author { get; set; }

        // Файлы, прикрепленные к комментарию (опционально)
        public ICollection<CommentAttachment>? Attachments { get; set; }
    }
}