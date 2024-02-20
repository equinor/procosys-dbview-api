using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SqlDeleteStatementBuilderTests : TestsBase
{
    private SqlDeleteStatementBuilder _dut;

    private readonly string _expectedSqlDeleteStatement = "delete from TestTargetTable where TestGuid = :TestGuid";

    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup() => _dut = new SqlDeleteStatementBuilder(_pcs4RepositoryMock);

    [TestMethod]
    public async Task BuildSqlDeleteStatement_ShouldReturnSqlStatement_WhenInputIsCorrect()
    {
        // Arrange
        _pcs4RepositoryMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
        Assert.AreEqual(_expectedSqlDeleteStatement, actualSqlUpdateStatement);

        AssertTheSqlParameters(_expectedSqlParameters, actualSqlParams);
    }

    [TestMethod]
    public async Task BuildAsync_ShouldThrowException_WhenMissingPrimaryKey()
    {
        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObjectMissingPrimaryKey, null!, default);
        });

        // Assert
        Assert.AreEqual("Primary key given by the property 'TestGuid' is not found in the source object.", exception.Message);
    }
}
