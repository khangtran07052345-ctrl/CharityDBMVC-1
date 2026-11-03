using System;

namespace Charity.Models
{
    public class CampaignViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal RaisedAmount { get; set; }   // tổng tiền đã nhận (tính bằng LINQ)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Cột Status trong database (kiểu SỐ). Quy ước tạm: 1 = đang hoạt động
        public int DbStatus { get; set; }

        // Tiến độ %, làm tròn, tối đa 100 để thanh không bị tràn
        public int ProgressPercent
        {
            get
            {
                if (TargetAmount <= 0) return 0;
                var p = (int)Math.Round(RaisedAmount / TargetAmount * 100);
                return p > 100 ? 100 : p;
            }
        }

        // "reached" = đạt mục tiêu, "ended" = kết thúc, "ongoing" = đang diễn ra
        public string StatusKey
        {
            get
            {
                if (RaisedAmount >= TargetAmount) return "reached";
                if (DbStatus != 1 || EndDate < DateTime.Now) return "ended";
                return "ongoing";
            }
        }

        public string StatusText
        {
            get
            {
                switch (StatusKey)
                {
                    case "reached": return "Đạt mục tiêu";
                    case "ended": return "Đã kết thúc";
                    default: return "Đang diễn ra";
                }
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (StatusKey)
                {
                    case "reached": return "bg-success";
                    case "ended": return "bg-secondary";
                    default: return "bg-primary";
                }
            }
        }
    }
}