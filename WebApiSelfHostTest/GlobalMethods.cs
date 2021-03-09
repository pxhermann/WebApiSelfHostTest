using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebApiSelfHostTest
{
    class GM
    {
        static GM()
        {
            if (desKey != null && desKey.Length > 16) { desKey[8] = 5; desKey[16] = 3; }
            if (desIV != null && desIV.Length > 7) { desIV[1] = 9; desIV[7] = 2; }

            // logFileName = Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName, ".log");
            logFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName), "WebApiSelfHostTest.log");
        }

        public static string FormatException(Exception ex)
        {
            if (ex == null)
                return "";

            string retVal = ex.Message; // retVal = string.Format("{0}: {1}", ex.Source, ex.Message);
            for (Exception subEx = ex.InnerException; subEx != null; subEx = subEx.InnerException)
                retVal += string.Format("{0}{1}", Environment.NewLine, subEx.Message);

            retVal += string.Format("{0}{0}{1}", Environment.NewLine, ex.ToString());

            return retVal;
        }

        #region DES crypting
        private static byte[] desKey = Encoding.ASCII.GetBytes("123456789012345678901234");
        private static byte[] desIV = Encoding.ASCII.GetBytes("12345678");
        public static string DESEncrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, TripleDES.Create().CreateEncryptor(desKey, desIV), CryptoStreamMode.Write))
                {
                    byte[] buf = Encoding.Unicode.GetBytes(data);
                    cs.Write(buf, 0, buf.Length);
                } // !!! call ms.ToArray() till after closing cs, otherwise cs could be unflushed and ms contained uncomplete data
                return Convert.ToBase64String(ms.ToArray());  // do not use Convert.ToString because ms buffer can contain unreadable characters
            }
        }
        public static string DESDecrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, TripleDES.Create().CreateDecryptor(desKey, desIV), CryptoStreamMode.Write))
                {
                    byte[] pwdBuf = Convert.FromBase64String(data);
                    cs.Write(pwdBuf, 0, pwdBuf.Length);
                } // !!! call ms.ToArray() till after closing cs, otherwise cs could be unflushed and ms contained uncomplete data
                return Encoding.Unicode.GetString(ms.ToArray());
            }
        }
#endregion

#region DB value casting
        public static string ConvertToString(object value)
        {
            return (value == null || value is System.DBNull) ? "" : Convert.ToString(value);
        }
        public static DateTime ConvertToDateTime(object value)
        {
            return (value == null || value is System.DBNull) ? DateTime.MinValue : Convert.ToDateTime(value);
        }
        public static decimal ConvertToDecimal(object value)
        {
            return (value == null || value is System.DBNull) ? 0m : Convert.ToDecimal(value);
        }
        public static int ConvertToInt(object value)
        {
            return (value == null || value is System.DBNull) ? 0 : Convert.ToInt32(value);
        }
        public static long ConvertToLong(object value)
        {
            return (value == null || value is System.DBNull) ? 0 : Convert.ToInt64(value);
        }
        public static bool ConvertToBool(object value)
        {
            return (value == null || value is System.DBNull) ? false : Convert.ToBoolean(value);
        }
        public static Guid ConvertToGuid(object value)
        {
            if (value == null || value is System.DBNull)
                return Guid.Empty;
            string s = value.ToString();
            if (string.IsNullOrEmpty(s))
                return Guid.Empty;
            else
                return new Guid(s);
        }
#endregion

#region string extension
        public static string LStr(int length, string src, char paddingChar = ' ')
        {
            if (string.IsNullOrEmpty(src))
                return new string(paddingChar, length);

            if (src.Length > length)
                return src.Substring(0, length);

            return src.PadRight(length, paddingChar);
        }
        public static string RStr(int length, string src, char paddingChar = ' ')
        {
            if (string.IsNullOrEmpty(src))
                return new string(paddingChar, length);

            if (src.Length > length)
                return src.Substring(0, length);

            return src.PadLeft(length, paddingChar);
        }
#endregion
#region logging
        private static string logFileName = "";
        public static void WriteLog(Exception ex, string msg = "")
        {
            if ( !String.IsNullOrEmpty(msg) )
                msg += " --> ";
            WriteLog(LogType.Error, msg+FormatException(ex));
        }
        public static void WriteLog(LogType logType, string msg)
        {
            try
            {
            // try to log to database 
/*                if (DB.ConnData == null || DB.ConnData.IsValid())
                    try
                    {
                        using (SqlConnection conn = DB.CreateConnection())
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO EventLog (log_created,log_user,log_app,prog_id,log_type,log_descr)" +
                                              " VALUES (GETDATE(), @user, @app, @prog, @type, @descr)";
                            cmd.Parameters.AddWithValue("@user", userName);
                            cmd.Parameters.AddWithValue("@app", app);
                            cmd.Parameters.AddWithValue("@prog", progID);
                            cmd.Parameters.AddWithValue("@type", logType);
                            cmd.Parameters.AddWithValue("@descr", msg.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine));

                            conn.Open();
                            cmd.ExecuteScalar();
                        }
                        return;
                    }
                    catch { }
*/                
            // if logging to database not sucessfull, log to file
                FileInfo fLog = new FileInfo(logFileName);
                if (fLog != null && fLog.Exists && fLog.Length > 1000000)
                    fLog.MoveTo(Path.ChangeExtension(logFileName, ".bak"));

                string strType = "";
                switch ( logType )
				{
                case LogType.Info:      strType = "INFO"; break;
                case LogType.Warning:   strType = "WARNING"; break;
                default:                strType = "CHYBA"; break;
                }

                string s = string.Format("{1}  {2}({3}) {4}{0}", 
                        Environment.NewLine,
                        DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss"),
                        strType,
                        Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName),
                        msg.Replace(Environment.NewLine, " --> "));
                File.AppendAllText(logFileName, s);

                // write to console if run in UI
                if (Environment.UserInteractive)
                    Console.WriteLine(string.Format("{0}  {1}", strType, msg));
            }
            catch (Exception e)
            {
#if DEBUG
                throw;
#endif
            }
        }
        #endregion
    }

    public enum LogType
    {
        Error = 0,
        Warning,
        Info
    }
}
