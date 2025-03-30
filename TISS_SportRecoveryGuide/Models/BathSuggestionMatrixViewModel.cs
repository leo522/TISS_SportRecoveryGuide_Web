using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
}