using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Designer.xaml
    /// </summary>
    public partial class Designer : Window
    {
        static List<Thickness> ellipses = new List<Thickness>();
        static Designer designer;
   //   static Ellipse ellipseA =
            
            public static Designer getDefaultDesigner()
       {
           if(designer == null)
               designer = new Designer();
           return designer;
       }

            public Designer()
            {
              Ellipse ellipse1 = new Ellipse();
              Ellipse ellipse2 = new Ellipse();
              Ellipse ellipse3 = new Ellipse();
              Ellipse ellipse4 = new Ellipse();
              Ellipse ellipse5 = new Ellipse();
              Ellipse ellipse6 = new Ellipse();
              Ellipse ellipse7 = new Ellipse();
              Ellipse ellipse8 = new Ellipse();
              Ellipse ellipse9 = new Ellipse();


              InitializeComponent();
            }


        public  Thickness placement1(int index)
       {
           ellipses.Add( new Thickness(ellipse1.Margin.Left-640, ellipse1.Margin.Top-360,0,0));
           ellipses.Add(new Thickness(ellipse2.Margin.Left - 640, ellipse2.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse3.Margin.Left - 640, ellipse3.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse4.Margin.Left - 640, ellipse4.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse5.Margin.Left - 640, ellipse5.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse6.Margin.Left - 640, ellipse6.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse7.Margin.Left - 640, ellipse7.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse8.Margin.Left - 640, ellipse8.Margin.Top - 360, 0, 0));
           ellipses.Add(new Thickness(ellipse9.Margin.Left - 640, ellipse9.Margin.Top - 360, 0, 0));
            
            return ellipses[index];
        }

    }
}
