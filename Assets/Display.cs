using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDecodingHelper;


public class Display {
	private const int numRows = 6;
	private const int numCols = 6;
	private List<List<Cell>> board;
	private List<Constraint> constraints;
	private Dictionary<int,Constraint> constraintHashMap = new Dictionary<int,Constraint>();

	public Display (List<Constraint> constraints){
		this.constraints = constraints;
	}

	public void generateRandomValidBoard(){
		board = new List<List<Cell>>();
        List<int> slots = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        for (int i = 0; i < 6; i++)
            this.board.Add(new List<Cell> { new Cell(), new Cell(), new Cell(), new Cell(), new Cell(), new Cell() });

        while (true) {
            slots.Shuffle();
            List<int> slot_permutation = slots.GetRange(0, constraints.Count);
            if (this.applyConstraints(constraints, slot_permutation))
                break;
        }
	}

	private bool applyConstraints(List<Constraint> constraints, List<int> slot_permutation){
		if (constraints.Count == 0) {
			return this.fillBoard (0, 0);
		}
		int slotnum = slot_permutation [slot_permutation.Count - 1];
		List<Cell> slot = this.getSlot (slotnum);
		Constraint constraint = constraints [constraints.Count - 1];
		constraints.RemoveAt (constraints.Count - 1);
		this.constraintHashMap[slotnum] = constraint;
		List<Cell> original_slot_state = new List<Cell> ();
		foreach (Cell cell in this.getSlot(slotnum))
			original_slot_state.Add (new Cell (cell.getColor ()));

		if (constraint.GetType() == typeof(None_Constraint)){
			if (((None_Constraint) constraint).willAlwaysFail (slotnum, this)) {
				constraints.Add (constraint);
				this.constraintHashMap.Remove (slotnum);
				return false;
			}
		}
		List<List<Cell>> possible_placements = constraint.enumeratePlacementsInSlot (slot);
		possible_placements.Shuffle ();
		foreach (List<Cell> placement in possible_placements) {
			this.setSlot (slotnum, placement);
			if (this.isValidSlot (slotnum, constraints)) {
				int testedslot = slot_permutation[slot_permutation.Count - 1];
				slot_permutation.RemoveAt (slot_permutation.Count - 1);
				if (this.applyConstraints (constraints, slot_permutation))
					return true;
				slot_permutation.Add (testedslot);
			}
		}
		this.constraintHashMap.Remove(slotnum);
		this.setSlot(slotnum,original_slot_state);
		constraints.Add(constraint);
		return false;
	}
    
	private bool fillBoard(int row, int col){
		if (row == Display.numRows)
			return true;
		//Current cell color is SET
		if (!this.board[row][col].getColor().Equals(_colors.U)){
			if (col == Display.numCols - 1)
				return fillBoard (row + 1, 0);
			else
				return fillBoard (row, col + 1);
		}
		//Current cell color is NOT SET
		List<_colors> rand_colors = _colors.GetValues(typeof(_colors)).Cast<_colors>().ToList().GetRange(0, (int) _colors.U);
		rand_colors.Shuffle ();
		foreach (_colors color in rand_colors) {
			this.board [row] [col].setColor (color);
			if (this.isValidCellColor (row, col)) {
				if (col == Display.numCols - 1) {
					if (this.fillBoard (row + 1, 0))
						return true;
				} else if (this.fillBoard (row, col + 1))
					return true;
			}
		}
		this.board [row] [col].setColor (_colors.U);
		return false;
	}

	private bool isValidSlot(int slotnum, List<Constraint> remaining_constraints){
		//Check cross slots. For each cross slot, if a constraint is assigned to that slot, check that it is valid.
		//Otherwise, check that every known constraint is invalid.
		int start;
		int end;
		if (slotnum < 6) {
			start = 6;
			end = 12;
		} else {
			start = 0;
			end = 6;
		}
		for (int i = start; i < end; i++) {
			//check assigned constraint validity in cross slots
			List<Cell> cross_slot = this.getSlot (i);
			foreach (int key in this.constraintHashMap.Keys) {
				if (i == key) {
					if (!this.constraintHashMap [key].isValidInSlot (cross_slot))
						return false;
				} else {
					if (this.constraintHashMap [key].isValidInSlot (cross_slot) || this.constraintHashMap[key].isOverValidInSlot(cross_slot))
						return false;
				}
			}
			//check unassigned constraint validity in cross slots
			foreach (Constraint constraint in remaining_constraints){
				if (constraint.isValidInSlot(cross_slot) || constraint.isOverValidInSlot(cross_slot))
					return false;
			}
		}
		//check assigned constraint validity in slot
		List<Cell> current_slot = this.getSlot(slotnum);
		foreach (int key in this.constraintHashMap.Keys){
			if (key.Equals (slotnum) && !this.constraintHashMap [key].isValidInSlot (current_slot))
				return false;
			else if (!key.Equals (slotnum) && (this.constraintHashMap [key].isValidInSlot (current_slot) || this.constraintHashMap[key].isOverValidInSlot(current_slot)))
				return false;
		}
		//check unassigned constraint validity in slot
		foreach (Constraint constraint in remaining_constraints) {
			if (constraint.isValidInSlot(current_slot) || constraint.isOverValidInSlot(current_slot))
				return false;
		}
		return true;
	}

	private bool isValidCellColor(int row, int col){
		foreach (int key in this.constraintHashMap.Keys) {
			if (key == row && !this.constraintHashMap[key].isValidInSlot(getSlot(row)))
				return false;
			else if (key != row && (this.constraintHashMap [key].isValidInSlot (getSlot (row))) || this.constraintHashMap[key].isOverValidInSlot(getSlot(row)))
				return false;
			if (key == col + 6 && !this.constraintHashMap[key].isValidInSlot(getSlot(col + 6)))
				return false;
			else if (key != col + 6 && (this.constraintHashMap [key].isValidInSlot (getSlot (col + 6)) || this.constraintHashMap[key].isOverValidInSlot(getSlot(col + 6))))
				return false;
		}
		return true;
	}

	private void setSlot(int slotnum, List<Cell> slot){
		if (slotnum < 6) {
			for (int i = 0; i < 6; i++)
				this.board [slotnum] [i].setColor (slot [i].getColor ());
		} else {
			for (int i = 0; i < 6; i++) {
				this.board [i] [slotnum - 6].setColor (slot [i].getColor ());
			}
		}
	}

	public List<Cell> getSlot(int slotnum){
		if (slotnum < 6)
			return this.board [slotnum];
		
		List<Cell> slot = new List<Cell> ();
		for (int row = 0; row < 6; row++)
			slot.Add (this.board [row] [slotnum - 6]);
		return slot;

	}

	public List<List<Cell>> getBoard(){
		return this.board;
	}

	public Dictionary<int,Constraint> getConstraintHashMap(){
		return this.constraintHashMap;
	}

	public void debugLogBoard(){
		string toprint = " 6789AB\n ______\n";
		for (int row = 0; row < this.board.Count; row++){
			toprint += row;
			for (int col = 0; col < this.board [row].Count; col++) {
				toprint += this.board [row] [col].getColor ();
			}
			toprint += "\n";
		}
		toprint += " ¯¯¯¯¯¯";
		Debug.LogFormat(toprint);
	}
}


