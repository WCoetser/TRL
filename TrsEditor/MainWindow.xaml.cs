/*
This program is an implementation of the Term Rewriting Language, or TRL. 
In that sense it is also a specification for TRL by giving a reference
implementation. It contains a parser and interpreter.

Copyright (C) 2012 Wikus Coetser, 
Contact information on my blog: http://coffeesmudge.blogspot.com/

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.


This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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

namespace TrsEditor
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private MainPageViewModel code = null;
    private MainPageController controller = null;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
      DataContext = (code = new MainPageViewModel());
      controller = new MainPageController(code);
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
      try
      {
        int sliderIncrement = 0;
        var position = Convert.ToInt32(Math.Round(sldRedoUndoSlider.Value));
        Mouse.SetCursor(Cursors.Wait);
        if (code.GetProgramAtPosition(position) != code.ProgramBlock)
        {
          code.AddRedoUndo(position);
          sliderIncrement++;
        }
        string textIn = code.ProgramBlock;
        controller.ExecuteRewriteStep();
        string textOut = code.ProgramBlock;
        if (textIn != textOut)
        {
          code.AddRedoUndo(position + sliderIncrement);
          sliderIncrement++;
        }
        sldRedoUndoSlider.Value += sliderIncrement;
        Mouse.SetCursor(Cursors.Arrow);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unexpected Error: " + ex.Message, "Error");
      }
      finally
      {
        txtCode.Focus();
      }
    }
  }
}
