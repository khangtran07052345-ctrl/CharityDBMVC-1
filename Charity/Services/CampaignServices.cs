using Charity.Models;
using System.Collections.Generic;
using System.Linq;
namespace Charity.Services
{
    public class CampaignService
    {
        private readonly CharityDBEntities db;   // đúng tên DbContext của nhóm
        public CampaignService(CharityDBEntities db) { this.db = db; }

        // Lấy danh sách có tìm kiếm + lọc trạng thái
        public List<CampaignViewModel> Search(string keyword = null, string status = null)
        {
            var query = db.Campaigns.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Title.Contains(keyword));

            var list = query.Select(c => new CampaignViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                TargetAmount = c.TargetAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DbStatus = c.Status ?? 0,
                // LINQ: chỉ cộng những khoản quyên góp thành công
                RaisedAmount = c.Donations
                                .Where(d => d.Status == 1)
                                .Sum(d => (decimal?)d.Amount) ?? 0
            }).ToList();

            if (!string.IsNullOrEmpty(status))
                list = list.Where(x => x.StatusKey == status).ToList();

            return list;
        }

        public CampaignViewModel GetById(int id)
        {
            return Search().FirstOrDefault(x => x.Id == id);
        }
    }
}