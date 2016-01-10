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
using System.ComponentModel;

namespace TrsEditor
{
  public class MainPageViewModel : INotifyPropertyChanged
  {
    public LinkedList<string> redoUndo = new LinkedList<string>();

    public MainPageViewModel() 
    {
      AddRedoUndo(0);
    }

    public string GetProgramAtPosition(int position)
    {
      if (MaxUndoValue == 0)
      {
        return initialPogramBlock;
      }
      else
      {
        LinkedListNode<string> currentNode = redoUndo.First;
        for (int i = 0; i < redoUndo.Count(); i++)
        {
          if (i == position && currentNode != null)
          {
            return currentNode.Value;
          }
          if (currentNode != null) currentNode = currentNode.Next;
        }
      }
      return string.Empty;
    }

    public void AddRedoUndo(int position)
    {
      if (redoUndo.Count == 0)
      {
        redoUndo.AddFirst(ProgramBlock);
      }
      else
      {
        LinkedListNode<string> currentNode = redoUndo.First;
        for (int i = 0; i < redoUndo.Count; i++)
        {
          if (i == position && currentNode != null)
          {
            redoUndo.AddAfter(currentNode, ProgramBlock);
            break;
          }
          if (currentNode != null) currentNode = currentNode.Next;
        }
      }
      if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MaxUndoValue"));
    }

    public int RedoUndoPosition
    {
      set
      {
        ProgramBlock = GetProgramAtPosition(value);
      }
    }

    public int MaxUndoValue 
    {
      get
      {
        return redoUndo.Count == 0 ? 0 : redoUndo.Count - 1;
      }
    }

    public string ProgramBlock
    {
      get
      {
        return string.IsNullOrWhiteSpace(programBlock) ? string.Empty : programBlock;
      }
      set
      {
        if (value != programBlock)
        {
          programBlock = value;
          if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ProgramBlock"));
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    const string initialPogramBlock = @"
// This is a more complex sample showing most of the language features.
// It has native functions and a custom unifier registerred with the interpreter.
// Custom unifiers solve terms that would not unify under the Martelli-Montanari rules.
// In this sample terms represents the cells on a cellular automaton,
// modelling a house next to water on a 2D grid.
// The system will automatically derive a tree next to the house and the water.

// :x should unify to 3, y to 4. should produce tree(3,5)
water(1,1);
water(3,4);
house(4,4);

[water(:x,:y), house(add[:x,1],:y)] => tree(:x,add[:y,1]);
house(:x,:y) => house_processed(:x,:y);
water(:x,:y) => water_processed(:x,:y);

// Native
limit :a,:b to $TrsNumber;
add[:a,:b] => native;
mul[:a,:b] => native;
sub(:a,:b) => native;
div(:a,:b) => native;
";

    private string programBlock = initialPogramBlock;

  }
}
