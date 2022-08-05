# TemplateVariableResolver

Resolves template variables in YAML configuration files by combining values from environment variables, `.env` files, and explicit overrides. It expands placeholders of the form `${VARIABLE_NAME}` or `$VARIABLE_NAME` within YAML content, reports any variables that remain unresolved, and supports loading variables from dotenv files with optional override semantics.

## API

### Constructors

```csharp
public TemplateVariableResolver()
public TemplateVariableResolver(IReadOnlyDictionary<string, string>? builtIns)
```

Creates a new resolver instance. The parameterless constructor initialises the resolver with no built-in variables. The overload accepting an `IReadOnlyDictionary<string, string>?` populates the resolver with a set of built-in variables that serve as default values—these can be superseded by environment variables, dotenv files, or explicit overrides.

**Parameters:**
- `builtIns`: An optional dictionary of variable names to their default values. May be `null`, which is equivalent to calling the parameterless constructor.

**Throws:** No exceptions are thrown by the constructors themselves.

---

### SetOverride

```csharp
public void SetOverride(string variable, string value)
```

Registers an explicit override for a given variable. Overrides take the highest precedence and will be used during expansion regardless of values from environment variables, dotenv files, or built-ins.

**Parameters:**
- `variable`: The name of the variable to override (case-sensitive).
- `value`: The value to assign. Can be an empty string; passing `null` will store a literal `null` string representation.

**Return value:** None.

**Throws:**
- `ArgumentNullException` if `variable` is `null`.

---

### LoadDotEnvFile

```csharp
public int LoadDotEnvFile(string path, bool overrideExisting = false)
```

Loads variable definitions from a dotenv file at the specified path. Each line is expected to follow the `KEY=VALUE` format. Variables loaded from the file are merged into the resolver's state.

**Parameters:**
- `path`: The file system path to the `.env` file.
- `overrideExisting`: When `false` (default), variables from the file will not replace values already present from built-ins, environment variables, or prior `LoadDotEnvFile` calls. When `true`, the file's values overwrite any existing entries.

**Return value:** The number of variables successfully loaded and applied from the file.

**Throws:**
- `FileNotFoundException` if the specified path does not exist.
- `IOException` if the file cannot be read.
- `FormatException` if a line in the file is malformed and cannot be parsed as a variable assignment.

---

### Expand

```csharp
public (string ExpandedYaml, List<string> Unresolved) Expand(string yamlTemplate)
```

Expands all recognised variable placeholders in the provided YAML template string. Variables are resolved using the following precedence (highest to lowest): explicit overrides set via `SetOverride`, environment variables from the process environment, variables loaded from dotenv files, and built-in defaults. Placeholders that cannot be resolved against any source are left intact in the output and collected in the unresolved list.

**Parameters:**
- `yamlTemplate`: A raw YAML string potentially containing `${VAR}` or `$VAR` placeholders.

**Return value:** A tuple containing:
- `ExpandedYaml`: The YAML string with all resolvable placeholders replaced by their values.
- `Unresolved`: A `List<string>` of variable names that were referenced in the template but could not be resolved. The list is empty if all variables were resolved.

**Throws:**
- `ArgumentNullException` if `yamlTemplate` is `null`.

---

### CollectPlaceholders

```csharp
public static IReadOnlySet<string> CollectPlaceholders(string yamlTemplate)
```

Scans a YAML template string and extracts all unique variable names referenced via `${VAR}` or `$VAR` syntax. This is a static utility method that does not perform any resolution—it only identifies which placeholders are present.

**Parameters:**
- `yamlTemplate`: A raw YAML string to scan for placeholders.

**Return value:** An `IReadOnlySet<string>` containing the distinct variable names found in the template. Returns an empty set if no placeholders are present.

**Throws:**
- `ArgumentNullException` if `yamlTemplate` is `null`.

## Usage

### Example 1: Basic resolution with dotenv and overrides

```csharp
var resolver = new TemplateVariableResolver();
resolver.LoadDotEnvFile("/etc/coolify/.env");

// Override a value regardless of environment or .env file
resolver.SetOverride("APP_PORT", "9090");

string template = @"
app:
  name: ${APP_NAME}
  port: ${APP_PORT}
  debug: ${DEBUG_MODE}
";

var (expanded, unresolved) = resolver.Expand(template);

Console.WriteLine(expanded);
// app:
//   name: MyCoolApp
//   port: 9090
//   debug: ${DEBUG_MODE}

foreach (var variable in unresolved)
{
    Console.WriteLine($"Warning: '{variable}' was not resolved.");
}
// Warning: 'DEBUG_MODE' was not resolved.
```

### Example 2: Pre-scanning placeholders before expansion

```csharp
string template = @"
server:
  host: $HOST
  port: $PORT
  tls: ${ENABLE_TLS}
";

// Collect all referenced variables before resolving
IReadOnlySet<string> requiredVars = TemplateVariableResolver.CollectPlaceholders(template);

Console.WriteLine($"Template references {requiredVars.Count} variables:");
foreach (var v in requiredVars)
    Console.WriteLine($"  - {v}");

// Prepare resolver with built-in defaults
var builtIns = new Dictionary<string, string>
{
    ["HOST"] = "localhost",
    ["PORT"] = "8080"
};
var resolver = new TemplateVariableResolver(builtIns);

var (expanded, unresolved) = resolver.Expand(template);

// ENABLE_TLS remains unresolved unless set in environment or overridden
if (unresolved.Count > 0)
    Console.WriteLine($"Unresolved: {string.Join(", ", unresolved)}");
```

## Notes

- **Placeholder syntax:** Both `${VARIABLE}` and `$VARIABLE` forms are recognised. The `$VARIABLE` form terminates at the first character that is not a letter, digit, or underscore. Adjacent characters such as `$VAR_$OTHER` are parsed as two separate placeholders.
- **Unresolved handling:** Variables that cannot be resolved are left as literal text in the expanded output. Callers should inspect the `Unresolved` list to detect missing configuration and decide whether to abort, warn, or proceed.
- **Override precedence:** Values set via `SetOverride` always win, even over environment variables. This allows callers to force specific values in programmatic scenarios.
- **Dotenv loading order:** Multiple calls to `LoadDotEnvFile` are cumulative. When `overrideExisting` is `false`, the first value encountered for a variable is retained; later files will not replace it unless `overrideExisting` is `true`.
- **Empty values:** A variable defined with an empty string (e.g., `KEY=` in a dotenv file or `SetOverride("KEY", "")`) is considered resolved. The placeholder will be replaced with an empty string, and the variable will not appear in the unresolved list.
- **Thread safety:** `TemplateVariableResolver` instance methods (`SetOverride`, `LoadDotEnvFile`, `Expand`) are not thread-safe. Concurrent calls from multiple threads must be externally synchronised. The static method `CollectPlaceholders` is thread-safe as it performs no mutation.
- **Case sensitivity:** Variable names are treated case-sensitively. `$PORT` and `$port` are distinct variables.
