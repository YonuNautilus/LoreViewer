# First Simple Node <node type="MainNodeType"/>

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

## Required Section

This section IS required

## Optional Nested Node <node type="Simple Node"/>

This nested node is NOT required.

### Simple Section

This section is defined on the parent node type Simple Node, but is not required

## Parent Of Required Nested Node <node type="SimpleTypeWithRequiredNode"/>

This node is not required directly, but its child is

### Required Grandchild <node type="Simple Node"/>

This node IS required.


## Required Collection

### Simple Node <node type="Simple Node 2"/>

- Simple Required Field: VALUE