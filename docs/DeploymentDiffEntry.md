# DeploymentDiffEntry

A `DeploymentDiffEntry` represents a single entry in a deployment comparison, capturing differences between current and proposed states of a deployable property. It is used to visualize what will change when a deployment is executed, including nested differences for hierarchical structures.

## API

### `Property`
- **Purpose**: The name of the property being compared.
- **Type**: `string`
- **Remarks**: Never `null`; empty string indicates an unnamed or root-level entry.

### `CurrentValue`
- **Purpose**: The current value of the property in the deployed environment.
- **Type**: `string`
- **Remarks**: May be `null` if the property has no current value (e.g., newly added property).

### `ProposedValue`
- **Purpose**: The value proposed for the property in the upcoming deployment.
- **Type**: `string`
- **Remarks**: May be `null` if the property is to be removed or unset.

### `Category`
- **Purpose**: A classification of the property (e.g., "Environment", "Config", "Secret").
- **Type**: `string`
- **Remarks**: Never `null`; empty string indicates an uncategorized property.

### `ApplicationId`
- **Purpose**: A unique identifier for the application associated with this diff entry.
- **Type**: `int`
- **Remarks**: Zero indicates no associated application.

### `ApplicationName`
- **Purpose**: The human-readable name of the application associated with this diff entry.
- **Type**: `string`
- **Remarks**: May be `null` if no application is associated.

### `ComputedAt`
- **Purpose**: The timestamp when this diff entry was computed.
- **Type**: `DateTime`
- **Remarks**: Always reflects the time of computation, not the time of the deployment.

### `Entries`
- **Purpose**: A list of child `DeploymentDiffEntry` objects representing nested differences.
- **Type**: `List<DeploymentDiffEntry>`
- **Remarks**: Empty if the property has no nested differences.

### `Compute`
- **Purpose**: Computes a `DeploymentDiff` by comparing two deployment states.
- **Signature**: `public static DeploymentDiff Compute(DeploymentState current, DeploymentState proposed)`
- **Parameters**:
  - `current`: The current deployment state.
  - `proposed`: The proposed deployment state.
- **Return Value**: A `DeploymentDiff` object containing the computed differences.
- **Throws**:
  - `ArgumentNullException`: If either `current` or `proposed` is `null`.
- **Remarks**: The comparison is deep and includes all properties, including nested structures.

## Usage

### Example 1: Computing and Inspecting Differences
