using MathCalcPrice.RevitsUtils;
using System.Collections.Generic;
using System.ComponentModel;

namespace MathCalcPrice.Entity
{
    public enum ExcelFileType
    {
        Original,
        Debug,
        Filtration,
        Rebar,
        Calculator
    }

    public enum FiltrationType
    {
        UseOnlyOne,
        UseAll
    }

    public class Settings
    {
        public List<LinkFile> LinkedFiles { get; set; } = new List<LinkFile>();
        public BindingList<Filter> Filters { get; set; } = new BindingList<Filter>();
        public ClassificatorsCache ClassificatorsCache { get; set; } = default;
        public FiltrationType FiltrationType { get; set; } = FiltrationType.UseOnlyOne;

        public ExcelFileType ExcelFileType { get; set; } = ExcelFileType.Original;

        public bool LoadToGoogleDrive { get; set; } = false;
        public bool UpdateCalcTemplate { get; set; } = false;
        public string ObjectNameToUpdate { get; set; } = "";
    }
}
