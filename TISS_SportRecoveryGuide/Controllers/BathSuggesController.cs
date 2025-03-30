using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TISS_SportRecoveryGuide.Models;

namespace TISS_SportRecoveryGuide.Controllers
{
    public class BathSuggesController : Controller
    {
        private TISS_SportRecoveryGuideDBEntities _db = new TISS_SportRecoveryGuideDBEntities(); //資料庫

        #region 三溫暖泡水建議方案
        public ActionResult BathSugges()
        {
            var conditions = _db.BathCondition.ToList();
            var bathTypes = _db.BathType.OrderBy(bt => bt.BathTypeID).ToList();
            var map = _db.BathSuggestionMap.ToList();
            var guidelines = _db.BathUsageGuideline
                                .OrderBy(g => g.BathTypeID)
                                .ThenBy(g => g.SortOrder)
                                .ToList();

            var matrix = conditions.Select(condition => new BathSuggestionMatrixViewModel
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
                        IsRecommended = match != null && match.IsRecommended == true
                    };
                }).ToList()
            }).ToList();

            ViewBag.BathTypes = bathTypes;
            ViewBag.Guidelines = guidelines;

            return View(matrix);
        }
        #endregion
    }
}