using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApplePlayer {
 public static class Tests {
  static StreamWriter log;
  static void Check(bool value,string name){if(!value)throw new Exception(name);log.WriteLine("PASS "+name);log.Flush();}
  public static int Run(string[] args) {
   string root=Path.GetFullPath(args.Length>0?args[0]:".local/tests");Directory.CreateDirectory(root);
   int exit=1;var app=new Application();
   log=new StreamWriter(Path.Combine(root,"results.txt"),false);
   app.Startup+=async delegate {
    Player p=null;
    try {
     string fixture=Path.Combine(root,"sample.mp3");Check(File.Exists(fixture),"Real MP3 fixture exists");
     var tag=Tags.Read(fixture);Check(tag.Title=="Тестовый трек"&&tag.Artist=="Apple Player Test"&&tag.Album=="Проверка", "ID3 UTF-8/UTF-16 metadata");Check(Tags.Cover(fixture)!=null,"Embedded cover parsed");
     string state=Path.Combine(root,"test-library.json");if(File.Exists(state))File.Move(state,state+"."+Guid.NewGuid()+".old");
     p=new Player(state);p.Window.Show();await Task.Delay(300);
     Check(p.Data.Tracks.Count==0,"Fresh empty library");
     await p.Import(new[]{fixture,fixture});Check(p.Data.Tracks.Count==1,"Import and duplicate prevention");
     string second=Path.Combine(root,"second.mp3");File.Copy(fixture,second,true);await p.Import(new[]{second});Check(p.Data.Tracks.Count==2,"Add second MP3");
     p.Get<TextBox>("Search").Text="несуществующий трек";Check(p.Visible().Count==0,"Search excludes unmatched tracks");p.Get<TextBox>("Search").Text="Apple Player Test";Check(p.Visible().Count==2,"Search by artist");p.Get<TextBox>("Search").Text="";
     p.Favorite(p.Data.Tracks[0]);Check(p.Data.Tracks[0].Favorite,"Favorite toggle");
     p.Get<Button>("FavoritesButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(p.Visible().Count==1,"Favorites filter");p.Get<Button>("LibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
     p.Get<Slider>("Volume").Value=0; p.Play(p.Data.Tracks[0]);
     for(int i=0;i<80&&!p.Opened;i++)await Task.Delay(100);
     Check(p.Opened,"MP3 decoder opens audio");Check(p.Audio.NaturalDuration.HasTimeSpan&&p.Audio.NaturalDuration.TimeSpan.TotalSeconds>7,"Decoded MP3 duration");await Task.Delay(650);Check(p.Audio.Position.TotalSeconds>0.2,"Playback position advances");
     p.Toggle();double paused=p.Audio.Position.TotalSeconds;await Task.Delay(400);Check(!p.Playing&&Math.Abs(p.Audio.Position.TotalSeconds-paused)<0.2,"Pause stops playback clock");
     p.Get<Slider>("Seek").Value=3;await Task.Delay(200);Check(Math.Abs(p.Audio.Position.TotalSeconds-3)<0.4,"Seek to three seconds");
     p.Toggle();Check(p.Playing,"Resume");p.Next(false);Check(p.Current.Path==second,"Next track");
     p.Previous();Check(p.Current.Path==fixture,"Previous uses playback history");
     p.Data.Repeat=2;p.Next(true);Check(p.Current.Path==fixture,"Repeat one");p.Data.Repeat=0;p.Data.Shuffle=true;p.Next(false);Check(p.Current.Path==second,"Shuffle avoids immediate repetition");
     p.Data.Shuffle=false;p.Next(true);Check(!p.Playing,"End of playlist stops when repeat disabled");
     p.Data.Repeat=1;p.Next(true);Check(p.Current.Path==fixture,"Repeat all wraps to start");
     p.Toggle();p.Get<Slider>("Volume").Value=0.42;p.Data.Save(state);var loaded=Library.Load(state);Check(loaded.Tracks.Count==2&&loaded.Tracks[0].Favorite&&Math.Abs(loaded.Volume-0.42)<0.001,"Library favorites and volume survive save/reload");
     p.Get<Slider>("Volume").Value=0;
     p.Window.UpdateLayout();await Task.Delay(250);
     var bmp=new RenderTargetBitmap((int)p.Window.ActualWidth,(int)p.Window.ActualHeight,96,96,PixelFormats.Pbgra32);bmp.Render(p.Window);var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(bmp));using(var file=File.Create(Path.Combine(root,"player.png")))enc.Save(file);Check(File.Exists(Path.Combine(root,"player.png")),"UI screenshot rendered");
     p.Remove(p.Current);Check(p.Current==null&&!p.Playing&&File.Exists(fixture),"Removing current track stops audio and preserves file");
     p.Play(p.Data.Tracks[0]);for(int i=0;i<50&&!p.Opened;i++)await Task.Delay(100);Check(p.Opened,"Remaining track plays after removal");
     p.Window.Close();p=null;
     File.WriteAllText(Path.Combine(root,"broken.mp3"),"invalid audio");Check(Tags.Read(Path.Combine(root,"broken.mp3")).Title=="broken","Malformed metadata handled");
     File.WriteAllText(Path.Combine(root,"corrupt.json"),"broken");Check(Library.Load(Path.Combine(root,"corrupt.json")).Tracks.Count==0,"Invalid settings handled");
     exit=0;log.WriteLine("ALL CHECKS PASSED");
    }catch(Exception e){log.WriteLine("FAIL "+e);}finally{if(p!=null)p.Window.Close();log.Dispose();app.Shutdown(exit);}
   };app.Run();return exit;
  }
 }
}
