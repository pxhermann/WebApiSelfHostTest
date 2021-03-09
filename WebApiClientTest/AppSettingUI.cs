using System;
using System.IO;
using System.Xml.Serialization;

namespace WebApiClientTest
{
	public class AppSettingUI
	{
		public System.Windows.Forms.FormWindowState WindowState;
		public System.Drawing.Rectangle WindowPosition;
		public bool AlwaysOnTop;
		private static string cfgFileName
		{
			get { return Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName, ".cfg"); }
		}

		public AppSettingUI()
		{
			Reset();
		}
		public void Reset()
		{
			WindowState = System.Windows.Forms.FormWindowState.Normal;
			WindowPosition = System.Drawing.Rectangle.Empty;
			AlwaysOnTop = false;
		}
		public static AppSettingUI Load()
		{
			AppSettingUI data = null;

			string cfgFile = cfgFileName;
			if (File.Exists(cfgFile))
				try
				{
					using (StreamReader sm = new StreamReader(cfgFile))
					{
						XmlSerializer x = new XmlSerializer(typeof(AppSettingUI));
						data = (AppSettingUI)x.Deserialize(sm);
					}
				}
				catch (Exception ex)
				{
#if DEBUG
					GM.ShowErrorMessageBox(null, "Loading application setting failed!", ex);
#endif
				}

			if (data == null)
				data = new AppSettingUI();
			return data;
		}
		public void Save()
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(cfgFileName))
				{
					XmlSerializer x = new XmlSerializer(typeof(AppSettingUI));
					x.Serialize(sw, this);
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				GM.ShowErrorMessageBox(null, "Saving application setting failed!", ex);
#endif
			}
		}
	}
}
