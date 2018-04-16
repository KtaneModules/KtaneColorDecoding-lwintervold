using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
	private _colors color;

	public Cell(){
		this.color = _colors.U;
	}

	public Cell(_colors color){
		this.color = color;
	}

	public _colors getColor(){
		return this.color;
	}

	public void setColor(_colors color){
		this.color = color;
	}
}
