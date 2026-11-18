using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ApplePlayer {
 // Keep the native resize/snap behavior, while painting our own caption.
 public sealed class WindowFrame {
  readonly Window window;
  public WindowFrame(Window value) {
   window=value;
   window.SourceInitialized+=delegate{HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(Hook);};
   window.SizeChanged+=delegate{UpdateClip();};window.StateChanged+=delegate{UpdateClip();};
  }
  void UpdateClip(){var frame=window.FindName("WindowFrame") as FrameworkElement;if(frame!=null)frame.Clip=new RectangleGeometry(new Rect(0,0,Math.Max(0,frame.ActualWidth),Math.Max(0,frame.ActualHeight)),window.WindowState==WindowState.Maximized?0:12,window.WindowState==WindowState.Maximized?0:12);}
  IntPtr Hook(IntPtr hwnd,int message,IntPtr wParam,IntPtr lParam,ref bool handled) {
   if(message==0x24) {
    var monitor=MonitorFromWindow(hwnd,2);var info=new MonitorInfo();info.Size=Marshal.SizeOf(typeof(MonitorInfo));
    if(GetMonitorInfo(monitor,ref info)) {
     var limits=(MinMaxInfo)Marshal.PtrToStructure(lParam,typeof(MinMaxInfo));
     limits.MaxPosition.X=info.Work.Left-info.Monitor.Left;limits.MaxPosition.Y=info.Work.Top-info.Monitor.Top;
     limits.MaxSize.X=info.Work.Right-info.Work.Left;limits.MaxSize.Y=info.Work.Bottom-info.Work.Top;
     var source=PresentationSource.FromVisual(window);var matrix=source!=null&&source.CompositionTarget!=null?source.CompositionTarget.TransformToDevice:Matrix.Identity;
     limits.MinTrackSize.X=(int)Math.Ceiling(window.MinWidth*matrix.M11);limits.MinTrackSize.Y=(int)Math.Ceiling(window.MinHeight*matrix.M22);
     Marshal.StructureToPtr(limits,lParam,false);handled=true;
    }
   }
   return IntPtr.Zero;
  }
  [StructLayout(LayoutKind.Sequential)] struct Point {public int X,Y;}
  [StructLayout(LayoutKind.Sequential)] struct RectNative {public int Left,Top,Right,Bottom;}
  [StructLayout(LayoutKind.Sequential)] struct MinMaxInfo {public Point Reserved,MaxSize,MaxPosition,MinTrackSize,MaxTrackSize;}
  [StructLayout(LayoutKind.Sequential)] struct MonitorInfo {public int Size;public RectNative Monitor,Work;public uint Flags;}
  [DllImport("user32.dll")]static extern IntPtr MonitorFromWindow(IntPtr hwnd,uint flags);
  [DllImport("user32.dll",CharSet=CharSet.Auto)]static extern bool GetMonitorInfo(IntPtr monitor,ref MonitorInfo info);
 }
}
