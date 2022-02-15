using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.ThreeDEcoTag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class DetailsHelper
{
    public int TagNoIdx { get; private set; }
    public int ProjectIdx { get; private set; }
    public int CommPkgNoIdx { get; private set; }
    public int McPkgNoIdx { get; private set; }
    public int RfccIdx { get; private set; }
    public int RfocIdx { get; private set; }
    public int ResponsibleIdx { get; private set; }
    public int FormularTypeIdx { get; private set; }

    public DetailsHelper(TagModel model) => AssertCorrectHeadings(model);

    private void AssertCorrectHeadings(TagModel model)
    {
        TagNoIdx =          GetAndVerifyColumnIdx(model.Heading, 0, "TagNo");
        ProjectIdx =        GetAndVerifyColumnIdx(model.Heading, 1, "Project");
                            GetAndVerifyColumnIdx(model.Heading, 2, "PunchCount");
        CommPkgNoIdx =      GetAndVerifyColumnIdx(model.Heading, 3, "CommPkgNo");
                            GetAndVerifyColumnIdx(model.Heading, 4, "CommPkgDesc");
        McPkgNoIdx =        GetAndVerifyColumnIdx(model.Heading, 5, "McPkgNo");
                            GetAndVerifyColumnIdx(model.Heading, 6, "McPkgDesc");
                            GetAndVerifyColumnIdx(model.Heading, 7, "Priority");
                            GetAndVerifyColumnIdx(model.Heading, 8, "Phase");
        RfccIdx =           GetAndVerifyColumnIdx(model.Heading, 9, "RFCC");
        RfocIdx =           GetAndVerifyColumnIdx(model.Heading, 10, "RFOC");
        ResponsibleIdx =    GetAndVerifyColumnIdx(model.Heading, 11, "Responsible");
                            GetAndVerifyColumnIdx(model.Heading, 12, "Status");
        FormularTypeIdx =   GetAndVerifyColumnIdx(model.Heading, 13, "FormularType");
    }

    public string GetUniqeKeyForTag(IEnumerable<object> tagData)
        => $"{tagData.ElementAt(TagNoIdx)}_{tagData.ElementAt(ProjectIdx)}_{tagData.ElementAt(CommPkgNoIdx)}_{tagData.ElementAt(ResponsibleIdx)}_{tagData.ElementAt(FormularTypeIdx)}";

    private static int GetAndVerifyColumnIdx(IEnumerable<string> heading, int colIdx, string colName)
    {
        var col = heading.ElementAt(colIdx);
        Assert.AreEqual(colName, col);
        return colIdx;
    }
}
