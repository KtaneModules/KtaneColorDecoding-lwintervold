using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This class defines a constraint which must appear in its slot ONLY ONCE IN SEQUENCE in either normal or reversed order.

public class Sequence_Exact_Constraint : Constraint{
	private readonly string sequence;
	private readonly string sequence_reversed;

	public Sequence_Exact_Constraint (string sequence){
		this.sequence = sequence;
		string sequence_reversed = "";
		for (int i = sequence.Length - 1; i >= 0; i--)
			sequence_reversed = sequence_reversed + sequence[i].ToString();
		this.sequence_reversed = sequence_reversed;
	}

	public List<List<Cell>> enumeratePlacementsInSlot(List<Cell> slot){
		List<List<Cell>> possible_placements = this.getPossiblePlacements (slot, this.sequence);
		if (!this.sequence.Equals(this.sequence_reversed)){
			possible_placements.AddRange (this.getPossiblePlacements (slot, this.sequence_reversed));
		}
		List<List<Cell>> possible_valid_placements = new List<List<Cell>> ();
		foreach (List<Cell> possible_placement in possible_placements) {
			if (this.isValidInSlot (possible_placement))
				possible_valid_placements.Add (possible_placement);
		}
		return possible_valid_placements;
	}

	private List<List<Cell>> getPossiblePlacements(List<Cell> slot, string seq){
		List<List<Cell>> possible_placements = new List<List<Cell>> ();
		for (int i = 0; i < 6 - this.sequence.Length + 1; i++) {
			List<Cell> possible_placement = new List<Cell>();
			for (int start = 0; start < i; start++){
				possible_placement.Add (new Cell (slot [start].getColor ()));
			}
			for (int j = i; j < i + this.sequence.Length; j++) {
				if (!slot [j].getColor ().Equals (" ") && !slot [j].getColor ().Equals (this.sequence [j - i]))
					break;
				else
					possible_placement.Add (new Cell (this.sequence [j - i].ToString()));
			}
			for (int end = i + this.sequence.Length; end < 6; end++){
				possible_placement.Add (new Cell (slot [end].getColor ()));
			}
			if (possible_placement.Count () == 6) {
				possible_placements.Add (possible_placement);
			}
		}
		return possible_placements;
	}

	public bool isValidInSlot(List<Cell> slot){
		int found_matches = 0;
		for (int i = 0; i < 6 - this.sequence.Length + 1; i++) {
			string match = "";
			for (int j = i; j < i + this.sequence.Length; j++) {
				match = match + slot [j].getColor ();
			}
			if (match.Equals (this.sequence) || match.Equals (this.sequence_reversed))
				found_matches++;
		}
		return found_matches == 1;
	}

	public bool Equals(Constraint constraint){
		return constraint.GetType () == typeof(Sequence_Exact_Constraint) && constraint.getPattern().Equals(this.sequence);
	}

	public string getPattern(){
		return this.sequence;
	}
}


