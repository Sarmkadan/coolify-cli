using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Collections.Generic;
using CoolifyCli.Utilities;
using CoolifyCli.Infrastructure;

namespace CoolifyCli.Tests;

public class ConfigurationHelperTests : IDisposable
{
    private readonly string _testConfigDir;
    private readonly string _testConfigPath;

    public ConfigurationHelperTests()
    {
        // Create test paths in temp directory
        _testConfigDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testConfigPath = Path.Combine(_testConfigDir, "config.json");
    }

    public void Dispose()
    {
        // Clean up test directory
        try
        {
            if (Directory.Exists(_testConfigDir))
            {
                Directory.Delete(_testConfigDir, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private void SetTestConfigPath()
    {
        var configPathField = typeof(Constants.Paths).GetField("_configFile",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (configPathField != null)
        {
            // Store the current value before changing it
            var currentValue = configPathField.GetValue(null);
            // Use reflection to set the backing field value
            var backingField = typeof(Constants.Paths).GetField("<ConfigFile>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (backingField != null)
            {
                backingField.SetValue(null, _testConfigPath);
            }
        }
    }

    private void ResetConfigPath()
    {
        var backingField = typeof(Constants.Paths).GetField("<ConfigFile>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (backingField != null)
        {
            // Restore to original value
            backingField.SetValue(null, Constants.Paths.ConfigFile);
        }
    }

    [Fact]
    public void LoadConfiguration_WhenConfigFileDoesNotExist_ReturnsEmptyDictionary()
    {
        // Arrange
        SetTestConfigPath();
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }

        // Act
        var result = ConfigurationHelper.LoadConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        ResetConfigPath();
    }

    [Fact]
    public void LoadConfiguration_WhenConfigFileExists_ReturnsDeserializedDictionary()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "TestKey", "TestValue" },
            { "NumberKey", 42 },
            { "BoolKey", true }
        };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        var result = ConfigurationHelper.LoadConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().ContainKey("TestKey").WhoseValue.Should().Be("TestValue");
        result.Should().ContainKey("NumberKey").WhoseValue.Should().Be(42);
        result.Should().ContainKey("BoolKey").WhoseValue.Should().Be(true);
        ResetConfigPath();
    }

    [Fact]
    public void LoadConfiguration_WhenConfigFileIsInvalidJson_ReturnsEmptyDictionary()
    {
        // Arrange
        SetTestConfigPath();
        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, "invalid json {{{");

        // Act
        var result = ConfigurationHelper.LoadConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        ResetConfigPath();
    }

    [Fact]
    public void SaveConfiguration_WithValidConfig_SavesToFile()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "ApiKey", "test-api-key-123" },
            { "Timeout", 30 }
        };

        // Act
        ConfigurationHelper.SaveConfiguration(testConfig);

        // Assert
        File.Exists(_testConfigPath).Should().BeTrue();
        var savedContent = File.ReadAllText(_testConfigPath);
        savedContent.Should().Contain("ApiKey");
        savedContent.Should().Contain("test-api-key-123");
        ResetConfigPath();
    }

    [Fact]
    public void SaveConfiguration_CreatesDirectoryIfNotExists()
    {
        // Arrange
        SetTestConfigPath();
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }

        var testConfig = new Dictionary<string, object> { { "Key", "Value" } };

        // Act
        ConfigurationHelper.SaveConfiguration(testConfig);

        // Assert
        Directory.Exists(_testConfigDir).Should().BeTrue();
        File.Exists(_testConfigPath).Should().BeTrue();
        ResetConfigPath();
    }

    [Fact]
    public void GetConfigValue_WhenKeyExists_ReturnsValue()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "DatabaseUrl", "postgres://localhost:5432/mydb" },
            { "MaxConnections", 10 }
        };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        var result = ConfigurationHelper.GetConfigValue("DatabaseUrl");

        // Assert
        result.Should().Be("postgres://localhost:5432/mydb");
        ResetConfigPath();
    }

    [Fact]
    public void GetConfigValue_WhenKeyDoesNotExist_ReturnsDefaultValue()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "ExistingKey", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        var result = ConfigurationHelper.GetConfigValue("NonExistentKey", "DefaultValue");

        // Assert
        result.Should().Be("DefaultValue");
        ResetConfigPath();
    }

    [Fact]
    public void GetConfigValue_WhenKeyDoesNotExistAndNoDefault_ReturnsNull()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "ExistingKey", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        var result = ConfigurationHelper.GetConfigValue("NonExistentKey");

        // Assert
        result.Should().BeNull();
        ResetConfigPath();
    }

    [Fact]
    public void SetConfigValue_AddsNewKeyValuePair()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "ExistingKey", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.SetConfigValue("NewKey", "NewValue");

        // Assert
        var loadedConfig = ConfigurationHelper.LoadConfiguration();
        loadedConfig.Should().ContainKey("NewKey").WhoseValue.Should().Be("NewValue");
        loadedConfig.Should().ContainKey("ExistingKey").WhoseValue.Should().Be("Value");
        ResetConfigPath();
    }

    [Fact]
    public void SetConfigValue_UpdatesExistingKeyValuePair()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "ExistingKey", "OldValue" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.SetConfigValue("ExistingKey", "NewValue");

        // Assert
        var loadedConfig = ConfigurationHelper.LoadConfiguration();
        loadedConfig.Should().ContainKey("ExistingKey").WhoseValue.Should().Be("NewValue");
        loadedConfig.Should().HaveCount(1);
        ResetConfigPath();
    }

    [Fact]
    public void DeleteConfigValue_WhenKeyExists_RemovesKey()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "KeyToDelete", "Value" },
            { "KeyToKeep", "Value" }
        };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.DeleteConfigValue("KeyToDelete");

        // Assert
        var loadedConfig = ConfigurationHelper.LoadConfiguration();
        loadedConfig.Should().NotContainKey("KeyToDelete");
        loadedConfig.Should().ContainKey("KeyToKeep");
        loadedConfig.Should().HaveCount(1);
        ResetConfigPath();
    }

    [Fact]
    public void DeleteConfigValue_WhenKeyDoesNotExist_DoesNotThrow()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "ExistingKey", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act & Assert (should not throw)
        ConfigurationHelper.DeleteConfigValue("NonExistentKey");

        var loadedConfig = ConfigurationHelper.LoadConfiguration();
        loadedConfig.Should().HaveCount(1);
        ResetConfigPath();
    }

    [Fact]
    public void InitializeConfigDirectory_CreatesConfigDirectory()
    {
        // Arrange
        SetTestConfigPath();
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }

        // Act
        ConfigurationHelper.InitializeConfigDirectory();

        // Assert
        Directory.Exists(_testConfigDir).Should().BeTrue();
        File.Exists(_testConfigPath).Should().BeTrue();
        ResetConfigPath();
    }

    [Fact]
    public void InitializeConfigDirectory_CreatesDefaultConfigWithDefaultValues()
    {
        // Arrange
        SetTestConfigPath();
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }

        // Act
        ConfigurationHelper.InitializeConfigDirectory();

        // Assert
        var config = ConfigurationHelper.LoadConfiguration();
        config.Should().ContainKey("DefaultEnvironment").WhoseValue.Should().Be("production");
        config.Should().ContainKey("LastUsedApplication").WhoseValue.Should().Be(-1);
        config.Should().ContainKey("LastUsedDatabase").WhoseValue.Should().Be(-1);
        config.Should().ContainKey("VerboseMode").WhoseValue.Should().Be(false);
        ResetConfigPath();
    }

    [Fact]
    public void InitializeConfigDirectory_CreatesLogsDirectory()
    {
        // Arrange
        SetTestConfigPath();
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }

        // Act
        ConfigurationHelper.InitializeConfigDirectory();

        // Assert
        var logsDir = Path.Combine(_testConfigDir, "logs");
        Directory.Exists(logsDir).Should().BeTrue();
        ResetConfigPath();
    }

    [Fact]
    public void DisplayConfiguration_DoesNotThrow()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "NormalKey", "NormalValue" },
            { "ApiKey", "secret-key-123" },
            { "Password", "secret-password" }
        };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act & Assert (should not throw)
        ConfigurationHelper.DisplayConfiguration();
        ResetConfigPath();
    }

    [Fact]
    public void ResetConfiguration_RemovesConfigFile()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "Key", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.ResetConfiguration();

        // Assert
        File.Exists(_testConfigPath).Should().BeFalse();
        ResetConfigPath();
    }

    [Fact]
    public void ResetConfiguration_CreatesEmptyConfig()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object> { { "Key", "Value" } };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.ResetConfiguration();

        // Assert
        var config = ConfigurationHelper.LoadConfiguration();
        config.Should().BeEmpty();
        ResetConfigPath();
    }

    [Fact]
    public void ValidateConfiguration_WhenApiKeyEnvironmentVariableIsSet_ReturnsNoErrors()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Constants.Environment.ApiKeyVariableName, "test-api-key");
        Environment.SetEnvironmentVariable(Constants.Environment.ApiUrlVariableName, "https://api.example.com");

        // Act
        var errors = ConfigurationHelper.ValidateConfiguration();

        // Assert
        errors.Should().BeEmpty();

        // Cleanup
        Environment.SetEnvironmentVariable(Constants.Environment.ApiKeyVariableName, null);
        Environment.SetEnvironmentVariable(Constants.Environment.ApiUrlVariableName, null);
    }

    [Fact]
    public void ValidateConfiguration_WhenApiKeyEnvironmentVariableIsNotSet_ReturnsError()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Constants.Environment.ApiKeyVariableName, null);
        Environment.SetEnvironmentVariable(Constants.Environment.ApiUrlVariableName, null);

        // Act
        var errors = ConfigurationHelper.ValidateConfiguration();

        // Assert
        errors.Should().ContainSingle();
        errors[0].Should().Contain(Constants.Environment.ApiKeyVariableName);
    }

    [Fact]
    public void ValidateConfiguration_WhenApiUrlIsInvalidUri_ReturnsError()
    {
        // Arrange
        Environment.SetEnvironmentVariable(Constants.Environment.ApiKeyVariableName, "test-api-key");
        Environment.SetEnvironmentVariable(Constants.Environment.ApiUrlVariableName, "not-a-valid-uri");

        // Act
        var errors = ConfigurationHelper.ValidateConfiguration();

        // Assert
        errors.Should().ContainSingle();
        errors[0].Should().Contain("API URL must be a valid URI");

        // Cleanup
        Environment.SetEnvironmentVariable(Constants.Environment.ApiKeyVariableName, null);
        Environment.SetEnvironmentVariable(Constants.Environment.ApiUrlVariableName, null);
    }

    [Fact]
    public void ExportConfiguration_WithValidFilePath_ExportsConfig()
    {
        // Arrange
        SetTestConfigPath();
        var testConfig = new Dictionary<string, object>
        {
            { "ExportKey", "ExportValue" },
            { "Number", 123 }
        };

        Directory.CreateDirectory(_testConfigDir);
        File.WriteAllText(_testConfigPath, System.Text.Json.JsonSerializer.Serialize(testConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var exportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "export.json");
        var exportDir = Path.GetDirectoryName(exportPath);
        Directory.CreateDirectory(exportDir);

        // Act
        ConfigurationHelper.ExportConfiguration(exportPath);

        // Assert
        File.Exists(exportPath).Should().BeTrue();
        var exportedContent = File.ReadAllText(exportPath);
        exportedContent.Should().Contain("ExportKey");
        exportedContent.Should().Contain("ExportValue");

        // Cleanup
        if (File.Exists(exportPath)) File.Delete(exportPath);
        if (Directory.Exists(exportDir)) Directory.Delete(exportDir, true);
        ResetConfigPath();
    }

    [Fact]
    public void ImportConfiguration_WithValidFilePath_ImportsConfig()
    {
        // Arrange
        SetTestConfigPath();
        var importConfig = new Dictionary<string, object>
        {
            { "ImportKey", "ImportValue" },
            { "Enabled", true }
        };

        var importPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "import.json");
        var importDir = Path.GetDirectoryName(importPath);
        Directory.CreateDirectory(importDir);
        File.WriteAllText(importPath, System.Text.Json.JsonSerializer.Serialize(importConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // Act
        ConfigurationHelper.ImportConfiguration(importPath);

        // Assert
        var loadedConfig = ConfigurationHelper.LoadConfiguration();
        loadedConfig.Should().ContainKey("ImportKey").WhoseValue.Should().Be("ImportValue");
        loadedConfig.Should().ContainKey("Enabled").WhoseValue.Should().Be(true);

        // Cleanup
        if (File.Exists(importPath)) File.Delete(importPath);
        if (Directory.Exists(importDir)) Directory.Delete(importDir, true);
        ResetConfigPath();
    }

    [Fact]
    public void ImportConfiguration_WhenFileDoesNotExist_DoesNotThrow()
    {
        // Arrange
        SetTestConfigPath();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent.json");

        // Act & Assert (should not throw)
        ConfigurationHelper.ImportConfiguration(nonExistentPath);
        ResetConfigPath();
    }

    [Fact]
    public void ImportConfiguration_WithInvalidJson_DoesNotThrow()
    {
        // Arrange
        SetTestConfigPath();
        var invalidJsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "invalid.json");
        var invalidJsonDir = Path.GetDirectoryName(invalidJsonPath);
        Directory.CreateDirectory(invalidJsonDir);
        File.WriteAllText(invalidJsonPath, "invalid json {{{");

        // Act & Assert (should not throw)
        ConfigurationHelper.ImportConfiguration(invalidJsonPath);

        // Cleanup
        if (File.Exists(invalidJsonPath)) File.Delete(invalidJsonPath);
        if (Directory.Exists(invalidJsonDir)) Directory.Delete(invalidJsonDir, true);
        ResetConfigPath();
    }
}
