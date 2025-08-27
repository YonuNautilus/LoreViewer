using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using System;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Services
{
  internal static class EntityTreeDataGridSourceBuilder
  {
    internal static HierarchicalTreeDataGridSource<LoreEntityViewModel> BuildShallowTreeSource(
      ObservableCollection<LoreEntityViewModel> roots, ValidationStore valStore)
    {
      return new HierarchicalTreeDataGridSource<LoreEntityViewModel>(roots)
      {
        Columns = {
          new HierarchicalExpanderColumn<LoreEntityViewModel>(
            new TemplateColumn<LoreEntityViewModel>(
              header: "Name",
              width: new GridLength(1, GridUnitType.Star),
              cellTemplate: new FuncDataTemplate<LoreEntityViewModel>((node, _) =>
              {
                if (node == null) return new Panel();

                Label retBox = new Label
                {
                  [!ContentControl.ContentProperty] = new Binding("DisplayName")
                };

                return retBox;

              })
            ),
            childSelector: x => x.ShallowChildren
          ),
          new TemplateColumn<LoreEntityViewModel>(
            header: "Val",
            cellTemplate: new FuncDataTemplate<LoreEntityViewModel>((node, _) =>
            {
              if(node == null) return new Panel();
              string imgPath = "";
              switch (valStore.Result.GetValidationStateForElement(node.entity))
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
                Source = new Bitmap(AssetLoader.Open(new Uri(imgPath))),
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
