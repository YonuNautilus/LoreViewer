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
    
    public string SectionContent { get => (entity as LoreSection).Summary.Trim(); }

    public ObservableCollection<LoreAttributeViewModel> Attributes { get; set; }

    public ObservableCollection<LoreSectionViewModel> Sections { get; set; }

    public bool HasSections { get => (entity as ISectionContainer).HasSections; }
    public bool HasAttributes { get => (entity as IAttributeContainer).HasAttributes; }
    public bool HasContent { get => (entity as LoreSection).HasNarrativeText; }

    public override LoreEntityViewModel GetChildVM(LoreEntity eToGet)
    {
      if (entity == eToGet) return this;
     
      if(Attributes != null && Attributes.Count > 0)
        foreach(LoreAttributeViewModel a in Attributes)
        {
          LoreEntityViewModel evm = a.GetChildVM(eToGet);
          if (evm != null) return evm;
        }

      if(Sections != null && Sections.Count > 0)
        foreach(LoreSectionViewModel s in Sections)
        {
          LoreEntityViewModel evm = s.GetChildVM(eToGet);
          if (evm != null) return evm;
        }

      return null;
    }

    public LoreSectionViewModel(LoreSection sec) : base(sec)
    {
      if ((sec as IAttributeContainer).HasAttributes) Attributes = new(sec.Attributes.Select(a => new LoreAttributeViewModel(a)).ToList());
      if ((sec as ISectionContainer).HasSections) Sections = new(sec.Sections.Select(s => new LoreSectionViewModel(s)).ToList());
    }
  }
}
