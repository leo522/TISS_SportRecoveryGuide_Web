using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult SubmitReferenceInfo(string Gender, int UserAge, string TeamName)
        {
            var record = new UserReferenceRecord
            {
                Gender = Gender,
                UserAge = UserAge,
                TeamName = TeamName
            };

            _db.UserReferenceRecord.Add(record);
            _db.SaveChanges();

            return RedirectToAction("BathSugges", new { message = "success" });
        }
        #endregion
    }
}