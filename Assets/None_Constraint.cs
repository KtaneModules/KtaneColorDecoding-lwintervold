using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using ColorDecodingHelper;

//This class defines a constraint whose antipattern color must not appear in its assigned slot.
public class None_Constraint : Constraint{
	private readonly string antipattern;
	
	public None_Constraint (string antipattern){
		this.antipattern = antipattern;
	}

	public List<List<Cell>> enumeratePlacementsInSlot(List<Cell> slot){
		List<string> possible_colors = new List<string> {"R","G","B","Y","P"};
		possible_colors.Remove (this.antipattern);
		List<int> unused_indexes = new List<int> ();
		for (int i = 0; i < slot.Count; i++) {
			if (slot [i].getColor ().Equals (" "))
				unused_indexes.Add (i);
			else if (slot [i].getColor ().Equals (this.antipattern))
				return new List<List<Cell>> ();
		}
		unused_indexes.Shuffle ();
		return this.doEnumeration (unused_indexes, possible_colors, slot);
	}

	private List<List<Cell>> doEnumeration(List<int> unused_indexes, List<string> possible_colors, List<Cell> possible_placement){
		possible_colors.Shuffle ();
		List<List<Cell>> possible_placements = new List<List<Cell>> ();

		if (unused_indexes.Count == 0) {
			List<Cell> possible_placement_copy = new List<Cell> ();
			foreach (Cell cell in possible_placement)
				possible_placement_copy.Add (new Cell (cell.getColor ()));
			possible_placements.Add (possible_placement_copy);
			return possible_placements;
		}
		int unused_index = unused_indexes[unused_indexes.Count - 1];

		foreach (string color in possible_colors) {
			possible_placement[unused_index].setColor(color);
			List<int> unused_indexes_to_pass = unused_indexes.GetRange(0,unused_indexes.Count - 1);
			possible_placements.AddRange(this.doEnumeration(unused_indexes_to_pass, possible_colors, possible_placement));
		}
		possible_placement [unused_index].setColor (" ");
		return possible_placements;
	}

	public bool willAlwaysFail(int slotnum, Display display_board){
		//Optimization. If any cross slots do not currently contain the antipattern and have no empty cells, no enumeration will be valid.
		int start;
		int end;
		int crossnum;
		if (slotnum < 6) {
			start = 6;
			end = 12;
			crossnum = slotnum;
		} else {
			start = 0;
			end = 6;
			crossnum = slotnum - 6;
		}
		for (int i = start; i < end; i++) {
			bool contains_red_or_blank = false;
			for (int j = 0; j < display_board.getSlot (i).Count; j++) {
				if (crossnum != j && (display_board.getSlot (i) [j].getColor ().Equals (this.antipattern) || display_board.getSlot (i) [j].getColor ().Equals (" ")))
					contains_red_or_blank = true;
			}
			if (!contains_red_or_blank)
				return true;
		}
		return false;
	}

	public bool isValidInSlot(List<Cell> slot){
		foreach (Cell cell in slot) {
			if (cell.getColor ().Equals (this.antipattern) || cell.getColor ().Equals (" "))
				return false;
		}
		return true;
	}

	public bool Equals(Constraint constraint){
		return constraint.GetType () == typeof(None_Constraint) && constraint.getPattern().Equals(this.antipattern);
	}

	public string getPattern(){
		return this.antipattern;
	}
}
