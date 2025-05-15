using LoreViewer.Exceptions;
using LoreViewer.LoreNodes;
using LoreViewer.Settings;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static LoreViewer.Exceptions.LoreAttributeParsingException;
using static LoreViewer.Exceptions.LoreSectionParsingException;

namespace LoreViewer
{
  public class LoreParser
  {
    const string HeaderWithoutTagRegex = ".+(?={)";
    const string TypeTagRegex = "(?<={).+(?=})";
    const string NestedTypeTagRegex = "(?<=:).+";
    const string LoreSettingsFileName = "Lore_Settings.yaml";

    private LoreSettings _settings;
    private string _folderPath;
    private string _currentFile;

    public List<LoreNodeCollection> _collections = new List<LoreNodeCollection>();
    public List<LoreNode> _nodes = new List<LoreNode>();
    public List<string> _errors = new List<string>();
    public List<string> _warnings = new List<string>();

    /// <summary>
    /// Key is the file name (path relative to the lore folder), and value is the index of the block that is 'orphaned'
    /// (i.e. not tagged when it needs to be)
    /// </summary>
    public Dictionary<string, int> OrphanedBlocks { get; set; } = new Dictionary<string, int>();

    public LoreSettings Settings { get { return _settings; } }

    public LoreParser() { }

    public LoreParser(LoreSettings settings)
    {
      _settings = settings;
    }

    public void ParseSettingsFromFile(string settingsFilePath)
    {
      var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

      string settingsText = File.ReadAllText(settingsFilePath);

      _settings = deserializer.Deserialize<LoreSettings>(settingsText);
    }

    public void BeginParsingFromFolder(string FolderPath)
    {
      _folderPath = FolderPath;

      string fullSettingsPath = Path.Combine(FolderPath, LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        throw new Exception($"Did not find file {fullSettingsPath}");

      ParseSettingsFromFile(fullSettingsPath);


      string[] files = Directory.GetFiles(FolderPath, "*.md");

      foreach (string filePath in files)
      {
        try
        {
          ParseFile(filePath);
        }
        catch (LoreParsingException pe)
        {
          Console.WriteLine(pe.Message);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"File {Path.GetFileName(filePath)} had problem: {ex.Message}");
        }
      }
    }
    private static string GetCollectionType(string fullTypeName) => Regex.Match(fullTypeName, NestedTypeTagRegex).Value;
    private static string ExtractTag(HeadingBlock block) => Regex.Match(block.Inline.FirstChild.ToString(), TypeTagRegex)?.Value;
    private static string ExtractTitle(HeadingBlock block)
    {
      string title = Regex.Match(block.Inline.FirstChild.ToString(), HeaderWithoutTagRegex)?.Value;

      if (string.IsNullOrEmpty(title))
        title = block.Inline.FirstChild.ToString();

      return title.Trim();
    }

    private string TrimFieldName(string untrimmed)
    {
      string ret = untrimmed.Trim();
      if (ret.EndsWith(":"))
        ret = ret.Substring(0, ret.Length - 1).Trim();

      return ret;
    }

    private static bool BlockIsACollection(HeadingBlock block) => ExtractTag(block).Contains("collection:");

    private static bool BlockIsANestedCollection(HeadingBlock block) => BlockIsACollection(block) && GetCollectionType(ExtractTag(block)).Contains("collection:");

    private static bool BlockIsASection(HeadingBlock block) => ExtractTag(block).Contains("section");

    /// <summary>
    /// The top-level, Markdown-first parsing method. Reads in a markdown file and begins parsing.
    /// <para />
    /// This method is where the variable <c>currentIndex</c> is declared. It gets passed as a ref to all subsequent parsing methods to keep it up to date here.
    /// <para />
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Before finding the first heading, any non-heading blocks get added to a list of orphaned blocks</item>
    ///   <item>If the first heading block is not tagged with a valid type or a collection, throw an error</item>
    ///   <item>If a type-tagged heading is found, create a <c>LoreNode</c> by calling <c>ParseType</c>. Add that node to the <c>_nodes</c> list.</item>
    ///   <item>If a collection-tagged heading is found, create a <c>LoreNodeCollection</c> by calling <c>ParseCollection</c>. Add that collection to the <c>_collections</c> list.</item>
    /// </list>
    /// </summary>
    /// <param name="filePath">Path of the Markdown file being parsed.</param>
    public void ParseFile(string filePath)
    {
      _currentFile = filePath;
      int currentIndex = 0;
      try
      {
        string fileContent = File.ReadAllText(filePath);
        MarkdownDocument document = Markdown.Parse(fileContent);

        // Once here, loop through the document, looking for headers

        while (currentIndex < document.Count)
        {
          // Start with the top level, get its type
          if (document[currentIndex] is HeadingBlock)
          {
            HeadingBlock block = (HeadingBlock)document[currentIndex];
            string tag = ExtractTag(block);
            string title = ExtractTitle(block);

            if (!string.IsNullOrEmpty(tag))
            {
              if (BlockIsACollection(block))
              {
                if (BlockIsANestedCollection(block))
                {
                  _collections.Add(ParseCollection(document, ref currentIndex, block, tag));
                }
                else
                  _collections.Add(ParseCollection(document, ref currentIndex, block, _settings.GetTypeDefinition(GetCollectionType(tag))));
              }
              else if (BlockIsASection(block))
              {
                throw new FirstHeadingTagException(filePath, currentIndex, block.Line + 1);
              }
              else
              {
                int nodeIndex = currentIndex;
                int nodeLineInFile = block.Line;
                LoreNode newNode = ParseType(document, ref currentIndex, block, _settings.GetTypeDefinition(tag));

                newNode.SourcePath = filePath;
                newNode.BlockIndex = nodeIndex;

                _nodes.Add(newNode);
              }
            }
            else
            {
              throw new NoTagParsingException(filePath, currentIndex, block.Line + 1);
            }
          }
          else
          {
            OrphanedBlocks.Add(Path.GetRelativePath(_folderPath, filePath), currentIndex);
            currentIndex++;
          }

        }
      }
      catch (Exception ex)
      {
        _errors.Add(string.Join("\r\n", new string[] { filePath, currentIndex.ToString(), ex.Message }));
        throw;
      }
    }

    /// <summary>
    /// Creates a LoreNode representing a lore element from a LoreTypeDefinition and markdown blocks representing that element.
    /// <para />
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Parsing fields/attributes MUST COME FIRST - if a bullet list or table is found, parse that into fields/attributes</item>
    ///   <item>If a heading tag is {collection:type}, parse that heading as a nested collection</item>
    ///   <item>If a tag is {type}, parse that heading as a new nested LoreNode</item>
    ///   <item>If a heading has no tag or a {section} tag, match to the LoreTypeDefinition section definitions and parse as a Section</item>
    ///   <item>If a tag is unrecognized, throw an error.</item>
    /// </list>
    /// <para/>
    /// EXIT CONDITIONS:
    /// <list type="bullet">
    ///   <item>HeadingBlock found at same or lower number level -> current node ends (return)</item>
    ///   <item>Reached the end of the markdown document -> return</item>
    /// </list>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="currentIndex"></param>
    /// <param name="heading"></param>
    /// <param name="typeDef"></param>
    /// <returns></returns>
    private LoreNode ParseType(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreTypeDefinition typeDef)
    {
      bool parsingFields = true;

      string title = ExtractTitle(heading);
      LoreNode newNode = new LoreNode(typeDef, title);
      newNode.BlockIndex = currentIndex;

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        var currentBlock = doc[currentIndex];

        if (parsingFields)
        {
          if (currentBlock is ListBlock)
          {
            ListBlock lb = currentBlock as ListBlock;
            Dictionary<string, LoreAttribute> attributes = ParseListAttributes(doc, currentIndex, lb, typeDef.fields);
            newNode.Attributes = newNode.Attributes.Concat(attributes).ToDictionary();
          }
          else
          {
            parsingFields = false;
          }
        }

        if (!parsingFields)
        {
          switch (currentBlock)
          {
            // Sections
            case HeadingBlock hb:
              // If the found heading is at same or lower number level than the heading for this node, return this node.
              if (heading.Level >= hb.Level)
                return newNode;

              string newTag = ExtractTag(hb);
              string newTitle = ExtractTitle(hb);

              // If a tag is defined, and the tag does NOT have 'section' in the name, treat it as new nested node or nested collection.
              if (!string.IsNullOrEmpty(newTag) && !BlockIsASection(hb))
              {
                // Block is tagged with {collection:...}
                if (BlockIsACollection(hb))
                {
                  LoreNodeCollection newCollection;

                  // if tagged like {collection:collection:type}
                  if (BlockIsANestedCollection(hb))
                    newCollection = ParseCollection(doc, ref currentIndex, hb, newTag);

                  // otherwise, tagged as {collection:type}
                  else
                    newCollection = ParseCollection(doc, ref currentIndex, hb, _settings.GetTypeDefinition(GetCollectionType(newTag)));


                  newNode.CollectionChildren.Add(newTag, newCollection);
                }

                // Parse as a nested node of the type specified in the tag.
                else if (_settings.HasTypeDefinition(newTag))
                {
                  LoreTypeDefinition newNodeType = _settings.GetTypeDefinition(newTag);
                  LoreNode newNodeNode = ParseType(doc, ref currentIndex, hb, newNodeType);
                  newNode.Children.Add(newNodeNode.Name, newNodeNode);

                }
              }
              // Here, if no tag or a section tag:
              else
              {
                // Regardless if the block has a section tag or not, check if the block's title is present in the type definition's sections.
                if (typeDef.HasSectionName(newTitle))
                {
                  LoreSectionDefinition sectionDef = typeDef.sections.FirstOrDefault(sec => newTitle.Contains(sec.name));
                  LoreSection newSection = ParseSection(doc, ref currentIndex, hb, sectionDef);
                  newNode.Sections.Add(newSection);
                  currentIndex--;
                }
                else
                {
                  throw new UnexpectedSectionNameException(_currentFile, currentIndex, currentBlock.Line + 1, title, newTitle);
                }
              }
              break;

            // freeform
            case ParagraphBlock pb:
              KeyValuePair<string, LoreAttribute> lines = ParseParagraphBlocks(doc, ref currentIndex, pb, typeDef);
              if (!newNode.Attributes.ContainsKey("summary"))
                newNode.Attributes.Add(lines.Key, lines.Value);
              else
                newNode.Attributes["summary"].Append(lines.Value);
              break;

            // Fields
            case ListBlock lb:

            default:
              break;
          }

        }
        currentIndex++;
      }

      return newNode;
    }

    /// <summary>
    /// Creates a LoreNodeCollection of LoreNodes. Starts from a heading block with tag {collection:type}
    /// <para />
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Step forward, expect headings on level deeper than the original heading (UNSURE IF THESE HEADING NEED TYPE TAGS OR NOT)</item>
    ///   <item>If a heading is UNTAGGED, Give a WARNING (not an error, yet)</item>
    ///   <item>If a heading is tagged with a type not accepted by the collection, throw an error</item>
    ///   <item>If a non-heading block is encountered before the first type heading is found, throw an error (LoreNodeCollection does not have metadata!)</item>
    ///   <item>Each Subheading is parsed into a LoreNode with ParseType, and that node is added to the collection</item>
    /// </list>
    /// <para/>
    /// EXIT CONDITIONS:
    /// <list type="bullet">
    ///   <item>HeadingBlock found at same or lower number level -> current node collection ends (return)</item>
    ///   <item>Reached the end of the markdown document -> return</item>
    /// </list>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="currentIndex"></param>
    /// <param name="heading"></param>
    /// <param name="typeDef"></param>
    /// <returns></returns>
    private LoreNodeCollection ParseCollection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreTypeDefinition typeDef)
    {
      string title = ExtractTitle(heading);
      LoreNodeCollection newCollection = new LoreNodeCollection();

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        if (doc[currentIndex] is HeadingBlock)
        {
          HeadingBlock newHeader = (HeadingBlock)doc[currentIndex];
          LoreNode newNode = ParseType(doc, ref currentIndex, newHeader, typeDef);
          newCollection.Add(newNode);
        }
        else
        {

        }

        currentIndex++;
      }

      return newCollection;
    }

    private LoreNodeCollection ParseCollection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, string collectionTag)
    {
      string title = ExtractTitle(heading);
      LoreNodeCollection newCollection = new LoreNodeCollection();

      return newCollection;
    }


    /// <summary>
    /// Creates a new LoreSection, where the input HeadingBlock is the one with the {section} tag.
    /// <para/>
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Get the title for this section, create new LoreSection to return</item>
    ///   <item>When encountering a HeadingBlock, if it is a superheader or sibling (lower or same level number), this section is done, return.</item>
    ///   <item>When encountering a HeadingBlock, if it is a subheader (lower level number), throw an error.</item>
    ///   <item>Otherwise, add the block encountered to the LoreSection's Blocks list.</item>
    /// </list>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="currentIndex"></param>
    /// <param name="heading"></param>
    /// <param name="secDef"></param>
    /// <returns></returns>
    private LoreSection ParseSection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreSectionDefinition secDef)
    {
      LoreSection newSection = new LoreSection(ExtractTitle(heading));
      newSection.Definition = secDef;

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        Block currentBlock = doc[currentIndex];

        switch(currentBlock)
        {
          case HeadingBlock hb:
            // If the header we're iterating over is subheader to the section's header..
            if (heading.Level < hb.Level)
            {
              // If the subheader is not a defined section, throw error.
              string headingTitle = ExtractTitle(hb);
              LoreSectionDefinition subSecDef = secDef.sections?.FirstOrDefault(sd => sd.name.Equals(headingTitle));
              if (subSecDef == null)
                throw new UnexpectedSectionNameException(_currentFile, currentIndex, hb.Line, newSection.Name, headingTitle);

              LoreSection newSubSection = ParseSection(doc, ref currentIndex, hb, subSecDef);
              newSection.SubSections.Add(newSubSection);
              currentIndex--;
            }
            // If it is instead a sibling or lower number header level, return this sectoin
            else if (heading.Level >= hb.Level)
              return newSection;
            break;

          case ParagraphBlock pb:
            newSection.Text += GetStringFromParagraphBlock(pb);
            break;
          
          // ListBlock can be a list of attributes ONLY if the section definition has fields defined.
          // Otherwise, the ListBlock is treated as text;
          case ListBlock lb:
            if (secDef.HasFields)
              newSection.Attributes = ParseListAttributes(doc, currentIndex, lb, secDef.fields);
            else
              newSection.Text += GetStringFromListBlock(lb);
            break;
          default:
            break;

        }
        newSection.Blocks.Add(currentBlock);

        currentIndex++;
      }

      return newSection;
    }


    private KeyValuePair<string, LoreAttribute> ParseParagraphBlocks(MarkdownDocument doc, ref int currentIndex, ParagraphBlock paragraphBlock, LoreTypeDefinition typeDef)
    { 
      LoreAttributeDefinition field = typeDef.fields.FirstOrDefault(fieldDef => fieldDef.style == EStyle.Freeform);
      if (field == null)
        throw new Exception($"Started parsing a Freeform paragraph attribute for a type that does not define one! Type{typeDef}");

      KeyValuePair<string, LoreAttribute> paragraphs = new KeyValuePair<string, LoreAttribute>(field.name, new LoreAttribute());
      paragraphs.Value.Values = new List<string>();

      while (currentIndex < doc.Count)
      {
        if (doc[currentIndex] is ParagraphBlock)
        {
          // Get the first line from the current ParagraphBlock
          paragraphs.Value.Values.AddRange(ParseContainerInline((doc[currentIndex] as ParagraphBlock).Inline));

          currentIndex++;
        }
        else
          break;
      }
      return paragraphs;
    }

    private List<string> ParseContainerInline(ContainerInline containerInline)
    {
      List<string> lines = new List<string>();

      foreach (var child in containerInline)
        if (child is LineBreakInline)
          lines.Add("\r\n");
        else
          lines.Add(child.ToString());

      return lines;
    }

    private bool IsFlatAttributeDeclaration(string flatInlineText) => flatInlineText.Contains(":") && !flatInlineText.EndsWith(":");

    private Dictionary<string, LoreAttribute> ParseListAttributes(MarkdownDocument doc, int currentIndex, ListBlock listBlock, List<LoreAttributeDefinition> attributeDefinitions)
    {
      string fieldValue = string.Empty;
      Dictionary<string, LoreAttribute> attributeDict = new Dictionary<string, LoreAttribute>();

      //listBlock = (ListBlock)doc[currentIndex];

      foreach (var item in listBlock)
      {
        if (item is not ListItemBlock) { break; }

        var contentItem = (item as ListItemBlock)[0] as ParagraphBlock;
        var inline = contentItem.Inline.FirstChild;

        LoreAttribute newAttribute = new LoreAttribute();

        string parsedFieldName = string.Empty;
        string parsedFieldValue = string.Empty;
        List<string> parsedFieldValues = new List<string>();

        // STEP 1: Get the field name:.

        string parsedInlineText = GetStringFromParagraphBlock(contentItem);


        /* FLAT PARSING
         * ex:
         * - **Date:** June 30, 2002
         * - Date: June 30, 2002
         */
        if (IsFlatAttributeDeclaration(parsedInlineText))
        {
          var fieldAndVal = parsedInlineText.Split(':');
          parsedFieldName = TrimFieldName(fieldAndVal[0]);
          parsedFieldValue = fieldAndVal[1];
          newAttribute.Value = parsedFieldValue.Trim();
        }
        // otherwise, it does not have a value, just trim the inline text to a field name.
        else
        {
          parsedFieldName = TrimFieldName(parsedInlineText);
        }

        // either way, we have the field name. Let's see if we can find a definition -- if not, it remains null
        newAttribute.Definition = attributeDefinitions.Where(lad => lad.name.Equals(parsedFieldName)).FirstOrDefault();

        if (newAttribute.Definition == null)
          throw new UnexpectedFieldNameException(string.Empty, currentIndex, contentItem.Line + 1, parsedFieldName);


        /* NESTED PARSING & SUBBULLET
         * 
         * example of tested values:
         * - Occurence time
         *   - Start date: September 9, 1999
         *   - End date: November 11, 2011
         * 
         * example of multivalue:
         * - Name:
         *   - Paula Mer Verdell
         *   - Green Bean (nickname)
         * 
         * example of subbullet single value:
         * - Name:
         *   - Jimmy
         *   
         * or flat fields with values AND nested attributes:
         * - Name: Jack
         *   - Alias: Jimmy
         */
        if((item as ListItemBlock).Count > 1)
        {
          // Need to get the ListBlock for the indented ListItemBlocks
          // This ListBlock contains ListItemBlocks that are either nested fields multiple values, NEVER BOTH
          ListBlock lb = (item as ListItemBlock)[1] as ListBlock;

          // if it has nested fields
          if (newAttribute.Definition.HasNestedFields)
          {
            newAttribute.NestedAttributes = ParseListAttributes(doc, currentIndex, lb, newAttribute.Definition.nestedFields);
          }

          // if it allows multiple values
          else if (newAttribute.Definition.multivalue)
          {
            newAttribute.Values = new List<string>();
            foreach(ListItemBlock block in lb)
            {
              newAttribute.Values.Add(GetStringFromParagraphBlock(block[0] as ParagraphBlock));
            }
          }

          // if no nested fields and not multivalue, we sure better hope this nested ListBlock is just a single value...
          else
          {
            if(lb.Count > 1)
              throw new NestedBulletsOnSingleValueChildlessAttributeException(_currentFile, currentIndex, lb.Line + 1, newAttribute.Definition.name);

            ListItemBlock lib = lb[0] as ListItemBlock;

            newAttribute.Value = GetStringFromParagraphBlock(lib[0] as ParagraphBlock);
          }
        }

        attributeDict[parsedFieldName] = newAttribute;

      }


      return attributeDict;
    }


    //private string ParseListAttributes(MarkdownDocument doc, ref int currentIndrex, ListBlock listBlock, List<Lore>)


    private string GetStringFromParagraphBlock(ParagraphBlock pgBlock)
    {
      string ret = string.Empty;

      ContainerInline contInl = pgBlock.Inline;

      Inline currentBlock = contInl.FirstChild;

      while (currentBlock != null)
      {
        switch (currentBlock)
        {
          case EmphasisInline emph:
            ret += emph.FirstChild.ToString();
            break;
          case LiteralInline lit:
            ret += lit.ToString();
            break;
        }
        currentBlock = currentBlock.NextSibling;
      }
      return ret;
    }

    private string GetStringFromListBlock(ListBlock lb)
    {
      string ret = string.Empty;

      foreach (ListItemBlock lib in lb)
      {
        ParagraphBlock pb = lib.LastChild as ParagraphBlock;
        if (pb != null)
          ret += GetStringFromParagraphBlock(pb) + "\r\n";
      }

      return ret;
    }
  }
}
