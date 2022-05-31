// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Services;

/// <summary>
/// Manages resource quotas and limits for applications and databases.
/// Tracks usage against configured limits and prevents exceeding quotas.
/// Supports alerts when approaching limits.
/// </summary>
public class ResourceQuotaManager
{
    private readonly Dictionary<string, ResourceQuota> _quotas = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Registers a resource quota.
    /// </summary>
    public void RegisterQuota(string resourceId, ResourceQuota quota)
    {
        lock (_lockObject)
        {
            _quotas[resourceId] = quota;
        }
    }

    /// <summary>
    /// Checks if usage is within quota limits.
    /// </summary>
    public bool IsWithinQuota(string resourceId, ResourceType type, double usage)
    {
        lock (_lockObject)
        {
            if (!_quotas.TryGetValue(resourceId, out var quota))
                return true; // No quota = unlimited

            var limit = type switch
            {
                ResourceType.Cpu => quota.MaxCpuMillicores,
                ResourceType.Memory => quota.MaxMemoryMb,
                ResourceType.Disk => quota.MaxDiskGb,
                ResourceType.Bandwidth => quota.MaxBandwidthGbPerMonth,
                _ => double.MaxValue
            };

            return usage <= limit;
        }
    }

    /// <summary>
    /// Gets the percentage of quota used.
    /// </summary>
    public double GetQuotaUsagePercent(string resourceId, ResourceType type, double usage)
    {
        lock (_lockObject)
        {
            if (!_quotas.TryGetValue(resourceId, out var quota))
                return 0;

            var limit = type switch
            {
                ResourceType.Cpu => quota.MaxCpuMillicores,
                ResourceType.Memory => quota.MaxMemoryMb,
                ResourceType.Disk => quota.MaxDiskGb,
                ResourceType.Bandwidth => quota.MaxBandwidthGbPerMonth,
                _ => double.MaxValue
            };

            return (usage / limit) * 100;
        }
    }

    /// <summary>
    /// Checks if usage is approaching quota limit (>80%).
    /// </summary>
    public bool IsApproachingQuotaLimit(string resourceId, ResourceType type, double usage)
    {
        var percent = GetQuotaUsagePercent(resourceId, type, usage);
        return percent > 80;
    }

    /// <summary>
    /// Checks if usage exceeds quota limit.
    /// </summary>
    public bool ExceedsQuota(string resourceId, ResourceType type, double usage)
    {
        return !IsWithinQuota(resourceId, type, usage);
    }

    /// <summary>
    /// Gets remaining quota capacity.
    /// </summary>
    public double GetRemainingCapacity(string resourceId, ResourceType type, double usage)
    {
        lock (_lockObject)
        {
            if (!_quotas.TryGetValue(resourceId, out var quota))
                return double.MaxValue;

            var limit = type switch
            {
                ResourceType.Cpu => quota.MaxCpuMillicores,
                ResourceType.Memory => quota.MaxMemoryMb,
                ResourceType.Disk => quota.MaxDiskGb,
                ResourceType.Bandwidth => quota.MaxBandwidthGbPerMonth,
                _ => double.MaxValue
            };

            return Math.Max(0, limit - usage);
        }
    }

    /// <summary>
    /// Gets quota details for a resource.
    /// </summary>
    public ResourceQuota? GetQuota(string resourceId)
    {
        lock (_lockObject)
        {
            return _quotas.TryGetValue(resourceId, out var quota) ? quota : null;
        }
    }

    /// <summary>
    /// Updates resource usage.
    /// </summary>
    public void UpdateUsage(string resourceId, ResourceType type, double usage)
    {
        lock (_lockObject)
        {
            if (_quotas.TryGetValue(resourceId, out var quota))
            {
                switch (type)
                {
                    case ResourceType.Cpu:
                        quota.CurrentCpuMillicores = usage;
                        break;
                    case ResourceType.Memory:
                        quota.CurrentMemoryMb = usage;
                        break;
                    case ResourceType.Disk:
                        quota.CurrentDiskGb = usage;
                        break;
                    case ResourceType.Bandwidth:
                        quota.CurrentBandwidthGbThisMonth = usage;
                        break;
                }

                quota.LastUpdatedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Validates that a requested allocation doesn't exceed quota.
    /// Throws an exception if quota would be exceeded.
    /// </summary>
    public void ValidateAllocation(string resourceId, ResourceType type, double requestedAmount)
    {
        lock (_lockObject)
        {
            if (!_quotas.TryGetValue(resourceId, out var quota))
                return; // No quota = no validation

            var currentUsage = type switch
            {
                ResourceType.Cpu => quota.CurrentCpuMillicores,
                ResourceType.Memory => quota.CurrentMemoryMb,
                ResourceType.Disk => quota.CurrentDiskGb,
                ResourceType.Bandwidth => quota.CurrentBandwidthGbThisMonth,
                _ => 0
            };

            var newTotal = currentUsage + requestedAmount;

            if (!IsWithinQuota(resourceId, type, newTotal))
            {
                var remaining = GetRemainingCapacity(resourceId, type, currentUsage);
                throw new QuotaExceededException(
                    $"Resource {type} quota would be exceeded. " +
                    $"Requested: {requestedAmount}, Available: {remaining}");
            }
        }
    }

    /// <summary>
    /// Gets all quotas.
    /// </summary>
    public Dictionary<string, ResourceQuota> GetAllQuotas()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, ResourceQuota>(_quotas);
        }
    }
}

/// <summary>
/// Resource quota definition.
/// </summary>
public class ResourceQuota
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;

    // Limits
    public double MaxCpuMillicores { get; set; } = double.MaxValue;
    public double MaxMemoryMb { get; set; } = double.MaxValue;
    public double MaxDiskGb { get; set; } = double.MaxValue;
    public double MaxBandwidthGbPerMonth { get; set; } = double.MaxValue;

    // Current usage
    public double CurrentCpuMillicores { get; set; }
    public double CurrentMemoryMb { get; set; }
    public double CurrentDiskGb { get; set; }
    public double CurrentBandwidthGbThisMonth { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEnforced { get; set; } = true;
}

/// <summary>
/// Resource types for quota management.
/// </summary>
public enum ResourceType
{
    Cpu,
    Memory,
    Disk,
    Bandwidth
}

/// <summary>
/// Exception thrown when quota limit is exceeded.
/// </summary>
public class QuotaExceededException : Exception
{
    public QuotaExceededException(string message) : base(message) { }
}
