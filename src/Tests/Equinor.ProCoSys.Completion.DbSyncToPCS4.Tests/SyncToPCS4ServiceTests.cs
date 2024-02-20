using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SyncToPCS4ServiceTests : TestsBase
{
    protected readonly string SourceObjectNameMissingConfig = "NotInConfiguration";

    private IOptionsMonitor<SyncToPCS4Options> _optionsMock;
    private SyncToPCS4Service _dut;

    [TestInitialize]
    public void Setup()
    {
        _optionsMock = Substitute.For<IOptionsMonitor<SyncToPCS4Options>>();
        _optionsMock.CurrentValue.Returns(new SyncToPCS4Options { ConnectionString = "blah", Enabled = true});
        _dut = new SyncToPCS4Service(_pcs4RepositoryMock, _optionsMock);
    }

    [TestMethod]
    public async Task SyncNewObjectAsync_ShouldThrowException_WhenMissingConfigForObject()
    {
        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await _dut.SyncNewObjectAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
        });

        // Assert
        Assert.AreEqual($"Mapping is not implemented for source object with name '{SourceObjectNameMissingConfig}'.", exception.Message);
    }

    [TestMethod]
    public async Task SyncNewObjectAsync_ShouldNotThrowException_WhenMissingConfigForObject_AndDisabledSync()
    {
        // Arrange 
        _optionsMock.CurrentValue.Returns(new SyncToPCS4Options { ConnectionString = "blah", Enabled = false });

        // Act
        await _dut.SyncNewObjectAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
    }

    [TestMethod]
    public async Task SyncObjectUpdateAsync_ShouldThrowException_WhenMissingConfigForObject()
    {
        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await _dut.SyncObjectUpdateAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
        });

        // Assert
        Assert.AreEqual($"Mapping is not implemented for source object with name '{SourceObjectNameMissingConfig}'.", exception.Message);
    }

    [TestMethod]
    public async Task SyncObjectUpdateAsync_ShouldNotThrowException_WhenMissingConfigForObject_AndDisabledSync()
    {
        // Arrange 
        _optionsMock.CurrentValue.Returns(new SyncToPCS4Options { ConnectionString = "blah", Enabled = false });

        // Act
        await _dut.SyncObjectUpdateAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
    }

    [TestMethod]
    public async Task SyncObjectDeletionAsync_ShouldThrowException_WhenMissingConfigForObject()
    {
        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await _dut.SyncObjectDeletionAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
        });

        // Assert
        Assert.AreEqual($"Mapping is not implemented for source object with name '{SourceObjectNameMissingConfig}'.", exception.Message);
    }

    [TestMethod]
    public async Task SyncObjectDeletionAsync_ShouldNotThrowException_WhenMissingConfigForObject_AndDisabledSync()
    {
        // Arrange 
        _optionsMock.CurrentValue.Returns(new SyncToPCS4Options { ConnectionString = "blah", Enabled = false });

        // Act
        await _dut.SyncObjectDeletionAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
    }
}
