﻿using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TISS_SportRecoveryGuide.Models;
using TISS_SportRecoveryGuide.ViewModels;

namespace TISS_SportRecoveryGuide.Controllers
{
    public class BathSuggesController : Controller
    {
        private TISS_SportRecoveryGuideDBEntities _db = new TISS_SportRecoveryGuideDBEntities(); //資料庫

        #region 三溫暖泡水建議方案
        public ActionResult BathSugges(int? conditionId, string iceTolerance = null, string message = null)
        {
            var bathTypes = _db.BathType.OrderBy(bt => bt.BathTypeID).ToList();
            var allConditions = _db.BathCondition.ToList();
            var map = _db.BathSuggestionMap.ToList();
            var guidelines = _db.BathUsageGuideline.OrderBy(g => g.BathTypeID).ThenBy(g => g.SortOrder).ToList();

            var filteredGuidelines = string.IsNullOrEmpty(iceTolerance) ? guidelines
                : guidelines.Where(g => g.ToleranceType == iceTolerance).ToList();

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

            var recommendedBathIDs = matrix.SelectMany(m => m.Suggestions).Where(s => s.IsRecommended)
                                            .Select(s => s.BathTypeID).Distinct().ToList();

            var viewModel = new BathSuggestionViewModel
            {
                Matrix = matrix,
                IceBathGuidelines = filteredGuidelines,
                SelectedConditionID = conditionId,
                SelectedTolerance = iceTolerance,
                AllConditions = allConditions
                    .Select(c => new SelectListItem
                {
                    Value = c.ConditionID.ToString(),
                    Text = $"{c.Purpose} - {c.Timing}"
                }).ToList(),

                BathTypes = bathTypes,

                ShowIceBath = recommendedBathIDs.Contains(1),      // 假設 1 是冰水浴
                ShowContrastBath = recommendedBathIDs.Contains(2), // 假設 2 是對比浴
                ShowHotBath = recommendedBathIDs.Contains(3)       // 假設 3 是熱水浴
            };

            ViewBag.SuccessMessage = (message == "success") ? "感謝您提供資料！" : null;

            return View(viewModel);
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