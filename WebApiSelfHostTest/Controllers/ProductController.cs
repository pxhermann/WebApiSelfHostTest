using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Xml.Serialization;

namespace WebApiSelfHostTest.Models
{
    public class ProductController : ApiController
    {
        private List<Product> arrProd = null;

		private static string dataFileName { get; set; }
        static ProductController()
        {
			dataFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName), "Data.xml");
		}

        public ProductController()
        {
//           throw new Exception("Exception from constructor");
            loadData();
        }
        private void loadData()
        {
            if ( File.Exists(dataFileName) )
            {
                try
                {
			        using (StreamReader sm = new StreamReader(dataFileName))
			        {
				        XmlSerializer x = new XmlSerializer(typeof(List<Product>));
				        arrProd = (List<Product>)x.Deserialize(sm);
			        }
                }
                catch {}
            }
            if ( arrProd == null || arrProd.Count == 0 )
            {
                if ( !(arrProd is List<Product>) )
                    arrProd = new List<Product>();
                // some default values - just for testing
                arrProd.Add(new Product { ID = 1, Code = "Code1", Name = "Tomato Soup"});
                arrProd.Add(new Product { ID = 2, Code = "Code2", Name = "Yo-yo" });
                arrProd.Add(new Product { ID = 3, Code = "Code2", Name = "Hammer" });
            }
        }
        private void saveData()
        {
            if ( arrProd == null )
                return;

			using (StreamWriter sw = new StreamWriter(dataFileName))
			{
				XmlSerializer x = new XmlSerializer(typeof(List<Product>));
				x.Serialize(sw, arrProd);
			}
        }
        public IEnumerable<Product> GetAllProducts()
        {
            return arrProd;
        }
        public IHttpActionResult GetProduct(int id)
        {
            var prod = arrProd.FirstOrDefault(a => a.ID == id);
            if (prod == null)
//                throw new Exception(String.Format("Product ID = {0} not found", id));
                return NotFound();

            return Ok(prod);
        }
        /* create new object */
        public IHttpActionResult PostProduct([FromBody]Product newProd)
        {
            if ( newProd == null )
                throw new Exception("PostProduct called with null product");

            var prod = arrProd.FirstOrDefault(a => a.ID == newProd.ID);
            if (prod != null)
                throw new Exception(string.Format("Product with id = {0} already exists", newProd.ID));
//                return BadRequest(string.Format("Product with id = {0} already exists", newProd.ID));
            
            arrProd.Add(newProd);

            saveData();

            return Ok();
        }
        public IHttpActionResult PutProduct([FromBody]Product editProd)
        {
            if ( editProd == null )
                throw new Exception("PutProduct called with null product");

            var prod = arrProd.FirstOrDefault(a => a.ID == editProd.ID);
            if (prod == null)
                throw new Exception(string.Format("Product with id = {0} not found", editProd.ID));
            
            prod.CopyFrom(editProd);

            saveData();

            return Ok();
        }
        public IHttpActionResult DeleteProduct(int id)
        {
            var prod = arrProd.FirstOrDefault(a => a.ID == id);
            if (prod == null)
                throw new Exception(String.Format("Product ID = {0} not found", id));

            arrProd.Remove(prod);

            saveData();

            return Ok();
        }
    }
}
