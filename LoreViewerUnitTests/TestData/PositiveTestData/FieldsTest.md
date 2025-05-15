# Vela Orion {TestCharacter}

- Aliases:
  - *V*
  - **Orion** Ghost
  - Silent Flame

- **Employment History:**
  - Organization: *Nightfall Syndicate*
  - Roles:
    - Infiltrator
    - Handler
    - *Intel Courier*
  - Duration: 2012–2021


# This is a valid markdown with bullet point fields/attributes {ListFieldsObject}

- flat: This is a flat attribute
- **flat with bold:** flat attribute with bold name
- *flat with italics:* flat attribute with an italicized name
- subbullet
  - a subbullet
- **subbullet with bold**
  - a subbullet with a bold name
- *subbullet with italics*
  - a subbullet with italicized name
- multivalue
  - value 1
  - value 2
- **multivalue with bold**
  - value 3
  - value 4
- *multivalue with italics*
  - value 5
  - value 6


# Field Format Edge Case Test {FieldEdgeCases}

- FlatField: Normal value, no formatting

- **BoldField**: This value has no bold, just the field label

- *ItalicField*: Italicized field name, regular value

- ColonNoSpace:ThisShouldStillWork

- ColonAfterSpace : This should work too

- MixedFormatField: This *value* contains _emphasis_ and **bold**

- NestedField:
  - SubFieldOne: Value1
  - **SubFieldBold**: Bold-labeled nested value
  - *SubFieldItalic*: Italic-labeled nested value

- MultiField:
  - One
  - Two
  - **Three** (bolded value)

- NestedMultiField:
  - DetailList:
    - Alpha
    - _Beta_ (italic)
    - **Gamma**
