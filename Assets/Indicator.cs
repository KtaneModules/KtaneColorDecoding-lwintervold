using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDecodingHelper;

public class Indicator {
	private List<List<Cell>> board;
	private List<_colors> indicator_colors;
	private List<string> skipinfo;
	private int tablenum;
	private _patterns pattern;


public Indicator(KMBombInfo bombinfo, int stagenum){
        this.generateRandomState(bombinfo, stagenum);
	}

	public List<_colors> getIndicator_Colors(){
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

	public _patterns getPattern(){
		return pattern;
	}

	private void generateRandomState(KMBombInfo BombInfo, int stagenum){
		board = new List<List<Cell>> ();
		int numcolors = Random.Range (1, 5);
		//the solid pattern should be less frequent than 1/4 chance. 1/4 -> 1/16
		if (numcolors == 1)
			numcolors = Random.Range (1, 5);

        List<_colors> color_list = _colors.GetValues(typeof(_colors)).Cast<_colors>().ToList().GetRange(0, (int)_colors.U);
		color_list.Shuffle ();
		indicator_colors = color_list.GetRange (0, numcolors);

        if (numcolors == 1)
            pattern = _patterns.SOLID;
        else
			pattern = _patterns.GetValues(typeof(_patterns)).Cast<_patterns>().ToList()[Random.Range (0, 3)];

        for (int i = 0; i < 4; i++) {
            List<Cell> row = new List<Cell>();
            for (int j = 0; j < 4; j++) {
                switch (pattern) {
                    case _patterns.SOLID:
                        row.Add(new Cell(color_list[0]));
                        break;
                    case _patterns.HORIZONTAL:
                        row.Add(new Cell(color_list[i % indicator_colors.Count]));
                        break;
                    case _patterns.VERTICAL:
                        row.Add(new Cell(color_list[j % indicator_colors.Count]));
                        break;
                    case _patterns.CHECKERED:
                        switch (indicator_colors.Count) {
                            case 2:
                                row.Add(new Cell(indicator_colors[(i + j) % 2]));
                                break;
                            case 3:
                                row.Add(new Cell(indicator_colors[((j % 2) + 2 * (i % 2)) % 3]));
                                break;
                            case 4:
                                row.Add(new Cell(indicator_colors[((j % 2) + 2 * (i % 2))]));
                                break;
                        }
                        break;
                }
            }
            board.Add(row);
        }
		List<_colors> venncolors;

		switch (pattern) { 
            case _patterns.CHECKERED:
			    if (BombInfo.GetBatteryCount() <= 2) {
				    venncolors = new List<_colors> { _colors.R , _colors.G, _colors.B, _colors.Y };
				    skipinfo = new List<string> { "AC", "B", "BE" };
			    } else {
				    venncolors = new List<_colors> { _colors.P, _colors.B, _colors.Y, _colors.R };
				    skipinfo = new List<string> { "BD", "D", "CE" };
			    }
                break;
		    case _patterns.VERTICAL:
			    if (BombInfo.GetPortCount () <= 2) {
				    venncolors = new List<_colors> { _colors.G, _colors.R, _colors.P, _colors.Y };
				    skipinfo = new List<string> { "C", "AD", "AB" };
			    } else {
				    venncolors = new List<_colors> { _colors.B, _colors.Y, _colors.G, _colors.P };
				    skipinfo = new List<string> { "AE", "BD", "AD" };
			    }
                break;
            case _patterns.HORIZONTAL:
			    if (BombInfo.GetOnIndicators ().Count () <= 2) {
				    venncolors = new List<_colors> { _colors.Y, _colors.P, _colors.R, _colors.B };
				    skipinfo = new List<string> { "D", "AC", "BE" };
			    } else {
				    venncolors = new List<_colors> { _colors.G, _colors.B, _colors.P, _colors.R };
				    skipinfo = new List<string> { "CE", "A", "CD" };
			    }
                break;
            default:
			    if (stagenum == 0 || stagenum == 2) {
				    venncolors = new List<_colors> { _colors.P, _colors.G, _colors.B, _colors.R };
				    skipinfo = new List<string> { "AE", "BD", "C" };
			    } else {
				    venncolors = new List<_colors> { _colors.Y, _colors.R, _colors.G, _colors.P };
				    skipinfo = new List<string> { "E", "AD", "BC" };
			    }
                break;
		}
		//VennRegionToTableNum
		tablenum = 0;
		int region = 0;
		int v = 1;
		for (int i = 0; i < venncolors.Count; i++) {
			if (indicator_colors.Contains (venncolors [i]))
				region |= v;
			v = v << 1;
		}
		//Tablenums are indexed from 0.
        List<int> venn_region_mapping = new List<int> { 2, 7, 1, 5, 4, 1, 3, 4, 6, 2, 5, 0, 0, 3, 7, 6 };
        tablenum = venn_region_mapping[region];
	}
}
