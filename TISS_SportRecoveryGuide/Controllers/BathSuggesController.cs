using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TISS_SportRecoveryGuide.Models;

namespace TISS_SportRecoveryGuide.Controllers
{
    public class BathSuggesController : Controller
    {
        private TISS_SportRecoveryGuideDBEntities _db = new TISS_SportRecoveryGuideDBEntities(); //資料庫

        #region 三溫暖泡水建議方案
        public ActionResult BathSugges(int? conditionId, string message = null)
        {
            var bathTypes = _db.BathType.OrderBy(bt => bt.BathTypeID).ToList();
            var allConditions = _db.BathCondition.ToList();
            var map = _db.BathSuggestionMap.ToList();
            var guidelines = _db.BathUsageGuideline.OrderBy(g => g.BathTypeID).ThenBy(g => g.SortOrder).ToList();

            // 篩選條件
            var selectedConditions = conditionId.HasValue
                ? allConditions.Where(c => c.ConditionID == conditionId.Value).ToList()
                : allConditions;

            var matrix = selectedConditions.Select(condition => new BathSuggestionMatrixViewModel
            {
                ConditionID = condition.ConditionID,
                Purpose = condition.Purpose,
                Timing = condition.Timing,
                Suggestions = bathTypes.Select(bath =>
                {
                    var match = map.FirstOrDefault(m => m.ConditionID == condition.ConditionID && m.BathTypeID == bath.BathTypeID);
                    return new BathSuggestionItemViewModel
                    {
                        BathTypeID = bath.BathTypeID,
                        BathTypeName = bath.BathTypeName,
                        IsRecommended = match != null && match.IsRecommended
                    };
                }).ToList()
            }).ToList();

            ViewBag.BathTypes = bathTypes;
            ViewBag.Guidelines = guidelines;
            ViewBag.SelectedConditionID = conditionId;
            ViewBag.AllConditions = allConditions
                .Select(c => new SelectListItem
                {
                    Value = c.ConditionID.ToString(),
                    Text = $"{c.Purpose} - {c.Timing}"
                })
            .ToList();

            ViewBag.SuccessMessage = (message == "success") ? "感謝您提供資料！" : null;

            return View(matrix);
        }
        #endregion

        #region 使用者選填基本資料
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReferenceInfo(string Gender, int UserAge, string TeamName, string UsagePurpose, string[] UsedType,
    DateTime? UsedDate, bool? IsHelpful, string Feedback, string recaptchaResponse)
        {
            // 基本資料驗證
            if (string.IsNullOrWhiteSpace(TeamName) || TeamName.Length > 10)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "隊伍名稱格式不正確");

            if (UserAge < 10 || UserAge > 99)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "年齡應介於10到99歲之間");

            if (!string.IsNullOrEmpty(Feedback) && Feedback.Length > 150)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "留言請勿超過150字");

            // 驗證 reCAPTCHA
            var recaptchaSecret = "6Lezbh4qAAAAADGP0PVQCGXgPDtujjwPtY-EdyAB";
            using (var client = new WebClient())
            {
                var result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={recaptchaSecret}&response={recaptchaResponse}");
                dynamic json = JsonConvert.DeserializeObject(result);

                if (json.success != true)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "reCAPTCHA 驗證失敗");
            }

            // 建立資料
            var record = new UserReferenceRecord
            {
                Gender = Gender,
                UserAge = UserAge,
                TeamName = TeamName,
                UsagePurpose = UsagePurpose,
                UsedType = UsedType != null ? string.Join(",", UsedType) : null,
                UsedDate = UsedDate,
                IsHelpful = IsHelpful ?? false,
                Feedback = Feedback,
                IPAddress = Request.UserHostAddress,
                UserAgent = Request.UserAgent
            };

            _db.UserReferenceRecord.Add(record);
            _db.SaveChanges();

            return RedirectToAction("BathSugges", new { message = "success" });
        }
        #endregion
    }
}