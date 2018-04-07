using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDecodingHelper;

//This class defines a constraint whose components must appear ONLY ONCE in its slot in any order and placement.
//TODO: Finish implementation
public class Exact_Constraint : Constraint{
	private readonly string pattern;

	public Exact_Constraint(string pattern){
		this.pattern = pattern;
	}

	public List<List<Cell>> enumeratePlacementsInSlot(List<Cell> slot){
		List<int> unused_indexes = new List<int> ();
		List<string> unused_pattern_chars = new List<string> (this.pattern.Split());
		for (int i = 0; i < slot.Count; i++) {
			if (slot [i].getColor ().Equals (" "))
				unused_indexes.Add (i);
			for (int j = 0; j < this.pattern.Length; j++) {
				if (this.pattern [j].Equals (slot [i].getColor ()))
					unused_pattern_chars.Remove(this.pattern[j].ToString());
			}
		}
		unused_indexes.Shuffle ();
		return this.doEnumeration(unused_indexes, unused_pattern_chars, slot);
	}

	private List<List<Cell>> doEnumeration(List<int> unused_indexes, List<string> unused_pattern_chars, List<Cell> possible_placement){
		List<List<Cell>> possible_placements = new List<List<Cell>> ();

		if (unused_indexes.Count < unused_pattern_chars.Count)
			return possible_placements;
		
		if (unused_pattern_chars.Count == 0){
			List<Cell> possible_placements_copy = new List<Cell> ();
			foreach (Cell cell in possible_placement)
				possible_placements_copy.Add (new Cell (cell.getColor ()));
			return possible_placements;
		}

		List<int> unused_indexes_copy = new List<int> (unused_indexes);
		List<string> unused_pattern_chars_copy = new List<string> (unused_pattern_chars);

		for (int i = 0; i < unused_indexes.Count; i++) {
			for (int j = 0; j < unused_pattern_chars.Count; j++) {
				possible_placement [unused_indexes [i]].setColor (unused_pattern_chars [i]);
				unused_indexes_copy.RemoveAt (i);
				unused_pattern_chars_copy.RemoveAt (j);
				possible_placements.AddRange (this.doEnumeration (unused_indexes_copy, unused_pattern_chars_copy, possible_placement));

				unused_indexes_copy = new List<int> (unused_indexes);
				unused_pattern_chars_copy = new List<string> (unused_pattern_chars);
			}
			possible_placement [i].setColor (" ");
		}

		return possible_placements;
	}

	public bool isValidInSlot(List<Cell> slot){
		for (int i = 0; i < this.pattern.Length; i++) {
			int instances = 0;
			foreach (Cell cell in slot){
				if (cell.getColor ().Equals (this.pattern [i]))
					instances++;
			}
			if (instances != 1)
				return false;
		}
		return true;
	}

	public bool Equals(Constraint constraint){
		return constraint.GetType () == typeof(Exact_Constraint) && constraint.getPattern().Equals(this.pattern);
	}

	public string getPattern(){
		return this.pattern;
	}
}