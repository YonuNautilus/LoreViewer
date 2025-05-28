# A Node For Testing Narrative Text When Attributes Are Present {NodeTest}

- Field 1
  + Value 1 (using plus)
  * `Value 2` (using asterisk, including `CodeInline`)

And here's some text, should be a `ParagraphBlock`.  
Ideally, after this, additional `ListBlock`s should be inserted into the `Summary` property of the node.

+ Just Testing out another list
  + Putting in a nested ListBlock  
and a bit more text

What does it all ~~do~~ even *mean*?

## An Undefined Section (Should be a freeform narrative text block, no fields) {section}

+ So here's a listblock to see if it is,
  * deep down...

> just narrative text?

## Fields Section

1. Field Junior : A field on a *defined* section.

Now that the field has been listed, we can have another `ListBlock` that isn't fields, right?

a) Letter test.
   i. numeral test. 