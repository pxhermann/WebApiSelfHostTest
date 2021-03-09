using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebApiSelfHostTest
{
	public class AppSetting
	{
		public string UrlBase { get; set; }

		public string AuthorizationKeyBase64 { get; set; }
		[XmlIgnore]
		public string AuthorizationKey
		{
			get 
			{ 
				string authKey = "";
				if ( !string.IsNullOrEmpty(AuthorizationKeyBase64) )
					try { authKey = Encoding.ASCII.GetString(Convert.FromBase64String(AuthorizationKeyBase64)); }
					catch(Exception ex) { authKey = ""; }
				return authKey;
			}
			set { AuthorizationKeyBase64 = string.IsNullOrEmpty(value) ? "" : Convert.ToBase64String(Encoding.ASCII.GetBytes(value)); }
		}

		public AppSetting()
		{
			Reset();
		}
		private static string cfgFileName
		{
			//            get { return Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName, ".cfg"); }
			get { return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName), "WebApiSelfHostTest.cfg"); }// tato trida je pouzivana z vice projektu, ale vysledny soubor chci sdilet mezi vsemi projekty - proto pouzij jmeno natvrdo, aby se v kazde aplikaci nevytvarel s vlastnim jmenem
		}
		public void Reset()
		{
			UrlBase = "http://localhost:117";
			AuthorizationKey = "ABC123";
		}
		public static AppSetting Load()
		{
			AppSetting data = null;

			string cfgFile = cfgFileName;
			if (File.Exists(cfgFile))
//				try
				{
					using (StreamReader sm = new StreamReader(cfgFile))
					{
						XmlSerializer x = new XmlSerializer(typeof(AppSetting));
						data = (AppSetting)x.Deserialize(sm);
					}
				}
/*				catch (Exception ex)
				{
#if DEBUG
						if (Environment.UserInteractive)
							GM.ShowErrorMessageBox(null, "Nepodařilo se načíst nastavení aplikace!", ex);
#endif
				}
*/
			if (data == null)
				data = new AppSetting();
			return data;
		}
		public void Save()
		{
//			try
			{
				using (StreamWriter sw = new StreamWriter(cfgFileName))
				{
					XmlSerializer x = new XmlSerializer(typeof(AppSetting));
					x.Serialize(sw, this);
				}
			}
/*			catch (Exception ex)
			{
#if DEBUG
				if (Environment.UserInteractive)
					GM.ShowErrorMessageBox(null, "Nepodařilo se uložit nastavení aplikace!", ex);
#endif
			}
*/		}
	}
}