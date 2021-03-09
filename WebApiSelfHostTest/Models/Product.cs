using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiSelfHostTest.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public Product()
        {
            Reset();
        }
        public Product(int id, string code, string name)
        {
            ID = id;
            Code = code;
            Name = name;
        }
        public Product(Product srcProd)
        {
            CopyFrom(srcProd);
        }
        public void Reset()
        {
            ID = 1;
            Code = "";
            Name = "";
        }

        public void CopyFrom(Product src)
        {
            Reset();
            if ( src == null )
                return;

            ID = src.ID;
            Code = src.Code;
            Name = src.Name;
        }
        public override string ToString()
        {
            return Code;
        }
    }
}
