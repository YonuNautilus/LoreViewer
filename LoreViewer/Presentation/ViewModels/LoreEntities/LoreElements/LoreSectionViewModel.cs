using LoreViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class LoreSectionViewModel : LoreElementViewModel
  {
    public string SectionTitle { get => entity.Name; }
    
    public string SectionContent { get => (entity as LoreSection).Summary; }

    public ObservableCollection<LoreAttributeViewModel> Attributes { get; set; }

    public override LoreEntityViewModel GetChildVM(LoreEntity eToGet)
    {
      if (entity == eToGet) return this;
     
      if(Attributes != null && Attributes.Count > 0)
        foreach(LoreAttributeViewModel a in Attributes)
        {
          LoreEntityViewModel evm = a.GetChildVM(eToGet);
          if (evm != null) return evm;
        }

      return null;
    }

    public LoreSectionViewModel(LoreSection sec) : base(sec)
    {
      if (sec.Attributes != null && sec.Attributes.Any()) Attributes = new(sec.Attributes.Select(a => new LoreAttributeViewModel(a)).ToList());
    }
  }
}
