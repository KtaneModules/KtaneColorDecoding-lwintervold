using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


public interface Constraint{

	List<List<Cell>> enumeratePlacementsInSlot(List<Cell> slot);

	bool isValidInSlot(List<Cell> slot);

	bool Equals (Constraint constraint);

	List<_colors> getPattern ();

    string toString();
}



