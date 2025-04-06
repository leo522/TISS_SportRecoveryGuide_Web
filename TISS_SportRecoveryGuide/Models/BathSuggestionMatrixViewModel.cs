using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TISS_SportRecoveryGuide.Models
{
    public class BathSuggestionMatrixViewModel
    {
        public string Purpose { get; set; }
        public string Timing { get; set; }
        public int ConditionID { get; set; }
        public List<BathSuggestionItemViewModel> Suggestions { get; set; }
    }

    public class BathSuggestionItemViewModel
    {
        public int BathTypeID { get; set; }
        public string BathTypeName { get; set; }
        public bool IsRecommended { get; set; }
    }

    public class BathSuggestionViewModel
    {
        public List<BathSuggestionMatrixViewModel> Matrix { get; set; }
        public List<BathUsageGuideline> IceBathGuidelines { get; set; }
        public string SelectedTolerance { get; set; }
        public int? SelectedConditionID { get; set; }
        public List<SelectListItem> AllConditions { get; set; }
        public List<BathType> BathTypes { get; set; }
    }
}