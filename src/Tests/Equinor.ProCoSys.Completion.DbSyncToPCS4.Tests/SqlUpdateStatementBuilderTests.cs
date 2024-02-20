using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SqlUpdateStatementBuilderTests : TestsBase
{
    private SqlUpdateStatementBuilder _dut;

    private readonly string _expectedSqlUpdateStatement =
        "update TestTargetTable set " +
        "TestString = :TestString, " +
        "TestDateWithTime = :TestDateWithTime, " +
        "TestDate = :TestDate, " +
        "TestBool = :TestBool, " +
        "TestInt = :TestInt, " +
        "TestLibId = :TestLibId, " +
        "WoGuidLibId = :WoGuidLibId, " +
        "SwcrLibId = :SwcrLibId, " +
        "PersonOid = :PersonOid, " +
        "DocumentId = :DocumentId " +
        "where TestGuid = :TestGuid";


    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestString", "test" },
        { "TestDateWithTime", new DateTime(2023, 11, 29, 10, 20, 30)},
        { "TestDate", new DateTime(2023, 11, 30, 10, 20, 30) },
        { "TestBool", "Y" },
        { "TestInt" , 1234 },
        { "TestLibId", 123456789 } ,
        { "WoGuidLibId" , 123456789 },
        { "SwcrLibId" , 123456789 } ,
        { "PersonOid" , 123456789 },
        { "DocumentId" , 123456789 },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    private readonly Dictionary<string, object> _expectedSqlParametersNullValues = new()
    {
        { "TestString", null },
        { "TestDateWithTime", null},
        { "TestDate", null },
        { "TestBool", "N" },
        { "TestInt" , null },
        { "TestLibId", null } ,
        { "WoGuidLibId" , null },
        { "SwcrLibId" , null } ,
        { "PersonOid" , null },
        { "DocumentId" , null },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup() => _dut = new SqlUpdateStatementBuilder(_pcs4RepositoryMock);

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatement_WhenInputIsCorrect()
    {
        // Arrange
        _pcs4RepositoryMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
        Assert.AreEqual(_expectedSqlUpdateStatement, actualSqlUpdateStatement);

        AssertTheSqlParameters(_expectedSqlParameters, actualSqlParams);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatement_WhenSourceObjectHasNullValues()
    {
        // Arrange
        _pcs4RepositoryMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);
        _sourceTestObject = new SourceTestObject(null, TestGuid, null, null, null, false, null, new NestedSourceTestObject(null), null, null, null, null);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
        Assert.AreEqual(_expectedSqlUpdateStatement, actualSqlUpdateStatement);

        AssertTheSqlParameters(_expectedSqlParametersNullValues, actualSqlParams);

    }
}
