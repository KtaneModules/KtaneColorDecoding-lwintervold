using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
	private string color;

	public Cell(){
		this.color = " ";
	}

	public Cell(string color){
		this.color = color;
	}

	public string getColor(){
		return this.color;
	}

	public void setColor(string color){
		this.color = color;
	}
}
