using System.Collections.Generic;
using System.Linq;

namespace Extensions;

public class DesignList : List<FormDesign>
{
	public FormDesign this[string name] => this.FirstOrDefault(x => x.Name == name) ?? FormDesign.Modern;

	public new FormDesign this[int id] => this.FirstOrDefault(x => x.ID == id) ?? FormDesign.Modern;
}