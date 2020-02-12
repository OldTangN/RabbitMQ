using System;
using System.IO;
using System.Text;

namespace RabbitMQ
{
    static class  Log
    {

        private static string sLogFilePath = "";
        /// <summary>
        /// 日志文件绝对路劲
        /// </summary>
        /// <param name="spath"></param>
        public static void SetLogFilePath(string spath)
        {
            sLogFilePath = spath;
        }
        /// <summary>
        /// 向文件中写日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            StreamWriter sw = new StreamWriter(sLogFilePath,true);
            sw.Write(msg+"\r\n");
            sw.Close();
        }

        // add begin [2015-12-18 Renyl]
        private static object log_locker_ = new object();
        private static string str_log_date_;
        private static string str_log_file_;
        private static int n_log_num_ = 1;
        public enum LOG_TYPE
        {
            LOG_TYPE_DEBUG,
            LOG_TYPE_INFO,
            LOG_TYPE_WARN,
            LOG_TYPE_ERROR,
        }
        public static bool PrintLog(LOG_TYPE type, params string[] list)
        {
            try
            {
                lock (log_locker_)
                {
                    if (type == LOG_TYPE.LOG_TYPE_DEBUG)
                    {
                        StringBuilder sb = new StringBuilder();
                        DateTime dt_now = DateTime.Now.ToLocalTime();
                        sb.Append("[DBUG] ");
                        sb.Append(string.Format("[{0}-{1}-{2} {3}:{4}:{5}] ", dt_now.Year, dt_now.Month, dt_now.Day, dt_now.Hour, dt_now.Minute, dt_now.Second));
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(list[i]))
                                sb.Append(string.Format("[{0}] ", list[i]));
                        }
                        sb.Append("\r\n");
                        string str_date = string.Format("{0}-{1}-{2}", dt_now.Year, dt_now.Month, dt_now.Day);
                        if (str_date != str_log_date_)
                        {
                            str_log_date_ = str_date;
                            n_log_num_ = 0;
                        }
                        str_log_file_ = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + string.Format("log_{0}_{1}.txt"
                            , str_log_date_
                            , n_log_num_);
                        FileInfo fi = new FileInfo(str_log_file_);
                        if (!fi.Exists)
                        {
                            using (StreamWriter sw = fi.CreateText())
                            {
                                sw.Write(sb.ToString());
                            }
                        }
                        else
                        {
                            using (StreamWriter sw = new StreamWriter(str_log_file_, true))
                            {
                                sw.Write(sb.ToString());
                            }
                        }
                        fi.Refresh();
                        if (fi.Length > 20 * 1024 * 1024)
                        {
                            n_log_num_++;
                            str_log_file_ = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + string.Format("log_{0}_{1}.txt"
                                , str_log_date_
                                , n_log_num_);
                        }
#endregion
#endif
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        DateTime dt_now = DateTime.Now.ToLocalTime();
                        //按照这个时间看看文件夹里有没有这个按这个年月的文件夹 没有就创建
                    
                        string strfilepath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                        string strfilepathdate = string.Format("{0}-{1}", dt_now.Year, dt_now.Month);
                        strfilepath +=@"MQ_log\"+ strfilepathdate;
                        if (Directory.Exists(strfilepath))
                        {
                            //有此目录
                        }
                        else
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(strfilepath);
                            //无此目录 创建目录
                            directoryInfo.Create();
                        }
                        switch (type)
                        {
                            case LOG_TYPE.LOG_TYPE_INFO:
                                sb.Append("[INFO] ");
                                break;
                            case LOG_TYPE.LOG_TYPE_WARN:
                                sb.Append("[WARN] ");
                                break;
                            case LOG_TYPE.LOG_TYPE_ERROR:
                                sb.Append("[ERRO] ");
                                break;
                            default:
                                return false;
                        }
                        sb.Append(string.Format("[{0}-{1}-{2} {3}:{4}:{5}] ", dt_now.Year, dt_now.Month, dt_now.Day, dt_now.Hour, dt_now.Minute, dt_now.Second));
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(list[i]))
                                sb.Append(string.Format("[{0}] ", list[i]));
                        }
                        sb.Append("\r\n");
                        string str_date = string.Format("{0}-{1}-{2}", dt_now.Year, dt_now.Month, dt_now.Day);
                        if (str_date != str_log_date_)
                        {
                            str_log_date_ = str_date;
                            n_log_num_ = 0;
                        }
                        str_log_file_ = strfilepath+"\\"+ string.Format("Fromlog_{0}_{1}.txt"
                            , str_log_date_
                            , n_log_num_);
                        FileInfo fi = new FileInfo(str_log_file_);
                        if (!fi.Exists)
                        {
                            using (StreamWriter sw = fi.CreateText())
                            {
                                sw.Write(sb.ToString());
                            }
                        }
                        else
                        {
                            using (StreamWriter sw = new StreamWriter(str_log_file_, true))
                            {
                                sw.Write(sb.ToString());
                            }
                        }
                        fi.Refresh();
                        if (fi.Length > 20 * 1024 * 1024)
                        {
                            n_log_num_++;
                            str_log_file_ = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + string.Format("{0}\\Fromlog_{1}_{2}.txt"
                                , strfilepathdate
                                , str_log_date_
                                , n_log_num_);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                //写失败
                return false;
            }
        }

        public delegate T SqlFunc<T>(string aSQL);
        public static T Sql<T>(string text, SqlFunc<T> func, string aSql, T def, bool is_print_sql = false)
        {
            T val = def;
            try
            {
                val = func(aSql);
            }
            catch (Exception ex)
            {
                Log.Error(text, "SQL:" + aSql + ", Exception:" + ex.ToString());
                return def;
            }
            if (is_print_sql)
                Log.Info(text, "SQL: " + aSql);
            return val;
        }

        public delegate T LogFunc<T>();
        public delegate void LogFunc();
        public static T PrintException<T>(LogFunc<T> func, T def)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return def;
            }
        }
        public static void PrintException(LogFunc func)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public static void Error(params string[] list)
        {
            Log.PrintLog(LOG_TYPE.LOG_TYPE_ERROR, list);
        }
        public static void Warn(params string[] list)
        {
            Log.PrintLog(LOG_TYPE.LOG_TYPE_WARN, list);
        }
        public static void Info(params string[] list)
        {
            Log.PrintLog(LOG_TYPE.LOG_TYPE_INFO, list);
        }
        public static void Debug(params string[] list)
        {
            Log.PrintLog(LOG_TYPE.LOG_TYPE_DEBUG, list);
        }
        // add end

    }
}
