using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDecodingHelper;

public class Indicator {
	private static readonly List<string> _colors = new List<string> { "R", "G", "B", "Y", "P" };
	private static readonly List<string> _multipatterns = new List<string> { "checkered", "vertical", "horizontal" };
	private List<List<Cell>> board;
	private List<string> indicator_colors;
	private List<string> skipinfo;
	private int tablenum;
	private string pattern;


	public Indicator(){
	}

	public List<string> getIndicator_Colors(){
		return indicator_colors;
	}

	public List<List<Cell>> getBoard(){
		return board;
	}

	public List<string> getSkipInfo(){
		return skipinfo;
	}

	public int getTableNum(){
		return tablenum;
	}

	public string getPattern(){
		return pattern;
	}

	public void generateRandomState(KMBombInfo BombInfo, int stagenum){
		board = new List<List<Cell>> ();
		int numcolors = Random.Range (1, 5);
		//the solid pattern should be less frequent than 1/4 chance. 1/4 -> 1/16
		if (numcolors == 1)
			numcolors = Random.Range (1, 5);
		
		List<string> copycolors = new List<string>();
		foreach (string color in _colors){
			copycolors.Add (color);
		}
		copycolors.Shuffle ();
		indicator_colors = copycolors.GetRange (0, numcolors);

		if (numcolors == 1) 
			pattern = "solid";
		else
			pattern = _multipatterns [Random.Range (0, 3)];

		List<Cell> row;
		if (pattern == "solid") {
			string color = indicator_colors [0];
			for (int i = 0; i < 4; i++) {
				row = new List<Cell> ();
				for (int j = 0; j < 4; j++) {
					row.Add (new Cell (color));
				}
				board.Add (row);
			}
		} else if (pattern == "horizontal") {
			for (int i = 0; i < 4; i++) {
				row = new List<Cell> ();
				string color = indicator_colors[i % indicator_colors.Count];
				for (int j = 0; j < 4; j++) {
					row.Add (new Cell (color));
				}
				board.Add (row);
			}
		} else if (pattern == "vertical"){
			for (int i = 0; i < 4; i++){
				row = new List<Cell> ();
				for (int j = 0; j < 4; j++) {
					string color = indicator_colors [j % indicator_colors.Count];
					row.Add (new Cell(color));
				}
				board.Add (row);
			}
		} else if (pattern == "checkered" && indicator_colors.Count == 2) {
			for (int i = 0; i < 4; i++) {
				row = new List<Cell> ();
				for (int j = 0; j < 4; j++) {
					string color = indicator_colors [(i + j) % 2];
					row.Add (new Cell(color));
				}
				board.Add (row);
			}
		} else if (pattern == "checkered" && indicator_colors.Count == 3) {
			for (int i = 0; i < 4; i++) {
				row = new List<Cell> ();
				for (int j = 0; j < 4; j++) {
					string color = indicator_colors [((j + i % 2) * 2 % 4) / (2 - j % 2)];
					row.Add (new Cell (color));
				}
				board.Add (row);
			}
		} else if (pattern == "checkered" && indicator_colors.Count == 4) {
			for (int i = 0; i < 4; i++) {
				row = new List<Cell> ();
				for (int j = 0; j < 4; j++) {
					string color = indicator_colors [(((j % 2) - 2 *(i % 2)) + 4) % 4];
					row.Add (new Cell (color));
				}
				board.Add (row);
			}
		}
		List<string> venncolors;

		if (pattern == "checkered") {
			if (BombInfo.GetBatteryCount() <= 2) {
				venncolors = new List<string> { "R", "G", "B", "Y" };
				skipinfo = new List<string> { "AC", "B", "BE" };
			} else {
				venncolors = new List<string> { "P", "B", "Y", "R" };
				skipinfo = new List<string> { "BD", "D", "CE" };
			}
		} else if (pattern == "vertical") {
			if (BombInfo.GetPortCount () <= 2) {
				venncolors = new List<string> { "G", "R", "P", "Y" };
				skipinfo = new List<string> { "C", "AD", "AB" };
			} else {
				venncolors = new List<string> { "B", "Y", "G", "P" };
				skipinfo = new List<string> { "AE", "BD", "AD" };
			}
		} else if (pattern == "horizontal") {
			if (BombInfo.GetOnIndicators ().Count () <= 2) {
				venncolors = new List<string> { "Y", "P", "R", "B" };
				skipinfo = new List<string> { "D", "AC", "BE" };
			} else {
				venncolors = new List<string> { "G", "B", "P", "R" };
				skipinfo = new List<string> { "CE", "A", "CD" };
			}
		} else {
			if (stagenum == 0 || stagenum == 2) {
				venncolors = new List<string> { "P", "G", "B", "R" };
				skipinfo = new List<string> { "AE", "BD", "C" };
			} else {
				venncolors = new List<string> { "Y", "R", "G", "P" };
				skipinfo = new List<string> { "E", "AD", "BC" };
			}
		}
		//VennColorsToTableNum
		tablenum = 0;
		int region = 0;
		int v = 1;
		for (int i = 0; i < venncolors.Count; i++) {
			if (indicator_colors.Contains (venncolors [i]))
				region |= v;
			v = v << 1;
		}
		//Tablenums are indexed from 0.
		switch (region) {
		case 0:
			tablenum = 2;
			break;
		case 1:
			tablenum = 7;
			break;
		case 2:
			tablenum = 1;
			break;
		case 3:
			tablenum = 5;
			break;
		case 4:
			tablenum = 4;
			break;
		case 5:
			tablenum = 1;
			break;
		case 6:
			tablenum = 3;
			break;
		case 7:
			tablenum = 4;
			break;
		case 8:
			tablenum = 6;
			break;
		case 9:
			tablenum = 2;
			break;
		case 10:
			tablenum = 5;
			break;
		case 11:
			tablenum = 0;
			break;
		case 12:
			tablenum = 0;
			break;
		case 13:
			tablenum = 3;
			break;
		case 14:
			tablenum = 7;
			break;
		case 15:
			tablenum = 6;
			break;
		}
	}
}
