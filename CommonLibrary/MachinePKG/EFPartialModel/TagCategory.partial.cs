using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.EFModel
{
    public partial class TagCategory
    {
        public TagCategory()
        {

        }

        public TagCategory(Guid CategoryID)
        {
            Id = CategoryID;
        }

        public int TagCount => Tags.Count;
    }
}
