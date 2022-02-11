using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.ThreeDEcoTag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class DetailsHelper
{
    public int TagNoIdx { get; }
    public int ProjectIdx { get; }
    public int CommPkgNoIdx { get; }
    public int McPkgNoIdx { get; }
    public int RfccIdx { get; }
    public int RfocIdx { get; }
    public int ResponsibleIdx { get; }

    public DetailsHelper(TagModel model)
    {
        TagNoIdx = GetAndVerifyColumnIdx(model.Heading, 0, "TagNo");
        ProjectIdx = GetAndVerifyColumnIdx(model.Heading, 1, "Project");
        CommPkgNoIdx = GetAndVerifyColumnIdx(model.Heading, 3, "CommPkgNo");
        McPkgNoIdx = GetAndVerifyColumnIdx(model.Heading, 5, "McPkgNo");
        RfccIdx = GetAndVerifyColumnIdx(model.Heading, 9, "RFCC");
        RfocIdx = GetAndVerifyColumnIdx(model.Heading, 10, "RFOC");
        ResponsibleIdx = GetAndVerifyColumnIdx(model.Heading, 11, "Responsible");
    }

    public string GetUniqeKeyForTag(IEnumerable<object> tagData)
        => $"{tagData.ElementAt(TagNoIdx)}_{tagData.ElementAt(ProjectIdx)}_{tagData.ElementAt(CommPkgNoIdx)}_{tagData.ElementAt(ResponsibleIdx)}";

    private static int GetAndVerifyColumnIdx(IEnumerable<string> heading, int colIdx, string colName)
    {
        var col = heading.ElementAt(colIdx);
        Assert.AreEqual(colName, col);
        return colIdx;
    }
}
