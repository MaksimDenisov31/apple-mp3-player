using System.Windows;
using System.Windows.Media;

namespace Apple.MP3.Player {
 public static class Theme {
  static readonly string[,] colors={
   {"Surface","#FAFAFC","#1C1C1E"},
   {"Ink","#202027","#F2F2F7"},
   {"ControlText","#565661","#D6D6E0"},
   {"Hover","#EAEAF0","#3A3A40"},
   {"SearchBg","#EEEEF3","#2C2C31"},
   {"Placeholder","#9999A5","#9696A6"},
   {"Accent","#FA3559","#FF5C80"},
   {"ThumbStroke","#D3D3DE","#626270"},
   {"TrackBg","#E5E5ED","#3D3D46"},
   {"RowHover","#F1F1F6","#2E2E34"},
   {"Selected","#FDE7ED","#4A2536"},
   {"FrameBorder","#DEDEE7","#42424B"},
   {"Chrome","#F3F3F8","#252529"},
   {"ChromeText","#858590","#A1A1AD"},
   {"Sidebar","#F0F0F5","#232327"},
   {"SidebarLine","#E5E5EB","#38383F"},
   {"Muted","#93939E","#A0A0AE"},
   {"Navigation","#E3E3EB","#393940"},
   {"Danger","#C84865","#FF809B"},
   {"FooterMuted","#A2A2AC","#9292A0"},
   {"Muted2","#9595A0","#A0A0AE"},
   {"TrackSecondary","#90909B","#A5A5B5"},
   {"EmptyIcon","#D3D3DF","#555563"},
   {"Aside","#F4F4F8","#222226"},
   {"AsideLine","#E8E8EE","#38383F"},
   {"SmallLabel","#9696A1","#9696A6"},
   {"NowSecondary","#90909C","#B0B0BF"},
   {"Album","#A3A3AD","#9595A4"},
   {"HeartBg","#FCE4EB","#462A38"},
   {"Tagline","#A2A2AE","#90909E"},
   {"PlayerLine","#E6E6EE","#3A3A43"},
   {"MiniSubtitle","#9898A2","#A0A0AE"},
   {"TimeLabel","#9B9BA5","#9696A6"},
   {"ModalLine","#E2E2EB","#454551"},
   {"ModalText","#777784","#B2B2C0"},
   {"CancelBg","#EDEDF3","#36363E"},
   {"ScrollThumb","#B9B9C5","#717180"},
   {"ScrollHover","#8F8F9E","#A0A0B0"},
   {"MenuBg","#FCFCFE","#2C2C32"},
   {"MenuInk","#282832","#EEEEF5"},
   {"MenuLine","#E1E1EA","#4A4A55"},
   {"MenuText","#34343E","#E2E2EA"},
   {"MenuHighlightInk","#D9284B","#FF85A4"},
   {"Separator","#E8E8EF","#42424D"},
   {"PlayerBg","#FFFFFF","#252529"},
   {"ThumbFace","#FFFFFF","#ECECF4"},
   {"TooltipBg","#34343E","#3D3D49"}
  };
  public static void Apply(bool dark){
   for(int i=0;i<colors.GetLength(0);i++){
    var brush=(SolidColorBrush)new BrushConverter().ConvertFromString(colors[i,dark?2:1]);brush.Freeze();
    Application.Current.Resources[colors[i,0]]=brush;
   }
  }
  public static Brush Get(string key){return (Brush)Application.Current.Resources[key];}
 }
}
