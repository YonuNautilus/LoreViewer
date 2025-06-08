# First Simple Node {MainNodeType}

- field with required:
  - required nested field: value

This node will be missing the required field "required field"
It will ALSO be missing the grandparent of the required field

## Non-Required Section

This section is not required, but it has a required subsection and a required field

- required section-nested field: value

### Required nested section

This subsection is required

## Required Section

This section IS required

## Optional Nested Node {Simple Node}

This nested node is NOT required.

## Required Nested Node {Simple Node 2}

- Simple Required Field: A value, since this field is required

This nested node IS required.

### Simple Section

This section is defined on the parent node type Simple Node, but is not required

## Parent Of Required Nested Node {SimpleTypeWithRequiredNode}

This node is not required directly, but its child is

### Required Grandchild {Simple Node}

This node IS required.