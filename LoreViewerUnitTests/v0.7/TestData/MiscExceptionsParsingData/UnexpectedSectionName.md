# Defined Type with unexpected field name 2 <node type="DefinedType" />

## A Section

No need for section to have a tag, it's name matches a section definition in DefinedType

### A Subsection

Same here

### A Subsection 2!!

This one will throw. Name does not match a section definition, and it is not tagged as a section

## A Section 2!! <section/>

This will not throw an error, because even though it does not match a section definition name, it is tagged as a section