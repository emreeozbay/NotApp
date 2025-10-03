using System.ComponentModel.DataAnnotations;

namespace NotApp.Models
{
    public enum DocumentType
    {
        [Display(Name = "Ders Notu")] LectureNote = 0,
        [Display(Name = "Sunum / Slayt")] Presentation = 1,
        [Display(Name = "Sınav Kağıdı")] ExamPaper = 2,
        [Display(Name = "Ödev")] Assignment = 3,
        [Display(Name = "Lab Raporu")] LabReport = 4,
        [Display(Name = "Proje Raporu")] ProjectReport = 5,
        [Display(Name = "Kitap Özeti")] BookSummary = 6,
        [Display(Name = "Diğer")] Other = 7
    }
}
