# Test Node {BothTestType}

The top level node

## First Embedded Node {EmbeddedType2}

The first embedded node

## Second Embedded Node {EmbeddedType1}

The second embedded node with its own embedded nodes. This will test if derived types can be used in place of base types, as in C# or whatever

### A Deeply Embedded Node Derived From The Defined Type {EmbeddedType3}

This node *is* allowed despite not being directly of type EmbeddedType1

### A Deeply Embedded Node Of The Defined Type {EmbeddedType1}

This node type matches the allowed embedded node type exactly