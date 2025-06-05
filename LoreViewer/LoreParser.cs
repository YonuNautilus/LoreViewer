using DynamicData;
using LoreViewer.Exceptions;
using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

    private bool _hadFatalError = false;
    public bool HadFatalError => _hadFatalError;

    public ObservableCollection<LoreCollection> _collections = new ObservableCollection<LoreCollection>();
    public ObservableCollection<ILoreNode> _nodes = new ObservableCollection<ILoreNode>();
    public ObservableCollection<Tuple<string, int, int, Exception>> _errors = new();
    public ObservableCollection<string> _warnings = new ObservableCollection<string>();

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

    public ILoreNode? GetNode(string nodeName) => _nodes.FirstOrDefault(node => node.Name.Equals(nodeName));
    public bool HasNode(string nodeName) => _nodes.Any(node => node.Name.Equals(nodeName));


    public void ParseSettingsFromFile(string settingsFilePath)
    {
      var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

      string settingsText = File.ReadAllText(settingsFilePath);

      _settings = deserializer.Deserialize<LoreSettings>(settingsText);

      _settings.PostProcess();
    }

    public void BeginParsingFromFolder(string FolderPath)
    {
      _hadFatalError = false;
      _folderPath = FolderPath;

      string fullSettingsPath = Path.Combine(FolderPath, LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        throw new Exception($"Did not find file {fullSettingsPath}");

      try
      {
        ParseSettingsFromFile(fullSettingsPath);
      }
      catch (Exception e)
      {
        _errors.Add(new Tuple<string, int, int, Exception>("Lore_Settings.yaml", -1, -1, e));
        _hadFatalError = true;
        return;
      }

      IEnumerable<string> temp = Directory.EnumerateFiles(FolderPath, "*.md", SearchOption.AllDirectories);

      string[] files = (_settings.Settings == null) ?
        temp.ToArray() :
        temp.Where(fp => !_settings.Settings.blockedPaths.Any(bp => fp.Contains(bp))).ToArray();

      foreach (string filePath in files)
      {
        try
        {
          ParseFile(filePath);
        }
        catch (LoreParsingException lpe)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? filePath : Path.GetRelativePath(_folderPath, filePath);
          _errors.Add(new Tuple<string, int, int, Exception>(pathForException, lpe.BlockIndex, lpe.LineNumber, lpe));
        }
        catch (Exception ex)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? filePath : Path.GetRelativePath(_folderPath, filePath);
          _errors.Add(new Tuple<string, int, int, Exception>(pathForException, -1, -1, ex));
          throw;
        }
      }
    }
    private static string GetCollectionType(string fullTypeName) => Regex.Match(fullTypeName, NestedTypeTagRegex).Value;
    private static string ExtractTag(HeadingBlock block) => Regex.Match(GetStringFromContainerInline(block.Inline), TypeTagRegex)?.Value;
    private static string ExtractTitle(HeadingBlock block)
    {
      string headerText = GetStringFromContainerInline(block.Inline);
      string title = Regex.Match(headerText, HeaderWithoutTagRegex)?.Value;

      if (string.IsNullOrEmpty(title))
        title = headerText;

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

    private static bool TagIsANestedCollection(string tag) => Regex.Count(tag, "collection") > 1;

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
    ///   <item>If a type-tagged heading is found, create a <c>LoreNode</c> by calling <c>ParseType</c>. AddNode that node to the <c>_nodes</c> list.</item>
    ///   <item>If a collection-tagged heading is found, create a <c>LoreCollection</c> by calling <c>ParseCollection</c>. AddNode that collection to the <c>_collections</c> list.</item>
    /// </list>
    /// </summary>
    /// <param name="filePath">Path of the Markdown file being parsed.</param>
    public void ParseFile(string filePath)
    {
      _currentFile = filePath;
      int currentIndex = 0;
      string fileContent = File.ReadAllText(filePath);
      MarkdownDocument document = Markdown.Parse(fileContent);

      // Once here, loop through the document, looking for headers

      // Start collecting a set of LoreElements for this file.

      List<ILoreNode> elementsInThisFile = new List<ILoreNode>();
      List<LoreCollection> collectionsInThisFile = new List<LoreCollection>();

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
                //_collections.AddNode(ParseCollection(document, ref currentIndex, block, tag));
                collectionsInThisFile.Add(ParseCollection(document, ref currentIndex, block, MakeNestedDefinitions(tag)));
              }
              else
              {
                LoreCollectionDefinition lcd = new LoreCollectionDefinition() { ContainedType = _settings.GetTypeDefinition(GetCollectionType(tag)) };
                collectionsInThisFile.Add(ParseCollection(document, ref currentIndex, block, lcd));
              }
            }
            else if (_settings.HasTypeDefinition(tag))
            {
              //_nodes.AddNode(ParseType(document, ref currentIndex, block, _settings.GetTypeDefinition(tag)));
              elementsInThisFile.Add(ParseType(document, ref currentIndex, block, _settings.GetTypeDefinition(tag)));
            }
            else if (BlockIsASection(block))
            {
              throw new FirstHeadingTagException(filePath, currentIndex, block.Line + 1);
            }
            else
            {
              throw new DefinitionNotFoundException(filePath, currentIndex, block.Line + 1, title);
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

      // Now, all elements have been collected.
      IEnumerable<ILoreNode> nodesFromThisFile = elementsInThisFile.Where(le => le is LoreNode);
      IEnumerable<LoreCollection> collectionsFromThisFile = collectionsInThisFile.Where(le => le is LoreCollection);

      foreach(LoreNode ln in nodesFromThisFile)
      {
        ILoreNode nodeWithSameName = _nodes.FirstOrDefault(node => node.Name == ln.Name && node.Definition == ln.Definition);
        if (nodeWithSameName != null)
          _nodes.Replace(nodeWithSameName, nodeWithSameName.MergeWith(ln));
        else
          _nodes.Add(ln);
      }

      _collections.AddRange(collectionsFromThisFile);
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
      LoreNode newNode = new LoreNode(title, typeDef);
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
            ObservableCollection<LoreAttribute> attributes = ParseListAttributes(doc, currentIndex, lb, typeDef.fields);
            newNode.Attributes.AddRange(attributes);
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


              // If a tag IS declared in markdown, parse as its LoreElement type
              if (!string.IsNullOrWhiteSpace(newTag))
              {
                // CHECK FOR COLLECTIONS FIRST
                // Block is tagged with {collection:...}
                if (BlockIsACollection(hb))
                {
                  LoreCollection newCollection;

                  // if tagged like {collection:collection:type}
                  if (BlockIsANestedCollection(hb))
                  {
                    newCollection = ParseCollection(doc, ref currentIndex, hb, MakeNestedDefinitions(newTag));
                  }

                  // otherwise, tagged as {collection:type}
                  else
                  {

                    LoreCollectionDefinition lcd = new LoreCollectionDefinition() { ContainedType = _settings.GetTypeDefinition(GetCollectionType(newTag)) };
                    newCollection = ParseCollection(doc, ref currentIndex, hb, lcd);
                  }

                  newCollection.Name = newTitle;
                  newNode.Collections.Add(newCollection);
                  continue;
                }

                // Parse as a nested node of the type specified in the tag.
                else if (_settings.HasTypeDefinition(newTag))
                {
                  // had to make sure the type definition existed first
                  LoreTypeDefinition newTypeDef = _settings.GetTypeDefinition(newTag);
                  // If the type of node we have found is allowed as an embedded type in this current node...
                  if (typeDef.AllowsEmbeddedType(newTypeDef))
                  {
                    LoreTypeDefinition newNodeType = _settings.GetTypeDefinition(newTag);
                    LoreNode newNodeNode = ParseType(doc, ref currentIndex, hb, newNodeType);
                    newNode.Nodes.Add(newNodeNode);
                    continue;
                  }
                  // If the node (the one this method returns) does NOT allow this embedded type...
                  else
                  {
                    throw new EmbeddedNodeTypeNotAllowedException(_currentFile, currentIndex, currentBlock.Line + 1, typeDef.name, newTag);
                  }
                }
                
                // Parse as a section, if it has the {section} tag
                else if (newTag.Equals("section"))
                {
                  // Check if there's actually a section definition. If not, make a new section
                  LoreSectionDefinition lsd = typeDef.GetSectionDefinition(newTitle) ?? new LoreSectionDefinition(newTitle, true);
                  newNode.Sections.Add(ParseSection(doc, ref currentIndex, hb, lsd));
                  continue;
                }
              }
              // Here if no tag in markdown
              else
              {
                if (typeDef.HasCollectionDefinition(newTitle))
                {
                  LoreCollectionDefinition lcd = typeDef.GetCollectionDefinition(newTitle);

                  if (!_settings.HasTypeDefinition(lcd.entryTypeName) && lcd == null)
                    throw new UnexpectedTypeNameException(_currentFile, currentIndex, hb.Line + 1, newTitle);

                  if(lcd.IsCollectionOfCollections)
                    newNode.Collections.Add(ParseCollection(doc, ref currentIndex, hb, lcd));
                  else
                  {
                    LoreCollection newCol = ParseCollection(doc, ref currentIndex, hb, lcd);
                    newNode.Collections.Add(newCol);
                  }
                  continue;
                }
                else if (typeDef.HasSectionDefinition(newTitle))
                {
                  LoreSectionDefinition lsd = typeDef.GetSectionDefinition(newTitle);
                  newNode.Sections.Add(ParseSection(doc, ref currentIndex, hb, lsd));
                  continue;
                }
                else
                {
                  throw new DefinitionNotFoundException(_currentFile, currentIndex, hb.Line + 1, newTitle);
                }
              }
              break;

            // freeform
            case ParagraphBlock pb:
              newNode.AddNarrativeText(ParseParagraphBlocks(doc, ref currentIndex, pb, typeDef));
              //continue;
              break;

            // Fields
            case ListBlock lb:
              newNode.AddNarrativeText(GetStringFromListBlock(lb));
              break;
            default:
              break;
          }

        }
        currentIndex++;
      }

      return newNode;
    }

    private LoreCollectionDefinition MakeNestedDefinitions(string innerTag)
    {
      LoreCollectionDefinition lcd = new LoreCollectionDefinition();

      if (TagIsANestedCollection(innerTag))
        lcd.ContainedType = MakeNestedDefinitions(GetCollectionType(innerTag));
      else if (_settings.HasTypeDefinition(GetCollectionType(innerTag)))
      {
        lcd.ContainedType = _settings.GetTypeDefinition(GetCollectionType(innerTag));
      }
      else
      {
        throw new CollectionWithUnknownTypeException(_currentFile, -1, -1, innerTag);
      }

      return lcd;
    }



    /// <summary>
    /// Creates a LoreCollection of LoreElements. Starts from a heading block.
    /// If heading had tag (example {collection:type}), a LoreCollectionDefinition object with ContainedType set
    /// to the LoreTypeDefinition of the 'type' in the tag should be created from the calling method.
    /// <para />
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Step forward, expect headings on level deeper than the original heading (UNSURE IF THESE HEADING NEED TYPE TAGS OR NOT)</item>
    ///   <item>If a heading is UNTAGGED, Give a WARNING (not an error, yet)</item>
    ///   <item>If a heading is tagged with a type not accepted by the collection, throw an error</item>
    ///   <item>If a non-heading block is encountered before the first type heading is found, throw an error (LoreCollection does not have metadata!)</item>
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
    /// <param name="colType"></param>
    /// <returns></returns>
    private LoreCollection ParseCollection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreCollectionDefinition colType)
    {
      string title = ExtractTitle(heading);

      LoreCollection newCollection = new LoreCollection(title, colType);

      currentIndex++;

      while(currentIndex < doc.Count)
      {
        Block currentBlock = doc[currentIndex];

        switch (currentBlock)
        {
          case ParagraphBlock pb:
            newCollection.AddNarrativeText(GetStringFromParagraphBlock(pb));
            break;
          case HeadingBlock hb:
            if (hb.Level > heading.Level)
            {
              if (colType.IsCollectionOfCollections)
                newCollection.Collections.Add(ParseCollection(doc, ref currentIndex, hb, colType.ContainedType as LoreCollectionDefinition));
              else
                newCollection.Nodes.Add(ParseType(doc, ref currentIndex, hb, colType.ContainedType as LoreTypeDefinition));
              continue;
            }
            else
              return newCollection;
            currentIndex--;
            break;
          default:
            break;
        }
        currentIndex++;
      }


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
      LoreSection newSection = new LoreSection(ExtractTitle(heading), secDef);

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        Block currentBlock = doc[currentIndex];

        switch (currentBlock)
        {
          case HeadingBlock hb:
            // If the heading we're iterating over is subheader to the section's heading,
            // it must be a subsection (by rules: sections can only contain sections as subheadings)
            if (heading.Level < hb.Level)
            {
              // If the subheading is not a defined section, check if it has a section tag. If throw error.
              string headingTitle = ExtractTitle(hb);
              string headingTag = ExtractTag(hb);
              LoreSectionDefinition subSecDef = secDef.sections?.FirstOrDefault(sd => sd.name.Equals(headingTitle));
              if (subSecDef == null && !headingTag.StartsWith("section"))
                throw new UnexpectedSectionNameException(_currentFile, currentIndex, hb.Line, newSection.Name, headingTitle);
              else if (headingTag.StartsWith("section")) // If an undefined but {section} tagged header, force a new freeform section
                subSecDef = new LoreSectionDefinition(headingTitle, true);

              LoreSection newSubSection = ParseSection(doc, ref currentIndex, hb, subSecDef);
              newSection.Sections.Add(newSubSection);
              currentIndex--;
            }
            // If it is instead a sibling or lower number header level, return this section
            else if (heading.Level >= hb.Level)
              return newSection;
            break;

          case ParagraphBlock pb:
            newSection.AddNarrativeText(GetStringFromParagraphBlock(pb) + "\r\n");
            break;

          // ListBlock can be a list of attributes ONLY if the section definition has fields defined.
          // Otherwise, the ListBlock is treated as text;
          case ListBlock lb:
            if (secDef.HasFields)
              newSection.Attributes = ParseListAttributes(doc, currentIndex, lb, secDef.fields);
            else
              newSection.AddNarrativeText(GetStringFromListBlock(lb));
            break;
          default:
            break;

        }
        newSection.Blocks.Add(currentBlock);

        currentIndex++;
      }

      return newSection;
    }


    private string ParseParagraphBlocks(MarkdownDocument doc, ref int currentIndex, ParagraphBlock paragraphBlock, LoreTypeDefinition typeDef)
    {
      List<string> rets = new List<string>();

      while (currentIndex < doc.Count)
      {
        if (doc[currentIndex] is ParagraphBlock)
        {
          // Get the first line from the current ParagraphBlock
          rets.Add(ParseContainerInline((doc[currentIndex] as ParagraphBlock).Inline));
          currentIndex++;
        }
        else
        {
          currentIndex--;
          break;
        }
      }
      return string.Join("\r\n", rets);
    }

    private string ParseContainerInline(ContainerInline containerInline)
    {
      string lines = string.Empty;

      foreach (var child in containerInline)
        switch (child)
        {
          case LineBreakInline lb:
            lines += "\r\n";
            break;
          case EmphasisInline em:
            lines += em.LastChild.ToString();
            break;
          default:
            lines += child.ToString();
            break;
        }
      return lines;
    }

    private bool IsFlatAttributeDeclaration(string flatInlineText) => flatInlineText.Contains(":") && !flatInlineText.EndsWith(":");

    private ObservableCollection<LoreAttribute> ParseListAttributes(MarkdownDocument doc, int currentIndex, ListBlock listBlock, List<LoreFieldDefinition> attributeDefinitions)
    {
      string fieldValue = string.Empty;
      ObservableCollection<LoreAttribute> attributeList = new ObservableCollection<LoreAttribute>();

      //listBlock = (ListBlock)doc[currentIndex];

      foreach (var item in listBlock)
      {
        if (item is not ListItemBlock) { break; }

        var contentItem = (item as ListItemBlock)[0] as ParagraphBlock;
        var inline = contentItem.Inline.FirstChild;

        string parsedFieldName = string.Empty;
        List<string> parsedFieldValues = new List<string>();
        List<LoreAttribute> parsedNestedAttributes = new List<LoreAttribute>();

        // STEP 1: Get the field name:.

        string parsedInlineText = GetStringFromParagraphBlock(contentItem);


        /* FLAT PARSING
         * ex:
         * - **Date:** June 30, 2002
         * - Date: June 30, 2002
         */
        if (IsFlatAttributeDeclaration(parsedInlineText))
        {
          var fieldAndVal = parsedInlineText.Split(':', 2);
          parsedFieldName = TrimFieldName(fieldAndVal[0]);
          parsedFieldValues.Add(fieldAndVal[1].Trim());
        }
        // otherwise, it does not have a value, just trim the inline text to a field name.
        else
        {
          parsedFieldName = TrimFieldName(parsedInlineText);
        }

        // either way, we have the field name. Let's see if we can find a definition -- if not, it remains null
        LoreFieldDefinition newDef = attributeDefinitions.Where(lad => lad.name.Equals(parsedFieldName)).FirstOrDefault();

        if (newDef == null)
          throw new UnexpectedFieldNameException(string.Empty, currentIndex, contentItem.Line + 1, parsedFieldName);

        LoreAttribute newAttribute = new LoreAttribute(parsedFieldName, newDef);


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
        if ((item as ListItemBlock).Count > 1)
        {
          // Need to get the ListBlock for the indented ListItemBlocks
          // This ListBlock contains ListItemBlocks that are either nested fields multiple values, NEVER BOTH
          ListBlock lb = (item as ListItemBlock)[1] as ListBlock;

          // if it has nested fields
          if (newDef.HasFields)
          {
            parsedNestedAttributes.Add(ParseListAttributes(doc, currentIndex, lb, newDef.fields));
          }

          // if it allows multiple values
          else if (newDef.style == EStyle.MultiValue)
          {
            newAttribute.Values = new List<string>();
            foreach (ListItemBlock block in lb)
            {
              parsedFieldValues.Add(GetStringFromParagraphBlock(block[0] as ParagraphBlock).Trim());
            }
          }
          else if (newDef.style == EStyle.Textual)
          {
            parsedFieldValues.Add(GetStringFromListBlock(lb));
          }

          // if no nested fields and not multivalue, we sure better hope this nested ListBlock is just a single value...
          else
          {
            if (lb.Count > 1)
              throw new NestedBulletsOnSingleValueChildlessAttributeException(_currentFile, currentIndex, lb.Line + 1, newAttribute.Definition.name);

            ListItemBlock lib = lb[0] as ListItemBlock;

            parsedFieldValues.Add(GetStringFromParagraphBlock(lib[0] as ParagraphBlock));
          }
        }

        newAttribute.Append(parsedFieldValues);
        newAttribute.Attributes.Add(parsedNestedAttributes);
        attributeList.Add(newAttribute);

      }


      return attributeList;
    }


    private static string GetStringFromParagraphBlock(ParagraphBlock pgBlock)
    {
      return GetStringFromContainerInline(pgBlock.Inline);
      /*
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
          default:
            ret += "\r\n";
            break;
        }
        //ret += "\r\n";
        currentBlock = currentBlock.NextSibling;
      }
      return ret;
      */
    }

    private static string GetStringFromContainerInline(ContainerInline cInl)
    {
      string ret = string.Empty;

      Inline currentBlock = cInl.FirstChild;

      while (currentBlock != null)
      {
        switch (currentBlock)
        {
          case EmphasisInline emph:
            ret += emph.FirstChild.ToString();
            break;
          case CodeInline code:
            ret += code.Content;
            break;
          case LiteralInline lit:
            ret += lit.ToString();
            break;
          case HtmlInline:
            // Do not add HTML tags into text. For now.
            break;
          default:
            ret += "\r\n";
            break;
        }
        //ret += "\r\n";
        currentBlock = currentBlock.NextSibling;
      }
      return ret;
    }

    private static string GetStringFromListBlock(ListBlock lb)
    {
      string ret = string.Empty;

      foreach (ListItemBlock lib in lb)
      {
        ret += GetStringFromListItemBlock(lib);
      }

      return ret;
    }

    private static string GetStringFromListItemBlock(ListItemBlock lib)
    {
      string ret = string.Empty;
      foreach(Block block in lib)
      {
        switch (block)
        {
          case ParagraphBlock pb:
            ret += new string(' ', lib.Column) + (lib.Parent as ListBlock).BulletType + ' ' + GetStringFromParagraphBlock(pb);
            ret += "\r\n";
            break;
          case ListBlock lb:
            ret += GetStringFromListBlock(lb);
            break;
          default:
            ret += "\r\n";
            break;
        }
      }

      return ret;
    }
  }
}
