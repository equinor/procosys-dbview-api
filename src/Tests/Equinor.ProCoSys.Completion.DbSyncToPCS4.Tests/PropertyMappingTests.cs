using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class PropertyMappingTests
{
    private readonly Guid _expectedGuid1Value = new();
    private readonly Guid _expectedGuid2Value = new();
    private SourceTestObject _testObject;
    private readonly string _expectedPropertyInTheObject = "TestGuid";
    private readonly string _expectedNestedPropertyInTheObject = "NestedObject.Guid";
    private readonly string _nestedPropertyMoreThanOneLevel = "NestedObject.NestedNestedObject.Guid";
    private readonly string _expectedNonExistingProperty = "NonExistingProperty";

    [TestInitialize]
    public void Setup() => _testObject = new SourceTestObject(null, _expectedGuid1Value, null, null, null, true, null, new NestedSourceTestObject(_expectedGuid2Value), null, null, null, null);

    [TestMethod]
    public void GetSourceProperty_ShouldReturnProperty_WhenPropertyExist()
    {
        // Act
        var (actualValue, propertyExists) = PropertyMapping.GetSourcePropertyValue(_expectedPropertyInTheObject, _testObject);

        // Assert
        Assert.AreEqual(_expectedGuid1Value, actualValue);
        Assert.AreEqual(true, propertyExists);
    }

    [TestMethod]
    public void GetSourceProperty_ShouldReturnNestedProperty_WhenPropertyExist()
    {
        // Act
        var (actualValue, propertyValueExists) = PropertyMapping.GetSourcePropertyValue(_expectedNestedPropertyInTheObject, _testObject);

        // Assert
        Assert.AreEqual(_expectedGuid2Value, actualValue);
        Assert.IsTrue(propertyValueExists);
    }

    [TestMethod]
    public void GetSourcePropertyValue_ShouldReturnNullAndFalse_WhenPropertyDoesNotExist()
    {
        //Act 
        var (actualValue, propertyExists) = PropertyMapping.GetSourcePropertyValue(_expectedNonExistingProperty, _testObject);

        // Assert
        Assert.IsNull(actualValue);
        Assert.IsFalse(propertyExists);
    }

    [TestMethod]
    public void GetSourceProperty_ShouldReturnException_WhenNestedMoreThanOneLevel()
    {
        //Act 
        var exception = Assert.ThrowsException<Exception>(() => PropertyMapping.GetSourcePropertyValue(_nestedPropertyMoreThanOneLevel, _testObject));

        // Assert
        Assert.AreEqual($"Only one nested level is supported for entities, so {_nestedPropertyMoreThanOneLevel} is not supported.", exception.Message);
    }
}
