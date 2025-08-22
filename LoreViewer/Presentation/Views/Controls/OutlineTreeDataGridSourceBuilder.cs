using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.SettingsVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.Views.Controls
{
  internal static class OutlineTreeDataGridSourceBuilder
  {
    internal static HierarchicalTreeDataGridSource<OutlineItemViewModel> BuildShallowTreeSource(
      ObservableCollection<OutlineItemViewModel> roots)
    {
      return new HierarchicalTreeDataGridSource<OutlineItemViewModel>(roots)
      {
        Columns = {
          new HierarchicalExpanderColumn<OutlineItemViewModel>(
            new TemplateColumn<OutlineItemViewModel>(
              header: "Name",
              width: new GridLength(1, GridUnitType.Star),
              cellTemplate: new FuncDataTemplate<OutlineItemViewModel>((node, _) =>
              {
                if (node == null) return new Panel();

                Label retBox = new Label
                {
                  [!Label.ContentProperty] = new Binding("DisplayName")
                };

                return retBox;

              })
            ),
            childSelector: x => x.Children
          ),
          new TemplateColumn<OutlineItemViewModel>(
            header: "Val",
            cellTemplate: new FuncDataTemplate<OutlineItemViewModel>((node, _) =>
            {
              if(node == null) return new Panel();
              string imgPath = "";
              switch (node.ValidationState)
              {
                case Core.Validation.EValidationState.Warning:
                  imgPath = "avares://LoreViewer/Resources/warning.png";
                  break;
                case Core.Validation.EValidationState.Failed:
                case Core.Validation.EValidationState.ChildFailed:
                  imgPath = "avares://LoreViewer/Resources/delete.png";
                  break;
                case Core.Validation.EValidationState.Passed:
                  imgPath = "avares://LoreViewer/Resources/valid.png";
                  break;
                case Core.Validation.EValidationState.ChildWarning:
                  imgPath = "avares://LoreViewer/Resources/childWarning.png";
                  break;
                default: return new Panel();
              }

              return new Image
              {
                Source = new Bitmap(AssetLoader.Open(new System.Uri(imgPath))),
                Width = 24
              };
            })
          ),
        }
      };
    }

    //internal static HierarchicalTreeDataGridSource<OutlineItemViewModel> BuildTreeSource(
    //  ObservableCollection<OutlineItemViewModel> roots)
    //{ }
  }
}
