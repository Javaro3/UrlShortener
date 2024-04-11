using System.ComponentModel.DataAnnotations;

namespace Domains.ViewModels {
    public class UpdateShortUrlViewModel {
        public int Id { get; set; }

        [Display(Name = "Url")]
        [Required(ErrorMessage = "Url is required")]
        public string Url { get; set; }
    }
}
