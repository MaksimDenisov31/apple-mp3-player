using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApplePlayer {
 public static class Tests {
  static StreamWriter log;
  [StructLayout(LayoutKind.Sequential)]struct NativeRect{public int Left,Top,Right,Bottom;}
  [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr hwnd,int message,IntPtr wParam,IntPtr lParam);
  [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr hwnd,out NativeRect rect);
  static IntPtr Position(Point point){return new IntPtr(((int)point.X&65535)|(((int)point.Y&65535)<<16));}
  static int HitTest(Window window,double x,double y){return SendMessage(new WindowInteropHelper(window).Handle,0x84,IntPtr.Zero,Position(window.PointToScreen(new Point(x,y)))).ToInt32();}
  static void Check(bool value,string name){if(!value)throw new Exception(name);log.WriteLine("PASS "+name);log.Flush();}
  static T Child<T>(DependencyObject parent) where T:DependencyObject {
   for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);if(child is T)return (T)child;var found=Child<T>(child);if(found!=null)return found;}return null;
  }
  static void Snapshot(FrameworkElement element,string path){var bitmap=new RenderTargetBitmap((int)element.ActualWidth,(int)element.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(element);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path))encoder.Save(file);}
  public static int Run(string[] args) {
   string root=Path.GetFullPath(args.Length>0?args[0]:".local/tests");Directory.CreateDirectory(root);
   int exit=1;var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
   log=new StreamWriter(Path.Combine(root,"results.txt"),false);
   app.Startup+=async delegate {
    Player p=null;
    try {
     string fixture=Path.Combine(root,"sample.mp3");Check(File.Exists(fixture),"Real MP3 fixture exists");
     var tag=Tags.Read(fixture);Check(tag.Title=="Тестовый трек"&&tag.Artist=="Apple Player Test"&&tag.Album=="Проверка", "ID3 UTF-8/UTF-16 metadata");Check(Tags.Cover(fixture)!=null,"Embedded cover parsed");
     string state=Path.Combine(root,"test-library.json");if(File.Exists(state))File.Move(state,state+"."+Guid.NewGuid()+".old");
     p=new Player(state);p.Window.Show();await Task.Delay(300);
     var chrome=WindowChrome.GetWindowChrome(p.Window);
     Check(p.Window.WindowStyle==WindowStyle.None&&chrome!=null&&!chrome.UseAeroCaptionButtons,"Native caption replaced by custom Apple title bar");
     Check(p.Window.Icon is BitmapSource&&((BitmapSource)p.Window.Icon).PixelWidth==256,"Window uses full-resolution embedded music icon");
     using(var icon=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ApplePlayer.Icon")){var decoder=new IconBitmapDecoder(icon,BitmapCreateOptions.None,BitmapCacheOption.OnLoad);Check(decoder.Frames.Count==9&&decoder.Frames.Any(f=>f.PixelWidth==16)&&decoder.Frames.Any(f=>f.PixelWidth==256),"Music ICO includes nine sizes from 16 to 256 pixels");}
     Check(HitTest(p.Window,300,23)==2,"Custom title bar supports native window dragging");
     Check(HitTest(p.Window,29,23)==1,"Traffic light buttons receive client clicks");
     Check(HitTest(p.Window,2,200)==10&&HitTest(p.Window,p.Window.ActualWidth-2,p.Window.ActualHeight-2)==17,"Native edge and corner resizing remain available");
     double previousWidth=p.Window.ActualWidth,previousHeight=p.Window.ActualHeight;
     p.Get<Button>("MinimizeWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(150);Check(p.Window.WindowState==WindowState.Minimized,"Yellow button minimizes");p.Window.WindowState=WindowState.Normal;await Task.Delay(150);
     p.Get<Button>("MaximizeWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(200);Check(p.Window.WindowState==WindowState.Maximized,"Green button maximizes");
     NativeRect bounds;var hwnd=new WindowInteropHelper(p.Window).Handle;GetWindowRect(hwnd,out bounds);var work=System.Windows.Forms.Screen.FromHandle(hwnd).WorkingArea;
     Check(Math.Abs(bounds.Left-work.Left)<=1&&Math.Abs(bounds.Top-work.Top)<=1&&Math.Abs(bounds.Right-work.Right)<=1&&Math.Abs(bounds.Bottom-work.Bottom)<=1,"Maximized window fits monitor work area without covering taskbar");
     p.Get<Button>("MaximizeWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(150);Check(p.Window.WindowState==WindowState.Normal&&Math.Abs(p.Window.ActualWidth-previousWidth)<1&&Math.Abs(p.Window.ActualHeight-previousHeight)<1,"Green button restores original window size");
     SendMessage(hwnd,0xA3,new IntPtr(2),Position(p.Window.PointToScreen(new Point(300,23))));await Task.Delay(150);Check(p.Window.WindowState==WindowState.Maximized,"Double-click on custom title bar maximizes");
     SendMessage(hwnd,0xA3,new IntPtr(2),Position(p.Window.PointToScreen(new Point(300,23))));await Task.Delay(150);Check(p.Window.WindowState==WindowState.Normal,"Double-click restores window");
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
     var list=p.Get<ListBox>("Tracks");int originalCount=p.Data.Tracks.Count;
     for(int i=0;i<70;i++)p.Data.Tracks.Add(new Track{Path=fixture,Title="Проверка прокрутки · "+(i+1).ToString("00"),Artist="Тестовая медиатека",Album="Длинный список"});
     p.Refresh();p.Window.UpdateLayout();await Task.Delay(150);
     var viewer=Child<ScrollViewer>(list);Check(viewer!=null&&viewer.ScrollableHeight>0,"Long playlist overflows");
     var bar=(ScrollBar)viewer.Template.FindName("PART_VerticalScrollBar",viewer);bar.ApplyTemplate();
     Check(bar.IsVisible&&Math.Abs(bar.ActualWidth-12)<0.1,"Slim scrollbar visible on long playlist");
     var track=(System.Windows.Controls.Primitives.Track)bar.Template.FindName("PART_Track",bar);
     Check(track!=null&&track.Thumb!=null&&track.DecreaseRepeatButton.Command==ScrollBar.PageUpCommand&&track.IncreaseRepeatButton.Command==ScrollBar.PageDownCommand,"Scrollbar has thumb and page areas without arrow buttons");
     viewer.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice,Environment.TickCount,-120){RoutedEvent=Mouse.MouseWheelEvent});p.Window.UpdateLayout();
     Check(viewer.VerticalOffset>0,"Mouse wheel scrolls long playlist");
     viewer.ScrollToTop();p.Window.UpdateLayout();
     ((RoutedCommand)track.IncreaseRepeatButton.Command).Execute(null,track.IncreaseRepeatButton);p.Window.UpdateLayout();
     Check(viewer.VerticalOffset>0,"Click below thumb pages down");
     viewer.ScrollToTop();p.Window.UpdateLayout();
     track.Thumb.RaiseEvent(new DragStartedEventArgs(0,0){RoutedEvent=Thumb.DragStartedEvent});
     track.Thumb.RaiseEvent(new DragDeltaEventArgs(0,80){RoutedEvent=Thumb.DragDeltaEvent});
     track.Thumb.RaiseEvent(new DragCompletedEventArgs(0,80,false){RoutedEvent=Thumb.DragCompletedEvent});p.Window.UpdateLayout();
     Check(viewer.VerticalOffset>0,"Dragging thumb scrolls content");
     list.Focus();list.SelectedIndex=0;
     list.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(p.Window),Environment.TickCount,Key.End){RoutedEvent=Keyboard.KeyDownEvent});p.Window.UpdateLayout();await Task.Delay(100);
     Check(list.SelectedIndex==list.Items.Count-1&&viewer.VerticalOffset>0,"End key selects and reveals last track");
     Snapshot(p.Window,Path.Combine(root,"scrollbar.png"));
     var context=list.ContextMenu;context.PlacementTarget=list;context.IsOpen=true;await Task.Delay(100);context.UpdateLayout();
     Check(context.Template!=null&&((MenuItem)context.Items[0]).Template!=null,"Custom playlist context menu opens");
     Snapshot(context,Path.Combine(root,"context-menu.png"));context.IsOpen=false;
     var search=p.Get<TextBox>("Search");search.Text="тест";search.Select(0,0);var searchMenu=search.ContextMenu;searchMenu.PlacementTarget=search;searchMenu.IsOpen=true;await Task.Delay(100);
     var selectAll=(MenuItem)searchMenu.Items[4];((RoutedCommand)selectAll.Command).Execute(selectAll.CommandParameter,selectAll.CommandTarget);
     Check(selectAll.CommandTarget==search&&search.SelectedText=="тест","Styled search menu preserves text commands");searchMenu.IsOpen=false;search.Text="";
     var tip=new ToolTip{Content="Следующий трек",PlacementTarget=p.Get<Button>("NextButton")};tip.IsOpen=true;await Task.Delay(100);tip.UpdateLayout();Snapshot(tip,Path.Combine(root,"tooltip.png"));Check(tip.Template!=null,"Styled tooltip opens");tip.IsOpen=false;
     p.Window.Width=p.Window.MinWidth;p.Window.Height=p.Window.MinHeight;p.Window.UpdateLayout();Check(bar.IsVisible&&viewer.ViewportHeight>0,"Scrollbar remains usable at minimum window size");
     p.Data.Tracks.RemoveRange(originalCount,p.Data.Tracks.Count-originalCount);p.Refresh();p.Window.UpdateLayout();
     Check(viewer.ComputedVerticalScrollBarVisibility==Visibility.Collapsed,"Scrollbar hides when playlist fits");p.Window.Width=1120;p.Window.Height=806;p.Window.UpdateLayout();
     p.Remove(p.Current);Check(p.Current==null&&!p.Playing&&File.Exists(fixture),"Removing current track stops audio and preserves file");
     p.Play(p.Data.Tracks[0]);for(int i=0;i<50&&!p.Opened;i++)await Task.Delay(100);Check(p.Opened,"Remaining track plays after removal");
     foreach(string extension in AudioFormats.Extensions.Where(x=>x!=".mp3")){
      string audioPath=Path.Combine(root,"sample"+extension);int count=p.Data.Tracks.Count;await p.Import(new[]{audioPath});
      Check(p.Data.Tracks.Count==count+1,"Import supported audio "+extension);p.Play(p.Data.Tracks.Last());
      for(int i=0;i<50&&!p.Opened;i++)await Task.Delay(100);await Task.Delay(250);
      Check(p.Opened&&p.Audio.Position.TotalSeconds>0.1,"Playback advances for "+extension);p.Toggle();
     }
     Check(Player.DefaultAppsUri=="ms-settings:defaultapps?registeredAppUser=ApplePlayer"&&p.Get<Button>("DefaultAppsButton")!=null,"Default player button targets registered per-user app");
     p.Toggle();int beforeClear=p.Data.Tracks.Count;p.Get<Button>("ClearLibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
     Check(p.Get<Border>("ClearLibraryOverlay").Visibility==Visibility.Visible&&p.Data.Tracks.Count==beforeClear,"Clear button asks for confirmation without removing tracks");
     p.Window.UpdateLayout();await Task.Delay(150);Snapshot(p.Window,Path.Combine(root,"clear-library.png"));
     p.Get<Button>("CancelClearLibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
     Check(p.Data.Tracks.Count==beforeClear&&p.Playing&&p.Get<Border>("ClearLibraryOverlay").Visibility==Visibility.Collapsed,"Cancel clear preserves library and playback");
     p.Get<Button>("ClearLibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
     p.Window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(p.Window),Environment.TickCount,Key.Escape){RoutedEvent=Keyboard.PreviewKeyDownEvent});
     Check(p.Data.Tracks.Count==beforeClear&&p.Get<Border>("ClearLibraryOverlay").Visibility==Visibility.Collapsed,"Escape cancels library clear");
     p.Get<Button>("FavoritesButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));p.Get<TextBox>("Search").Text="Apple";
     p.Get<Slider>("Volume").Value=0.31;double volume=p.Data.Volume;int repeat=p.Data.Repeat;
     p.Get<Button>("ClearLibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));p.Get<Button>("ConfirmClearLibraryButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
     Check(p.Data.Tracks.Count==0&&p.Current==null&&!p.Playing&&!p.Opened,"Confirm clears entire library and stops playback");
     Check(p.Visible().Count==0&&p.Get<TextBox>("Search").Text==""&&p.Get<TextBlock>("Elapsed").Text=="0:00"&&p.Get<Image>("Cover").Source==null,"Clear resets filtered list search position and cover");
     Check(File.Exists(fixture)&&File.Exists(second)&&AudioFormats.Extensions.All(x=>File.Exists(Path.Combine(root,"sample"+x))),"Clear preserves original audio files");
     Check(Math.Abs(p.Data.Volume-volume)<0.001&&p.Data.Repeat==repeat,"Clear preserves playback preferences");
     var emptyLibrary=Library.Load(state);Check(emptyLibrary.Tracks.Count==0&&emptyLibrary.Current==null,"Empty library persisted to disk");
     p.Previous();p.Next(false);p.Toggle();Check(p.Current==null&&!p.Playing,"Cleared playback history cannot restore old tracks");
     Check(!p.Get<Button>("ClearLibraryButton").IsEnabled,"Clear button disabled for empty library");
     var pending=p.Import(new[]{fixture,second});p.ClearLibrary();await pending;
     Check(p.Data.Tracks.Count==0&&Library.Load(state).Tracks.Count==0,"Clear cancels pending import results");
     await p.Import(new[]{fixture});Check(p.Data.Tracks.Count==1&&p.Get<Button>("ClearLibraryButton").IsEnabled,"Music can be added again after clearing");
     double savedWidth=Math.Max(p.Window.MinWidth,Math.Min(1037,SystemParameters.WorkArea.Width));
     double savedHeight=Math.Max(p.Window.MinHeight,Math.Min(743,SystemParameters.WorkArea.Height));
     p.Window.Width=savedWidth;p.Window.Height=savedHeight;p.Window.UpdateLayout();await Task.Delay(650);
     var sized=Library.Load(state);Check(Math.Abs(sized.WindowWidth-savedWidth)<1&&Math.Abs(sized.WindowHeight-savedHeight)<1,"Resize automatically persists latest normal dimensions");
     bool closed=false;p.Window.Closed+=delegate{closed=true;};p.Get<Button>("CloseWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(closed&&File.Exists(state),"Red button closes window and saves library");p=null;
     p=new Player(state);p.Window.Show();await Task.Delay(200);
     Check(Math.Abs(p.Window.ActualWidth-savedWidth)<1&&Math.Abs(p.Window.ActualHeight-savedHeight)<1&&p.Data.Tracks.Count==1,"Reopened player restores custom size and music library");
     p.Get<Button>("MaximizeWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(650);sized=Library.Load(state);
     Check(sized.WindowMaximized&&Math.Abs(sized.WindowWidth-savedWidth)<1&&Math.Abs(sized.WindowHeight-savedHeight)<1,"Maximize persists state while retaining normal restore dimensions");
     p.Window.WindowState=WindowState.Minimized;await Task.Delay(500);p.Window.Close();p=null;sized=Library.Load(state);
     Check(sized.WindowMaximized&&Math.Abs(sized.WindowWidth-savedWidth)<1,"Closing minimized player preserves previous maximized state");
     p=new Player(state);p.Window.Show();await Task.Delay(250);
     Check(p.Window.WindowState==WindowState.Maximized,"Player reopens maximized after minimized close");
     p.Get<Button>("MaximizeWindowButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(600);
     Check(p.Window.WindowState==WindowState.Normal&&Math.Abs(p.Window.ActualWidth-savedWidth)<1&&Math.Abs(p.Window.ActualHeight-savedHeight)<1,"Restore after reopening recovers custom normal dimensions");
     p.Window.WindowState=WindowState.Minimized;p.Window.Close();p=null;
     p=new Player(state);p.Window.Show();await Task.Delay(150);
     Check(p.Window.WindowState==WindowState.Normal&&Math.Abs(p.Window.ActualWidth-savedWidth)<1,"Closing minimized normal player reopens in normal mode");p.Window.Close();p=null;
     sized=Library.Load(state);sized.WindowWidth=9999;sized.WindowHeight=9999;sized.Save(state);
     p=new Player(state);p.Window.Show();await Task.Delay(150);
     Check(p.Window.Width<=Math.Max(p.Window.MinWidth,SystemParameters.WorkArea.Width)&&p.Window.Height<=Math.Max(p.Window.MinHeight,SystemParameters.WorkArea.Height),"Saved dimensions are clamped to current display work area");p.Window.Close();p=null;
     string legacy=Path.Combine(root,"legacy-library.json");File.WriteAllText(legacy,"{\"Tracks\":[],\"Volume\":0.42,\"Repeat\":1,\"Shuffle\":true}");
     p=new Player(legacy);p.Window.Show();await Task.Delay(150);
     Check(p.Window.WindowState==WindowState.Normal&&p.Window.Width>=p.Window.MinWidth&&p.Window.Height>=p.Window.MinHeight,"Older libraries without size fields use valid defaults");
     Check(Math.Abs(p.Data.Volume-0.42)<0.001&&p.Data.Repeat==1&&p.Data.Shuffle,"Window size migration preserves existing playback settings");p.Window.Close();p=null;
     File.WriteAllText(Path.Combine(root,"broken.mp3"),"invalid audio");Check(Tags.Read(Path.Combine(root,"broken.mp3")).Title=="broken","Malformed metadata handled");
     File.WriteAllText(Path.Combine(root,"corrupt.json"),"broken");Check(Library.Load(Path.Combine(root,"corrupt.json")).Tracks.Count==0,"Invalid settings handled");
     exit=0;log.WriteLine("ALL CHECKS PASSED");
    }catch(Exception e){log.WriteLine("FAIL "+e);}finally{if(p!=null)p.Window.Close();log.Dispose();app.Shutdown(exit);}
   };app.Run();return exit;
  }
 }
}
