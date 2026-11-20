using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

[assembly: AssemblyTitle("Apple MP3 Player")]
[assembly: AssemblyProduct("Apple MP3 Player")]
[assembly: AssemblyVersion("1.1.1.0")]
namespace Apple.MP3.Player {
 public class Player {
  public Window Window;
  public readonly MediaPlayer Audio=new MediaPlayer();
  public Library Data;
  public Track Current;
  public bool Playing;
  public bool Opened;
  readonly string store;
  bool updating, favorites, importing;
  int importRevision;
  public const string DefaultAppsUri="ms-settings:defaultapps?registeredAppUser=Apple%20MP3%20Player";
  readonly Random random=new Random();
  readonly DispatcherTimer timer=new DispatcherTimer();
  readonly DispatcherTimer sizeSave=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(400)};
  readonly List<Track> history=new List<Track>();
  public T Get<T>(string name) where T:class {return Window.FindName(name) as T;}
  void Label(string name,string text) {Get<TextBlock>(name).Text=text;}
  void Button(string name,Action action){Get<Button>(name).Click+=delegate{action();};}
  public Player(string path) {
   store=path;Data=Library.Load(path);
   Theme.Apply(Data.Dark);
   if(!Application.Current.Resources.Contains("AppleScrollBar"))using(var styles=Assembly.GetExecutingAssembly().GetManifestResourceStream("Apple.MP3.Player.ControlStyles.xaml"))Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(styles));
   using(var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("Apple.MP3.Player.MainWindow.xaml"))Window=(Window)XamlReader.Load(s);
   using(var icon=Assembly.GetExecutingAssembly().GetManifestResourceStream("Apple.MP3.Player.Icon"))Window.Icon=new IconBitmapDecoder(icon,BitmapCreateOptions.None,BitmapCacheOption.OnLoad).Frames.OrderByDescending(frame=>frame.PixelWidth).First();
   new WindowFrame(Window);
   Rect workArea=SystemParameters.WorkArea;
   Window.Width=Math.Max(Window.MinWidth,Math.Min(Data.WindowWidth>0?Data.WindowWidth:Window.Width,Math.Max(Window.MinWidth,workArea.Width)));
   Window.Height=Math.Max(Window.MinHeight,Math.Min(Data.WindowHeight>0?Data.WindowHeight:Window.Height,Math.Max(Window.MinHeight,workArea.Height)));
   if(Data.WindowMaximized)Window.WindowState=WindowState.Maximized;
   sizeSave.Tick+=delegate{sizeSave.Stop();Save();};
   Window.SizeChanged+=delegate{if(Window.IsLoaded&&Window.WindowState==WindowState.Normal){RememberWindowSize();sizeSave.Stop();sizeSave.Start();}};
   Window.StateChanged+=delegate{if(Window.IsLoaded&&Window.WindowState!=WindowState.Minimized){RememberWindowSize();sizeSave.Stop();sizeSave.Start();}};
   Button("CloseWindowButton",delegate{Window.Close();});
   Button("MinimizeWindowButton",delegate{Window.WindowState=WindowState.Minimized;});
   Button("MaximizeWindowButton",delegate{Window.WindowState=Window.WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;});
   Button("ThemeButton",delegate{Data.Dark=!Data.Dark;ApplyTheme();Save();});
   Button("AddButton",AddDialog);Button("MoreAddButton",AddDialog);Button("FolderButton",FolderDialog);
   Button("DefaultAppsButton",OpenDefaultApps);
   Button("ClearLibraryButton",delegate{Get<Border>("ClearLibraryOverlay").Visibility=Visibility.Visible;Get<Button>("CancelClearLibraryButton").Focus();});
   Button("CancelClearLibraryButton",CancelClearLibrary);
   Button("ConfirmClearLibraryButton",ClearLibrary);
   Button("PlayButton",Toggle);Button("PreviousButton",Previous);Button("NextButton",delegate{Next(false);});
   Button("LibraryButton",delegate{favorites=false;Refresh();});Button("FavoritesButton",delegate{favorites=true;Refresh();});
   Button("HeartButton",delegate{Favorite(Current);});
   Button("ShuffleButton",delegate{Data.Shuffle=!Data.Shuffle;Modes();});Button("RepeatButton",delegate{Data.Repeat=(Data.Repeat+1)%3;Modes();});
   Button("MuteButton",delegate{Get<Slider>("Volume").Value=Get<Slider>("Volume").Value>0?0:0.7;});
   Get<Slider>("Volume").Value=Data.Volume;Audio.Volume=Data.Volume;
   Get<Slider>("Volume").ValueChanged+=delegate{Audio.Volume=Get<Slider>("Volume").Value;Data.Volume=Audio.Volume;};
   Get<Slider>("Seek").ValueChanged+=delegate{if(!updating&&Opened)Audio.Position=TimeSpan.FromSeconds(Get<Slider>("Seek").Value);};
   Get<TextBox>("Search").TextChanged+=delegate{Refresh();};
   Get<ListBox>("Tracks").MouseDoubleClick+=delegate{var t=Get<ListBox>("Tracks").SelectedItem as Track;if(t!=null)Play(t);};
   Get<ListBox>("Tracks").PreviewMouseRightButtonDown+=delegate(object sender,MouseButtonEventArgs e){var element=e.OriginalSource as DependencyObject;while(element!=null&&!(element is ListBoxItem))element=VisualTreeHelper.GetParent(element);if(element is ListBoxItem)((ListBoxItem)element).IsSelected=true;};
   var menu=Get<ListBox>("Tracks").ContextMenu;
   ((MenuItem)menu.Items[0]).Click+=delegate{var t=Get<ListBox>("Tracks").SelectedItem as Track;if(t!=null)Play(t);};
   ((MenuItem)menu.Items[1]).Click+=delegate{Favorite(Get<ListBox>("Tracks").SelectedItem as Track);};
   ((MenuItem)menu.Items[3]).Click+=delegate{Remove(Get<ListBox>("Tracks").SelectedItem as Track);};
   Window.PreviewKeyDown+=Keys;
   Window.DragOver+=delegate(object o,DragEventArgs e){e.Effects=e.Data.GetDataPresent(DataFormats.FileDrop)?DragDropEffects.Copy:DragDropEffects.None;e.Handled=true;};
   Window.Drop+=async delegate(object o,DragEventArgs e){if(e.Data.GetDataPresent(DataFormats.FileDrop))await Import((string[])e.Data.GetData(DataFormats.FileDrop));};
   Audio.MediaOpened+=delegate{Opened=true;Get<Slider>("Seek").Maximum=Audio.NaturalDuration.HasTimeSpan?Math.Max(1,Audio.NaturalDuration.TimeSpan.TotalSeconds):1;Label("Duration",Time(Get<Slider>("Seek").Maximum));if(Playing)Audio.Play();};
   Audio.MediaEnded+=delegate{Next(true);};
   Audio.MediaFailed+=delegate(object o,ExceptionEventArgs e){Playing=false;Opened=false;Audio.Close();Get<Button>("PlayButton").Content="▶";Label("Status","Не удалось воспроизвести файл. Проверьте доступность и формат.");};
   timer.Interval=TimeSpan.FromMilliseconds(250);timer.Tick+=delegate{if(!Opened)return;if(!Get<Slider>("Seek").IsMouseCaptureWithin){updating=true;Get<Slider>("Seek").Value=Audio.Position.TotalSeconds;updating=false;}Label("Elapsed",Time(Audio.Position.TotalSeconds));};timer.Start();
   Window.Closing+=delegate(object sender,System.ComponentModel.CancelEventArgs closing){try{sizeSave.Stop();RememberWindowSize();Data.Current=Current==null?null:Current.Path;Data.Save(store);}catch(Exception e){if(MessageBox.Show("Не удалось сохранить медиатеку: "+e.Message+"\nЗакрыть плеер?","Apple MP3 Player",MessageBoxButton.YesNo,MessageBoxImage.Warning)==MessageBoxResult.No)closing.Cancel=true;}};
   Window.Closed+=delegate{timer.Stop();sizeSave.Stop();Audio.Close();};
   Current=Data.Tracks.FirstOrDefault(t=>t.Path==Data.Current);if(Current!=null)ShowTrack();ApplyTheme();
  }
  public static string Time(double seconds){var t=TimeSpan.FromSeconds(Math.Max(0,seconds));return ((int)t.TotalMinutes)+":"+t.Seconds.ToString("00");}
  public List<Track> Visible() {string q=Get<TextBox>("Search").Text.Trim();return Data.Tracks.Where(t=>(!favorites||t.Favorite)&&(q.Length==0||(t.Title+" "+t.Artist+" "+t.Album).IndexOf(q,StringComparison.CurrentCultureIgnoreCase)>=0)).ToList();}
  public void Refresh() {
   var selected=Get<ListBox>("Tracks").SelectedItem;var list=Visible();Get<ListBox>("Tracks").ItemsSource=list;Get<ListBox>("Tracks").SelectedItem=selected;
   Get<StackPanel>("Empty").Visibility=list.Count==0?Visibility.Visible:Visibility.Collapsed;
   Label("PageTitle",favorites?"Избранное":"Вся музыка");Label("CountLabel",list.Count+" треков · Ваша личная коллекция");
   Label("EmptyTitle",Data.Tracks.Count==0?"Здесь будет ваша музыка":"Треки не найдены");Label("EmptyHint",Data.Tracks.Count==0?"Перетащите музыку сюда или добавьте файлы":"Измените поиск или добавьте треки в избранное");
   Get<Button>("LibraryButton").Background=favorites?Brushes.Transparent:Theme.Get("Navigation");Get<Button>("FavoritesButton").Background=favorites?Theme.Get("Navigation"):Brushes.Transparent;
   Get<Button>("ClearLibraryButton").IsEnabled=Data.Tracks.Count>0||importing;
  }
  public async Task Import(IEnumerable<string> paths) {
   if(importing){Label("Status","Дождитесь завершения добавления файлов.");return;}importing=true;int revision=importRevision;Get<Button>("ClearLibraryButton").IsEnabled=true;Label("Status","Добавляем музыку…");
   try {
    var input=paths.ToArray();var result=await Task.Run(delegate{
     var tracks=new List<Track>();var files=new List<string>();
     foreach(string p in input) {if(Directory.Exists(p))Collect(p,files);else if(File.Exists(p))files.Add(p);}
     foreach(string p in files.Distinct(StringComparer.OrdinalIgnoreCase))if(AudioFormats.Supports(p))tracks.Add(Tags.Read(p));return tracks;
    });
    if(revision!=importRevision)return;
    var known=new HashSet<string>(Data.Tracks.Select(t=>t.Path),StringComparer.OrdinalIgnoreCase);int count=0;foreach(var t in result)if(known.Add(t.Path)){Data.Tracks.Add(t);count++;}
    Refresh();Label("Status","Добавлено треков: "+count+". Двойной щелчок — воспроизведение.");Save();
   }catch(Exception e){Label("Status","Не удалось добавить музыку: "+e.Message);}finally{importing=false;Get<Button>("ClearLibraryButton").IsEnabled=Data.Tracks.Count>0;}
  }
  static void Collect(string path,List<string> files) {
   try {files.AddRange(Directory.GetFiles(path).Where(AudioFormats.Supports));foreach(string d in Directory.GetDirectories(path))if((File.GetAttributes(d)&FileAttributes.ReparsePoint)==0)Collect(d,files);}catch(IOException){}catch(UnauthorizedAccessException){}
  }
  async void AddDialog(){var d=new OpenFileDialog{Filter=AudioFormats.Filter,Multiselect=true,Title="Добавить музыку"};if(d.ShowDialog(Window)==true)await Import(d.FileNames);}
  async void FolderDialog(){using(var d=new System.Windows.Forms.FolderBrowserDialog{Description="Выберите папку с музыкой — вложенные папки тоже будут добавлены"})if(d.ShowDialog()==System.Windows.Forms.DialogResult.OK)await Import(new[]{d.SelectedPath});}
  public void Play(Track track) {
   if(!File.Exists(track.Path)){Label("Status","Файл недоступен. Возможно, он перемещён или диск отключён.");return;}
   Audio.Close();Opened=false;Current=track;Playing=true;Data.Current=track.Path;updating=true;Get<Slider>("Seek").Value=0;updating=false;Label("Elapsed","0:00");Label("Duration","0:00");
   ShowTrack();Get<Button>("PlayButton").Content="Ⅱ";Audio.Open(new Uri(track.Path));Label("Status","Воспроизведение · "+track.Title);Save();
  }
  public void Toggle() {
   if(Current==null){var first=Get<ListBox>("Tracks").SelectedItem as Track??Visible().FirstOrDefault();if(first!=null)Play(first);return;}
   if(!Opened){Play(Current);return;}Playing=!Playing;if(Playing)Audio.Play();else Audio.Pause();Get<Button>("PlayButton").Content=Playing?"Ⅱ":"▶";
  }
  public void Next(bool ended) {
   if(Current==null){Toggle();return;}
   if(ended && Data.Repeat==2){Play(Current);return;}
   var list=Visible();if(list.Count==0)list=Data.Tracks.ToList();if(list.Count==0)return;
   int index=list.IndexOf(Current);Track next=null;
   if(Data.Shuffle&&list.Count>1){var candidates=list.Where(t=>t!=Current).ToList();next=candidates[random.Next(candidates.Count)];}
   else if(index+1<list.Count)next=list[index+1];else if(!ended||Data.Repeat==1)next=list[0];
   if(next==null){Playing=false;Audio.Pause();Audio.Position=TimeSpan.Zero;Get<Button>("PlayButton").Content="▶";return;}
   history.Add(Current);Play(next);
  }
  public void Previous(){if(Current==null){Toggle();return;}if(Opened&&Audio.Position.TotalSeconds>3){Audio.Position=TimeSpan.Zero;return;}if(history.Count>0){var last=history[history.Count-1];history.RemoveAt(history.Count-1);Play(last);return;}var list=Visible();if(list.Count==0)return;int i=list.IndexOf(Current);Play(list[(i-1+list.Count)%list.Count]);}
  public void Favorite(Track t){if(t==null)return;t.Favorite=!t.Favorite;Refresh();if(t==Current)ShowTrack();Save();}
  public void CancelClearLibrary(){Get<Border>("ClearLibraryOverlay").Visibility=Visibility.Collapsed;Get<Button>("ClearLibraryButton").Focus();}
  public void ClearLibrary(){
   importRevision++;Audio.Close();Playing=false;Opened=false;Current=null;history.Clear();Data.Tracks.Clear();Data.Current=null;
   favorites=false;Get<TextBox>("Search").Text="";Get<Button>("PlayButton").Content="▶";
   updating=true;Get<Slider>("Seek").Value=0;Get<Slider>("Seek").Maximum=1;updating=false;Label("Elapsed","0:00");Label("Duration","0:00");
   ShowTrack();Refresh();Get<Border>("ClearLibraryOverlay").Visibility=Visibility.Collapsed;
   Get<Button>("AddButton").Focus();Label("Status","Медиатека очищена. Музыкальные файлы сохранены на диске.");Save();
  }
  public void OpenDefaultApps(){try{Process.Start(new ProcessStartInfo(DefaultAppsUri){UseShellExecute=true});Label("Status","В настройках Windows выберите Apple MP3 Player для музыкальных файлов и подтвердите выбор.");}catch(Exception e){Label("Status","Откройте Параметры Windows → Приложения → Приложения по умолчанию. "+e.Message);}}
  public void Remove(Track t){if(t==null)return;if(t==Current){Audio.Close();Current=null;Playing=false;Opened=false;Get<Button>("PlayButton").Content="▶";ShowTrack();updating=true;Get<Slider>("Seek").Value=0;updating=false;Label("Elapsed","0:00");Label("Duration","0:00");}Data.Tracks.Remove(t);history.RemoveAll(x=>x==t);Refresh();Save();Label("Status","Трек убран из медиатеки. Файл сохранён на диске.");}
  void ShowTrack(){Label("NowTitle",Current==null?"Поймайте настроение":Current.Title);Label("NowArtist",Current==null?"Выберите любимый трек":Current.Artist);Label("NowAlbum",Current==null?"":Current.Album);Label("MiniTitle",Current==null?"Музыка начинается здесь":Current.Title);Label("MiniArtist",Current==null?"Добавьте первый трек":Current.Artist);Get<Button>("HeartButton").Content=Current!=null&&Current.Favorite?"♥   В избранном":"♡   В избранное";Get<Image>("Cover").Source=null;
   if(Current!=null)try{byte[] bytes=Tags.Cover(Current.Path);if(bytes!=null)using(var s=new MemoryStream(bytes)){var img=new BitmapImage();img.BeginInit();img.CacheOption=BitmapCacheOption.OnLoad;img.DecodePixelWidth=600;img.StreamSource=s;img.EndInit();img.Freeze();Get<Image>("Cover").Source=img;}}catch{}
  }
  public void ApplyTheme(){Theme.Apply(Data.Dark);Get<Button>("ThemeButton").Content=Data.Dark?"☀  Светлая":"◐  Тёмная";Refresh();Modes();}
  void Modes(){Get<Button>("ShuffleButton").Foreground=Theme.Get(Data.Shuffle?"Accent":"ControlText");Get<Button>("ShuffleButton").Background=Data.Shuffle?Theme.Get("Selected"):Brushes.Transparent;Get<Button>("RepeatButton").Foreground=Theme.Get(Data.Repeat>0?"Accent":"ControlText");Get<Button>("RepeatButton").Content=Data.Repeat==2?"↻₁":"↻";Get<Button>("RepeatButton").ToolTip="Повтор: "+(Data.Repeat==0?"выключен":Data.Repeat==1?"все треки":"один трек");}
  public void RememberWindowSize(){
   if(!Window.IsLoaded||Window.WindowState==WindowState.Minimized)return;
   Rect bounds=Window.WindowState==WindowState.Maximized?Window.RestoreBounds:new Rect(0,0,Window.ActualWidth,Window.ActualHeight);
   if(!bounds.IsEmpty&&bounds.Width>=Window.MinWidth&&bounds.Height>=Window.MinHeight){Data.WindowWidth=bounds.Width;Data.WindowHeight=bounds.Height;}
   Data.WindowMaximized=Window.WindowState==WindowState.Maximized;
  }
  void Save(){try{RememberWindowSize();Data.Current=Current==null?null:Current.Path;Data.Save(store);}catch(Exception e){Label("Status","Не удалось сохранить медиатеку: "+e.Message);}}
  void Keys(object o,KeyEventArgs e){
   if(Get<Border>("ClearLibraryOverlay").Visibility==Visibility.Visible){if(e.Key==Key.Escape){CancelClearLibrary();e.Handled=true;}return;}
   if(e.Key==Key.O && Keyboard.Modifiers==ModifierKeys.Control){AddDialog();e.Handled=true;}else if(e.Key==Key.Right&&Keyboard.Modifiers==ModifierKeys.Control){Next(false);e.Handled=true;}else if(e.Key==Key.Left&&Keyboard.Modifiers==ModifierKeys.Control){Previous();e.Handled=true;}else if(e.Key==Key.Space && !(Keyboard.FocusedElement is TextBox) && !(Keyboard.FocusedElement is Button)){Toggle();e.Handled=true;}else if(e.Key==Key.Delete && Keyboard.FocusedElement==Get<ListBox>("Tracks")){Remove(Get<ListBox>("Tracks").SelectedItem as Track);e.Handled=true;}
  }
 }
 public static class Program {
  [STAThread] public static int Main(string[] args) {
   if(args.Length>0&&args[0]=="--self-test")return Tests.Run(args.Skip(1).ToArray());
   var app=new Application();
   app.DispatcherUnhandledException+=delegate(object s,DispatcherUnhandledExceptionEventArgs e){MessageBox.Show("Ошибка: "+e.Exception.Message,"Apple MP3 Player",MessageBoxButton.OK,MessageBoxImage.Warning);e.Handled=true;};
   string store=UpgradeCompatibility.DataFile();
   var player=new Player(store);player.Window.Loaded+=async delegate{if(args.Length>0){await player.Import(args);var first=player.Data.Tracks.FirstOrDefault(t=>String.Equals(t.Path,Path.GetFullPath(args[0]),StringComparison.OrdinalIgnoreCase));if(first!=null)player.Play(first);}};app.Run(player.Window);return 0;
  }
 }
}
