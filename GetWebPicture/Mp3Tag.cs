using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;

namespace Mp3AlbumCoverUpdater
{       

   
    /// <summary>  
    /// Mp3文件信息类  
    /// </summary>  
    public class Mp3FileInfo 
    {
        private string _identify;     //TAG，三个字节   
        public string identify
        {
            get { return _identify; }
            set { _identify = value; }
        }
        private string _Title;        //歌曲名,30个字节
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        private string _Artist;       //歌手名,30个字节   
        public string Artist
        {
            get { return _Artist; }
            set { _Artist = value; }
        }
        private string _Album;        //所属唱片,30个字节   
        public string Album
        {
            get { return _Album; }
            set { _Album = value; }
        }
        private string _Year;         //年,4个字符   
        public string Year
        {
            get { return _Year; }
            set { _Year = value; }
        }
        private string _Comment;      //注释,28个字节
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }
        private Image _AlbumCover;    //专辑封面
        public Image AlbumCover
        {
            get { return _AlbumCover; }
            set { _AlbumCover = value; }
        }
        private char _reserved1;      //保留位，一个字节
        public char reserved1
        {
            get { return _reserved1; }
            set { _reserved1 = value; }
        }
        private char _reserved2;      //保留位，一个字节 
        public char reserved2
        {
            get { return _reserved2; }
            set { _reserved2 = value; }
        }
        private char _reserved3;      //保留位，一个字节 
        public char reserved3
        {
            get { return _reserved3; }
            set { _reserved3 = value; }
        }

        private string mp3FilePath=null;

        /// <summary>  
        /// 构造函数,输入文件名即得到信息  
        /// </summary>  
        /// <param name="mp3FilePos"></param>  
        public Mp3FileInfo(String mp3FilePos)
        {
            mp3FilePath=mp3FilePos;
            getMp3Info(mp3FilePos);
        }

        /// <summary>  
        /// 去除/0字符  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        private  String formatString(String str)
        {
            return str.Replace("/0", "").Replace("\0", "").Trim();
        }
        /// <summary>   
        /// 获取MP3文件最后128个字节   
        /// </summary>   
        /// <param name="FileName">文件名</param>   
        /// <returns>返回字节数组</returns>   
        public  byte[] getLast128(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            Stream stream = fs;
            stream.Seek(-128, SeekOrigin.End);
            const int seekPos = 128;
            int rl = 0;
            byte[] Info = new byte[seekPos];
            rl = stream.Read(Info, 0, seekPos);
            fs.Close();
            stream.Close();
            return Info;
        }
        /// <summary>   
        /// 获取MP3歌曲的相关信息   
        /// </summary>   
        /// <param name = "Info">从MP3文件中截取的二进制信息</param>   
        /// <returns>返回一个Mp3Info结构</returns>   
        public  void getMp3Info(string mp3Path)
        {
            byte[] Info = getLast128(mp3Path);
            string str = null;
            int i;
            int position = 0;//循环的起始值   
            int currentIndex = 0;//Info的当前索引值  
            //获取TAG标识  
            for (i = currentIndex; i < currentIndex + 3; i++)
            {
                str = str + (char)Info[i];
                position++;
            }
            currentIndex = position;
            identify = str;

            //获取歌名   
            str = null;
            byte[] bytTitle = new byte[30];//将歌名部分读到一个单独的数组中   
            int j = 0;
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytTitle[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            Title = formatString(byteToString(bytTitle));

            //获取歌手名  
            str = null;
            j = 0;
            byte[] bytArtist = new byte[30];//将歌手名部分读到一个单独的数组中  
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytArtist[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            Artist = formatString(byteToString(bytArtist));

            //获取唱片名   
            str = null;
            j = 0;
            byte[] bytAlbum = new byte[30];//将唱片名部分读到一个单独的数组中  
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytAlbum[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            Album = formatString(byteToString(bytAlbum));

            //获取年   
            str = null;
            j = 0;
            byte[] bytYear = new byte[4];//将年部分读到一个单独的数组中  
            for (i = currentIndex; i < currentIndex + 4; i++)
            {
                bytYear[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            Year = formatString(byteToString(bytYear));
            //获取注释  
            str = null;
            j = 0;
            byte[] bytComment = new byte[28];//将注释部分读到一个单独的数组中  
            for (i = currentIndex; i < currentIndex + 25; i++)
            {
                bytComment[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            Comment = formatString(byteToString(bytComment));

            //获取专辑封面
            AlbumCover = GetAlbumCover(mp3Path);         

            //以下获取保留位   
            reserved1 = (char)Info[++position];
            reserved2 = (char)Info[++position];
            reserved3 = (char)Info[++position];            
        }
        /// <summary>  
        /// 将字节数组转换成字符串   
        /// </summary>   
        /// <param name = "b">字节数组</param>   
        /// <returns>返回转换后的字符串</returns>  
        public string byteToString(byte[] b)
        {
            Encoding enc = Encoding.GetEncoding("GB2312");
            string str = enc.GetString(b);
            str = str.Substring(0, str.IndexOf("#CONTENT#") >= 0 ? str.IndexOf("#CONTENT#") : str.Length);//去掉无用字符               
            return str;
        }

        public Image GetAlbumCover(string path)
        {
            Image iamge = null;
            FileStream fs = new FileStream(path, FileMode.Open);
            try
            {
                byte[] header = new byte[10]; //标签头
                int offset = 0;
                bool haveAPIC = false;
                fs.Read(header, 0, 10);
                offset += 10;
                string head = Encoding.Default.GetString(header, 0, 3);

                if (head.Equals("ID3"))
                {
                    int sizeAll = header[6] * 0x200000 //获取该标签的尺寸，不包括标签头
                    + header[7] * 0x4000
                    + header[8] * 0x80
                    + header[9];
                    int size = 0;
                    byte[] body = new byte[10]; //数据帧头,这里认为数据帧头不包括编码方式
                    fs.Read(body, 0, 10);
                    offset += 10;
                    head = Encoding.Default.GetString(body, 0, 4);
                    while (offset < sizeAll) //当数据帧不是图片的时候继续查找
                    {
                        if (("APIC".Equals(head))) { haveAPIC = true; break; }
                        size = body[size + 4] * 0x1000000 //获取该数据帧的尺寸(不包括帧头)
                        + body[size + 5] * 0x10000
                        + body[size + 6] * 0x100
                        + body[size + 7];
                        body = new byte[size + 10];
                        fs.Read(body, 0, size + 10);
                        offset += size + 10;
                        head = Encoding.Default.GetString(body, size, 4);
                    }
                    if (haveAPIC)
                    {
                        size = body[size + 4] * 0x1000000
                        + body[size + 5] * 0x10000
                        + body[size + 6] * 0x100
                        + body[size + 7];
                        byte[] temp = new byte[9];
                        byte[] temptype = new byte[10];
                        fs.Seek(1, SeekOrigin.Current);
                        fs.Read(temp, 0, 9);                       
                        int i = 0;
                        switch (Encoding.Default.GetString(temp))
                        {
                            case "image/jpe":
                                
                                while (i < size) //jpeg开头0xFFD8
                                {
                                    if (temptype[0] == 0 && temptype[1] == 255 && temptype[2] == 216)
                                    {
                                        break;
                                    }
                                    fs.Seek(-2, SeekOrigin.Current); 
                                    fs.Read(temptype, 0, 3);
                                    i++;
                                }
                                fs.Seek(-2, SeekOrigin.Current);
                                break;
                            case "image/jpg":
                                
                                while (i < size) //jpg开头0xFFD8
                                {
                                    if (temptype[0] == 0 && temptype[1] == 255 && temptype[2] == 216)
                                    {
                                        break;
                                    }
                                    fs.Seek(-2, SeekOrigin.Current); 
                                    fs.Read(temptype, 0, 3);
                                    i++;
                                }
                                fs.Seek(-2, SeekOrigin.Current);
                                break;
                            case "image/gif":

                                while (i < size) //gif开头474946
                                {
                                    if (temptype[0] == 71 && temptype[1] == 73 && temptype[2] == 70)
                                    {
                                        break;
                                    }
                                    fs.Seek(-2, SeekOrigin.Current);
                                    fs.Read(temptype, 0, 3);
                                    i++;
                                }
                                fs.Seek(-3, SeekOrigin.Current);
                                break;
                            case "image/bmp":

                                while (i < size) //bmp开头424d
                                {
                                    if (temptype[0] == 66 && temptype[1] == 77)
                                    {
                                        break;
                                    }
                                    fs.Seek(-1, SeekOrigin.Current);
                                    fs.Read(temptype, 0, 2);
                                    i++;
                                }
                                fs.Seek(-2, SeekOrigin.Current);
                                break;
                            case"image/png":
                                while (i < size) //png开头89 50 4e 47 0d 0a 1a 0a
                                {
                                    if (temptype[0] == 137 && temptype[1] == 80 && temptype[2] == 78 && temptype[3] == 71 && temptype[4] == 13)
                                    {
                                        break;
                                    }
                                    fs.Seek(-9, SeekOrigin.Current);
                                    fs.Read(temptype, 0, 10);
                                    i++;
                                }
                                fs.Seek(-10, SeekOrigin.Current);
                                break;
                            default://FFFB为音频的开头
                                break;
                        }

                        byte[] image = new byte[size];
                        fs.Read(image, 0, size);
                        MemoryStream ms = new MemoryStream(image);
                        iamge = Image.FromStream(ms);                       
                    }
                    else
                    {
                        iamge = null;
                    }
                }
                else { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(path + ex.Message);
            }
            finally
            {
                fs.Close();
            }
            return iamge;
        }

        public void UpdateAlbumCover(Image newImage)
        {
            FileStream fs = new FileStream(mp3FilePath, FileMode.Open, FileAccess.ReadWrite);           
            try
            {
                
                byte[] header = new byte[10]; //标签头
                byte[] byteNewMp3;
                int offset = 0;
                bool haveAPIC = false;
                fs.Read(header, 0, 10);
                offset += 10;
                string head = Encoding.Default.GetString(header, 0, 3);
                MemoryStream msImage = new MemoryStream();
                newImage.Save(msImage, ImageFormat.Jpeg);
                byte[] byteImage = msImage.ToArray();
                byte[] byteJpegType = new byte[] { 0,105,109,97,103,101,47,106, 112, 101, 103, 0, 0, 0 };                
                int newImageSize = byteImage.Length;
                byte[] byteNewSize = new byte[4];
                int dsize = 0;
                if (head.Equals("ID3"))
                {
                    int sizeAll = header[6] * 0x200000 //获取该标签的尺寸，不包括标签头
                    + header[7] * 0x4000
                    + header[8] * 0x80
                    + header[9];
                    int size = 0;

                    byte[] body = new byte[10]; 
                    fs.Read(body, 0, 10);
                    offset += 10;
                    head = Encoding.Default.GetString(body, 0, 4);
                    while (offset < sizeAll)
                    {
                        if (("APIC".Equals(head))) { haveAPIC = true; break; }
                        size = body[size + 4] * 0x1000000
                        + body[size + 5] * 0x10000
                        + body[size + 6] * 0x100
                        + body[size + 7];
                        body = new byte[size + 10];
                        fs.Read(body, 0, size + 10);
                        offset += size + 10;
                        head = Encoding.Default.GetString(body, size, 4);
                    }
                    if (haveAPIC)
                    {
                        size = body[size + 4] * 0x1000000
                        + body[size + 5] * 0x10000
                        + body[size + 6] * 0x100
                        + body[size + 7];
                        dsize = newImageSize - size;
                        byteNewSize[3] = (byte)(newImageSize & 0xff);
                        byteNewSize[2] = (byte)((newImageSize >> 8) & 0xff);
                        byteNewSize[1] = (byte)((newImageSize >> 16) & 0xff);
                        byteNewSize[0] = (byte)((newImageSize >> 24) & 0xff);
                        fs.Seek(-6, SeekOrigin.Current);
                        if (!File.Exists(mp3FilePath + ".bak"))
                        {
                            File.Copy(mp3FilePath, mp3FilePath + ".bak");
                        }                     
                        fs.Write(byteNewSize, 0, 4);

                        int Current = (int)fs.Position + 2;
                        fs.Seek(0, SeekOrigin.Begin);
                        byte[] byteID3V2_1 = new byte[Current];
                        fs.Read(byteID3V2_1, 0, Current);

                        fs.Seek(sizeAll + 10, SeekOrigin.Begin);
                        byte[] byteID3V2_2 = new byte[(int)(fs.Length - fs.Position)];
                        fs.Read(byteID3V2_2, 0, (int)(fs.Length - fs.Position));

                        //组合MP3文件
                        byteNewMp3 = new byte[byteID3V2_1.Length + byteJpegType.Length + byteImage.Length + byteID3V2_2.Length];
                        byteID3V2_1.CopyTo(byteNewMp3, 0);
                        byteJpegType.CopyTo(byteNewMp3, byteID3V2_1.Length);
                        byteImage.CopyTo(byteNewMp3, byteID3V2_1.Length + byteJpegType.Length);
                        byteID3V2_2.CopyTo(byteNewMp3, byteID3V2_1.Length + byteJpegType.Length + byteImage.Length);
                        byteNewMp3[9] = (byte)((sizeAll+dsize) & 0x7f);
                        byteNewMp3[8] = (byte)(((sizeAll+dsize) >> 7) & 0x7f);
                        byteNewMp3[7] = (byte)(((sizeAll+dsize) >> 14) & 0x7f);
                        byteNewMp3[6] = (byte)(((sizeAll+dsize) >> 21) & 0x7f);
                        fs.Close();
                        FileStream fsMp3 = new FileStream(mp3FilePath, FileMode.Create, FileAccess.ReadWrite);
                        fsMp3.Write(byteNewMp3, 0, byteNewMp3.Length);
                        fsMp3.Close();

                    }
                    else
                    {
                        if (!File.Exists(mp3FilePath + ".bak"))
                        {
                            File.Copy(mp3FilePath, mp3FilePath + ".bak");
                        }                     
                        byte[] byteID3V2_1Temp = new byte[sizeAll+10];
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(byteID3V2_1Temp, 0, sizeAll+10);
                        byte[] byteID3V2_1 = RemoveBackZero(byteID3V2_1Temp);
                        byte[] byteAPIC = new byte[] { 65, 80, 73, 67, (byte)((newImageSize >> 24) & 0xff), (byte)((newImageSize >> 16) & 0xff), (byte)((newImageSize >> 8) & 0xff), (byte)(newImageSize & 0xff), 0, 0 };
                        byte[] byteID3V2_2Temp = new byte[fs.Length - sizeAll-10];
                        fs.Read(byteID3V2_2Temp, 0, byteID3V2_2Temp.Length);
                        byte[] byteID3V2_2 = RemoveFrontZero(byteID3V2_2Temp);
                        //组合MP3文件
                        byteNewMp3 = new byte[byteID3V2_1.Length +byteAPIC.Length+byteJpegType.Length + byteImage.Length + byteID3V2_2.Length];
                        byteID3V2_1.CopyTo(byteNewMp3, 0);
                        byteAPIC.CopyTo(byteNewMp3, byteID3V2_1.Length);
                        byteJpegType.CopyTo(byteNewMp3,byteID3V2_1.Length+byteAPIC.Length);
                        byteImage.CopyTo(byteNewMp3, byteID3V2_1.Length + byteAPIC.Length+byteJpegType.Length);
                        byteID3V2_2.CopyTo(byteNewMp3, byteID3V2_1.Length +byteAPIC.Length+ byteJpegType.Length + byteImage.Length);
                        sizeAll = byteID3V2_1.Length + byteAPIC.Length + byteJpegType.Length + byteImage.Length;
                        byteNewMp3[9] = (byte)((sizeAll-10) & 0x7f);
                        byteNewMp3[8] = (byte)(((sizeAll-10) >> 7) & 0x7f);
                        byteNewMp3[7] = (byte)(((sizeAll-10) >> 14) & 0x7f);
                        byteNewMp3[6] = (byte)(((sizeAll-10) >> 21) & 0x7f);
                        fs.Close();
                        FileStream fsMp3 = new FileStream(mp3FilePath, FileMode.Create, FileAccess.ReadWrite);
                        fsMp3.Write(byteNewMp3, 0, byteNewMp3.Length);
                        fsMp3.Close();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally
            {
                fs.Close();                
            }
      
        }
        private byte[] RemoveBackZero(byte[] bytes)
        {
            ArrayList al = new ArrayList(bytes);
            for (int i = bytes.Length; i > 0; i--)
            {
                if ((int)bytes[i - 1] == 0)
                {
                    al.RemoveAt(i - 1);
                }
                else
                {
                    break;
                }

            }         
            return (byte[])al.ToArray(typeof(byte));
        }

        private byte[] RemoveFrontZero(byte[] bytes)
        {
            ArrayList al = new ArrayList(bytes);
            for (int i = 0; i < bytes.Length; i++)
            {
                if ((int)bytes[i] == 0)
                {
                    al.RemoveAt(i);
                }
                else
                {
                    break;
                }

            }
            return (byte[])al.ToArray(typeof(byte));
        }

    }
}
