using System;
using System.Collections.Generic;
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

namespace LoreViewer
{
  internal class LoreParser
  {
    const string HeaderWithoutTagRegex = ".+(?={)";
    const string TypeTagRegex = "(?<={).+(?=})";
    const string NestedTypeTagRegex = "(?<=:).+";
    const string LoreSettingsFileName = "Lore_Settings.yaml";

    private LoreSettings _settings;
    public LoreParser() { }

    public LoreParser(LoreSettings settings)
    {
      _settings = settings;
    }

    public void BeginParsingFromFolder(string FolderPath)
    {
      string fullSettingsPath = Path.Combine(FolderPath, LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        throw new Exception($"Did not find file {fullSettingsPath}");

      _settings.ParseSettingsFromFile(fullSettingsPath);

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

    public void ParseFile(string filePath)
    {
      try
      {
        string fileContent = File.ReadAllText(filePath);
        MarkdownDocument document = Markdown.Parse(fileContent);

        // Start with the top level, get its type
        if (document[0] is HeadingBlock)
        {
          string headerText = (document[0] as HeadingBlock).Inline.FirstChild.ToString();
          string typeTag = Regex.Match(headerText, TypeTagRegex).Value;
          DoParse(document, 0, typeTag);
        }
      }
      catch (Exception ex)
      {

      }
    }

    private static string GetCollectionType(string fullTypeName) => Regex.Match(fullTypeName, NestedTypeTagRegex).Value;

    private void DoParse(MarkdownDocument doc, int startIndex, string type)
    {
      switch (doc[startIndex])
      {
        case HeadingBlock hb:
          int levelOfCurrentBlock = hb.Level;
          ParseFromHeading(doc, startIndex, levelOfCurrentBlock, type);
          break;
        default:
          return;
      }
    }

    private void ParseFromHeading(MarkdownDocument doc, int headingIndex, int levelOfCurrentBlock, string type)
    {
      HeadingBlock block = doc[headingIndex] as HeadingBlock;

      string headerTitle = Regex.Match(block.Inline.FirstChild.ToString(), HeaderWithoutTagRegex).Value;

      if (type.Contains("collection"))
      {
        //string nameForThisHeader = block.
        string TypeForThisHeader = GetCollectionType(type);
        // Travel down the document until the next heading. Info encountered before next heading will be notes/additional data.
        List<Block> additionalBlocks = new List<Block>();
        int nextHeaderIndex = headingIndex + 1;

        while (doc[nextHeaderIndex] is not HeadingBlock)
        {
          additionalBlocks.Add(doc[nextHeaderIndex]);
          nextHeaderIndex++;
        }

        HeadingBlock nextHeader = doc[nextHeaderIndex] as HeadingBlock;

        if (nextHeader.Level < levelOfCurrentBlock)
          throw new Exception($"Got a header {nextHeader.Level} before finding ones of higher level after {levelOfCurrentBlock}");

        ParseFromHeading(doc, nextHeaderIndex, nextHeader.Level, TypeForThisHeader);

        //LoreNode collectionNode = new LoreNode(type, headerTitle, _settings.Types["collection"]);
      }
      else
      {
        if (!_settings.HasTypeDefinition(type))
          throw new Exception($"No type {type} found in lore settings");

        LoreTypeDefinition typeDef = _settings.GetTypeDefinition(type);

        LoreNode newNode = ParseIntoNewLoreNode(doc, headingIndex, levelOfCurrentBlock, type, headerTitle, typeDef);
      }
    }

    private LoreNode ParseIntoNewLoreNode(MarkdownDocument doc, int headingIndex, int levelOfCurrentBlock, string type, string name, LoreTypeDefinition typeDef)
    {
      LoreNode newNode = new LoreNode(type, name);

      HeadingBlock currentBlock = doc[levelOfCurrentBlock] as HeadingBlock;

      foreach (LoreFieldDefinition fieldDef in typeDef.Fields)
      {
        string fieldValue = string.Empty;
        switch (fieldDef.Style)
        {
          case "bullet_point":
            fieldValue = ParseBulletPointField(doc, headingIndex, fieldDef.Name);
            break;

          case "body":

            break;

          default:
            return null;
        }
        newNode.Attributes.Add(fieldDef.Name, fieldValue);
      }

      return newNode;
    }

    private string ParseBulletPointField(MarkdownDocument doc, int startHeadingIndex, string fieldName)
    {
      int currentSearchIndex = startHeadingIndex;
      while (doc[currentSearchIndex] is not ListBlock) { currentSearchIndex++; }

      ListBlock currentList = doc[currentSearchIndex] as ListBlock;

      string fieldValue = string.Empty;
      int listItemIndex = 0;
      foreach (var item in currentList)
      {
        if (item is not ListItemBlock) { break; }
        var contentItem = (item as ListItemBlock)[0] as ParagraphBlock;

        EmphasisInline inline = contentItem.Inline.FirstChild as EmphasisInline;

        string foundFieldName = inline.FirstChild.ToString();

        if (foundFieldName.Contains(fieldName))
        {
          return inline.NextSibling.ToString();
        }
      }

      return string.Empty;
    }
  }
}
