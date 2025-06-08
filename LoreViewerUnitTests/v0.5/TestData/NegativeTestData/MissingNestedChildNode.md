# First Simple Node {MainNodeType}

- required field: attribute
- field with required:
  - required nested field: value
- grandparent of required field
  - parent of required field
    - double-nested required field: VALUE

## Non-Required Section

This section is not required, but it has a required subsection and a required field

- required section-nested field: value

### Required nested section

This subsection is required


## Required Nested Node {Simple Node 2}

- Simple Required Field: A value, since this field is required

This nested node IS required.

### Simple Section

This section is defined on the parent node type Simple Node, but is not required

## Required Section

This section IS required

## Optional Nested Node {Simple Node}

This nested node is NOT required.

### Simple Section

This section is defined on the parent node type Simple Node, but is not required

## Parent Of Required Nested Node {SimpleTypeWithRequiredNode}

This node is not required directly, but its child is (WHICH IS MISSING in this test)