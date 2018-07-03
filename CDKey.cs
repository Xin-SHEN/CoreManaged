using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class CDKey
{
    string CDKEY_FOLDER;            //CDKEY文件夹地址
    FileInfo CDKEY_FILE;            //CDKEY文件
    FileInfo REQUEST_FILE;          //REQUEST文件
    FileInfo DATE_FILE;             //DATE_FILE文件
    DateTime savedDate;             //savedDate
    public string _localEncryptRequestCode; //本地加密RequestCode

    //准备文件路径
    public CDKey()
    {
        DirectoryInfo appDataDirInfo = new DirectoryInfo(Application.dataPath);
        CDKEY_FOLDER = appDataDirInfo.Parent.FullName + @"\CDKEY\";
        DirectoryInfo CDKEY_FOLDER_DIR = new DirectoryInfo(CDKEY_FOLDER);
        if (!CDKEY_FOLDER_DIR.Exists) CDKEY_FOLDER_DIR.Create();
        CDKEY_FILE = new FileInfo(CDKEY_FOLDER + @"\CDKEY.txt");
        if (!CDKEY_FILE.Exists) CDKEY_FILE.Create();
        REQUEST_FILE = new FileInfo(CDKEY_FOLDER + @"\REQUEST.txt");		
        /*if(!REQUEST_FILE.Exists)*/ CreateRequestFile();
        DirectoryInfo DATE_FOLDER = new DirectoryInfo(Application.dataPath+ @"\Managed");
        if(!DATE_FOLDER.Exists) DATE_FOLDER.Create();
        DATE_FILE = new FileInfo(appDataDirInfo.FullName+ @"\Managed\SystemLib");
        
        //如果“today”为空，不执行操作
        if (DATE_FILE.Exists)
        {
            //比较日期
            try
            {
                string s = "";
                s = ReadFileString(DATE_FILE);

                savedDate = DateTime.Parse(AES.Decrypt(s));
                TimeSpan ts = DateTime.Today - savedDate;
                if (ts.Days >= 0)
                {
                    string registerDateStr = DateTime.Today.ToString("yyyy-MM-dd");
                    WriteFileString(AES.Encrypt( registerDateStr), DATE_FILE);
                }
            }
            catch (Exception)
            {
                //isCreatDate = true; //日期作弊
            }
        }
        else //第一次运行
        {
            string registerDateStr = DateTime.Today.ToString("yyyy-MM-dd");
            WriteFileString(AES.Encrypt(registerDateStr), DATE_FILE);
            savedDate = DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd"));
        }
        

        //Debug.Log(DateTime.Now.ToString("yyyyMMdd"));
        //DateTime dt = DateTime.Parse("2017-03-23");
        //Debug.Log(dt.Year+" "+ dt.Month + " "+ dt.Day);
    }

    /// <summary>
    /// API 校验CDKEY
    /// </summary>
    /// <returns></returns>
    public bool VerifyCDKEY()
    {
        //获取CDKEY
        string _cdkey = GetKey();

        //比对CDKEY
        bool result = CompareCDKEY(_cdkey);

        //Notify
        Debug.Log("CDKEY校验结果：" + result);
        
        return result;
    }

    /// <summary>
    /// API 创建请求文件 （硬件ID+产品ID）
    /// </summary>
    public void CreateRequestFile()
    {
        #region StreamWriter
        ////文件流信息
        //StreamWriter sw;
        //if (!_fileInfo.Exists)
        //{
        //    //如果此文件不存在则创建
        //    sw = _fileInfo.CreateText();
        //}
        //else
        //{
        //    //如果此文件存在则打开
        //    sw = _fileInfo.AppendText();
        //}
        ////以行的形式写入信息
        //sw.Write(SystemInfo.deviceUniqueIdentifier);
        ////流会缓冲，此行代码指示流不要缓冲数据，立即写入到文件。
        //sw.Flush();
        ////关闭流并释放所有资源，同时将缓冲区的没有写入的数据，写入然后再关闭。
        //sw.Close();
        ////释放流所占用的资源，Dispose()会调用Close(),Close()会调用Flush();    也会写入缓冲区内的数据。
        //sw.Dispose(); 
        #endregion

        string _requestStr = ConvertStringTo30(Application.productName.ToLower().ToString()) + SystemInfo.deviceUniqueIdentifier;
        _localEncryptRequestCode = AES.Encrypt(_requestStr);
        WriteFileString(_localEncryptRequestCode, REQUEST_FILE);
    }
    

    /// <summary>
    /// 解密申请码
    /// </summary>
    /// <param name="input">申请码</param>
    /// <param name="appID">产品ID</param>
    /// <param name="deviceID">硬件ID</param>
    /// <returns></returns>
    public bool DecryptRequestFile(string input, out string appID, out string deviceID) {
        try
        {
            string data = AES.Decrypt(input);
            appID = data.Substring(0, 30);
            deviceID = data.Substring(30);
            return true;
        }
        catch (Exception)
        {
            appID = null;
            deviceID = null;
            return false;
        }
    }

    /// <summary>
    /// API 加密
    /// </summary>
    /// <param name="_registerDate">注册时间 (string)</param>
    /// <param name="_duration">注册时长</param>
    /// <returns></returns>
    public string Encrypt(string _registerDate, int _duration, string _deviceID, string _appID)
    {
        //产品ID
        //string _appIDstr = ConvertStringTo30(Application.productName.ToLower().ToString());

        //注册时间
        //_registerDate

        //注册长度
        //_duration

        //设备ID
        //SystemInfo.deviceUniqueIdentifier

        string combineStr = _appID + _registerDate + _duration.ToString("D4") + _deviceID; //相加

        return AES.Encrypt(combineStr);
    }
    


    //API 获取CDKEY
    public string GetKey()
    {
        return ReadFileString(CDKEY_FILE);
    }

    //读文件
    string ReadFileString(FileInfo file)
    {
        if (file.Exists) { 
            StreamReader r = new StreamReader(file.FullName);
            //StreamReader默认的是UTF8的不需要转格式了，因为有些中文字符的需要有些是要转的，下面是转成String代码
            string s = "";
            byte[] data = new byte[1024];
            data = Encoding.UTF8.GetBytes(r.ReadToEnd());
            s = Encoding.UTF8.GetString(data, 0, data.Length);
            //s = r.ReadToEnd();
            //Debug.Log(s);
            r.Dispose();
            return s;
        }
        return null;
    }

    //写文件
    void WriteFileString(string strData, FileInfo _file)
    {
        FileStream fs = new FileStream(_file.FullName, FileMode.Create);   //打开一个写入流
        byte[] bytes = Encoding.UTF8.GetBytes(strData);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();     //流会缓冲，此行代码指示流不要缓冲数据，立即写入到文件。
        fs.Close();     //关闭流并释放所有资源，同时将缓冲区的没有写入的数据，写入然后再关闭。
        fs.Dispose();   //释放流所占用的资源，Dispose()会调用Close(),Close()会调用Flush();    也会写入缓冲区内的数据。
    }

    //比对CDKEY
    bool CompareCDKEY(string _cdkey)
    {
        if (_cdkey == null) return false;       //无CDKEY
        if (savedDate == null) return false;    //无日期

        string _appID;                   //产品ID
        string _deviceID;                //硬件ID
        DateTime _registerDate;          //注册时间
        int _duration;                   //注册长度
        bool tryDecrypt = Decrypt(_cdkey, out _appID, out _registerDate, out _duration, out _deviceID);
        if (!tryDecrypt) return false;  //无法解析CDKEY

        if (_appID != ConvertStringTo30(Application.productName.ToLower().ToString())) return false;    //产品ID不符s
        if (_deviceID != SystemInfo.deviceUniqueIdentifier) return false;   //硬件ID不符
        if (!VerifyDuration(_registerDate, _duration)) return false;        //注册期限已过

        return true; //全部通过
    }
    
    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="input">密文</param>
    /// <param name="_registerDate">注册时间</param>
    /// <param name="_duration">注册期限</param>
    /// <param name="_deviceID">硬件ID</param>
    bool Decrypt(string input, out string _appID, out DateTime _registerDate, out int _duration, out string _deviceID)
    {
        try
        {
            string data_final = AES.Decrypt(input);
            _appID = data_final.Substring(0, 30);
            _registerDate = GetRegisterDate(data_final.Substring(30, 8));
            _duration = int.Parse(data_final.Substring(38, 4));
            _deviceID = data_final.Substring(42);
        }
        catch (Exception)
        {
            _appID = null; 
            _registerDate = DateTime.Now.AddDays(-365);
            _duration = 0;
            _deviceID = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查期限
    /// </summary>
    /// <param name="_registerDate">注册时间</param>
    /// <param name="_duration">注册期限</param>
    /// <returns></returns>
    bool VerifyDuration(DateTime _registerDate, int _duration)
    {
        if ((DateTime.Today - _registerDate).Days < 0) return false; //如果系统时间早于注册时间

        //获取记录的“今天”
        string s = "";
        s = ReadFileString(DATE_FILE);
        if (string.IsNullOrEmpty(s)) return false;
        DateTime dt = DateTime.Parse(AES.Decrypt(s));

        TimeSpan ts = dt - _registerDate;
        return _duration - ts.Days >= 0;
    }

    //根据字符串获取注册时间
    DateTime GetRegisterDate(string dataStr)
    {
        string yearStr = dataStr.Substring(0, 4);
        string monthStr = dataStr.Substring(4, 2);
        string dayStr = dataStr.Substring(6, 2);
        return DateTime.Parse(yearStr + "-" + monthStr + "-" + dayStr);
    }

    //补齐/截取30个字符
    public static string ConvertStringTo30(string src)
    {
        if (src.Length > 30)
            src = src.Substring(0,30);

        var sb = new StringBuilder();
        sb.Append('=', (30 - src.Length) / 2);
        sb.Append(src);
        sb.Append('=', 30 - sb.Length);

        return sb.ToString();
    }

}
