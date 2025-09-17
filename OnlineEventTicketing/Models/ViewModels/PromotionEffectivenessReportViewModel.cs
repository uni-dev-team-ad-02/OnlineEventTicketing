using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class PromotionEffectivenessReportViewModel
    {
        [Display(Name = "Total Promotions")]
        public int TotalPromotions { get; set; }

        [Display(Name = "Active Promotions")]
        public int ActivePromotions { get; set; }

        [Display(Name = "Total Discount Given")]
        public decimal TotalDiscountGiven { get; set; }

        [Display(Name = "Total Discount Given")]
        public string FormattedTotalDiscountGiven => TotalDiscountGiven.ToString("C");

        [Display(Name = "Total Promotion Usage")]
        public int TotalPromotionUsage { get; set; }

        [Display(Name = "Average Discount")]
        public decimal AverageDiscount { get; set; }

        [Display(Name = "Average Discount")]
        public string FormattedAverageDiscount => $"{AverageDiscount:F1}%";

        [Display(Name = "Promotion Details")]
        public List<PromotionDetailViewModel> PromotionDetails { get; set; } = new List<PromotionDetailViewModel>();

        [Display(Name = "Top Performing Promotions")]
        public List<PromotionDetailViewModel> TopPerformingPromotions => 
            PromotionDetails.OrderByDescending(p => p.UsageCount).Take(5).ToList();
    }

    public class PromotionDetailViewModel
    {
        public int PromotionId { get; set; }

        [Display(Name = "Promotion Code")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Event Title")]
        public string EventTitle { get; set; } = string.Empty;

        [Display(Name = "Discount Percentage")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Discount Percentage")]
        public string FormattedDiscountPercentage => $"{DiscountPercentage}%";

        [Display(Name = "Usage Count")]
        public int UsageCount { get; set; }

        [Display(Name = "Total Discount Given")]
        public decimal TotalDiscountGiven { get; set; }

        [Display(Name = "Total Discount Given")]
        public string FormattedTotalDiscountGiven => TotalDiscountGiven.ToString("C");

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Duration")]
        public string FormattedDuration => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayName => GetStatusDisplayName();

        [Display(Name = "Status")]
        public string StatusBadgeClass => GetStatusBadgeClass();

        [Display(Name = "Effectiveness Rating")]
        public string EffectivenessRating => GetEffectivenessRating();

        [Display(Name = "Effectiveness Rating")]
        public string EffectivenessBadgeClass => GetEffectivenessBadgeClass();

        private string GetStatusDisplayName()
        {
            if (!IsActive)
                return "Disabled";

            var now = DateTime.Now;
            
            if (now < StartDate)
                return "Scheduled";
            
            if (now > EndDate)
                return "Expired";
                
            return "Active";
        }

        private string GetStatusBadgeClass()
        {
            if (!IsActive)
                return "bg-secondary";

            var now = DateTime.Now;
            
            if (now < StartDate)
                return "bg-warning";
            
            if (now > EndDate)
                return "bg-danger";
                
            return "bg-success";
        }

        private string GetEffectivenessRating()
        {
            return UsageCount switch
            {
                >= 50 => "Excellent",
                >= 20 => "Good",
                >= 10 => "Average",
                >= 5 => "Below Average",
                _ => "Poor"
            };
        }

        private string GetEffectivenessBadgeClass()
        {
            return UsageCount switch
            {
                >= 50 => "bg-success",
                >= 20 => "bg-primary",
                >= 10 => "bg-info",
                >= 5 => "bg-warning",
                _ => "bg-danger"
            };
        }
    }
}