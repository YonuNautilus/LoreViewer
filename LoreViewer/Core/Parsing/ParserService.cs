using DynamicData;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Domain.Settings.Definitions;
using LoreViewer.Exceptions.LoreParsingExceptions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using SharpYaml.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LoreViewer.Core.Parsing
{
  public enum ETagType { Unknown, node, collection, section }

  public struct LoreTagInfo
  {
    public ETagType tagType;
    public string tagname;

    public bool IsNode => tagType == ETagType.node;
    public bool IsSection => tagType == ETagType.section;
    public bool IsCollection => tagType == ETagType.collection;
    public bool IsNestedCollection => TypeName.Contains("collection") && TypeName.Split(":").Length > 1 && TypeName.Split(":")[0].Equals("collection");

    public bool HasID => m_dAttributes != null && m_dAttributes.ContainsKey("ID");
    public string ID => HasID ? m_dAttributes["ID"] : string.Empty;

    public bool HasType => m_dAttributes != null && m_dAttributes.ContainsKey("type");
    public string TypeName => m_dAttributes != null ? m_dAttributes.ContainsKey("type") ? m_dAttributes["type"] : string.Empty : string.Empty;

    public string CollectionName => "collection:" + TypeName;

    public readonly Dictionary<string, string> Attributes => m_dAttributes;

    private Dictionary<string, string> m_dAttributes;

    private void ParseHTML(HtmlInline html)
    {
      var elem = XElement.Parse(html.Tag);
      tagname = elem.Name.LocalName;
      m_dAttributes = elem.Attributes().ToDictionary(attr => attr.Name.LocalName, attr => attr.Value);

      Enum.TryParse<ETagType>(tagname, out tagType);
    }

    public LoreTagInfo(HtmlInline html) => ParseHTML(html);

    public void SetID(string newID) => m_dAttributes["ID"] = newID;

    public static bool operator ==(LoreTagInfo left, LoreTagInfo right)
    {
      // If no ID defined, NO MERGING is allowed
      if (!left.HasID || !right.HasID) return false;

      // First see if either has missing info while the other does not
      if ((left.Attributes == null && right.Attributes != null) || (left.Attributes != null && right.Attributes == null)) return false;
      if ((left.HasID && !right.HasID) || (!left.HasID && right.HasID)) return false;
      if ((left.Attributes == null && right.Attributes != null) || (left.Attributes != null && right.Attributes == null)) return false;

      //If both actually have an ID, check that they match
      if (left.HasID && right.HasID && left.ID != right.ID) return false;

      // Then check their tagType matches
      if (left.tagType != right.tagType) return false;

      // Then check for attribute matching (this will cover type attribute)
      if (left.Attributes != null && right.Attributes != null)
      {
        // If different amounts, they are not equivalent.
        if (left.Attributes.Count != right.Attributes.Count) return false;

        // If attribute dictiontionaries have the same amount, go through and check that each have the same keys with the same values.
        foreach (var kvp in left.Attributes)
        {
          if (right.Attributes.ContainsKey(kvp.Key))
          {
            if (right.Attributes[kvp.Key] != kvp.Value) return false;
          }
        }
      }

      return true;
    }

    public static bool operator !=(LoreTagInfo left, LoreTagInfo right)
    {
      // First see if either has missing info while the other does not
      if ((left.Attributes == null && right.Attributes != null) || (left.Attributes != null && right.Attributes == null)) return true;
      if ((left.HasID && !right.HasID) || (!left.HasID && right.HasID)) return true;
      if ((left.Attributes == null && right.Attributes != null) || (left.Attributes != null && right.Attributes == null)) return true;

      //If both actually have an ID, check that they differ
      if (left.HasID && right.HasID && left.ID != right.ID) return true;

      // Then check their tagType differ
      if (left.tagType != right.tagType) return true;

      // Then check for attribute differences (this will cover type attribute)
      if (left.Attributes != null && right.Attributes != null)
      {
        // If different amounts, they are not equivalent.
        if (left.Attributes.Count != right.Attributes.Count) return true;

        // If attribute dictiontionaries have the same amount, go through and check that each have the same keys with the same values.
        foreach (var kvp in left.Attributes)
        {
          if (right.Attributes.ContainsKey(kvp.Key))
          {
            if (right.Attributes[kvp.Key] != kvp.Value) return true;
          }
        }
      }

      return false;
    }

    public bool CanMergeWith(LoreTagInfo incoming)
    {
      if (!this.HasID || !incoming.HasID) return false;
      if (this.tagType != incoming.tagType) return false;
      if (string.IsNullOrEmpty(this.TypeName) || string.IsNullOrEmpty(incoming.TypeName)) return false;
      if (this.TypeName != incoming.TypeName) return false;
      return this.ID == incoming.ID;
    }

    internal LoreTagInfo? CreateCompositeNodeTag(LoreTagInfo value)
    {
      LoreTagInfo ret = new LoreTagInfo();
      ret.m_dAttributes = new Dictionary<string, string>();

      ret.SetID(this.ID);

      ret.Attributes["type"] = this.TypeName;
      ret.tagType = this.tagType;

      return ret;
    }
  }

  public class ParserService
  {
    const string HeaderWithoutTagRegex = ".+(?={)";
    const string NestedTypeTagRegex = "(?<=:).+";

    private LoreSettings _settings;
    private string _folderPath;

    private bool _hadFatalError = false;
    public bool HadFatalError => _hadFatalError;

    private List<LoreCollection> _collections = new List<LoreCollection>();
    private List<ILoreNode> _nodes = new List<ILoreNode>();
    private List<ParseError> _errors = new();
    private List<string> _warnings = new List<string>();

    public IReadOnlyList<ILoreNode> Nodes => _nodes.ToList();
    public IReadOnlyList<LoreCollection> Collections => _collections.ToList();
    public IReadOnlyList<ParseError> Errors => _errors.ToList();
    public IReadOnlyList<string> Warnings => _warnings.ToList();


    public int m_iProgress;

    public int m_iFileCount;

    private readonly ConcurrentBag<ILoreNode> _parsedNodes = new();
    private readonly ConcurrentBag<LoreCollection> _parsedCollections = new();
    private readonly ConcurrentBag<ParseError> _parsedErrors = new();
    private readonly ConcurrentBag<string> _parsedWarnings = new();

    public IEnumerable<LoreEntity> AllEntities => _nodes.Cast<LoreEntity>().Concat<LoreEntity>(_collections);


    /// <summary>
    /// Key is the file name (path relative to the lore folder), and value is the index of the block that is 'orphaned'
    /// (i.e. not tagged when it needs to be)
    /// </summary>
    public Dictionary<string, int> OrphanedBlocks { get; set; } = new Dictionary<string, int>();

    public LoreSettings Settings { get { return _settings; } }

    public ParserService() { }

    public ParserService(LoreSettings settings)
    {
      _settings = settings;
    }

    public ILoreNode? GetNodeByName(string nodeName) => _nodes.FirstOrDefault(node => node.Name.Equals(nodeName));
    public ILoreNode? GetNodeByID(string id) => _nodes.Cast<LoreEntity>().FirstOrDefault(node => node.ID.Equals(id)) as ILoreNode;
    public bool HasNode(string nodeName) => _nodes.Any(node => node.Name.Equals(nodeName));


    public string[] ParsableFiles(string folderPath, LoreSettings settingsWithBlocked)
    {
      IEnumerable<string> temp = Directory.EnumerateFiles(folderPath, "*.md", SearchOption.AllDirectories);

      string[] files = (settingsWithBlocked.settings == null) ?
        temp.ToArray() :
        temp.Where(fp => !settingsWithBlocked.settings.blockedPaths.Any(bp => fp.Contains(bp))).ToArray();

      return files;
    }


    public void Clear()
    {
      _nodes.Clear();
      _collections.Clear();
      _errors.Clear();
      _warnings.Clear();
      m_iFileCount = 0;
      m_iProgress = 0;

      ClearParsed();
    }

    public void ClearParsed()
    {
      _parsedNodes.Clear();
      _parsedCollections.Clear();
      _parsedErrors.Clear();
      _parsedWarnings.Clear();
    }


    /// <summary>
    /// Handles all lore parsing process, including creating the settings model, and returning the ParseResult which will contain any parsed models
    /// or indicate an error if there was a problem while parsing.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="progress"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ParseResult> ParseFolderAsync(string folderPath, IProgress<int>? progress, CancellationToken ct)
    {
      ParseResult res = new();

      // start by trying to deserialize settings from YAML
      string fullSettingsPath = Path.Combine(folderPath, LoreSettings.LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        return ParseResult.Fatal(EFatalCode.CouldNotFindSettingsFile,
          $"Could not find file {LoreSettings.LoreSettingsFileName} in folder {folderPath}");

      try
      {
        _settings = LoreSettings.ParseSettingsFromFolder(folderPath);
      }
      catch (Exception ex)
      {
        return ParseResult.Fatal(EFatalCode.CouldNotParseSettingsFile,
          $"Could not parse settings file.");
      }

      var files = ParsableFiles(folderPath, _settings);
      int count = 0;

      await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 20 }, async (file, token) =>
      {
        try
        {
          ParseFile(file);
        }
        catch (LoreParsingException lpe)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? file : Path.GetRelativePath(folderPath, file);
          _parsedErrors.Add(new ParseError(pathForException, lpe.BlockIndex, lpe.LineNumber, lpe));
        }
        catch (Exception ex)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? file : Path.GetRelativePath(folderPath, file);
          _parsedErrors.Add(new ParseError(pathForException, -1, -1, ex));
        }

        Interlocked.Increment(ref count);
        progress?.Report(count);
      });


      // Now, all elements have been collected, we can start the merge process.
      PerformMergeFromParsed();


      // Now that all have been merged, we can resolve references (like the reflist field/attribute).
      PerformReferenceResolution();

      res.Settings = _settings;
      res.Models = AllEntities.ToList();
      res.Errors = Errors.ToList();

      return res;
    }

    /// <summary>
    /// Merge all mergeable parsed lore elements (nodes). Elements are mergeable when they have a matching ID and TypeDefinition.
    /// </summary>
    public void PerformMergeFromParsed()
    {
      foreach (LoreEntity e in _parsedNodes)
      {
        if (e is LoreNode node)
        {
          ILoreNode nodeWithSameIDAndType = _nodes.FirstOrDefault(existingNode => existingNode.CanMergeWith(node));

          if (nodeWithSameIDAndType != null)
            _nodes.Replace(nodeWithSameIDAndType, nodeWithSameIDAndType.MergeWith(node));
          else
            _nodes.Add(node);
        }
      }

      _collections.AddRange(_parsedCollections);
      _errors.AddRange(_parsedErrors);
      _warnings.AddRange(_parsedWarnings);

      ClearParsed();
    }

    public void PerformReferenceResolution()
    {
      foreach (ILoreNode n in _nodes)
        foreach (LoreAttribute la in n.Attributes)
          la.ResolveNodeRefs(this);

      foreach (LoreCollection c in _collections)
      {
        if (c.ContainsNodes)
        {
          foreach (ILoreNode n in c.Nodes)
            foreach (LoreAttribute la in n.Attributes)
              la.ResolveNodeRefs(this);
        }
      }
    }

    public void ParseSettingsFromFile(string settingsFilePath)
    {
      var deserializer = new Serializer();

      string settingsText = File.ReadAllText(settingsFilePath);

      _settings = deserializer.Deserialize<LoreSettings>(settingsText);

      _settings.PostProcess();
    }

    public void BeginParsingFromFolder(string FolderPath)
    {
      _hadFatalError = false;
      _folderPath = FolderPath;

      string[] files = ParsableFiles(FolderPath, _settings);

      foreach (string filePath in files)
      {
        try
        {
          ParseSingleFile(filePath);
        }
        catch (LoreParsingException lpe)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? filePath : Path.GetRelativePath(_folderPath, filePath);
          _parsedErrors.Add(new ParseError(pathForException, lpe.BlockIndex, lpe.LineNumber, lpe));
        }
        catch (Exception ex)
        {
          string pathForException = string.IsNullOrEmpty(_folderPath) ? filePath : Path.GetRelativePath(_folderPath, filePath);
          _parsedErrors.Add(new ParseError(pathForException, -1, -1, ex));
          throw;
        }
      }

      PerformMergeFromParsed();

      PerformReferenceResolution();
    }


    private static LoreTagInfo? GetTagInfo(HeadingBlock hb, LoreParsingContext ctx, int blockIndex)
    {
      if (hb.Inline is ContainerInline ci && ci.LastChild is HtmlInline html)
        try
        {
          return new LoreTagInfo(html);
        }
        catch (Exception ex)
        {
          throw new LoreTagParsingException(ctx.FilePath, blockIndex, hb.Line + 1, html.Tag, ex);
        }
      else return null;
    }

    private static string GetCollectionType(string fullTypeName) => Regex.Match(fullTypeName, NestedTypeTagRegex).Value;
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

    private static bool TagIsANestedCollection(string tag) => Regex.Count(tag, "collection") > 1;

    public void ParseSingleFile(string filePath)
    {
      ParseFile(filePath);

      PerformMergeFromParsed();
    }

    /// <summary>
    /// The top-level, Markdown-first parsing method. Reads in a markdown file and begins parsing.
    /// <para />
    /// This method is where the variable <c>currentIndex</c> is declared. It gets passed as a ref to all subsequent parsing methods to keep it up to date here.
    /// <para />
    /// RULES/PROCESS:
    /// <list type="number">
    ///   <item>Before finding the first heading, any non-heading blocks get added to a list of orphaned blocks</item>
    ///   <item>If the first heading block is not tagged with a valid type or a collection, throw an error</item>
    ///   <item>If a type-tagged heading is found, create a <c>LoreNode</c> by calling <c>ParseType</c>. AddNode that node to the <c>Nodes</c> list.</item>
    ///   <item>If a collection-tagged heading is found, create a <c>LoreCollection</c> by calling <c>ParseCollection</c>. AddNode that collection to the <c>_collections</c> list.</item>
    /// </list>
    /// </summary>
    /// <param name="filePath">Path of the Markdown file being parsed.</param>
    public void ParseFile(string filePath)
    {
      int currentIndex = 0;
      string fileContent = File.ReadAllText(filePath);
      MarkdownDocument document = Markdown.Parse(fileContent);

      LoreParsingContext ctx = new LoreParsingContext(filePath);


      // Once here, loop through the document, looking for headers

      while (currentIndex < document.Count)
      {
        // Start with the top level, get its type
        if (document[currentIndex] is HeadingBlock block)
        {
          if (block.Level != 1) throw new HeadingLevelErrorException(filePath, currentIndex, block.Line + 1, block);

          LoreTagInfo? uTag = GetTagInfo(block, ctx, currentIndex);

          string title = ExtractTitle(block);

          // if a type was given
          if (uTag.HasValue)
          {
            LoreTagInfo tag = uTag.Value;
            // Parse collection based on <collection ... /> tag, which is not even locally defined (in schema), it is MARKDOWN defined at the TOP LEVEL of a file.
            // It should not be treated as a locally (YAML) defined collection (ie no OwningDefinition needed to be set)
            if (tag.IsCollection)
            {
              if (tag.IsNestedCollection)
              {
                _parsedCollections.Add(ParseCollection(document, ref currentIndex, block, MakeNestedDefinitions(tag.CollectionName, ctx), ctx));
              }
              else
              {
                LoreCollectionDefinition lcd = new LoreCollectionDefinition();

                if (_settings.GetTypeDefinition(tag.TypeName) == null)
                  throw new CollectionWithUnknownTypeException(filePath, currentIndex, block.Line + 1, tag.TypeName);

                lcd.SetContainedType(_settings.GetTypeDefinition(tag.TypeName));
                _parsedCollections.Add(ParseCollection(document, ref currentIndex, block, lcd, ctx));
              }
            }
            // Parse collection based on the collection definition
            else if (_settings.HasCollectionDefinition(tag.TypeName))
            {
              _parsedCollections.Add(ParseCollection(document, ref currentIndex, block, _settings.GetCollectionDefinition(tag.TypeName), ctx));
            }
            else if (tag.IsNode && _settings.HasTypeDefinition(tag.TypeName))
            {
              LoreNode newNode = ParseNode(document, ref currentIndex, block, _settings.GetTypeDefinition(tag.TypeName), ctx);
              newNode.SetTag(tag);
              _parsedNodes.Add(newNode);
            }
            else if (tag.IsSection)
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
    private LoreNode ParseNode(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreTypeDefinition typeDef, LoreParsingContext ctx)
    {
      bool parsingFields = true;

      string title = ExtractTitle(heading);
      LoreNode newNode = new LoreNode(title, typeDef, ctx.FilePath, currentIndex, doc[currentIndex].Line + 1);
      newNode.BlockIndex = currentIndex;

      // Even though we have the typeDef parameter, if this method is called from ParseCollection, the type of this node may not be typeDef,
      // it may actually be tagged with a derived type, so we need to check that.
      LoreTagInfo? lti = GetTagInfo(heading, ctx, currentIndex);

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        var currentBlock = doc[currentIndex];

        if (parsingFields)
        {
          if (currentBlock is ListBlock)
          {
            ListBlock lb = currentBlock as ListBlock;
            List<LoreAttribute> attributes = ParseListAttributes(doc, currentIndex, lb, typeDef.fields, ctx);
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

              LoreTagInfo? newUTag = GetTagInfo(hb, ctx, currentIndex);
              string newTitle = ExtractTitle(hb);


              // If a tag IS declared in markdown, parse as its LoreElement type
              if (newUTag.HasValue)
              {
                LoreTagInfo newTag = newUTag.Value;

                // CHECK FOR COLLECTIONS FIRST
                // Block is tagged with {collection:...}
                if (newTag.IsCollection)
                {
                  LoreCollection newCollection;

                  // if tagged like <collection type="collection:TYPE_NAME" />
                  if (newTag.IsNestedCollection)
                  {
                    newCollection = ParseCollection(doc, ref currentIndex, hb, MakeNestedDefinitions(newTag.CollectionName, ctx), ctx);
                  }

                  // otherwise, tagged as <collection type="TYPE_NAME" />
                  else
                  {
                    // TYPE_NAME can be the name of a type definition, a global collection definition, or a local collection definition.
                    // Check where the TYPE_NAME matches.
                    // Check which it is.
                    LoreCollectionDefinition lcd;

                    if (_settings.HasCollectionDefinition(newTag.TypeName))
                    {
                      lcd = new LoreCollectionDefinition() { OwningDefinition = typeDef, entryCollectionName = newTag.TypeName };
                      lcd.SetContainedType(_settings.GetCollectionDefinition(newTag.TypeName));
                    }
                    else if (_settings.HasTypeDefinition(newTag.TypeName))
                    {
                      lcd = new LoreCollectionDefinition() { OwningDefinition = typeDef, entryTypeName = newTag.TypeName };
                      lcd.SetContainedType(_settings.GetTypeDefinition(GetCollectionType(newTag.TypeName)));
                    }
                    else if (typeDef.HasCollectionDefinition(newTag.TypeName))
                    {
                      lcd = new LoreCollectionDefinition() { OwningDefinition = typeDef, entryCollectionName = newTag.TypeName };
                      lcd.SetContainedType(typeDef.GetCollectionDefinition(newTag.TypeName));
                    }
                    else
                    {
                      lcd = new LoreCollectionDefinition() { OwningDefinition = typeDef, entryTypeName = GetCollectionType(newTag.TypeName) };
                      lcd.SetContainedType(_settings.GetTypeDefinition(GetCollectionType(newTag.TypeName)));
                    }


                    newCollection = ParseCollection(doc, ref currentIndex, hb, lcd, ctx);
                  }

                  newCollection.Name = newTitle;
                  newNode.Collections.Add(newCollection);
                  continue;
                }

                // Parse as a nested node of the type specified in the tag.
                else if (newTag.IsNode)
                {
                  if (_settings.HasTypeDefinition(newTag.TypeName))
                  {
                    // had to make sure the type definition existed first
                    LoreTypeDefinition newTypeDef = _settings.GetTypeDefinition(newTag.TypeName);
                    // If the type of node we have found is allowed as an embedded type in this current node...
                    if (typeDef.IsAllowedEmbeddedType(newTypeDef))
                    {
                      if (typeDef.IsAllowedEmbeddedNode(newTypeDef, newTitle))
                      {
                        if (!newNode.ContainsEmbeddedNode(newTypeDef, newTitle))
                        {
                          LoreTypeDefinition newNodeType = _settings.GetTypeDefinition(newTag.TypeName);
                          LoreNode newNodeNode = ParseNode(doc, ref currentIndex, hb, newNodeType, ctx);
                          newNodeNode.SetTag(newTag);
                          newNode.Nodes.Add(newNodeNode);
                          continue;
                        }
                        else
                          throw new EmbeddedNodeAlreadyAddedException(ctx.FilePath, currentIndex, currentBlock.Line + 1, newNode, newTypeDef, newTitle);
                      }
                      else
                      {
                        throw new EmbeddedNodeInvalidNameException(ctx.FilePath, currentIndex, currentBlock.Line + 1, typeDef, newTypeDef, newTitle);
                      }
                    }
                    // If the node (the one this method returns) does NOT allow this embedded type...
                    else
                    {
                      throw new EmbeddedNodeTypeNotAllowedException(ctx.FilePath, currentIndex, currentBlock.Line + 1, typeDef.name, newTag.TypeName);
                    }
                  }
                  else
                  {
                    throw new TypeNotDefinedxception(ctx.FilePath, currentIndex, currentBlock.Line + 1, newTag.TypeName);
                  }
                }
                // Parse as a section, if it has the <section/> tag
                else if (newTag.IsSection)
                {
                  // Check if there's actually a section definition. If not, make a new section
                  LoreSectionDefinition lsd = typeDef.GetSectionDefinition(newTitle) ?? new LoreSectionDefinition(newTitle, true);
                  newNode.Sections.Add(ParseSection(doc, ref currentIndex, hb, lsd, ctx));
                  continue;
                }
              }
              // Here if no tag in markdown
              else
              {
                if (typeDef.HasCollectionDefinition(newTitle))
                {
                  LoreCollectionDefinition lcd = typeDef.GetCollectionDefinition(newTitle);

                  if (lcd.IsCollectionOfCollections)
                    newNode.Collections.Add(ParseCollection(doc, ref currentIndex, hb, lcd, ctx));
                  else
                  {
                    LoreCollection newCol = ParseCollection(doc, ref currentIndex, hb, lcd, ctx);
                    newNode.Collections.Add(newCol);
                  }
                  continue;
                }
                else if (typeDef.HasSectionDefinition(newTitle))
                {
                  LoreSectionDefinition lsd = typeDef.GetSectionDefinition(newTitle);
                  newNode.Sections.Add(ParseSection(doc, ref currentIndex, hb, lsd, ctx));
                  continue;
                }
                else
                {
                  throw new DefinitionNotFoundException(ctx.FilePath, currentIndex, hb.Line + 1, newTitle);
                }
              }
              break;

            // freeform
            case ParagraphBlock pb:
              newNode.AddNarrativeText(ParseParagraphBlocks(doc, ref currentIndex, pb, typeDef, ctx));
              //continue;
              break;

            // Fields
            case ListBlock lb:
              newNode.AddNarrativeText(GetStringFromListBlock(lb));
              break;
            default:
              Trace.WriteLine($"ParseType encountered block of type {currentBlock.GetType()}");
              break;
          }

        }
        currentIndex++;
      }

      return newNode;
    }

    // This creates on-the-fly 'locally-defined' collection definitions. So having OwningDefinition might be useful
    private LoreCollectionDefinition MakeNestedDefinitions(string innerTag, LoreParsingContext ctx)
    {
      LoreCollectionDefinition lcd = new LoreCollectionDefinition();

      if (TagIsANestedCollection(innerTag))
      {
        lcd.SetContainedType(MakeNestedDefinitions(GetCollectionType(innerTag), ctx));
        (lcd.ContainedType as LoreCollectionDefinition).OwningDefinition = lcd;
      }
      else if (_settings.HasTypeDefinition(GetCollectionType(innerTag)))
      {
        lcd.SetContainedType(_settings.GetTypeDefinition(GetCollectionType(innerTag)));
      }
      else
      {
        throw new CollectionWithUnknownTypeException(ctx.FilePath, -1, -1, innerTag);
      }

      lcd.IsLocallyDefined = true;
      return lcd;
    }



    /// <summary>
    /// Creates a LoreCollection of LoreElements. Starts from a heading block.
    /// If heading had tag (example &lt;collection type="name" /&gt;), a LoreCollectionDefinition object with ContainedType set
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
    private LoreCollection ParseCollection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreCollectionDefinition colType, LoreParsingContext ctx)
    {
      string title = ExtractTitle(heading);
      LoreTagInfo? lti = GetTagInfo(heading, ctx, currentIndex);

      LoreCollection newCollection = new LoreCollection(title, colType, ctx.FilePath, currentIndex, doc[currentIndex].Line + 1);

      currentIndex++;

      while (currentIndex < doc.Count)
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
                newCollection.Collections.Add(ParseCollection(doc, ref currentIndex, hb, colType.ContainedType as LoreCollectionDefinition, ctx));
              else
              {
                // If we're putting a type of node that is derived from the required type, it is allowed
                // but the node must be tagged with that derived type!
                // Otherwise it is assUmed to be the base type
                LoreTypeDefinition nodeType = colType.ContainedType as LoreTypeDefinition;
                LoreTagInfo? uTag = GetTagInfo(hb, ctx, currentIndex);

                // If the subheading was tagged as a particular type
                if (uTag.HasValue)
                {
                  /* This case:
                    * # Departments of the FBSA <collection type="Organization" />
                    * 
                    * ## Department of Containment (DOC) <node ID="FBSA.DOC />
                    * 
                    * The above IS VALID. The tag on the level 2 heading is tricky because it has no type attribute, but we can infer that it is
                    * of type Organization because the owning collection contains nodes of type Organization
                    */

                  LoreTagInfo tag = uTag.Value;
                  if (!tag.HasType)
                  {
                    nodeType = colType.ContainedType as LoreTypeDefinition;
                  }
                  else
                  {
                    nodeType = _settings.GetTypeDefinition(tag.TypeName);
                    if (nodeType == null)
                      throw new UnknownTypeInCollectionException(ctx.FilePath, currentIndex, hb.Line + 1, tag.TypeName, colType.ContainedType);
                  }

                  if (!nodeType.IsATypeOf(colType.ContainedType as LoreTypeDefinition))
                    throw new InvalidTypeInCollectionException(ctx.FilePath, currentIndex, hb.Line + 1, tag.TypeName, colType.ContainedType);
                }
                // If the subheading was not tagged, we will parse it as the type contained as defined by the collection
                else if (colType.ContainedType != null)
                  nodeType = colType.ContainedType as LoreTypeDefinition;
                else
                  nodeType = _settings.GetTypeDefinition(colType.entryTypeName);

                LoreNode newNodeToAdd = ParseNode(doc, ref currentIndex, hb, nodeType, ctx);
                newNodeToAdd.SetTag(uTag);
                newCollection.Nodes.Add(newNodeToAdd);
              }
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
    private LoreSection ParseSection(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreSectionDefinition secDef, LoreParsingContext ctx)
    {
      LoreSection newSection = new LoreSection(ExtractTitle(heading), secDef, ctx.FilePath, currentIndex, doc[currentIndex].Line + 1);

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        Block currentBlock = doc[currentIndex];

        switch (currentBlock)
        {
          case HeadingBlock hb:

            LoreTagInfo? uTag = GetTagInfo(hb, ctx, currentIndex);

            // If the heading we're iterating over is subheader to the section's heading,
            // it *must* be a subsection (by rules: sections can only contain sections as subheadings)
            if (heading.Level < hb.Level)
            {
              // As long as the heading title for the subsection matches a section definition in the current section's definition,
              // we don't need the heading to be tagged. If the title does have a match, great. If not, check for a <section> tag.
              // If no <section> tag, that's an error.
              string headingTitle = ExtractTitle(hb);

              LoreSectionDefinition subSecDef = secDef.sections?.FirstOrDefault(sd => sd.name.Equals(headingTitle));

              // 'if we did not find a match for the section definition...'
              if (subSecDef == null)
              {
                if (!uTag.HasValue)
                  throw new UnexpectedSectionNameException(ctx.FilePath, currentIndex, hb.Line, newSection.Name, headingTitle);
                else if (!uTag.Value.IsSection)
                  throw new UnexpectedTagTypeException(ctx.FilePath, currentIndex, hb.Line, uTag.Value.tagname);
                else if (uTag.Value.IsSection)
                  subSecDef = new LoreSectionDefinition(headingTitle, true);
              }

              LoreSection newSubSection = ParseSection(doc, ref currentIndex, hb, subSecDef, ctx);
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
          case QuoteBlock qb:
            newSection.AddNarrativeText(GetStringFromQuoteBlock(qb) + "\r\n");
            break;

          // ListBlock can be a list of attributes ONLY if the section definition has fields defined.
          // Otherwise, the ListBlock is treated as text;
          case ListBlock lb:
            if (secDef.HasFields)
              newSection.Attributes = ParseListAttributes(doc, currentIndex, lb, secDef.fields, ctx);
            else
              newSection.AddNarrativeText(GetStringFromListBlock(lb));
            break;
          default:
            Trace.WriteLine($"ParseSection is not set up to parse a block of type {currentBlock.GetType()}");
            break;

        }
        newSection.Blocks.Add(currentBlock);

        currentIndex++;
      }

      return newSection;
    }


    private string ParseParagraphBlocks(MarkdownDocument doc, ref int currentIndex, ParagraphBlock paragraphBlock, LoreTypeDefinition typeDef, LoreParsingContext ctx)
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

    private List<LoreAttribute> ParseListAttributes(MarkdownDocument doc, int currentIndex, ListBlock listBlock, List<LoreFieldDefinition> attributeDefinitions, LoreParsingContext ctx)
    {
      string fieldValue = string.Empty;
      List<LoreAttribute> attributeList = new List<LoreAttribute>();

      //listBlock = (ListBlock)doc[currentIndex];

      foreach (var item in listBlock)
      {
        if (item is not ListItemBlock) { break; }

        var contentItem = (item as ListItemBlock)[0] as ParagraphBlock;
        var inline = contentItem.Inline.FirstChild;

        string parsedFieldName = string.Empty;
        List<string> readFieldValues = new List<string>();
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
          readFieldValues.Add(fieldAndVal[1].Trim());
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

        LoreAttribute newAttribute = new LoreAttribute(parsedFieldName, newDef, ctx.FilePath, contentItem.Parent.IndexOf(contentItem), contentItem.Line + 1);


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


          // in case there's an HTML comment block in some fields list.
          // Example:
          /*
           * 
          - **Appearance:**
          <!-- - Color Palette:
              - Signal Vesicle: Translucent gold 
              - Pigment Vesicle: Pale, lighter green-->
            - Degree of human presentation: near-full
            - **Height:** 5'4"
          */
          ListBlock lb = null;
          int ind = 1;
          while (lb == null && ind < (item as ListItemBlock).Count) { lb = (item as ListItemBlock)[ind] as ListBlock; ind++; }

          // if it has nested fields
          if (newDef.HasFields)
          {
            parsedNestedAttributes.Add(ParseListAttributes(doc, currentIndex, lb, newDef.fields, ctx));
          }

          // if it allows multiple values
          else if (newDef.cardinality == EFieldCardinality.MultiValue)
          {
            newAttribute.Values = new List<LoreAttributeValue>();
            foreach (ListItemBlock block in lb)
            {
              readFieldValues.Add(GetStringFromParagraphBlock(block[0] as ParagraphBlock).Trim());
            }
          }
          else if (newDef.structure == EFieldInputStructure.Textual)
          {
            readFieldValues.Add(GetStringFromListBlock(lb));
          }

          // if no nested fields and not multivalue, we sure better hope this nested ListBlock is just a single value...
          else
          {
            if (lb.Count > 1)
              throw new NestedBulletsOnSingleValueChildlessAttributeException(ctx.FilePath, currentIndex, lb.Line + 1, newAttribute.DefinitionAs<LoreFieldDefinition>());

            ListItemBlock lib = lb[0] as ListItemBlock;

            readFieldValues.Add(GetStringFromParagraphBlock(lib[0] as ParagraphBlock));
          }
        }

        newAttribute.Append(readFieldValues);
        newAttribute.Attributes.Add(parsedNestedAttributes);
        attributeList.Add(newAttribute);

      }


      return attributeList;
    }


    private static string GetStringFromParagraphBlock(ParagraphBlock pgBlock) => GetStringFromContainerInline(pgBlock.Inline);

    private static string GetStringFromQuoteBlock(QuoteBlock qBlock) => string.Join("\r\n", qBlock.Select(p => GetStringFromParagraphBlock(p as ParagraphBlock)));

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
      foreach (Block block in lib)
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
