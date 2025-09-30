using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название проекта")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Навигационное свойство для дефектов в этом проекте
        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}
