# TemplateVariableResolverExtensions

Provides extension methods for resolving template variables in strings and YAML documents, loading environment files, and checking resolvability. These helpers are intended for use in the Coolify CLI when processing configuration templates that reference variable placeholders.

## API

### ExpandOrThrow
**Purpose**  
Expands all variable placeholders (e.g., `{{VAR}}`) in the supplied input string using the provided variable map. If any placeholder cannot be resolved, the method throws an exception to signal failure.

**Parameters**  
- `template`: The string containing variable placeholders to expand.  
- `variables`: A read‑only dictionary mapping variable names to their replacement values.

**Return value**  
A new string with all placeholders replaced by their corresponding values.

**Exceptions**  
- `InvalidOperationException` (or a derived type) when one or more placeholders have no matching entry in `variables`.

### TryExpand
**Purpose**  
Attempts to expand variable placeholders in a YAML‑formatted string. Unlike `ExpandOrThrow`, this method never throws; instead it reports success or failure via a tuple.

**Parameters**  
- `yamlTemplate`: The YAML text that may contain variable placeholders.  
- `variables`: A read‑only dictionary of variable names to values used for substitution.

**Return value**  
A tuple `(bool Success, string ExpandedYaml)`.  
- `Success` is `true` when every placeholder was resolved; `false` otherwise.  
- `ExpandedYaml` contains the resulting YAML with placeholders replaced when `Success` is `true`; otherwise it contains the original input.

### LoadDotEnvFiles
**Purpose**  
Reads one or more `.env` files, parses each line as `KEY=VALUE` pairs, and merges them into the process environment (or an internal variable store). Returns the number of files successfully loaded.

**Parameters**  
- `file paths to be loaded.  
- The count of files that were parsed from the files.  
- `files`: An enumerable of file system paths to `.env` files.

**Return value**  
The count of `.env` files that were read and processed without error.

**Exceptions**  
- `IOException` or `UnauthorizedAccessException` if a file cannot be accessed or read.  
- `FormatException` if a line in a file does not conform to the expected `KEY=VALUE` format (depending on implementation).

### CanResolveAll
**Purpose**  
Determines whether all variable placeholders in a given string can be resolved using the supplied variable map, without performing the actual expansion.

**Parameters**  
- `input`: The string to inspect for placeholders.  
- `variables`: A read‑only dictionary of available variable names and values.

**Return value**  
`true` if every placeholder found in `input` has a corresponding entry in `variables`; otherwise `false`.

## Usage

### Example 1: Expanding a configuration string with error handling
```csharp
using CoolifyCli.Templating; // namespace containing TemplateVariableResolverExtensions

var vars = new Dictionary<string, string>
{
    ["ServiceName"] = "api",
    ["Port"] = "8080"
};

string raw = "service: {{ServiceName}}\nport: {{Port}}";

try
{
    string resolved = TemplateVariableResolverExtensions.ExpandOrThrow(raw, vars);
    Console.WriteLine(resolved);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Failed to resolve template: {ex.Message}");
}
```

### Example 2: Safely expanding YAML and reporting partial success
```csharp
string yamlTemplate = @"
image: {{Image}}
replicas: {{Replicas}}
env:
  - NAME={{EnvName}}
";

var vars = new Dictionary<string, string>
{
    ["Image"] = "myapp:latest",
    ["Replicas"] = "3"
    // EnvName is intentionally omitted to demonstrate failure
};

var result = TemplateVariableResolverExtensions.TryExpand(yamlTemplate, vars);

if (result.Success)
{
    Console.WriteLine("Expanded YAML:");
    Console.WriteLine(result.ExpandedYaml);
}
else
{
    Console.WriteLine("Some placeholders could not be resolved.");
    Console.WriteLine("Original YAML:");
    Console.WriteLine(yamlTemplate);
}
```

## Notes
- All extension methods are **pure** with respect to their input parameters; they do not modify the supplied `variables` dictionary or the original strings.
- The methods are **thread‑safe** when called with distinct variable dictionaries, as they only read from the provided mappings and perform no internal mutable operations on local copies of the input data.
- If the same `variables` instance is shared across concurrent calls, callers must ensure that the dictionary is not mutated during execution; concurrent reads are safe, but concurrent writes may lead to undefined behavior.
- `LoadDotEnvFiles` may modify the process environment; therefore, invoking it from multiple threads simultaneously could cause race conditions on environment variable updates. It is advisable to serialize calls to this method or ensure that the target environment is thread‑safe (e.g., by using per‑process isolation).
- Placeholder syntax is assumed to be `{{KEY}}`; any deviation will result in the placeholder being treated as literal text and will not cause a resolution failure.  
- When `CanResolveAll` returns `false`, the specific missing keys are not reported; callers needing diagnostic details should invoke `ExpandOrThrow` or `TryExpand` and handle the resulting exception or failure flag.
