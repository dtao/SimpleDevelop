using System.Data;

namespace SimpleDevelop {
    public partial class ReferencesDataSet
    {
        public DataView GetReferences()
        {
            var dataView = new DataView(References)
            {
                Sort = "Name Asc"
            };

            return dataView;
        }

        public DataView GetSelectedReferences()
        {
            var dataView = new DataView(References)
            {
                RowFilter = "Selected = True",
                Sort = "Name Asc"
            };

            return dataView;
        }
    }
}
