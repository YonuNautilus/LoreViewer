# Object For Subsections Testing <node type="SubsectionsTest" />

This object will have *various* sections with **subsections**

---

## Freeform With Freeform Child

This is the *freeform parent* section
- Here is a list
  - here is a subvalue

### Freeform Child

- Here is a list
- in a freeform subsection.
  - This list should be parsed as text

---

## Fields-Section With Freeform Child

- Field 1: Value1
- Field 2
  - First value on Field 2
  - Second value on Field 2

This section has fields *and* a freeform subsection. *This* text should be parsed after the fields and placed in the `Summary` property of the `LoreSection`.

You know, we have that freeform bool on the `LoreSectionDefinition`,  but it isn't really used for anything...

I suppose this is a test to see if a section with fields can parse paragraph blocks after a list of defined fields.

### Freeform Child

- this
- is
- a
- freeform
- child

Because this subsection is freeform, that list above should be parsed as text.

---

## Fields-Section With Fields-Child

- Field 3: Value1
- Field 4
  - First value on Field 4
  - Second value on Field 4

### Fields-Child

- Field 5: Value of Field 5
- Field 6:
  - First value on Field 6
  - Another value, doesn't matter
  - A third value

---

## Freeform With Fields-Child

- This is marked as freeform.

> I guess some code changes are needed to make sure that the freeform property is actually used.

### Fields-Child

- Field 7
  - V1
  - V2
  - V3
- Field 8
  - I don't care.