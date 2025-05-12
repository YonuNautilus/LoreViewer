# LoreViewer: Markdown-Based Lore Parser & Viewer

**LoreViewer** is a markdown-native, YAML-configurable lore parser and viewer designed to help worldbuilders, writers, and GMs structure their lore documents without sacrificing human readability.

This tool allows you to define your own schema for different types of lore elements (e.g., characters, timeline events, organizations), while keeping everything stored in plain `.md` files with rich structure.

---

## CURRENTLY ESTABLISH RULES FOR A WORKING PROTOTYPE (Hopefully made more flexible later)

1. A defined type can have only one style of field/attribute (table or list)
2. A type in markdown must put field/attributes FIRST before sections, text, or nested objects/collections

---

## Core Concepts

### What is a LoreNode?
A `LoreNode` is a single object of a defined type (character, timeline event, location, etc). It is defined in markdown by a heading with a type tag:

```` markdown
# Paula Mer Verdell {character}
````

LoreNodes can also appear in collections:

```` markdown
# Supporting Characters {collection:Character}
## Paula Mer Verdell {Character}
````

In the above snippet, the `## Paula Mer Verdell` headings's `{Character}` tag does not need to be defined 

## YAML Config Structure

Each project must include a `Lore_Settings.yaml` file that defines:
- types
- collections (optional, only define if collections need metadata)


---
---

## Lore Container Patterns

In LoreViewer, lore content can be grouped in two primary ways: as **typed nodes with embedded entries**, or as **collections of nodes**. Both are valid and serve different purposes depending on your worldbuilding needs.

### 🔹 Option 1: Typed Node with Embedded Nodes

Use this when the container (like a timeline, spellbook, or dossier) has its own metadata, description, or semantic role in the world.

```markdown
# Slime Integration Timeline {Timeline}

This timeline documents the key moments during early slime-human relations.

## First Contact {TimelineEvent}
- Date: June 3, 2014
- Impact: Historical

## Government Recognition {TimelineEvent}
- Date: May 9, 2015
```
- {Timeline} is a LoreNode with type "Timeline"
- Each {TimelineEvent} is an embedded LoreNode
- Useful when the container has a title, summary, tags, or other fields

### 🔹 Option 2: Collection of LoreNodes

Use this when the grouping is purely structural or display-oriented, such as a long list of standalone events, items, or characters.
```markdown
# Timeline of Key Events {collection:TimelineEvent}

## First Contact {TimelineEvent}
- Date: June 3, 2014

## Government Recognition {TimelineEvent}
- Date: May 9, 2015
```

- The parser treats this as a LoreNodeCollection
- The collection itself has no fields or metadata unless explicitly supported
- Cleaner for presentation of grouped items with no unifying "container" identity

### When to Use Which?

| Use Case                                                                 | Best Structure                       |
| ------------------------------------------------------------------------ | ------------------------------------ |
| The group needs a name, summary, or fields                               | **Typed node with embedded entries** |
| The group is just a flat list                                            | **Collection**                       |
| The container is part of the world (e.g., "The Founding Years Timeline") | **Typed node**                       |
| You just want to show all items of a type (e.g., "All Spells")           | **Collection**                       |

Both options can coexist across your project. Choose the one that matches the meaning you're trying to convey.

---
---


### Types
Type definitions are best used for single-object or single-concept things, like character, spell, organization, faction, or information.\
Types can have fields, sections, nested nodes, and nested node collections.
#### Defining a type
In the YAML config file, types are defined under the `types` mapping:
``` yaml
types:
  TimelineEvent:
    ...
  Character:
    ...
```

---

### Collections
Collections are a list of nodes of a certain type. This can be used for cases like creating a timeline of events, where events are defined as a type (`timelineEvent`, for example).\
Simple collections without metadata or extra info do not need to be defined in the YAML config.\
**NOTE:** if a collection should have metadata (fields, sections), it will need to be defined in the YAML config.
#### Defining a collection
In the YAML config file, collections are defined under the `collections` mapping:
```` yaml
collections:
  Timeline:
    ...
  SpellList:
    ...
````
#### Defining collection aliases
Users can create shorthand notations that can be used to tag defined collections.\
For example, a user can define an alias for a collection of timeline events and tag timeline headings with `{timeline}` instead of `{collection:timelineEvent}`\
In the YAML config file, aliases are defined under the `aliases` mapping:
```` yaml
aliases:
  Timeline: collection:TimelineEvent
  relations: collection:Relationship
````

---

### Fields, sections, and nested nodes/collections.
- **Fields:** Types and defined collections have attributes called field, used for giving a node of a type some common attributes.\
For example, a character type might have a name, age, affiliation, and species.
- **Sections:** Types and defined collections may also have sections which contain textual information that does not fit into fields.\
For example, a character type may have a section for background or history.
- **Nested Nodes:** A type (but not a collection) could also contain a node within itself - best used for things that are wholly dependent on its parent node\
For example, a culture-type node may have a nested node for that culture's religion (if there is also a defined religion type — otherwise, religion would work best as a section, in this example).
- **Nested Collections:** A type (but not a collection) can also have a collection of nodes of the same type.\
For example, a faction or organization can have a collection of timelineEvent-type nodes (tagged `{collection:timelineEvent}`) detailing its development and history.\
Or a character may have a collection of weapon-type nodes (tagged `{collection:weapon}`).

#### Defining Fields
Fields are defined for a node or collection for information that defines that type of collection, and represent a key-value pair info.\
In markdown, fields can be written in either a bullet-point list block, or a table.

#### Defining Sections
Sections are defined for a node or collection for information that cannot be reduced to key-value pairs, such are a character's backstory or a summary of a TimelineEvent collection.\
In markdown, sections have a heading that can be tagged with the `{section}` tag (but it may not be necessary, all depends on whatever). Currently, the only supported style for the information contained in a `section` is 'freeform' (meaning simple text body blocks).

#### Defining Nested Nodes and Collections
A type can define what nested nodes or collections may be present in that type's node.

---

## Markdown Type Tagging
All headings that define nodes or collections must end with a **type tag**:
- `{character}` — defines a single node with type 'character'
- `{collection:character}` — defines a collection of nodes

#### Nested Collections
Collections can contain collections themselves.
- `{collection:collection:character}` — defines a supercollection of collections of character nodes.

---

## Parser Logic
1. Top-level heading determines node or collection and type
2. Field blocks are matched to `fields` in YAML config
3. Section headings are matched to `sections` in YAML config
4. **Unknown** or **unmatched** blocks are logged for review.

---

## Additional Features (Planned)
- Graph visualization
- Links and relations between nodes and collections
- Edit mode with in-place Markdown updates
- Read-Only mode