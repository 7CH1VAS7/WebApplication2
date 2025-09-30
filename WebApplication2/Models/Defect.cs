using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public enum DefectStatus
    {
        New,        // Новая
        InProgress, // В работе
        OnReview,   // На проверке
        Closed,     // Закрыта
        Cancelled   // Отменена
    }

    public enum DefectPriority
    {
        Low,    // Низкий
        Medium, // Средний
        High    // Высокий
    }

    public class Defect
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Статус")]
        public DefectStatus Status { get; set; } = DefectStatus.New;

        [Display(Name = "Приоритет")]
        public DefectPriority Priority { get; set; } = DefectPriority.Medium;

        [Display(Name = "Срок устранения")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // Связь с Проектом
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // Связь с пользователем (Исполнитель)
        [Display(Name = "Исполнитель")]
        public string? AssigneeId { get; set; }
        public IdentityUser? Assignee { get; set; }

        // Связь с пользователем (Создатель)
        public string? CreatorId { get; set; }
        public IdentityUser? Creator { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства для комментариев и вложений
        public ICollection<DefectComment> Comments { get; set; } = new List<DefectComment>();
        public ICollection<DefectAttachment> Attachments { get; set; } = new List<DefectAttachment>();

    }
}
