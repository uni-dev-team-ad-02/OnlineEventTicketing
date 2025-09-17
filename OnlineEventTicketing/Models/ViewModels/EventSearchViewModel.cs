using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EventSearchViewModel
    {
        [Display(Name = "Search Term")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        public List<EventDisplayViewModel> Results { get; set; } = new List<EventDisplayViewModel>();

        public List<string> AvailableCategories { get; set; } = new List<string> 
        { 
            "Music", "Sports", "Theater", "Conference", "Workshop", "Festival", "Comedy", "Arts", "Food", "Technology" 
        };
    }
}