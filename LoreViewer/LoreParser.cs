using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using LoreViewer.LoreNodes;
using Markdig;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LoreViewer
{
  internal class LoreParser
  {
    const string HeaderWithoutTagRegex = ".+(?={)";
    const string TypeTagRegex = "(?<={).+(?=})";
    const string NestedTypeTagRegex = "(?<=:).+";
    const string LoreSettingsFileName = "Lore_Settings.yaml";

    private LoreSettings _settings;
    private string _folderPath;

    private List<LoreNodeCollection> _collections = new List<LoreNodeCollection>();
    private List<LoreNode> _nodes = new List<LoreNode>();

    /// <summary>
    /// Key is the file name (path relative to the lore folder), and value is the index of the block that is 'orphaned'
    /// (i.e. not tagged when it needs to be)
    /// </summary>
    public Dictionary<string, int> OrphanedBlocks { get; set; } = new Dictionary<string, int>();

    public LoreParser() { }

    public LoreParser(LoreSettings settings)
    {
      _settings = settings;
    }

    public void BeginParsingFromFolder(string FolderPath)
    {
      _folderPath = FolderPath;

      string fullSettingsPath = Path.Combine(FolderPath, LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        throw new Exception($"Did not find file {fullSettingsPath}");

      //_settings.ParseSettingsFromFile(fullSettingsPath);

      var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

      _settings = deserializer.Deserialize<LoreSettings>(File.ReadAllText(fullSettingsPath));


      string[] files = Directory.GetFiles(FolderPath, "*.md");

      foreach (string filePath in files)
      {
        try
        {
          ParseFile(filePath);
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

      return title;
    }

    private static bool BlockIsACollection(HeadingBlock block) => ExtractTag(block).Contains("collection:");

    private static bool BlockIsANestedCollection(HeadingBlock block) => BlockIsACollection(block) && GetCollectionType(ExtractTag(block)).Contains("collection:");
    public void ParseFile(string filePath)
    {
      try
      {
        string fileContent = File.ReadAllText(filePath);
        MarkdownDocument document = Markdown.Parse(fileContent);

        // Once where, loop through the document, looking for headers

        int curIndex = 0;
        while(curIndex < document.Count)
        {
          // Start with the top level, get its type
          if (document[curIndex] is HeadingBlock)
          {
            HeadingBlock block = (HeadingBlock)document[curIndex];
            string tag = ExtractTag(block);
            string title = ExtractTitle(block);

            if (!string.IsNullOrEmpty(tag))
            {
              if (BlockIsACollection(block))
              {
                if (BlockIsANestedCollection(block))
                {
                  _collections.Add(ParseCollection(document, ref curIndex, block, tag));
                }
                else
                  _collections.Add(ParseCollection(document, ref curIndex, block, _settings.GetTypeDefinition(GetCollectionType(tag))));
              }
              else
              {
                LoreNode newNode = ParseType(document, ref curIndex, block, _settings.GetTypeDefinition(tag));

                newNode.SourcePath = filePath;
                newNode.BlockIndex = curIndex;

                _nodes.Add(newNode);
              }
            }
            else
            {
              OrphanedBlocks.Add(Path.GetRelativePath(_folderPath, filePath), curIndex);
            }
          }
          else
          {
            OrphanedBlocks.Add(Path.GetRelativePath(_folderPath, filePath), curIndex);
          }

          curIndex++;
        }
      }
      catch (Exception ex)
      {

      }
    }

    private LoreNode ParseType(MarkdownDocument doc, ref int currentIndex, HeadingBlock heading, LoreTypeDefinition typeDef)
    {
      string title = ExtractTitle(heading);
      LoreNode newNode = new LoreNode(_settings.GetTypeName(typeDef), title);

      currentIndex++;

      while (currentIndex < doc.Count)
      {
        var currentBlock = doc[currentIndex];
        switch (currentBlock)
        {
          // Sections
          case HeadingBlock hb:

            break;

          // freeform
          case ParagraphBlock pb:

            break;

          // Fields
          case ListBlock lb:
            Dictionary<string, LoreAttribute> attributes = ParseBulletPointFields(doc, ref currentIndex, lb, typeDef);
            newNode.Attributes = newNode.Attributes.Concat(attributes).ToDictionary();
            break;

          default:
            break;
        }

        //currentIndex++;
      }


      return newNode;
    }

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

    private Dictionary<string, LoreAttribute> ParseBulletPointFields(MarkdownDocument doc, ref int currentIndex, ListBlock listBlock, LoreTypeDefinition typeDef)
    {
      string fieldValue = string.Empty;
      int listItemIndex = 0;
      foreach (var item in listBlock)
      {
        if (item is not ListItemBlock) { break; }

        var contentItem = (item as ListItemBlock)[0] as ParagraphBlock;
        var inline = contentItem.Inline.FirstChild;

        LoreAttribute newAttribute = new LoreAttribute();

        string parsedFieldName = string.Empty;
        List<string> parsedFieldValues = new List<string>();

        /* FLAT PARSING
         * ex:
         * - **Date:** June 30, 2002
         * - Date: June 30, 2002
         */
        if ((item as ListItemBlock).Count == 1)
        {

          // this format:
          // - Date: June 30, 2002
          if (inline is LiteralInline)
          {
            if ((inline as LiteralInline).NextSibling == null)
            {
              var fieldAndVal = inline.ToString().Split(':');
              parsedFieldName = fieldAndVal[0];
              parsedFieldValues.Add(fieldAndVal[1]);
            }
          }

          // this format:
          // - **Date:** June 30, 2002
          else if (inline is EmphasisInline)
          {
            //        variable inline is "**",          FirsChild is "Date:"
            parsedFieldName = (inline as EmphasisInline).FirstChild.ToString();
            // Next sibling of "Date:" is another "**"

            parsedFieldValues.Add((inline as EmphasisInline).NextSibling.ToString());
          }
          
        }

        
        /* NESTED PARSING
         * ex:
         * - Name:
         *   - Paula Mer Verdell
         *   - Green Bean (nickname)
         */
        else{
          parsedFieldName = ((item as ListItemBlock)[0] as ParagraphBlock).Inline.FirstChild.ToString();
        }
      }

      return new Dictionary<string, LoreAttribute>();
    }
  }
}
