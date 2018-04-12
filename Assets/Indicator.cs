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
	private string skipinfo;
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

	public string getSkipInfo(){
		return skipinfo;
	}

	public int getTableNum(){
		return tablenum;
	}

	public string getPattern(){
		return pattern;
	}

	public void generateRandomState(KMBombInfo BombInfo){
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
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "AC";
                        break;
                    case 3:
                        skipinfo = "B";
                        break;
                    case 4:
                        skipinfo = "BF";
                        break;
                }
			} else {
				venncolors = new List<string> { "P", "B", "Y", "R" };
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "BD";
                        break;
                    case 3:
                        skipinfo = "D";
                        break;
                    case 4:
                        skipinfo = "CE";
                        break;
                }
			}
		} else if (pattern == "vertical") {
			if (BombInfo.GetPortCount () <= 2) {
				venncolors = new List<string> { "G", "R", "P", "Y" };
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "C";
                        break;
                    case 3:
                        skipinfo = "AF";
                        break;
                    case 4:
                        skipinfo = "AB";
                        break;
                }
			} else {
				venncolors = new List<string> { "B", "Y", "G", "P" };
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "AE";
                        break;
                    case 3:
                        skipinfo = "BD";
                        break;
                    case 4:
                        skipinfo = "AD";
                        break;
                }
			}
		} else if (pattern == "horizontal") {
			if (BombInfo.GetOnIndicators ().Count () + BombInfo.GetOffIndicators().Count() <= 2) {
				venncolors = new List<string> { "Y", "P", "R", "B" };
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "D";
                        break;
                    case 3:
                        skipinfo = "AC";
                        break;
                    case 4:
                        skipinfo = "BE";
                        break;
                }
			} else {
				venncolors = new List<string> { "G", "B", "P", "R" };
                switch (numcolors)
                {
                    case 2:
                        skipinfo = "CF";
                        break;
                    case 3:
                        skipinfo = "A";
                        break;
                    case 4:
                        skipinfo = "CD";
                        break;
                }
			}
		} else {
            skipinfo = "";
			if (BombInfo.GetOnIndicators().Count() <= 1) {
				venncolors = new List<string> { "P", "G", "B", "R" };
			} else {
				venncolors = new List<string> { "Y", "R", "G", "P" };
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
