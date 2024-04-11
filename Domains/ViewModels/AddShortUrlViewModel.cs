using System.ComponentModel.DataAnnotations;

namespace Domains.ViewModels {
    public class AddShortUrlViewModel {
        [Display(Name = "Url")]
        [Required(ErrorMessage = "Url is required")]
        public string Url { get; set; }
    }
}
