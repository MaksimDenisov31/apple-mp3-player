using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ApplePlayer {
 public static class AudioFormats {
  public static readonly string[] Extensions={".mp3",".wav",".m4a",".aac",".wma",".flac"};
  public static bool Supports(string path){return Extensions.Contains(System.IO.Path.GetExtension(path),StringComparer.OrdinalIgnoreCase);}
  public static string Filter {get{return "Музыка (MP3, WAV, M4A, AAC, WMA, FLAC)|"+String.Join(";",Extensions.Select(e=>"*"+e));}}
 }
 [DataContract] public class Track {
  [DataMember] public string Path {get;set;}
  [DataMember] public string Title {get;set;}
  [DataMember] public string Artist {get;set;}
  [DataMember] public string Album {get;set;}
  [DataMember] public bool Favorite {get;set;}
  public string Subtitle {get {return Artist + (String.IsNullOrWhiteSpace(Album) ? "" : " · " + Album);}}
  public string Heart {get {return Favorite ? "♥" : "";}}
 }
 [DataContract] public class Library {
  [DataMember] public List<Track> Tracks = new List<Track>();
  [DataMember] public double Volume = 0.7;
  [DataMember] public bool Shuffle;
  [DataMember] public int Repeat;
  [DataMember] public string Current;
  [DataMember] public double WindowWidth;
  [DataMember] public double WindowHeight;
  [DataMember] public bool WindowMaximized;
  [DataMember] public bool Dark;
  public static Library Load(string path) {
   try { using(var stream=File.OpenRead(path)) {
    var data=(Library)new DataContractJsonSerializer(typeof(Library)).ReadObject(stream);
    data.Tracks=(data.Tracks ?? new List<Track>()).Where(t=>t!=null && !String.IsNullOrEmpty(t.Path)).GroupBy(t=>t.Path,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).ToList();
    data.Volume=Double.IsNaN(data.Volume)?0.7:Math.Max(0,Math.Min(1,data.Volume));
    data.Repeat=Math.Max(0,Math.Min(2,data.Repeat));
    if(Double.IsNaN(data.WindowWidth)||Double.IsInfinity(data.WindowWidth)||data.WindowWidth<0||data.WindowWidth>10000)data.WindowWidth=0;
    if(Double.IsNaN(data.WindowHeight)||Double.IsInfinity(data.WindowHeight)||data.WindowHeight<0||data.WindowHeight>10000)data.WindowHeight=0;
    return data;
   }} catch {return new Library();}
  }
  public void Save(string path) {
   Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
   string temp=path+".tmp";
   using(var stream=File.Create(temp)) new DataContractJsonSerializer(typeof(Library)).WriteObject(stream,this);
   if(File.Exists(path)) File.Replace(temp,path,path+".bak"); else File.Move(temp,path);
  }
 }
 public static class Tags {
  static int Sync(byte[] b,int p) {return ((b[p]&127)<<21)|((b[p+1]&127)<<14)|((b[p+2]&127)<<7)|(b[p+3]&127);}
  static string Text(byte[] b) {
   if(b.Length<2) return "";
   Encoding e=b[0]==0?Encoding.GetEncoding(28591):b[0]==1?Encoding.Unicode:b[0]==2?Encoding.BigEndianUnicode:Encoding.UTF8;
   int start=1;
   if(b[0]==1 && b.Length>3) {if(b[1]==0xFE && b[2]==0xFF)e=Encoding.BigEndianUnicode; if((b[1]==0xFF && b[2]==0xFE)||(b[1]==0xFE && b[2]==0xFF))start=3;}
   return e.GetString(b,start,b.Length-start).Trim('\0',' ','\uFEFF');
  }
  public static Track Read(string path) {
   var t=new Track {Path=System.IO.Path.GetFullPath(path),Title=System.IO.Path.GetFileNameWithoutExtension(path),Artist="Неизвестный исполнитель",Album=""};
   Parse(path,delegate(string id,byte[] b){if(id=="APIC")return;string value=Text(b); if(value.Length==0)return; if(id=="TIT2")t.Title=value; if(id=="TPE1")t.Artist=value; if(id=="TALB")t.Album=value;});
   return t;
  }
  public static byte[] Cover(string path) {
   byte[] result=null;
   Parse(path,delegate(string id,byte[] b){
    if(id!="APIC" || b.Length<6 || result!=null)return;
    int p=1; while(p<b.Length && b[p]!=0)p++; p+=2;
    bool wide=b[0]==1||b[0]==2;
    while(p<b.Length) {if(wide) {if(p+1>=b.Length)return; if(b[p]==0&&b[p+1]==0){p+=2;break;}p+=2;} else {if(b[p++]==0)break;}}
    if(p<b.Length) {result=new byte[b.Length-p];Buffer.BlockCopy(b,p,result,0,result.Length);}
   });return result;
  }
  static void Parse(string path,Action<string,byte[]> frame) {
   try {using(var s=File.OpenRead(path)) using(var r=new BinaryReader(s)) {
    byte[] h=r.ReadBytes(10);if(h.Length!=10||Encoding.ASCII.GetString(h,0,3)!="ID3"||(h[3]!=3&&h[3]!=4)||(h[5]&0x80)!=0)return;
    int size=Sync(h,6); if(size>16*1024*1024 || size>s.Length-10)return;
    long end=10+size;
    if((h[5]&0x40)!=0){byte[] ext=r.ReadBytes(4);if(ext.Length<4)return; int n=h[3]==4?Sync(ext,0):Big(ext,0)+4;if(n<4||s.Position+n-4>end)return;s.Position+=n-4;}
    while(s.Position+10<=end) {
     byte[] f=r.ReadBytes(10);if(f[0]==0)break;
     string id=Encoding.ASCII.GetString(f,0,4);int length=h[3]==4?Sync(f,4):Big(f,4);
     if(length<0||s.Position+length>end)break;
     if((id=="TIT2"||id=="TPE1"||id=="TALB"||id=="APIC") && f[9]==0 && f[8]==0)frame(id,r.ReadBytes(length)); else s.Position+=length;
    }
   }} catch(IOException) {} catch(UnauthorizedAccessException) {} catch(ArgumentException) {}
  }
  static int Big(byte[] b,int p) {return (b[p]<<24)|(b[p+1]<<16)|(b[p+2]<<8)|b[p+3];}
 }
}
