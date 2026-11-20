using System;
using System.IO;

namespace Apple.MP3.Player {
 public static class UpgradeCompatibility {
  public const string ProductName="Apple MP3 Player";
  // Historical components are used only to locate data from earlier releases.
  public static string PreviousDataFolder {get{return String.Concat("Apple","Player");}}
  public static string DataFile(){return MigrateLibrary(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));}
  public static string MigrateLibrary(string dataRoot){
   string previous=Path.Combine(dataRoot,PreviousDataFolder,"library.json");
   string current=Path.Combine(dataRoot,ProductName,"library.json");
   if(File.Exists(current)||!File.Exists(previous))return current;
   Directory.CreateDirectory(Path.GetDirectoryName(current));
   try{File.Copy(previous,current,false);}catch(IOException){if(!File.Exists(current))throw;}
   if(File.Exists(previous+".bak")&&!File.Exists(current+".bak"))File.Copy(previous+".bak",current+".bak",false);
   return current;
  }
 }
}
