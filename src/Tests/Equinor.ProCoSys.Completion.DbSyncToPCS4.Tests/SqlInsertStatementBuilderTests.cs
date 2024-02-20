using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SqlInsertStatementBuilderTests : TestsBase
{
    private SqlInsertStatementBuilder _dut;

    private readonly string _expectedSqlInsertStatement =
        "insert into TestTargetTable ( TestOnlyForInsert, TestFixedValue, TestGuid, TestString, TestDateWithTime, TestDate, TestBool, TestInt, TestLibId, WoGuidLibId, SwcrLibId, PersonOid, DocumentId ) " +
        "values ( :TestOnlyForInsert, 'Fixed value', :TestGuid, :TestString, :TestDateWithTime, :TestDate, :TestBool, :TestInt, :TestLibId, :WoGuidLibId, :SwcrLibId, :PersonOid, :DocumentId )";

    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestOnlyForInsert", "testOnlyForInsert" },
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
        { "TestBool", "N" },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup() => _dut = new SqlInsertStatementBuilder(_pcs4RepositoryMock);

    [TestMethod]
    public async Task BuildAsync_ShouldReturnSqlStatement_WhenInputIsCorrect()
    {
        // Arrange
        _pcs4RepositoryMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        // Act
        var (actualSqlInsertStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
        Assert.AreEqual(_expectedSqlInsertStatement, actualSqlInsertStatement);

        AssertTheSqlParameters(_expectedSqlParameters, actualSqlParams);
    }

    [TestMethod]
    public async Task BuildAsync_ShouldReturnSqlStatement_WhenSourceObjectHasNullValues()
    {
        // Arrange
        var sourceTestObject = new SourceTestObject(null, TestGuid, null, null, null, false, null, null!, null, null, null, null);

        _pcs4RepositoryMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        var expectedSqlInsertStatement = "insert into TestTargetTable ( TestFixedValue, TestGuid, TestBool ) " +
            "values ( 'Fixed value', :TestGuid, :TestBool )";

        // Act
        var (actualSqlInsertStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, sourceTestObject, null!, default);

        // Assert
        Assert.AreEqual(expectedSqlInsertStatement, actualSqlInsertStatement);

        AssertTheSqlParameters(_expectedSqlParametersNullValues, actualSqlParams);
    }
}
