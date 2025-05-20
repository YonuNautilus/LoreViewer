# LoreViewer: Markdown-Based Lore Parser & Viewer

**LoreViewer** is a markdown-native, YAML-configurable lore parser and viewer designed to help worldbuilders, writers, and GMs structure their lore documents without sacrificing human readability.

This tool allows you to define your own schema for different types of lore elements (e.g., characters, timeline events, organizations), while keeping everything stored in plain `.md` files with rich structure.

---

## Core Rules for the Current Prototype

1. A defined type can only use one field layout style (e.g. bullet list — table support is planned but not yet implemented)
2. All fields must be defined in the YAML schema; undefined fields will trigger errors
3. Fields must appear before any sections or narrative content
4. Sections must be defined in the YAML and may contain freeform narrative
5. Sections are parsed recursively; nested sections are supported
6. Sections can contain structured fields, but they must be placed before narrative-only text.
7. A field can either have values or nested fields, not both.
8. A field with multiple values can only be flat if there is only one value in the markdown.

---

## Core Concepts

### What is a LoreNode?

A `LoreNode` is a structured, typed object — like a character, timeline event, or location.

It is declared in markdown using a top-level heading with a type tag:

```markdown
# Paula Mer Verdell {Character}
```

LoreNodes can also appear inside collections:

```markdown
# Supporting Characters {collection:Character}
## Paula Mer Verdell {Character}
```

---

## YAML Configuration

Every lore project must define a `Lore_Settings.yaml` file, which specifies:

* `types`: All valid types of LoreNodes and their structure
* `aliases`: Optional shorthand for type or collection tags

Example:

```yaml
types:
  Character:
    fields:
      - name: Species
        style: bullet_point
        required: true
      - name: Aliases
        multi: true
      - name: Employment History
        nestedFields:
          - name: Organization
          - name: Roles
            multi: true
          - name: Duration
    sections:
      - name: Personality
        subsections:
          - name: Strengths
          - name: Weaknesses
      - name: History
        subsections:
          - name: Early Life
          - name: Later Years
      - name: Notes
```

---

## Field and Section Syntax

### Fields (Attributes)

Fields are defined at the top of a LoreNode or LoreSection in bullet-list format:

```markdown
# Paula Mer Verdell {Character}
- Species: Slime
- Aliases:
  - The Green Slime Secretary
  - Greenbean
- Employment History:
  - Organization: SIA
    - Roles:
      - Analyst
      - Envoy
    - Duration: 2022–2024

## Personality
- Tone: Timid

Paula is not very assertive, but is driven to do good and accurate work.
	
```

Table-style fields are not yet supported.

### Sections

Sections are introduced with subheadings inside a LoreNode (e.g. `## Personality`).

* Section title must be defined in the type's YAML under `sections:`
* Can contain nested subsections if listed under `sections:`
* Content inside sections is narrative: markdown blocks, bullet lists, quotes, etc.
* Sections CAN contain structured fields, but these must be defined in the section YAML. In the below example, the Personality section will need a Tone field defined.

Example:

```markdown
## Personality
- Tone: Confident

Vera is gentle and diligent.

### Strengths
- Resilient
- Empathic

### Weaknesses
- Hesitant
- Easily flustered
```

---

## Markdown Type Tagging

All headings that define nodes or collections must end with a **type tag**:

* `{Character}` — defines a single node
* `{collection:Character}` — defines a collection of character nodes
* `{collection:collection:Character}` — defines a nested collection group

---

## Parser Workflow

1. Deserializes the `Lore_Settings.yaml` file into a LoreSettings, containing definitions for types, sections, fields etc.
2. For each `.md` file:

   * Top-level heading determines node or collection type
   * Fields are parsed from first list block
   * Sections matched by heading level and name
   * Nested sections and subsections are parsed recursively
3. Unknown fields or sections → warning or error

---

## Future Features (Planned)

* Table field parsing
* Rich in-app editing
* Cross-file referencing and lookup
* Graphical relationship viewer
* Interactive dashboards for GMs and writers

---

To get started, prepare:

* A consistent folder of `.md` files
* A clean `Lore_Settings.yaml` defining all types and structure
