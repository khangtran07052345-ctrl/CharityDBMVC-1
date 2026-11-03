using System;
using System.Linq;
using System.Web.Mvc;
using Charity.Models;

namespace Charity.Controllers
{
    public class DonationController : Controller
    {
        private readonly CharityDBEntities db = new CharityDBEntities();

        [HttpGet]
        public ActionResult Create(int campaignId)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            var campaign = db.Campaigns.Find(campaignId);
            if (campaign == null) return HttpNotFound();
            if (!CanReceiveDonation(campaign))
            {
                TempData["Error"] = "Chiến dịch đã đóng, hết hạn hoặc đạt mục tiêu nên không nhận quyên góp nữa.";
                return RedirectToAction("Details", "Campaign", new { id = campaignId });
            }
            ViewBag.Campaign = campaign;
            return View(new Donation { CampaignId = campaignId, Amount = 10000m, IsAnonymous = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CampaignId,Amount,PaymentMethod,Message,IsAnonymous")] Donation donation)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            var campaign = db.Campaigns.Find(donation.CampaignId);
            if (campaign == null) return HttpNotFound();
            if (!CanReceiveDonation(campaign))
            {
                TempData["Error"] = "Chiến dịch đã đóng, hết hạn hoặc đạt mục tiêu nên không nhận quyên góp nữa.";
                return RedirectToAction("Details", "Campaign", new { id = campaign.Id });
            }
            if (donation.Amount <= 0) ModelState.AddModelError("Amount", "Số tiền phải lớn hơn 0.");
            if (donation.Amount > 1000000000000m) ModelState.AddModelError("Amount", "Số tiền vượt quá giới hạn demo.");
            if (string.IsNullOrWhiteSpace(donation.PaymentMethod)) donation.PaymentMethod = "Mô phỏng";
            if (!ModelState.IsValid)
            {
                ViewBag.Campaign = campaign;
                return View(donation);
            }

            donation.UserId = Convert.ToInt32(Session["UserId"]);
            donation.TransactionCode = "DEMO-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
            donation.DonatedAt = DateTime.Now;
            donation.Status = 1; // Giao dịch mô phỏng được ghi nhận là hợp lệ.
            db.Donations.Add(donation);
            db.SaveChanges();

            var raised = db.Donations.Where(d => d.CampaignId == campaign.Id && d.Status == 1).Select(d => (decimal?)d.Amount).Sum() ?? 0m;
            if (raised >= campaign.TargetAmount)
            {
                campaign.Status = 2; // Hoàn thành mục tiêu.
                db.SaveChanges();
            }
            TempData["Success"] = "Quyên góp mô phỏng thành công. Mã giao dịch: " + donation.TransactionCode;
            return RedirectToAction("Details", "Campaign", new { id = campaign.Id });
        }

        private bool CanReceiveDonation(Campaign campaign)
        {
            if (campaign.Status.HasValue && campaign.Status.Value != 0) return false;
            if (campaign.EndDate < DateTime.Now || campaign.StartDate > DateTime.Now) return false;
            var raised = db.Donations.Where(d => d.CampaignId == campaign.Id && d.Status == 1).Select(d => (decimal?)d.Amount).Sum() ?? 0m;
            return raised < campaign.TargetAmount;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
